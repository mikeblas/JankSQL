namespace JankSQL.Engines
{
    internal class IExpressionOperandComparer : IComparer<Tuple>
    {
        private readonly int[]? keyOrder;
        private readonly bool[]? descendings;

        public IExpressionOperandComparer(bool[] descendings)
        {
            this.keyOrder = null;
            this.descendings = descendings;
        }

        public IExpressionOperandComparer(int[] keyOrder)
        {
            this.keyOrder = keyOrder;
            this.descendings = null;
        }

        public IExpressionOperandComparer()
        {
            keyOrder = null;
            descendings = null;
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
                    if (descendings != null && keyNumber < descendings.Length && descendings[keyOrder[keyNumber]])
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
                    if (descendings != null && keyNumber < descendings.Length && descendings[keyNumber])
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
