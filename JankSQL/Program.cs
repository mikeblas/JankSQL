/*
 * Install-Package Antlr4.Runtime.Standard -Version 4.9.3
 *
 *    install, and setup Antlr (now, just setantlr.bat in c:\bin)
 *         https://github.com/antlr/antlr4/blob/master/doc/getting-started.md
 *
 *    walkthrough of TSQL grammar in Antlr:
 *         https://dskrzypiec.dev/parsing-tsql/
 *
 *    TSQL Grammar in Antlr:
 *         just this directory; this contains many many grammars
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
    internal class Program
    {
        static void Main()
        {
            string str;

            // str = "SELECT [city_name],  [population], [population]*2 FROM [mytable];";
            // str = "SELECT [mytable].[city_name], [mytable].[population], [population]*2 FROM [mytable];";
            // str = "SELECT * FROM [mytable];";
            // str = "SELECT * FROM [mytable] WHERE [population] = 37000 OR [keycolumn] = 1;";
            // str = "SELECT* FROM[mytable] WHERE[population] != 37000;";
            // str = "SELECT * FROM [mytable] WHERE [population] = 25000 AND [keycolumn] = 5-4;";
            // str = "SELECT* FROM [mytable] WHERE NOT [population] = 37000;";
            // str = "select * from mytable WHERE NOT population = 37000;";
            // str = "SELECT * FROM [mytable] WHERE NOT(NOT(NOT ([population] = 37000)));";
            // str = "SELECT * FROM [mytable] JOIN [states] ON [mytable].[state_code] = [states].[state_code]";
            // str = "SELECT * FROM [mytable] CROSS JOIN [states]";
            /*
            str = "SELECT [three].[number_id], [ten].[number_id], [three].[number_id] + 10 * [ten].[number_id] FROM [Three] CROSS JOIN [Ten] CROSS JOIN [MyTable]" +
                " WHERE [three].[number_id] + 10 * [ten].[number_id] > 30;";
            */

            // str = "SELECT N'hello', 'goodbye', 'Bob''s Burgers';";

            // str = "SELECT 'Hello' + ', world';";

            // str = "SELECT -32;";

            // str = "SELECT 'This'; SELECT 'That';";

            // str = "TRUNCATE TABLE [TargetTasdfasdfaable];";
            // str = "SELECT city_name FROM mytable;";
            //     012345678901

            // str = "INSERT INTO [Mytable] ([keycolumn], [city_name], [state_code], [population]) VALUES (92, 'Tacoma', 'WA', 520000);";
            // str = "INSERT INTO [Mytable] ([keycolumn], [city_name], [state_code], [population]) VALUES (92, 'Tacoma', 'WA', 520000), (101, 'Chehalis', 'WA', 12000);";
            // str = "INSERT INTO [Mytable] ([keycolumn], [city_name], [state_code], [population]) VALUES (92, 'Tacoma', 'WA', 520000), (101, 'Chehalis', 'WA', 12000), (3*5, 'Exponent', 'PA', POWER(10, 2));";
            // str = "SELECT SQRT(2) FROM [mytable];";

            // str = "SELECT [population] / [keycolumn] FROM [mytable];";

            str = "SELECT * FROM [mytable] WHERE [population] > POWER(2500, 2);";

            // -- these need tests --
            // str = "DROP TABLE mytable";
            // str = "CREATE TABLE [Schema].[NewTable] (keycolumn INTEGER, city_name VARCHAR(30), state_code VARCHAR, population DECIMAL);";
            // str = "DELETE FROM Mytable WHERE keycolumn = 2; SELECT * FROM MyTable;";
            // -- those need tests --

            // str = "UPDATE MyTable SET population = population * 1.12 WHERE keycolumn = 2;";
            // str = "SELECT [city_name], [population]*2, [population] FROM [mytable];";
            // str = "SELECT POWER((10/2), 15/5) FROM [mytable];";

            // str = "SELECT SUM(number_id) FROM ten";
            // str = "SELECT SUM(number_id), COUNT(number_id) FROM ten";
            str = "SELECT 10* SUM(number_id), COUNT(number_id) * 100 FROM ten";
            // str = "SELECT 23 * SUM(number_id), COUNT(number_id) FROM ten GROUP BY Polarity";

            // str = "SELECT MIN(number_name), MAX(number_name) FROM ten";

            // str = "SELECT MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even";
            // str = "SELECT is_even, MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even";
            // str = "SELECT number_name, MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even";

            // str = "SELECT state_code FROM mytable GROUP BY state_code ORDER BY state_code";
            // str = "SELECT number_name FROM ten ORDER BY number_name";
            // str = "SELECT number_name FROM ten ORDER BY number_name DESC";

            str = "CREATE INDEX MyIndex ON MyTable (number_id ASC, something, something_Else DESC)";


            // Engines.DynamicCSVEngine engine = Engines.DynamicCSVEngine.OpenAlways("F:\\JankTests\\Test33");

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
                .WithRow(new object[] { 3, "three", 0 })
                .WithRow(new object[] { 4, "four",  1 })
                .WithRow(new object[] { 5, "five",  0 })
                .WithRow(new object[] { 6, "six",   1 })
                .WithRow(new object[] { 7, "seven", 0 })
                .WithRow(new object[] { 8, "eight", 1 })
                .WithRow(new object[] { 9, "nine",  0 })
                .WithRow(new object[] { 0, "zero",  1 })
                .Build();

            csvEngine.InjectTestTable(tt10);
            btreeEngine.InjectTestTable(tt10);

            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("mytable")
                .WithColumnNames(new string[] { "keycolumn", "city_name", "state_code", "population" })
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.INTEGER, ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER })
                .WithRow(new object[] { 1, "Monroeville", "PA", 25000 })
                .WithRow(new object[] { 2, "Sammamish", "WA", 37000 })
                .WithRow(new object[] { 3, "New York", "NY", 11500000 })
                .Build();

            csvEngine.InjectTestTable(tt);
            btreeEngine.InjectTestTable(tt);


            var engine = btreeEngine;

            ExecutableBatch batch = Parser.ParseSQLFileFromString(str);
            if (batch.TotalErrors == 0)
            {
                batch.Dump();

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
                Console.WriteLine("Errors!");
            }

            Engines.DynamicCSVEngine.RemoveDatabase(tempPath);
        }
    }
}


