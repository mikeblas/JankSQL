namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Select : IOperatorOutput
    {
        private readonly IOperatorOutput myInput;
        private readonly SelectListContext selectList;
        private readonly TSqlParser.Select_list_elemContext[] selectListContexts;

        private List<FullColumnName>? effectiveColumns;

        // internal IOperatorOutput Input { get { return myInput; } set { myInput = value; } }

        internal Select(IOperatorOutput input, TSqlParser.Select_list_elemContext[] selectListContexts, SelectListContext selectList, string? derivedTableAlias)
        {
            myInput = input;
            this.selectListContexts = selectListContexts;
            this.selectList = selectList;
            this.DerivedTableAlias = derivedTableAlias;
        }

        internal string? DerivedTableAlias { get; set; }

        public void Rewind()
        {
            myInput.Rewind();
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            if (effectiveColumns == null)
                throw new ExecutionException("Select operator has not been bound yet, so it has no effective columns.");

            return effectiveColumns.ToArray();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {

            /*
            List<FullColumnName> allColumnNames = new(inputColumnNames);
            allColumnNames.AddRange(outerColumnNames);
            */

            BindResult br = myInput.Bind(engine, outerColumnNames, bindValues);
            if (!br.IsSuccessful)
                return br;

            //~~~~~
            // what columns are coming in?
            FullColumnName[] inputColumnNames = myInput.GetOutputColumnNames();


            // get an effective column list ...
            if (effectiveColumns == null)
            {
                effectiveColumns = new();
                int resultSetColumnIndex = 0;
                foreach (var c in selectListContexts)
                {
                    if (c.asterisk() != null)
                    {
                        // got an asterisk in the select list, so we add all of the columns we have from the input
                        FullTableName? tableName = FullTableName.FromPossibleTableNameContext(c.asterisk().table_name());

                        Console.WriteLine($"Asterisk!: {(object?)tableName ?? "no qualifier"}");
                        foreach (var inputFCN in inputColumnNames)
                        {
                            FullColumnName fcn = inputFCN;
                            if (DerivedTableAlias != null)
                                fcn = fcn.ApplyTableAlias(DerivedTableAlias);
                            if (fcn.ColumnNameOnly() == "bookmark_key")
                                continue;

                            // check scoped table name ...
                            // InvariantCultureIgnoreCase here so that identifiers can be localized
                            if (tableName?.TableNameOnly.Equals(fcn.TableNameOnly, StringComparison.InvariantCultureIgnoreCase) == false)
                            {
                                Console.WriteLine($"Skipping {tableName.TableNameOnly} != {fcn.TableNameOnly}");
                                continue;
                            }

                            effectiveColumns.Add(fcn);
                            var node = new ExpressionOperandFromColumn(inputFCN);
                            Expression expression = new() { node };
                            selectList.AddSelectListExpressionList(expression);
                        }
                    }
                    else
                    {
                        var fcn = selectList.RowsetColumnName(resultSetColumnIndex++);
                        if (DerivedTableAlias != null)
                            fcn = fcn.ApplyTableAlias(DerivedTableAlias);
                        effectiveColumns.Add(fcn);
                    }
                }
            }

            foreach (var ex in selectList.SelectExpressions)
            {
                br = ex.Bind(engine, inputColumnNames, outerColumnNames, bindValues);
                if (!br.IsSuccessful)
                {
                    throw new ExecutionException($"Binding failed: {br.ErrorMessage}");
                }
            }

            return BindResult.Success();
        }


        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            if (effectiveColumns == null)
                throw new ExecutionException("Select operator has not been bound yet, so it has no effective columns.");

            ResultSet rsInput = myInput.GetRows(engine, outerAccessor, max, bindValues);

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


            if (rsOutput.IsEOF)
            {
                Console.WriteLine($" {this} produced EOF with {rsOutput.RowCount} rows");
                rsOutput.Dump();
            }
            else
            {
                Console.WriteLine($" {this} produced {rsOutput.RowCount} rows");
                rsOutput.Dump();
            }
            return rsOutput;
        }
    }
}
