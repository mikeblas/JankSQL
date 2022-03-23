/// <summary>
/// This class implements IEqualityComparer for Tuples so that
/// such arrayts can be used as keys in hashed containers, like Dictionary<> in
/// the implementation of the Aggregation operator.
/// </summary>
namespace JankSQL
{
    internal class ExpressionOperandEqualityComparator : IEqualityComparer<Tuple>
    {
        public bool Equals(Tuple? x, Tuple? y)
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

        public int GetHashCode(Tuple obj)
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
