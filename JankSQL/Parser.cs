using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace JankSQL
{
    public class Parser
    {
        private static ExecutionContext ParseTreeFromLexer(TSqlLexer lexer)
        {
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new TSqlParser(tokenStream);
            var tree = parser.tsql_file();

            var ml = new JankListener();
            ParseTreeWalker.Default.Walk(ml, tree);

            return ml.ExecutionContext;
        }


        public static ExecutionContext ParseSQLFileFromString(string str)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(str));
            return ParseTreeFromLexer(lexer);
        }

        public static ExecutionContext ParseSQLFileFromTextReader(TextReader reader)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(reader));
            return ParseTreeFromLexer(lexer);
        }

        public static ExecutionContext ParseSQLFileFromFileName(string strFileName)
        {
            using (TextReader str = new StreamReader(strFileName))
            {
                return Parser.ParseSQLFileFromTextReader(str);
            }
        }

    }
}
