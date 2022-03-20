
namespace JankSQL
{
    internal class OrderByContext
    {
        readonly List<Expression> expressionList = new();
        readonly List<bool> isAscendingList = new();

        internal OrderByContext()
        {
        }

        internal List<Expression> ExpressionList { get { return expressionList; } }

        internal void AddExpression(Expression x, bool isAscending)
        {
            expressionList.Add(x);
            isAscendingList.Add(isAscending);
        }
    }

}

