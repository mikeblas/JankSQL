namespace JankSQL.Contexts
{
    using System.Collections.Immutable;

    using JankSQL.Expressions;

    internal class SelectListContext : ICloneable
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

        internal ImmutableList<Expression> SelectExpressions
        {
            get { return expressionList.ToImmutableList(); }
        }

        internal int ExpressionListCount
        {
            get { return expressionList.Count; }
        }

        internal int RowsetColumnNameCount
        {
            get { return rowsetColumnNames.Count; }
        }

        internal string? CurrentAlias
        {
            get { return currentAlias; }
            set { currentAlias = value; }
        }

        public object Clone()
        {
            var clone = new SelectListContext(this.context);
            foreach (var expression in this.expressionList)
                clone.expressionList.Add(expression);
            foreach (var name in this.rowsetColumnNames)
                clone.rowsetColumnNames.Add(name);
            clone.currentAlias = this.currentAlias;
            clone.unknownColumnID = this.unknownColumnID;

            return clone;
        }

        internal FullColumnName RowsetColumnName(int idx)
        {
            return rowsetColumnNames[idx];
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

        internal FullColumnName? BindNameForExpression(Expression x)
        {
            for (int i = 0; i < expressionList.Count; i++)
            {
                if (expressionList[i].Equals(x))
                    return RowsetColumnName(i);
            }

            return null;
        }

        internal void Dump()
        {
            Console.WriteLine($"rowsetColumnNames: {string.Join(", ", rowsetColumnNames)}");
            Console.WriteLine("SelectExpressions:");
            for (int i = 0; i < ExpressionListCount; i++)
                Console.WriteLine($"  #{i}: {expressionList[i]}");
        }
    }
}
