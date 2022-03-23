namespace JankSQL.Contexts
{
    internal class PredicateContext
    {
        private readonly List<Expression> predicateExpressionLists = new List<Expression>();

        internal PredicateContext()
        {
        }

        internal int PredicateExpressionListCount
        {
            get { return predicateExpressionLists.Count; }
        }

        internal List<Expression> PredicateExpressions
        {
            get { return predicateExpressionLists; }
        }

        internal void EndPredicateExpressionList(Expression expression)
        {
            predicateExpressionLists.Add(expression);
        }

        internal void EndAndCombinePredicateExpressionList(int arguments, Expression expression)
        {
            EndPredicateExpressionList(expression);

            int firstIndex = predicateExpressionLists.Count - arguments - 1;
            List<Expression> range = predicateExpressionLists.GetRange(firstIndex, arguments + 1);
            predicateExpressionLists.RemoveRange(firstIndex, arguments + 1);

            Expression newList = new Expression();
            foreach (var subList in range)
            {
                newList.AddRange(subList);
            }

            predicateExpressionLists.Add(newList);
        }
    }
}
