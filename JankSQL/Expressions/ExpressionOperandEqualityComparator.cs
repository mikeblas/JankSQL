

namespace JankSQL
{
    internal class ExpressionOperandEqualityComparator : IEqualityComparer<ExpressionOperand[]>
    {
        public bool Equals(ExpressionOperand[]? x, ExpressionOperand[]? y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (! x[i].Equals(y[i]))
                {
                    return false;
                }
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
                    result = result * 23 + obj[i].GetHashCode();
                }
            }
            return result;
        }
    }
}
