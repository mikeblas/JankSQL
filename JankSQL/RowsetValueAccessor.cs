using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class RowsetValueAccessor : IRowValueAccessor
    {
        ResultSet resultSet;
        int rowIndex;

        internal RowsetValueAccessor(ResultSet resultSet, int rowIndex)
        {
            this.resultSet = resultSet;
            this.rowIndex = rowIndex;
        }

        ExpressionOperand IRowValueAccessor.GetValue(FullColumnName fcn)
        {
            int idx = resultSet.ColumnIndex(fcn);
            // Console.WriteLine($"Need value from {r.ColumnName}, column index {idx}");

            ExpressionOperand[] thisRow = resultSet.Row(rowIndex);
            ExpressionOperand val = thisRow[idx];
            return val;
        }
    }
}
