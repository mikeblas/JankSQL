using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class JoinContext
    {
        string tableName;

        internal JoinContext(string tableName)
        {
            this.tableName = tableName;
        }

        internal string OtherTableName { get { return tableName; } }

        internal List<List<ExpressionNode>> PredicateExpressions { get; set; }

    }
}
