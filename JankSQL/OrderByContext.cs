
namespace JankSQL
{
    internal class OrderByContext
    {
        readonly List<Expression> expressionList = new();
        readonly List<bool> isAscendingList = new();

        internal OrderByContext()
        {
        }

        internal void AddExpression(Expression x, bool isAscending)
        {
            expressionList.Add(x);
            isAscendingList.Add(isAscending);
        }
    }

}

