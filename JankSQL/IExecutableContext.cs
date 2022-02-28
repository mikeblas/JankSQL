using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    public enum ExecuteStatus
    {
        NOT_EXECUTED,
        SUCCESSFUL,
        FAILED,
    }

    public class ExecuteResult
    {
        ResultSet? resultSet;

        internal ExecuteResult()
        {
            this.ExecuteStatus = ExecuteStatus.NOT_EXECUTED;
        }

        public ResultSet? ResultSet { get { return resultSet; } set { resultSet = value; } }

        public ExecuteStatus ExecuteStatus { get; set; }
    }

    public interface IExecutableContext
    {
        ExecuteResult Execute();
    }
}
