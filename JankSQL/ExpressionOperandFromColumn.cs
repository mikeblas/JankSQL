namespace JankSQL
{
    internal class ExpressionOperandFromColumn : ExpressionNode
    {
        internal FullColumnName columnName;

        internal ExpressionOperandFromColumn(FullColumnName columnName)
        {
            this.columnName = columnName;
        }

        internal FullColumnName ColumnName { get { return columnName; } }

        public override string ToString()
        {
            return $"FromColumn({columnName})";
        }
    }
}

