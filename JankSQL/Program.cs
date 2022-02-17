

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


using Antlr4.Runtime;
using Antlr4.Runtime.Tree;



namespace JankSQL
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // workFile("t5.sql");

            ExecutionContext ec = Parser.ParseSQLFile("SELECT [city_name],  [population] FROM [mytable];");
            ResultSet rs = ec.Execute();
            rs.Dump();
        }


        static void workFile(string sqlFile)
        {
            using (TextReader str = new StreamReader(sqlFile))
            {
                var lexer = new TSqlLexer(new AntlrInputStream(str));
                var tokenStream = new CommonTokenStream(lexer);
                var parser = new TSqlParser(tokenStream);
                var tree = parser.tsql_file();

                var ml = new MyParserListener();
                ParseTreeWalker.Default.Walk(ml, tree);
            }
        }


        public static string GetEffectiveName(string objectName)
        {
            if (objectName[0] != '[' || objectName[objectName.Length - 1] != ']')
                return objectName;

            return objectName.Substring(1, objectName.Length - 2);
        }
    }
}


