namespace JankSQL.Contexts
{
    internal enum JoinType
    {
        CROSS_JOIN,
        INNER_JOIN,
        LEFT_OUTER_JOIN,
        RIGHT_OUTER_JOIN,
        FULL_OUTER_JOIN,
    }

    internal class JoinContext : ICloneable
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
            get { return predicateExpressions!; }
            set { predicateExpressions = value; }
        }

        internal JoinType JoinType
        {
            get { return joinType; }
        }

        public object Clone()
        {
            JoinContext clone;
            if (selectSource != null)
                clone = new JoinContext(joinType, (SelectContext)selectSource.Clone());
            else if (tableName != null)
                clone = new JoinContext(joinType, tableName);
            else
                throw new InternalErrorException("join clone needs table name or selectSource");

            clone.derivedTableAlias = derivedTableAlias;

            if (predicateExpressions != null)
            {
                clone.predicateExpressions = new List<Expression>();
                foreach (var pe in predicateExpressions)
                    clone.predicateExpressions.Add(pe);
            }

            return clone;
        }


        internal void Dump()
        {
            string source = OtherTableName != null ? OtherTableName.ToString() : "DerivedSelect";
            bool hasPredicates = predicateExpressions != null && predicateExpressions.Any();

            Console.WriteLine($"{joinType} join {source} {(hasPredicates ? "on:" : "with no predicate")}");
            if (hasPredicates)
            {
                for (int i = 0; i < predicateExpressions!.Count; i++)
                    Console.WriteLine($"   #{i}: {predicateExpressions}");
            }

            if (selectSource != null)
                selectSource.Dump();
        }
    }
}
