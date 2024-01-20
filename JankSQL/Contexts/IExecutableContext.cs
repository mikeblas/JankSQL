namespace JankSQL.Contexts
{
    using JankSQL.Expressions;

    /// <summary>
    /// <para>
    /// IExecutableContext is the interface to an executable context class. Contexts are
    /// built from the walker, containing the context they need to cause execution of a
    /// statement.
    /// </para>
    /// <para>
    /// Generally, DML contexts will build operator trees to actually execute (with a data
    /// flow) while DDL contexts will be executed directly and ask the involved Engine to
    /// implement the desired mutations.
    /// </para>
    /// </summary>
    public interface IExecutableContext : ICloneable
    {
        /// <summary>
        /// Execute this context using the given engine.
        /// </summary>
        /// <param name="engine">IEngine which provides storage and enumeration capabilities.</param>
        /// <param name="accessor">an optional IRowValueAccessor that gives a supplemental binding context for evaluations in this execution.</param>
        /// <returns>An ExecuteResult indicating the outcome.</returns>
        ExecuteResult Execute(Engines.IEngine engine, IRowValueAccessor? accessor, Dictionary<string, ExpressionOperand> bindValues);


        BindResult Bind(Engines.IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues);

        /// <summary>
        /// Dump information about this executable context. This call will effectively show
        /// an "execution plan".
        /// </summary>
        void Dump();
    }
}
