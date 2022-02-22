
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
            // str = "SELECT * FROM [mytable];";
            // str = "SELECT * FROM [mytable] WHERE [population] = 37000 OR [keycolumn] = 1;";
            // str = "SELECT* FROM[mytable] WHERE[population] != 37000;";
            // str = "SELECT * FROM [mytable] WHERE [population] = 25000 AND [keycolumn] = 5-4;";
            // str = "SELECT * FROM [mytable] WHERE NOT [population] = 37000;";
            // str = "SELECT * FROM [mytable] WHERE NOT(NOT(NOT ([population] = 37000)));";
            str = "SELECT * FROM [table1] JOIN [table2] ON [table1].[state_id] = [table2].[state_id]";


            ExecutionContext ecString = Parser.ParseSQLFileFromString(str);
            ResultSet rs = ecString.Execute();
            rs.Dump();
        }


        public static string GetEffectiveName(string objectName)
        {
            if (objectName[0] != '[' || objectName[objectName.Length - 1] != ']')
                return objectName;

            return objectName.Substring(1, objectName.Length - 2);
        }
    }
}


