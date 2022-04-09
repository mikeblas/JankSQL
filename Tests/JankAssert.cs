
namespace Tests
{
    using JankSQL;

    using NUnit.Framework;

    internal class JankAssert
    {
        public static void RowsetExistsWithShape(ExecuteResult executeResult, int expectedColumns, int expectedRows)
        {
            if (executeResult.ResultSet == null)
                throw new AssertionException($"expected a non-null result set. Error message was {executeResult.ErrorMessage}");

            List<string> messages = new ();

            if (executeResult.ResultSet.ColumnCount != expectedColumns)
                messages.Add($"expected {expectedColumns} columns, found {executeResult.ResultSet.ColumnCount}");

            if (executeResult.ResultSet.RowCount != expectedRows)
                messages.Add($"expected {expectedRows} rows, found {executeResult.ResultSet.RowCount}");

            if (messages.Count > 0)
                throw new AssertionException(string.Join(';', messages));
        }
    }
}
