using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    internal class SelectListContext
    {
        TSqlParser.Select_listContext context;

        List<List<ExpressionNode>> expressionLists = new List<List<ExpressionNode>>();

        string? currentAlias = null;
        int unknownColumnID = 1001;
        List<FullColumnName> rowsetColumnNames = new List<FullColumnName>();


        internal SelectListContext(TSqlParser.Select_listContext context)
        {
            this.context = context;
        }

        internal void AddSelectListExpressionList(List<ExpressionNode> expressionList)
        {
            expressionLists.Add(expressionList);
        }


        internal void EndElement()
        {
            currentAlias = null;
        }

        internal void AddRowsetColumnName(FullColumnName fcn)
        {
            rowsetColumnNames.Add(fcn);
        }

        internal void AddUnknownRowsetColumnName()
        {
            FullColumnName fcn = FullColumnName.FromColumnName($"Anonymous{unknownColumnID}");
            AddRowsetColumnName(fcn);
            unknownColumnID += 1;
        }

        internal int RowsetColumnNamesCount { get { return rowsetColumnNames.Count; } }

        internal FullColumnName RowsetColumnName(int idx) { return rowsetColumnNames[idx]; }

        internal int ExpressionListCount { get { return expressionLists.Count; } }

        internal string? CurrentAlias { get { return currentAlias; } set { currentAlias = value; } }

        internal static ExpressionOperand Execute(List<ExpressionNode> expression, ResultSet resultSet, int rowIndex)
        {
            Stack<ExpressionNode> stack = new Stack<ExpressionNode>();

            do
            {
                foreach (ExpressionNode n in expression)
                {
                    if (n is ExpressionOperand)
                        stack.Push(n);
                    else if (n is ExpressionOperator)
                    {
                        // it's an operator
                        var oper = (ExpressionOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionOperandFromColumn)
                    {
                        var r = (ExpressionOperandFromColumn)n;
                        Console.WriteLine($"Need value from {r.ColumnName}");

                        int idx = resultSet.ColumnIndex(r.ColumnName);
                        ExpressionOperand[] thisRow = resultSet.Row(rowIndex);
                        ExpressionOperand val = thisRow[idx];
                        stack.Push(val);
                    }
                    else if (n is ExpressionComparisonOperator)
                    {
                        var oper = (ExpressionComparisonOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionBooleanOperator)
                    {
                        var oper = (ExpressionBooleanOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
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

        internal ExpressionOperand Execute(int index, ResultSet resultSet, int rowIndex)
        {
            return SelectListContext.Execute(expressionLists[index], resultSet, rowIndex);
        }

        internal void Dump()
        {
            Console.WriteLine($"rowsetColumnNames: {String.Join(", ", rowsetColumnNames)}");
            Console.WriteLine("SelectExpressions:");
            for (int i = 0; i < ExpressionListCount; i++)
            {
                Console.Write($"  #{i}: ");
                foreach (var x in expressionLists[i])
                {
                    Console.Write($"{x} ");
                }
                Console.WriteLine();
            }
        }
    }
}
