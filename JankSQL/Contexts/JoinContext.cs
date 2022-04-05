﻿namespace JankSQL.Contexts
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
        private readonly FullTableName? tableName;
        private readonly SelectContext? selectSource;
        private readonly JoinType joinType;
        private List<Expression>? predicateExpressions;

        private string? derivedTableAlias;

        internal JoinContext(JoinType joinType, FullTableName tableName)
        {
            this.tableName = tableName;
            this.joinType = joinType;
        }

        internal JoinContext(JoinType joinType, SelectContext selectSource)
        {
            this.selectSource = selectSource;
            this.joinType = joinType;
        }

        internal string? DerivedTableAlias
        {
            get { return derivedTableAlias; }
            set { derivedTableAlias = value; }
        }

        internal FullTableName? OtherTableName
        {
            get { return tableName; }
        }

        internal SelectContext? SelectSource
        {
            get { return selectSource; }
        }


        internal List<Expression> PredicateExpressions
        {
            get { return predicateExpressions!; } set { predicateExpressions = value; }
        }

        internal JoinType JoinType
        {
            get { return joinType; }
        }
    }
}
