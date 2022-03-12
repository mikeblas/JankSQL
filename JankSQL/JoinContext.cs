
namespace JankSQL
{
    internal enum JoinType
    {
        CROSS_JOIN,
        INNER_JOIN,
        LEFT_OUTER_JOIN,
        RIGHT_OUTER_JOIN,
        FULL_OUTER_JOIN,
    }

    internal class JoinContext
    {
        string tableName;
        JoinType joinType;
        List<Expression>? predicateExpressions;

        internal JoinContext(JoinType joinType, string tableName)
        {
            this.tableName = tableName;
            this.joinType = joinType;
        }

        internal string OtherTableName { get { return tableName; } }

        internal List<Expression> PredicateExpressions { get { return predicateExpressions!; } set { predicateExpressions = value; } }

        internal JoinType JoinType { get { return joinType; } }

    }
}
