namespace JankSQL.Expressions
{
    internal class ExpressionOperandFromColumn : ExpressionNode, IEquatable<ExpressionOperandFromColumn>
    {
        private FullColumnName columnName;

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
    }
}

