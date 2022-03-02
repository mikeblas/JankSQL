using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class InsertContext : IExecutableContext
    {
        TSqlParser.Insert_statementContext context;
        List<FullColumnName> targetColumns;

        internal InsertContext(TSqlParser.Insert_statementContext context)
        {
            this.context = context;
        }

        internal List<FullColumnName> TargetColumns { get { return targetColumns; } set { targetColumns = value; } }

        internal List<List<Expression>> constructors = null;

        internal void AddExpressionList(List<Expression> expressionList)
        {
            if (constructors is null)
                constructors = new();

            this.constructors.Add(expressionList);
        }

        internal void AddExpressionLists(List<List<Expression>> expressionLists)
        {
            if (constructors is null)
                constructors = new();

            this.constructors.AddRange(expressionLists);
        }

        internal string TableName { get; set; }

        public ExecuteResult Execute()
        {
            throw new NotImplementedException();
        }

        public void Dump()
        {
            Console.WriteLine($"INSERT into {TableName}");

            string str;

            str = string.Join(',', TargetColumns);
            Console.WriteLine($"   Columns: {str}");

            str = String.Join(',', constructors.Select(x => "[" + x + "]").Select(y => "{" + y + "}"));

            str = String.Join(',', constructors.Select(x => "[" + x + "]").ToArray().Select(y => "{" + y + "}"));

            str = String.Join(',', constructors.Select(x => "{" + String.Join(',', x.Select(y => "[" + y + "]")) + "}"));



            Console.WriteLine($"   Expressions: {str}");
        }
    }
}
