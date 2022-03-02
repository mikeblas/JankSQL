using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class TemporaryRowValueAccessor : IRowValueAccessor
    {
        readonly List<FullColumnName> names;
        readonly ExpressionOperand[] rowData;

        internal TemporaryRowValueAccessor(ExpressionOperand[] rowData, List<FullColumnName> names)
        {
            this.names = names;
            this.rowData = rowData;
        }

        ExpressionOperand IRowValueAccessor.GetValue(FullColumnName fcn)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i].Equals(fcn))
                    return rowData[i];
            }

            return null;
        }
    }
}
