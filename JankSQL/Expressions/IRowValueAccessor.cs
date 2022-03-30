namespace JankSQL.Expressions
{
    /// <summary>
    /// Provides by-name access for an Expression into a set of values representing a tuple.
    /// Expressions might work on a row within a RowSet, or might work on some temporary
    /// values in a loose ExpressionOperand array. This interface provides a way to
    /// give an adapter to the expression evaluator to acess either data type.
    /// </summary>
    internal interface IRowValueAccessor
    {
        /// <summary>
        /// Get a value from the given column name in the wrapped source.
        /// </summary>
        /// <param name="fcn">Column name being accssed.</param>
        /// <returns>ExpressionOperand with the value of that column.</returns>
        ExpressionOperand GetValue(FullColumnName fcn);


        /// <summary>
        /// Change the value (as in an UPDATE) in a row.
        /// </summary>
        /// <param name="fcn">Column name to change.</param>
        /// <param name="value">New value for that column.</param>
        void SetValue(FullColumnName fcn, ExpressionOperand value);
    }
}
