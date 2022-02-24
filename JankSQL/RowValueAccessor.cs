using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal interface RowValueAccessor
    {
        ExpressionOperand GetValue(FullColumnName fcn);
    }
}
