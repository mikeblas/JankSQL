/// <summary>
/// This class implements IEqualityComparer for ExpressionOperand[] arrays so that
/// such arrayts can be used as keys in hashed containers, like Dictionary<> in
/// the implementation of the Aggregation operator.
/// </summary>
namespace JankSQL
{
    internal class ExpressionOperandEqualityComparator : IEqualityComparer<ExpressionOperand[]>
    {
        public bool Equals(ExpressionOperand[]? x, ExpressionOperand[]? y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));

            if (x.Length != y.Length)
                return false;

            for (int i = 0; i < x.Length; i++)
            {
                // at first not equal, we know
                if (!x[i].Equals(y[i]))
                    return false;
            }

            return true;
        }

        public int GetHashCode(ExpressionOperand[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = (result * 23) + obj[i].GetHashCode();
                }
            }

            return result;
        }
    }
}
