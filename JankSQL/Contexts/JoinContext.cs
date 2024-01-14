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
        private List<Expression>? predicateExpressions;

        internal JoinContext(JoinType joinType, FullTableName tableName)
        {
            this.OtherTableName = tableName;
            this.JoinType = joinType;
        }

        internal JoinContext(JoinType joinType, SelectContext selectSource)
        {
            this.SelectSource = selectSource;
            this.JoinType = joinType;
        }

        internal JoinContext(JoinType joinType, SelectContext selectSource, string derivedTableAlias)
        {
            this.SelectSource = selectSource;
            this.JoinType = joinType;
            this.DerivedTableAlias = derivedTableAlias;
        }

        internal string? DerivedTableAlias { get; set; }

        internal FullTableName? OtherTableName { get; }

        internal SelectContext? SelectSource { get; }

        internal List<Expression> PredicateExpressions
        {
            get { return predicateExpressions!; }
            set { predicateExpressions = value; }
        }

        internal JoinType JoinType { get; }

        public object Clone()
        {
            JoinContext clone;
            if (SelectSource != null)
                clone = new JoinContext(JoinType, (SelectContext)SelectSource.Clone());
            else if (OtherTableName != null)
                clone = new JoinContext(JoinType, OtherTableName);
            else
                throw new InternalErrorException("join clone needs table name or selectSource");

            clone.DerivedTableAlias = DerivedTableAlias;

            if (predicateExpressions != null)
            {
                clone.predicateExpressions = new List<Expression>();
                clone.predicateExpressions.AddRange(predicateExpressions);
            }

            return clone;
        }

        internal void Dump()
        {
            string source = OtherTableName != null ? OtherTableName.ToString() : "DerivedSelect";
            bool hasPredicates = predicateExpressions?.Count > 0;

            Console.WriteLine($"{JoinType} join {source} {(hasPredicates ? "on:" : "with no predicate")}");
            if (hasPredicates)
            {
                for (int i = 0; i < predicateExpressions!.Count; i++)
                    Console.WriteLine($"   #{i}: {predicateExpressions}");
            }

            SelectSource?.Dump();
        }
    }
}
