namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    internal class Select : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly SelectListContext selectList;
        private readonly TSqlParser.Select_list_elemContext[] selectListContexts;

        private string? derivedTableAlias;

        private List<FullColumnName>? effectiveColumns;

        // internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal Select(IComponentOutput input, TSqlParser.Select_list_elemContext[] selectListContexts, SelectListContext selectList, string? derivedTableAlias)
        {
            myInput = input;
            this.selectListContexts = selectListContexts;
            this.selectList = selectList;
            this.derivedTableAlias = derivedTableAlias;
        }

        internal string? DerivedTableAlias
        {
            get { return derivedTableAlias; }
            set { derivedTableAlias = value; }
        }

        public void Rewind()
        {
            myInput.Rewind();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet rsInput = myInput.GetRows(engine, outerAccessor, max, bindValues);

            // get an effective column list ...
            if (effectiveColumns == null)
            {
                effectiveColumns = new ();
                int resultSetColumnIndex = 0;
                foreach (var c in selectListContexts)
                {
                    if (c.asterisk() != null)
                    {
                        FullTableName? tableName = FullTableName.FromPossibleTableNameContext(c.asterisk().table_name());

                        Console.WriteLine($"Asterisk!: {(tableName == null ? "no qualifier" : tableName)}");
                        for (int i = 0; i < rsInput.ColumnCount; i++)
                        {
                            FullColumnName fcn = rsInput.GetColumnName(i);
                            if (derivedTableAlias != null)
                                fcn = fcn.ApplyTableAlias(derivedTableAlias);
                            if (fcn.ColumnNameOnly() == "bookmark_key")
                                continue;

                            // check scoped table name ...
                            // InvariantCultureIgnoreCase here so that identifiers can be localized
                            if (tableName != null && !tableName.TableNameOnly.Equals(fcn.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
                            {
                                Console.WriteLine($"Skipping {tableName.TableNameOnly} != {fcn.TableNameOnly}");
                                continue;
                            }

                            effectiveColumns.Add(fcn);
                            var node = new ExpressionOperandFromColumn(rsInput.GetColumnName(i));
                            Expression expression = new () { node };
                            selectList.AddSelectListExpressionList(expression);
                        }
                    }
                    else
                    {
                        var fcn = selectList.RowsetColumnName(resultSetColumnIndex++);
                        if (derivedTableAlias != null)
                            fcn = fcn.ApplyTableAlias(derivedTableAlias);
                        effectiveColumns.Add(fcn);
                    }
                }
            }

            ResultSet rsOutput = new (effectiveColumns);
            if (rsInput.IsEOF)
            {
                rsOutput.MarkEOF();
                return rsOutput;
            }

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // add the row to the result set
                int exprIndex = 0;
                int rsIndex = 0;

                Tuple rowResults = Tuple.CreateEmpty(effectiveColumns.Count);
                foreach (FullColumnName columnName in effectiveColumns)
                {
                    try
                    {
                        ExpressionOperand result = selectList.SelectExpressions[exprIndex].Evaluate(new ResultSetValueAccessor(rsInput, i), engine, bindValues);

                        rowResults[rsIndex] = result;
                        exprIndex++;

                        rsIndex++;
                    }
                    catch (ExecutionException ex)
                    {
                        throw new ExecutionException($"Couldn't evaluate SELECT expression {selectList.SelectExpressions[exprIndex]} because of error: {ex.Message}");
                    }
                }

                rsOutput.AddRow(rowResults);
            }


            return rsOutput;
        }
    }
}
