namespace JankSQL.Operators.Aggregations
{
    /// <summary>
    /// Interface for an accumulator object used by the aggregation operator. Is offered values to
    /// aggregate, and then asked for a FinalValue after the end of the rowset.
    /// </summary>
    internal interface IAggregateAccumulator
    {
        /// <summary>
        /// Accepts a value to accumlate in an aggregation. Called once per row within a grouping.
        /// </summary>
        /// <param name="op">ExpressionOperand with the value to aggregate.</param>
        void Accumulate(ExpressionOperand op);

        /// <summary>
        /// Produces the final value of the aggregation. Called only once per grouping.
        /// </summary>
        /// <returns>an ExpressionOperand representing the value of this aggregation.</returns>
        ExpressionOperand FinalValue();
    }
}

