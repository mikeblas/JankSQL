namespace JankSQL.Expressions
{
    internal class ExpressionOperandFromColumn : ExpressionNode, IEquatable<ExpressionOperandFromColumn>
    {
        private readonly FullColumnName columnName;

        internal ExpressionOperandFromColumn(FullColumnName columnName)
        {
            this.columnName = columnName;
        }

        internal FullColumnName ColumnName
        {
            get { return columnName; }
        }

        public bool Equals(ExpressionOperandFromColumn? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (ReferenceEquals(this, other))
                return true;

            return columnName.Equals(other.ColumnName);
        }

        public override bool Equals(object? o)
        {
            return Equals((ExpressionOperandFromColumn?)o);
        }

        public override int GetHashCode()
        {
            return columnName.GetHashCode();
        }

        public override string ToString()
        {
            return $"FromColumn({columnName})";
        }

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            // value from a column
            if (accessor == null)
                throw new ExecutionException($"Not in a row context to evaluate {this}");
            ExpressionOperand ret = accessor.GetValue(ColumnName);
            stack.Push(ret);
        }
    }
}

