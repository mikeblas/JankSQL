using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal interface IRowValueAccessor
    {
        ExpressionOperand GetValue(FullColumnName fcn);
    }
}
