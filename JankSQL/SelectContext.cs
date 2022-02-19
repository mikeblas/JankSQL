

namespace JankSQL
{
    public class SelectContext
    {
        TSqlParser.Select_statementContext statementContext;
        SelectListContext selectList;

        // for WHERE clauses
        List<List<ExpressionNode>> predicateExpressionLists = new List<List<ExpressionNode>>();
        List<ExpressionNode> currentExpressionList = new List<ExpressionNode>();

        internal SelectContext(TSqlParser.Select_statementContext context)
        {
            statementContext = context;
        }

        internal void EndPredicateExpressionList()
        {
            predicateExpressionLists.Add(currentExpressionList);
            currentExpressionList = new List<ExpressionNode>();
        }

        internal void EndAndCombinePredicateExpressionList(int arguments)
        {
            EndPredicateExpressionList();

            int firstIndex = predicateExpressionLists.Count - arguments -1;
            List<List<ExpressionNode>> range = predicateExpressionLists.GetRange(firstIndex, arguments+1);
            predicateExpressionLists.RemoveRange(firstIndex, arguments + 1);


            List<ExpressionNode> newList = new List<ExpressionNode>();
            foreach (var subList in range)
            {
                newList.AddRange(subList);
            }

            predicateExpressionLists.Add(newList);
        }

        internal void EndSelectListExpressionList()
        {
            selectList.AddSelectListExpressionList(currentExpressionList);
            currentExpressionList = new List<ExpressionNode>();
        }

        internal List<ExpressionNode> ExpressionList { get { return currentExpressionList; } }

        internal int PredicateExpressionListCount { get { return predicateExpressionLists.Count; } }

        internal SelectListContext SelectListContext { get { return selectList; } set { selectList = value; } }

        internal ResultSet Execute()
        {
            var expressions = statementContext.query_expression();
            var querySpecs = expressions.query_specification();
            var sourceTable = querySpecs.table_sources().table_source().First().GetText();
            Console.WriteLine($"ExitSelect_Statement: {sourceTable}");

            string effectiveName = Program.GetEffectiveName(sourceTable);

            ResultSet resultSet = new ResultSet();

            // get systables
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv");
            sysTables.Load();

            // is this source table in there?
            int idxName = sysTables.ColumnIndex("table_name");
            int idxFile = sysTables.ColumnIndex("file_name");

            int foundRow = -1;
            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxName].Equals(effectiveName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundRow = i;
                    break;
                }
            }

            if (foundRow == -1)
                Console.WriteLine($"Table {effectiveName} does not exist");
            else
            {
                // found the source table, so load it
                Engines.DynamicCSV table = new Engines.DynamicCSV(sysTables.Row(foundRow)[idxFile]);
                table.Load();

                // get an effective column list ...
                List<string> effectiveColumns = new List<string>();
                int resultSetColumnIndex = 0;
                foreach (var c in querySpecs.select_list().select_list_elem())
                {
                    if (c.asterisk() != null)
                    {
                        Console.WriteLine("Asterisk!");
                        for (int i = 0; i < table.ColumnCount; i++)
                        {
                            effectiveColumns.Add(table.ColumnName(i));
                            ExpressionNode x = new ExpressionOperandFromColumn(table.ColumnName(i));
                            ExpressionList.Add(x);
                            EndSelectListExpressionList();
                        }
                    }
                    else
                    {
                        effectiveColumns.Add(selectList.RowsetColumnName(resultSetColumnIndex++));
                    }
                }

                resultSet.SetColumnNames(effectiveColumns);


                Dump();



                // for each row, for each column...
                for (int i = 0; i < table.RowCount; i++)
                {
                    // evaluate the where clauses, if any
                    bool predicatePassed = true;
                    foreach (var p in predicateExpressionLists)
                    {
                        ExpressionOperand result = SelectListContext.Execute(p, table, i);

                        if (!result.IsTrue())
                        {
                            predicatePassed = false;
                            break;
                        }
                    }

                    if (!predicatePassed)
                        continue;

                    // add the row to the result set
                    int exprIndex = 0;
                    int rsIndex = 0;
                    string[] thisRow = table.Row(i);
                    ExpressionOperand[] rowResults = new ExpressionOperand[effectiveColumns.Count];
                    foreach (string columnName in effectiveColumns)
                    {
                        int idx = table.ColumnIndex(columnName);
                        if (idx == -1 || true )
                        {
                            ExpressionOperand result = selectList.Execute(exprIndex, table, i);
                            rowResults[rsIndex] = result;
                            exprIndex++;
                        }
                        else
                        {
                            rowResults[rsIndex] = ExpressionOperand.NVARCHARFromString(thisRow[idx]);
                        }
                        rsIndex++;
                    }

                    resultSet.AddRow(rowResults);
                }
            }

            return resultSet;
        }


        internal void Dump()
        {
            selectList.Dump();

            Console.WriteLine("PredicateExpressions:");
            for (int i = 0; i < PredicateExpressionListCount; i++)
            {
                Console.Write($"  #{i}: ");
                foreach (var x in predicateExpressionLists[i])
                {
                    Console.Write($"{x} ");
                }
                Console.WriteLine();
            }
        }
    }
}
