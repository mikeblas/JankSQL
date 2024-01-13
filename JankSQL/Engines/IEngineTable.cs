﻿namespace JankSQL.Engines
{
    using JankSQL.Expressions;

    /// <summary>
    /// The IEngineTable interface provides an engine-independent contract for working with a table
    /// in a storage engine. Tables may be implemented in any way, but should expose indexable collections
    /// of tuples and offer a variety of mutations on those collections.
    /// </summary>
    public interface IEngineTable
    {
        // === meta data

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

        /// <summary>
        /// Get an IndexAccessor filtered to match the expression list. The expression list
        /// has a one-to-one correlation to the column list in the index. Only values matching the expresions
        /// with the given comparison operators will be returned.  An operator of "=" and an expression
        /// of "=" for the first column will return only rows matching 3 on that first column.
        /// </summary>
        /// <param name="indexName">string with a name of the index to retrieve.</param>
        /// <param name="comparisons">list of comparisons.</param>
        /// <param name="expressions">list of expression values.</param>
        /// <returns>IndexAccessor object for the index, null if not found.</returns>
        IndexAccessor? PredicateIndex(string indexName, IEnumerable<ExpressionComparisonOperator> comparisons, IEnumerable<Expression> expressions);

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

        /// <summary>
        /// Commits all changes to this table.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back all changes to this table.
        /// </summary>
        void Rollback();

        /// <summary>
        /// return a string identifying the best index to access this table along the given
        /// filter, or null if no index is viable.
        /// </summary>
        /// <param name="accessColumns">IEnumerable of pairs describing the desired access path.</param>
        /// <returns>name of index to use, or null if no viable indexed access path.</returns>
        string? BestIndex(IEnumerable<(string columnName, bool isEquality)> accessColumns);

        /// <summary>
        /// Get a row given a bookmark tuple.
        /// </summary>
        /// <param name="bmk">ExpressionOperandBookmark to be retrieved.</param>
        /// <returns>Tuple value from that bookmark.</returns>
        Tuple RowFromBookmark(ExpressionOperandBookmark bmk);
    }
}
