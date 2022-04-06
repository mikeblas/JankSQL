namespace JankSQL.Contexts
{
    /// <summary>
    /// IExecutableContext is the interface to an executable context class. Contexts are
    /// built from the wakler, containing the context they need to cause execution of a
    /// statement.
    ///
    /// Generally, DML contexts will build operator trees to actually execute (with a data
    /// flow) while DDL contexts will be executed directly and ask the involved Engine to
    /// implement the desired mutations.
    /// </summary>
    public interface IExecutableContext
    {
        /// <summary>
        /// Execute this context using the given engine.
        /// </summary>
        /// <param name="engine">IEngine which provides storage and enumeration capabilities.</param>
        /// <returns>An ExecuteResult indicating the outcome.</returns>
        ExecuteResult Execute(Engines.IEngine engine);

        /// <summary>
        /// Dump information about this executable context. This call will effectively show
        /// an "execution plan".
        /// </summary>
        void Dump();
    }
}

