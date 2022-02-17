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
                // load that table
                Engines.DynamicCSV table = new Engines.DynamicCSV(sysTables.Row(foundRow)[1]);
                table.Load();

                // get an effective column list ...
                List<string> effectiveColumns = new List<string>();
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
                    else if (c.column_elem() != null)
                    {
                        Console.WriteLine($"column element! {c.column_elem().full_column_name().column_name.SQUARE_BRACKET_ID()}");
                        effectiveColumns.Add(Program.GetEffectiveName(c.column_elem().full_column_name().column_name.SQUARE_BRACKET_ID().GetText()));
                    }
                }

                for (int i = 0; i < table.RowCount; i++)
                {
                    string[] thisRow = table.Row(i);
                    bool first = true;
                    foreach (string columnName in effectiveColumns)
                    {
                        int idx = table.ColumnIndex(columnName);
                        if (!first)
                            Console.Write(", ");
                        first = false;
                        Console.Write($"{thisRow[idx]}");
                    }
                    Console.WriteLine();

                }

                for (int i = 0; i < table.RowCount; i++)
                {

                    ExpressionOperand result = selectList.Execute();
                    ExpressionOperand[] rowResults = new ExpressionOperand[1];
                    rowResults[0] = result;

                    resultSet.AddRow(rowResults);

                    // for each row, for each column list ...
                    // querySpecs.select_list().select_list_elem();
                }
            }

            return resultSet;
        }
    }
}
