namespace JankSQL.Contexts
{
    internal class PredicateContext
    {
        private readonly List<Expression> predicateExpressionLists = new List<Expression>();

        internal PredicateContext()
        {
        }

        internal PredicateContext(Expression expression)
        {
            predicateExpressionLists.Add(expression);
        }

        internal int PredicateExpressionListCount
        {
            get { return predicateExpressionLists.Count; }
        }

        internal List<Expression> PredicateExpressions
        {
            get { return predicateExpressionLists; }
        }

    }
}
