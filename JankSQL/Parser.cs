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
        public static ExecutionContext ParseSQLFile(string str)
        {
            var lexer = new TSqlLexer(new AntlrInputStream(str));
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new TSqlParser(tokenStream);
            var tree = parser.tsql_file();

            var ml = new JankListener();
            ParseTreeWalker.Default.Walk(ml, tree);

            return ml.ExecutionContext;
        }
    }
}
