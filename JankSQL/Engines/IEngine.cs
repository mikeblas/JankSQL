namespace JankSQL.Engines
{
    using System.Collections.Immutable;

    /// <summary>
    /// The IEngine interface commjunicates with a modular storage engine. There
    /// are a couple of implementations of the engine, mainly for simplication
    /// of testing.
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// Drop a table. This removes the table and deletes all data, and
        /// will update the system catalogs.
        /// </summary>
        /// <param name="tableName">name of the table to drop.</param>
        public void DropTable(FullTableName tableName);

        /// <summary>
        /// Create a new table. This creates a table with the given column names
        /// and data types, at the given table name. Updates the system catalogs
        /// with the new columns and tables.
        ///
        /// Note that there are no constraints here.
        /// </summary>
        /// <param name="tableName">FullTableName with the name of this table.</param>
        /// <param name="columnNames">List of FullColumnNames for naming the columns.</param>
        /// <param name="columnTypes">Data type of each column, correlate to the columnNames parameter.</param>
        public void CreateTable(FullTableName tableName, IImmutableList<FullColumnName> columnNames, IImmutableList<ExpressionOperandType> columnTypes);


        /// <summary>
        /// Creates an index on the given table.
        /// </summary>
        /// <param name="tableName">table name upon which the index will be created.</param>
        /// <param name="indexName">string with a name of this index.</param>
        /// <param name="isUnique">true if this index is meant to be unique, false otherwise.</param>
        /// <param name="columnInfos">list of column infos for this index.</param>
        public void CreateIndex(FullTableName tableName, string indexName, bool isUnique, IEnumerable<(string columnName, bool isDescending)> columnInfos);

        /// <summary>
        /// Gets an object that implements IEngineTable to talk to a table. The talbe is identified by name.
        /// The returned interface can be used for operations on the individual table.
        /// </summary>
        /// <param name="tableName">Name of the table to retrieve.</param>
        /// <returns>Object with IEngineTable usable to manipulate the table. null if the table name is not known.</returns>
        public IEngineTable? GetEngineTable(FullTableName tableName);

        /// <summary>
        /// Get the sys_tables table object. This provides access to the system catalog.
        /// </summary>
        /// <returns>Object implementing IEngineTable on the sys_tables table.</returns>
        public IEngineTable GetSysTables();

        /// <summary>
        /// Get the sys_columns table object. This provides access to the system catalog.
        /// </summary>
        /// <returns>Object implementing IEngineTable on the sys_columns table.</returns>
        public IEngineTable GetSysColumns();

        /// <summary>
        /// Get the sys_indexes table object. This provides access to the system catalog.
        /// </summary>
        /// <returns>Object implementing IEngineTable on the sys_indexes table.</returns>
        public IEngineTable GetSysIndexes();

        /// <summary>
        /// Get the sys_indexcolumns table object. This provides access to the system catalog.
        /// </summary>
        /// <returns>Object implementing IEngineTable on the sys_indexcolumns table.</returns>
        public IEngineTable GetSysIndexColumns();

        /// <summary>
        /// InjectTestTable will inject a table for testing into this engine. The engine
        /// may or may not store the table. Once injected, the table can be queried and will
        /// appear in the system tables. Intended for tests and not applied usage.
        /// </summary>
        /// <param name="testTable">TestTable with the definition of this table.</param>
        /// <returns>Object implementing IEngineTable for the new table.</returns>
        public IEngineTable InjectTestTable(TestTable testTable);
    }
}
