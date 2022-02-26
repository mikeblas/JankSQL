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

        public ResultSet[] Execute()
        {
            List<ResultSet> results = new List<ResultSet>();
            foreach(SelectContext context in selectContext)
            {
                ResultSet result = context.Execute();
                results.Add(result);
            }

            return results.ToArray();
        }


        public ResultSet ExecuteSingle()
        {
            if (selectContext.Count != 1)
                return null;

            ResultSet result = selectContext[0].Execute();
            return result;
        }

    }
}
