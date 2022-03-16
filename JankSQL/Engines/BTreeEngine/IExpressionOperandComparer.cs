namespace JankSQL.Engines
{
    class IExpressionOperandComparer : IComparer<ExpressionOperand[]>
    {
        int[]? keyOrder;

        public IExpressionOperandComparer(int[] keyOrder)
        {
            this.keyOrder = keyOrder;
        }

        public IExpressionOperandComparer()
        {
            keyOrder = null;
        }

        public int Compare(ExpressionOperand[]? x, ExpressionOperand[]? y)
        {
            if (x == null)
                throw new ArgumentNullException("x");
            if (y == null)
                throw new ArgumentNullException("y");
            if (x.Length != y.Length)
                throw new ArgumentException($"sizes are different: {x.Length} and {y.Length}");

            int ret;
            if (keyOrder != null)
            {
                int keyNumber = 0;
                do
                {
                    ret = x[keyOrder[keyNumber]].CompareTo(y[keyOrder[keyNumber]]);
                    keyNumber++;
                } while (ret == 0 && keyNumber < keyOrder.Length);
            }
            else
            {
                int keyNumber = 0;
                do
                {
                    ret = x[keyNumber].CompareTo(y[keyNumber]);
                    keyNumber++;
                } while (ret == 0 && keyNumber < x.Length);
            }

            // Console.WriteLine($"{String.Join(",", x)} compared to {String.Join(",", y)} --> {ret}");

            return ret;
        }
    }
}
