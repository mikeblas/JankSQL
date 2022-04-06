namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    internal class Select : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly SelectListContext selectList;
        private readonly TSqlParser.Select_list_elemContext[] selectListContexts;

        private readonly string? derivedTableAlias;

        private List<FullColumnName>? effectiveColumns;

        // internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal Select(IComponentOutput input, TSqlParser.Select_list_elemContext[] selectListContexts, SelectListContext selectList, string? derivedTableAlias)
        {
            myInput = input;
            this.selectListContexts = selectListContexts;
            this.selectList = selectList;
            this.derivedTableAlias = derivedTableAlias;
        }

        public void Rewind()
        {
            myInput.Rewind();
        }

        public ResultSet? GetRows(int max)
        {
            ResultSet? rsInput = myInput.GetRows(max);
            if (rsInput == null)
                return null;

            // get an effective column list ...
            if (effectiveColumns == null)
            {
                effectiveColumns = new ();
                int resultSetColumnIndex = 0;
                foreach (var c in selectListContexts)
                {
                    if (c.asterisk() != null)
                    {
                        Console.WriteLine("Asterisk!");
                        for (int i = 0; i < rsInput.ColumnCount; i++)
                        {
                            FullColumnName fcn = rsInput.GetColumnName(i);
                            if (derivedTableAlias != null)
                                fcn.SetTableName(derivedTableAlias);
                            if (fcn.ColumnNameOnly() == "bookmark_key")
                                continue;
                            effectiveColumns.Add(fcn);
                            var node = new ExpressionOperandFromColumn(rsInput.GetColumnName(i));
                            Expression expression = new () { node };
                            selectList.AddSelectListExpressionList(expression);
                        }
                    }
                    else
                    {
                        var fcn = selectList.RowsetColumnName(resultSetColumnIndex++);
                        effectiveColumns.Add(fcn);
                    }
                }
            }

            ResultSet rsOutput = new (effectiveColumns);

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // add the row to the result set
                int exprIndex = 0;
                int rsIndex = 0;

                Tuple rowResults = Tuple.CreateEmpty(effectiveColumns.Count);
                foreach (FullColumnName columnName in effectiveColumns)
                {
                    int idx = rsInput.ColumnIndex(columnName);
                    ExpressionOperand result = selectList.Execute(exprIndex, rsInput, i);
                    rowResults[rsIndex] = result;
                    exprIndex++;

                    rsIndex++;
                }

                rsOutput.AddRow(rowResults);
            }


            return rsOutput;
        }
    }
}
