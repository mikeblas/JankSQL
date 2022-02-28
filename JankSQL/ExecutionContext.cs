using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public class ExecutionContext
    {
        List<SelectContext> selectContext = new List<SelectContext>();


        public List<SelectContext> SelectContexts { get { return selectContext; } set { selectContext = value; } }

        public ExecuteResult[] Execute()
        {
            List<ExecuteResult> results = new List<ExecuteResult>();
            foreach(SelectContext context in selectContext)
            {
                ExecuteResult result = context.Execute();
                results.Add(result);
            }

            return results.ToArray();
        }


        public ExecuteResult ExecuteSingle()
        {
            if (selectContext.Count != 1)
                return null;

            ExecuteResult result = selectContext[0].Execute();
            return result;
        }

    }
}
