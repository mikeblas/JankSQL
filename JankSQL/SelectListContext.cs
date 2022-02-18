using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    internal class SelectListContext
    {
        List<List<ExpressionNode>> expressionLists = new List<List<ExpressionNode>>();

        List<ExpressionNode> currentExpressionList = new List<ExpressionNode>();

        string currentAlias = null;
        int unknownColumnID = 1001;
        List<string> rowsetColumnNames = new List<string>();

        internal List<ExpressionNode> ExpressionList { get { return currentExpressionList; } }

        TSqlParser.Select_listContext context;

        internal SelectListContext(TSqlParser.Select_listContext context)
        {
            this.context = context;
        }

        internal void EndExpressionList()
        {
            expressionLists.Add(currentExpressionList);
            currentExpressionList = new List<ExpressionNode>();
        }

        internal void EndElement()
        {
            currentAlias = null;
        }

        internal void AddRowsetColumnName(string rowsetColumnName)
        {
            rowsetColumnNames.Add(rowsetColumnName);
        }

        internal void AddUnknownRowsetColumnName()
        {
            AddRowsetColumnName($"Anonymous{unknownColumnID}");
            unknownColumnID += 1;
        }

        internal int RowsetColumnNamesCount { get { return rowsetColumnNames.Count; } }

        internal string RowsetColumnName(int idx) { return rowsetColumnNames[idx]; }

        internal int ExpressionListCount { get { return expressionLists.Count; } }

        internal string CurrentAlias { get { return currentAlias; } set { currentAlias = value; } }

        internal ExpressionOperand Execute(int index, Engines.DynamicCSV table, int rowIndex)
        {
            Stack<ExpressionNode> stack = new Stack<ExpressionNode>();

            do
            {
                foreach (ExpressionNode n in expressionLists[index])
                {
                    if (n is ExpressionOperand)
                        stack.Push(n);
                    else if (n is ExpressionOperator)
                    {
                        // it's an operator
                        ExpressionOperator oper = (ExpressionOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionOperandFromColumn)
                    {
                        ExpressionOperandFromColumn r = (ExpressionOperandFromColumn)n;
                        Console.WriteLine($"Need value from {r.ColumnName}");

                        int idx = table.ColumnIndex(r.ColumnName);
                        string[] thisRow = table.Row(rowIndex);
                        ExpressionOperand val = new ExpressionOperandNVARCHAR(thisRow[idx]);
                        stack.Push(val);
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }

                }
            } while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand)stack.Pop();
            Console.WriteLine($"==> [{result}]");

            return result;
        }

        internal void Dump()
        {
            Console.WriteLine($"rowsetColumnNames: {String.Join(", ", rowsetColumnNames)}");
            for (int i = 0; i < ExpressionListCount; i++)
            {
                Console.Write($"  Expression {i}: ");
                foreach (var x in expressionLists[i])
                {
                    Console.Write($"{x} ");
                }
                Console.WriteLine();
            }
        }
    }
}
