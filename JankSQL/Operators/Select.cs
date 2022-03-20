namespace JankSQL
{
    internal class Select
    {
        readonly IComponentOutput myInput;
        TSqlParser.Select_list_elemContext[] selectListContexts;
        readonly SelectListContext selectList;

        // internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal Select(IComponentOutput input, TSqlParser.Select_list_elemContext[] selectListContexts, SelectListContext selectList)
        {
            this.myInput = input;
            this.selectListContexts = selectListContexts;
            this.selectList = selectList;
        }

        internal ResultSet? GetRows(int max)
        {
            ResultSet? rsInput = myInput.GetRows(max);
            if (rsInput == null)
                return null;

            // get an effective column list ...
            List<FullColumnName> effectiveColumns = new List<FullColumnName>();
            int resultSetColumnIndex = 0;
            foreach (var c in selectListContexts)
            {
                if (c.asterisk() != null)
                {
                    Console.WriteLine("Asterisk!");
                    for (int i = 0; i < rsInput.ColumnCount; i++)
                    {
                        FullColumnName fcn = rsInput.GetColumnName(i);
                        if (fcn.ColumnNameOnly() == "bookmark_key")
                            continue;
                        effectiveColumns.Add(fcn);
                        ExpressionNode node = new ExpressionOperandFromColumn(rsInput.GetColumnName(i));
                        Expression expression = new Expression { node };
                        selectList.AddSelectListExpressionList(expression);
                    }
                }
                else
                {
                    effectiveColumns.Add(selectList.RowsetColumnName(resultSetColumnIndex++));
                }
            }


            ResultSet rsOutput = new ResultSet();
            rsOutput.SetColumnNames(effectiveColumns);

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // add the row to the result set
                int exprIndex = 0;
                int rsIndex = 0;

                ExpressionOperand[] thisRow = rsInput.Row(i);
                ExpressionOperand[] rowResults = new ExpressionOperand[effectiveColumns.Count];
                foreach (FullColumnName columnName in effectiveColumns)
                {
                    int idx = rsInput.ColumnIndex(columnName);
                    if (idx == -1 || true)
                    {
                        ExpressionOperand result = selectList.Execute(exprIndex, rsInput, i);
                        rowResults[rsIndex] = result;
                        exprIndex++;
                    }
                    else
                    {
                        rowResults[rsIndex] = thisRow[idx];
                    }
                    rsIndex++;
                }

                rsOutput.AddRow(rowResults);
            }


            return rsOutput;
        }
    }
}