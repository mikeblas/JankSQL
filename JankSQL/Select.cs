namespace JankSQL
{
    internal class Select
    {
        IComponentOutput myInput;
        TSqlParser.Select_list_elemContext[] selectListContexts;
        SelectListContext selectList;

        internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal Select(TSqlParser.Select_list_elemContext[] selectListContexts, SelectListContext selectList)
        {
            this.selectListContexts = selectListContexts;
            this.selectList = selectList;
        }

        internal ResultSet GetRows(int max)
        {
            ResultSet rsInput = myInput.GetRows(max);

            // get an effective column list ...
            List<string> effectiveColumns = new List<string>();
            int resultSetColumnIndex = 0;
            foreach (var c in selectListContexts)
            {
                if (c.asterisk() != null)
                {
                    Console.WriteLine("Asterisk!");
                    for (int i = 0; i < rsInput.ColumnCount; i++)
                    {
                        effectiveColumns.Add(rsInput.GetColumnName(i));
                        ExpressionNode x = new ExpressionOperandFromColumn(rsInput.GetColumnName(i));
                        List<ExpressionNode> xlist = new List<ExpressionNode>();
                        xlist.Add(x);
                        selectList.AddSelectListExpressionList(xlist);
                        /*
                        ExpressionList.Add(x);
                        EndSelectListExpressionList();
                        */
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
                foreach (string columnName in effectiveColumns)
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