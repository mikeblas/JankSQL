﻿

namespace JankSQL
{

    internal class SelectListContext
    {
        readonly TSqlParser.Select_listContext context;
        readonly List<Expression> expressionList = new();

        string? currentAlias = null;
        int unknownColumnID = 1001;
        readonly List<FullColumnName> rowsetColumnNames = new();


        internal SelectListContext(TSqlParser.Select_listContext context)
        {
            this.context = context;
        }

        internal void AddSelectListExpressionList(Expression expressionList)
        {
            this.expressionList.Add(expressionList);
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

        internal int ExpressionListCount { get { return expressionList.Count; } }

        internal string? CurrentAlias { get { return currentAlias; } set { currentAlias = value; } }

        internal ExpressionOperand Execute(int index, ResultSet resultSet, int rowIndex)
        {
            return expressionList[index].Evaluate(new RowsetValueAccessor(resultSet, rowIndex));
        }

        internal void Dump()
        {
            Console.WriteLine($"rowsetColumnNames: {String.Join(", ", rowsetColumnNames)}");
            Console.WriteLine("SelectExpressions:");
            for (int i = 0; i < ExpressionListCount; i++)
            {
                Console.Write($"  #{i}: ");
                foreach (var x in expressionList[i])
                {
                    Console.Write($"{x} ");
                }
                Console.WriteLine();
            }
        }
    }
}
