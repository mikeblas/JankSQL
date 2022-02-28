using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    public class ExecuteResult
    {
        ResultSet? resultSet;

        internal ExecuteResult()
        {
        }

        public ResultSet? ResultSet { get { return resultSet; } set { resultSet = value; } }
    }

    internal interface IExecutableContext
    {
        ExecuteResult Execute();
    }
}
