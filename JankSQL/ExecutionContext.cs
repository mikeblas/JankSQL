using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public class ExecutionContext
    {
        List<IExecutableContext> executeContexts = new();


        public List<IExecutableContext> ExecuteContexts { get { return executeContexts; } set { executeContexts = value; } }

        public ExecuteResult[] Execute()
        {
            List<ExecuteResult> results = new ();
            foreach(IExecutableContext context in executeContexts)
            {
                try
                {
                    ExecuteResult result = context.Execute();
                    results.Add(result);
                }
                catch (ExecutionException ex)
                {
                    ExecuteResult result = new ExecuteResult(ExecuteStatus.FAILED, ex.Message);
                    results.Add(result);
                }
            }

            return results.ToArray();
        }


        public ExecuteResult ExecuteSingle()
        {
            ExecuteResult result;
            if (executeContexts.Count != 1)
            {
                result = new ExecuteResult(ExecuteStatus.FAILED, "ExecuteSingle() called on multiple-context batch");
            }
            else
            {
                result = executeContexts[0].Execute();
            }

            return result;
        }

    }
}
