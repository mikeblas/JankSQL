using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public class ExecutionContext
    {
        SelectContext selectContext = null;


        public SelectContext SelectContext { get { return selectContext; } set { selectContext = value; } }



    }
}
