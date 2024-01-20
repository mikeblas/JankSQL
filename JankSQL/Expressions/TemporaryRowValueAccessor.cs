using System.Xml.Linq;

namespace JankSQL.Expressions
{
    /// <summary>
    /// wraps a discrete row and a list of column names to be used by
    /// the Expression.Evaluate() method.
    /// </summary>
    internal class TemporaryRowValueAccessor : IRowValueAccessor
    {
        private readonly FullColumnName[] names;
        private readonly Tuple rowData;

        internal TemporaryRowValueAccessor(Tuple rowData, IEnumerable<FullColumnName> names)
        {
            this.names = names.ToArray();
            this.rowData = rowData;
        }

        ExpressionOperand IRowValueAccessor.GetValue(FullColumnName fcn)
        {
            int ret = -1;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(fcn))
                {
                    if (ret != -1)
                        throw new ExecutionException($"column name {fcn} is ambiguous because it matches both {names[ret]} and {names[i]}");
                    ret = i;
                }
            }

            if (ret != -1)
                return rowData[ret];

            throw new ExecutionException($"column {fcn} not found in TemporaryRowValueAccessor; available are {string.Join(",", (object[])names)}");
        }

        public bool TryGetValue(FullColumnName fullColumnName, out ExpressionOperand? value)
        {
            int ret = -1;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(fullColumnName))
                {
                    if (ret != -1)
                    {
                        value = null;
                        return false;
                    }
                    ret = i;
                }
            }

            if (ret != -1)
            {
                value = rowData[ret];
                return true;
            }

            value = null;
            return false;
        }

        void IRowValueAccessor.SetValue(FullColumnName fcn, ExpressionOperand op)
        {
            int ret = -1;
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(fcn))
                {
                    if (ret != -1)
                        throw new ExecutionException($"column name {fcn} is ambiguous because it matches both {names[ret]} and {names[i]}");
                    ret = i;
                }
            }

            if (ret != - 1)
            {
                rowData[ret] = op;
                return;
            }

            throw new ExecutionException($"column {fcn} not found in TemporaryRowValueAccessor; available are {string.Join(",", (object[])names)}");
        }
    }
}
