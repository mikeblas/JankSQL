namespace JankSQL.Operators
{
    using JankSQL.Expressions;

    /// <summary>
    /// IOperatorOutput is the interface implemented by a component that produces
    /// a ResultSet at its output. The GetRows() method is called to collect that
    /// output.
    /// </summary>
    internal interface IOperatorOutput
    {
        /// <summary>
        /// Get a ResultSet of, at most, max rows from this object. The ResultSet
        /// will always have the same shape. If the return value is null, then this
        /// object can't produce any further output and has exhausted itself.
        ///
        /// </summary>
        /// <param name="engine">object implementing the IEngines.Engine interface against which this object will run.</param>
        /// <param name="max">integer indicating the maximum number of rows to produce.</param>
        /// <returns>ResultSet with those rows, or null if no more data is available.</returns>
        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues);

        public FullColumnName[] GetOutputColumnNames();

        public BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues);

        /// <summary>
        /// Rewind causes the object to rewind to the beginning of its produced rows.
        /// </summary>
        void Rewind();
    }
}
