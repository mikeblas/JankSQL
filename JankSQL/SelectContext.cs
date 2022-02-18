using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public class SelectContext
    {
        TSqlParser.Select_statementContext statementContext;
        SelectListContext selectList;

        internal SelectContext(TSqlParser.Select_statementContext context, SelectListContext selectList)
        {
            statementContext = context;
            this.selectList = selectList;
        }

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
                        }
                    }
                    else
                    {
                        effectiveColumns.Add(selectList.RowsetColumnName(resultSetColumnIndex++));
                    }
                }

                resultSet.SetColumnNames(effectiveColumns);


                for (int i = 0; i < table.RowCount; i++)
                {
                    int exprIndex = 0;
                    int rsIndex = 0;
                    string[] thisRow = table.Row(i);
                    ExpressionOperand[] rowResults = new ExpressionOperand[effectiveColumns.Count];
                    foreach (string columnName in effectiveColumns)
                    {
                        int idx = table.ColumnIndex(columnName);
                        if (idx == -1)
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
    }
}
