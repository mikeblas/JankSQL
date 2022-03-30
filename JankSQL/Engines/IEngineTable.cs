namespace JankSQL.Engines
{
    using JankSQL.Expressions;

    /// <summary>
    /// The IEngineTable interface provides an engine-independent contract for working with a table
    /// in a storage engine. Tables may be implemented in any way, but should expose indexible collections
    /// of tuples and offer a variety of mutations on those collections.
    /// </summary>
    public interface IEngineTable
    {
        // === metadata

        /// <summary>
        /// Gets the number of columns in this table.
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Gets the name of a column given its index.
        /// </summary>
        /// <param name="n">integer index for column to be retrieved.</param>
        /// <returns>FullColumnName with the name of that column.</returns>
        FullColumnName ColumnName(int n);

        /// <summary>
        /// Gets the index of a given column name.
        /// </summary>
        /// <param name="columnName">string with the name of the column to look up.</param>
        /// <returns>integer index of the matching column name, -1 if not found.</returns>
        int ColumnIndex(string columnName);

        // === data access

        /// <summary>
        /// Returns a RowWithBookmark enumerator that will go over the rows in this table in no particular order.
        /// </summary>
        /// <returns>IEnumerator-implementing object for the enumeration.</returns>
        IEnumerator<RowWithBookmark> GetEnumerator();

        /// <summary>
        /// Get an IndexAccessor by name of the index. The Accessor can be used to
        /// iterate of that index in its order, and also contains the definition of the index.
        /// </summary>
        /// <param name="indexName">string with a name of the index to retrieve.</param>
        /// <returns>IndexAccessor object for the index, null if not found.</returns>
        IndexAccessor? Index(string indexName);

        // === data manipulation

        /// <summary>
        /// Truncate this table.
        /// </summary>
        void TruncateTable();

        /// <summary>
        /// Insert a new row in this table, updating all involved indexes.
        /// </summary>
        /// <param name="row">Tuple with the row to be inserted.</param>
        void InsertRow(Tuple row);

        /// <summary>
        /// Deletes rows with the given bookmarks, and updates all involved indexes.
        /// </summary>
        /// <param name="bookmarksToDelete">List of bookmarks to be deleted.</param>
        /// <returns>integer count of the number of rows actually deleted.</returns>
        int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete);
    }
}
