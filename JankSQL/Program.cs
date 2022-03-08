
// Install-Package Antlr4.Runtime.Standard -Version 4.9.3

// install, and setup Antlr (now, just setantlr.bat in c:\bin)
//      https://github.com/antlr/antlr4/blob/master/doc/getting-started.md

// walkthrough of TSQL grammar in Antlr:
//      https://dskrzypiec.dev/parsing-tsql/

// TSQL Grammar in Antlr:
//      just this directory; this contains many many grammars
//      https://github.com/antlr/grammars-v4/tree/master/sql/tsql

// build the grammar over in the $/grammar directory:
//      antlr4 -Dlanguage=CSharp TSqlLexer.g4 TSqlParser.g4 -o tmp -visitor
//
// then that's in tmp, so copy it back down:
//      copy . ..\..\JankSQL1
//
// and now can build ...



namespace JankSQL
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // ExecutionContext ecFile = Parser.ParseSQLFileFromFileName("t5.sql");

            String str;

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

            // str = "SELECT * FROM [mytable] WHERE [population] > POWER(2500, 2);";

            // str = "DROP TABLE mytable";
            // str = "CREATE TABLE [Schema].[NewTable] (keycolumn INTEGER, city_name VARCHAR(30), state_code VARCHAR, population DECIMAL);";
            str = "DELETE FROM Mytable WHERE keycolumn = 2;";


            ExecutableBatch batch = Parser.ParseSQLFileFromString(str);
            if (batch.TotalErrors == 0)
            {
                batch.Dump();

                ExecuteResult[] sets = batch.Execute();
                for (int i = 0; i < sets.Length; i++)
                {
                    Console.WriteLine($"ExecuteResult #{i} =====");
                    if (sets[i].ResultSet != null)
                    {
                        sets[i].ResultSet.Dump();
                        Console.WriteLine($"{sets[i].ResultSet.RowCount} total rows");
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

        }


        public static string GetEffectiveName(string objectName)
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges
            if (objectName[0] != '[' || objectName[^1] != ']')
                return objectName;

            return objectName[1..^1];
        }
    }
}


