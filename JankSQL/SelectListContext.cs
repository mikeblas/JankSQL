using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    internal class SelectListContext
    {
        List<ExpressionNode> expressionList = new List<ExpressionNode>();
        internal List<ExpressionNode> ExpressionList { get { return expressionList; } }

        TSqlParser.Select_listContext context;

        internal SelectListContext(TSqlParser.Select_listContext context)
        {
            this.context = context;
        }

        internal void Execute()
        {

            Stack<ExpressionNode> stack = new Stack<ExpressionNode>();

            do
            {
                foreach (ExpressionNode n in expressionList)
                {
                    if (n is ExpressionOperand)
                        stack.Push(n);
                    else
                    {
                        // it's an operator
                        ExpressionOperator oper = (ExpressionOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }

                }
            } while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand)stack.Pop();
            Console.WriteLine($"==> [{result}]");
        }

    }
}
