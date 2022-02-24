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
        JoinType joinType;

        internal enum JoinType
        {
            CROSS_JOIN,
            INNER_JOIN,
            LEFT_OUTER_JOIN,
            RIGHT_OUTER_JOIN,
            FULL_OUTER_JOIN,
        }

        internal JoinContext(JoinType joinType, string tableName)
        {
            this.tableName = tableName;
            this.joinType = joinType;
        }

        internal string OtherTableName { get { return tableName; } }

        internal List<Expression> PredicateExpressions { get; set; }

    }
}
