namespace JankSQL.Contexts
{
    internal class SelectListContext
    {
        private readonly TSqlParser.Select_listContext context;
        private readonly List<Expression> expressionList = new ();
        private readonly List<FullColumnName> rowsetColumnNames = new ();

        private string? currentAlias = null;

        private int unknownColumnID = 1001;

        internal SelectListContext(TSqlParser.Select_listContext context)
        {
            this.context = context;
        }

        internal int ExpressionListCount
        {
            get { return expressionList.Count; }
        }

        internal string? CurrentAlias
        {
            get { return currentAlias; }
            set { currentAlias = value; }
        }

        internal FullColumnName RowsetColumnName(int idx)
        {
            return rowsetColumnNames[idx];
        }

        internal List<Expression> SelectExpressions
        {
            get { return expressionList; }
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

        internal string? BindNameForExpression(Expression x)
        {
            for (int i = 0; i < expressionList.Count; i++)
            {
                if (expressionList[i].Equals(x))
                    return RowsetColumnName(i).ColumnNameOnly();
            }

            return null;
        }

        //TODO: refactor this into Select operator
        internal ExpressionOperand Execute(int index, ResultSet resultSet, int rowIndex)
        {
            return expressionList[index].Evaluate(new ResultSetValueAccessor(resultSet, rowIndex));
        }

        internal void Dump()
        {
            Console.WriteLine($"rowsetColumnNames: {string.Join(", ", rowsetColumnNames)}");
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
