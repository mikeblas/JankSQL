﻿namespace JankSQL.Operators
{
    using JankSQL.Expressions;

    /// <summary>
    /// IComponentOutput is the interface implemented by a component that produces
    /// a ResultSet at its output. The GetRows() method is called to collect that
    /// output.
    /// </summary>
    internal interface IComponentOutput
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
        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues);

        /// <summary>
        /// Rewind causes the object to rewind to the beginning of its produced rows.
        /// </summary>
        void Rewind();
    }
}
