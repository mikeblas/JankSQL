namespace JankSQL.Engines
{
    internal class IExpressionOperandComparer : IComparer<Tuple>
    {
        private readonly int[]? keyOrder;
        private readonly bool[]? descendingFlags;

        public IExpressionOperandComparer(bool[] descendingFlags)
        {
            this.keyOrder = null;
            this.descendingFlags = descendingFlags;
        }

        public IExpressionOperandComparer(int[] keyOrder)
        {
            this.keyOrder = keyOrder;
            this.descendingFlags = null;
        }

        public IExpressionOperandComparer()
        {
            keyOrder = null;
            descendingFlags = null;
        }

        public int Compare(Tuple? x, Tuple? y)
        {
            if (x == null)
                throw new ArgumentNullException(nameof(x));
            if (y == null)
                throw new ArgumentNullException(nameof(y));
            if (x.Length != y.Length)
                throw new ArgumentException($"sizes are different: {x.Length} and {y.Length}");

            int ret;
            if (keyOrder != null)
            {
                int keyNumber = 0;
                do
                {
                    ret = x[keyOrder[keyNumber]].CompareTo(y[keyOrder[keyNumber]]);
                    if (descendingFlags != null && keyNumber < descendingFlags.Length && descendingFlags[keyOrder[keyNumber]])
                        ret = -ret;
                    keyNumber++;
                }
                while (ret == 0 && keyNumber < keyOrder.Length);
            }
            else
            {
                int keyNumber = 0;
                do
                {
                    ret = x[keyNumber].CompareTo(y[keyNumber]);
                    if (descendingFlags != null && keyNumber < descendingFlags.Length && descendingFlags[keyNumber])
                        ret = -ret;
                    keyNumber++;
                }
                while (ret == 0 && keyNumber < x.Length);
            }

            // Console.WriteLine($"{String.Join(",", x)} compared to {String.Join(",", y)} --> {ret}");

            return ret;
        }
    }
}
