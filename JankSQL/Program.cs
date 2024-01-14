/*
 * Install-Package Antlr4.Runtime.Standard -Version 4.9.3
 *
 *    install, and setup Antlr (now, just setantlr.bat in c:\bin)
 *         https://github.com/antlr/antlr4/blob/master/doc/getting-started.md
 *
 *    walk-through of TSQL grammar in Antlr:
 *         https://dskrzypiec.dev/parsing-tsql/
 *
 *    TSQL Grammar in Antlr:
 *         just this directory; this contains many grammars
 *         https://github.com/antlr/grammars-v4/tree/master/sql/tsql
 *
 *    build the grammar over in the $/grammar directory:
 *         antlr4 -Dlanguage=CSharp TSqlLexer.g4 TSqlParser.g4 -o tmp -visitor
 *
 *    then that's in tmp, so copy it back down:
 *         copy . ..\..\JankSQL1
 *
 *    and now can build ...
*/

namespace JankSQL
{
    internal static class Program
    {
        public static void Main()
        {
            Test2();
        }

        public static void Test2()
        {
            string str;


            str = "SELECT number_id FROM ten WHERE number_id < (SELECT MAX(keycolumn) FROM mytable);";


            str = "SELECT number_id FROM ten WHERE number_id < (SELECT MAX(keycolumn) FROM mytable WHERE ten.is_even = 0)";

            str = "SELECT number_id FROM ten WHERE number_id < (SELECT MAX(keycolumn) FROM mytable WHERE ten.is_even = 1 AND keycolumn = 3)";

            str = "SELECT keycolumn FROM mytable WHERE keycolumn IN (SELECT number_id FROM ten);";

            str = "SELECT number_id FROM ten WHERE number_id IN (SELECT keycolumn FROM mytable)";


            str = "select number_id, keycolumn " +
                  " from ten " +
                  " join mytable on numbeR_id = keycolumn; ";

            str = "select number_id, keycolumn " +
                  " from ten " +
                  " RIGHT OUTER JOIN mytable on numbeR_id = keycolumn; ";


            str = "INSERT INTO ten(number_id, number_name, is_even) VALUES(@P1, @P2, @P3)";

            // name = "SELECT * FROM ten WHERE number_id IN (3, 5, 7, number_id);";


            // name = "SELECT LEN(city_name) FROM mytable;";

            var btreeEngine = Engines.BTreeEngine.CreateInMemory();

            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");
            var csvEngine = Engines.DynamicCSVEngine.OpenObliterate(tempPath);

            Engines.TestTable tt10 = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("ten")
                .WithColumnNames(new string[] { "number_id", "number_name", "is_even" })
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.INTEGER, ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER })
                .WithRow(new object[] { 1, "one",   0 })
                .WithRow(new object[] { 2, "two",   1 })
                .WithRow(new object[] { 4, "four",  1 })
                .WithRow(new object[] { 7, "seven", 0 })
                .WithRow(new object[] { 8, "eight", 1 })
                .WithRow(new object[] { 3, "three", 0 })
                .WithRow(new object[] { 5, "five",  0 })
                .WithRow(new object[] { 6, "six",   1 })
                .WithRow(new object[] { 9, "nine",  0 })
                .WithRow(new object[] { 0, "zero",  1 })
                .Build();

            csvEngine.InjectTestTable(tt10);
            btreeEngine.InjectTestTable(tt10);

            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("mytable")
                .WithColumnNames(new string[] { "keycolumn", "city_name", "state_code", "population" })
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.INTEGER, ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER })
                .WithRow(new object[] { 1, "Monroeville", "PA",     25_000 })
                .WithRow(new object[] { 2, "Sammamish",   "WA",     37_000 })
                .WithRow(new object[] { 3, "New York",    "NY", 11_500_000 })
                .Build();

            csvEngine.InjectTestTable(tt);
            btreeEngine.InjectTestTable(tt);


            var engine = btreeEngine;

            ExecutableBatch batch = Parser.ParseSQLFileFromString(str);
            if (batch.TotalErrors == 0)
            {
                batch.Dump();

                batch.SetBindValue("@P1", ExpressionOperand.IntegerFromInt(301));
                batch.SetBindValue("@P2", ExpressionOperand.VARCHARFromString("three hundred one"));
                batch.SetBindValue("@P3", ExpressionOperand.IntegerFromInt(0));

                ExecuteResult[] sets = batch.Execute(engine);
                for (int i = 0; i < sets.Length; i++)
                {
                    Console.WriteLine($"ExecuteResult #{i} =====");
                    ResultSet? rs = sets[i].ResultSet;
                    if (rs != null)
                    {
                        rs.Dump();
                        Console.WriteLine($"{rs.RowCount} total rows");
                    }
                    else
                    {
                        Console.WriteLine("(no result set)");
                    }
                }
            }
            else
            {
                Console.WriteLine($"{batch.TotalErrors} Errors!");
                if (batch.HadSemanticError)
                    Console.WriteLine($"Semantic error: {batch.SemanticError}");
            }


            const string str3 = "SELECT * FROM ten";
            ExecutableBatch batch3 = Parser.ParseSQLFileFromString(str3);
            if (batch3.TotalErrors == 0)
            {
                batch3.Dump();

                ExecuteResult[] sets = batch3.Execute(engine);
                for (int i = 0; i < sets.Length; i++)
                {
                    Console.WriteLine($"ExecuteResult #{i} =====");
                    ResultSet? rs = sets[i].ResultSet;
                    if (rs != null)
                    {
                        rs.Dump();
                        Console.WriteLine($"{rs.RowCount} total rows");
                    }
                    else
                    {
                        Console.WriteLine("(no result set)");
                    }
                }
            }
            else
            {
                Console.WriteLine("Errors!");
            }


            /*
            string str2 = "INSERT INTO Ten (numbeR_id, numbeR_name, is_even) VALUES (11, 'Eleven', 0)";
            ExecutableBatch batch2 = Parser.ParseSQLFileFromString(str2);
            if (batch2.TotalErrors == 0)
            {
                batch2.Dump();

                ExecuteResult[] sets = batch2.Execute(engine);
                for (int i = 0; i < sets.Length; i++)
                {
                    Console.WriteLine($"ExecuteResult #{i} =====");
                    ResultSet? rs = sets[i].ResultSet;
                    if (rs != null)
                    {
                        rs.Dump();
                        Console.WriteLine($"{rs.RowCount} total rows");
                    }
                    else
                    {
                        Console.WriteLine("(no result set)");
                    }
                }
            }
            else
            {
                Console.WriteLine("Errors!");
            }
            */

            Engines.DynamicCSVEngine.RemoveDatabase(tempPath);
        }
    }
}


