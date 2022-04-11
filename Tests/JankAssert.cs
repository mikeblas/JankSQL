
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

        public static void ValueMatchesString(ResultSet rs, int column, int row, string expectedValue)
        {
            if (rs == null)
                throw new AssertionException($"expected a non-null result set");

            if (rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected non-null string value at column {column}, row {row}");

            if (rs.Row(row)[column].NodeType != ExpressionOperandType.VARCHAR)
                throw new AssertionException($"expected string value at column {column}, row {row}, found {rs.Row(row)[column].NodeType}");

            Assert.AreEqual(expectedValue, rs.Row(row)[column].AsString());
        }

        public static void ValueMatchesInteger(ResultSet rs, int column, int row, int expectedValue)
        {
            if (rs == null)
                throw new AssertionException($"expected a non-null result set");

            if (rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected non-null integer value at column {column}, row {row}");

            if (rs.Row(row)[column].NodeType != ExpressionOperandType.INTEGER)
                throw new AssertionException($"expected integer value at column {column}, row {row}, found {rs.Row(row)[column].NodeType}");

            Assert.AreEqual(expectedValue, rs.Row(row)[column].AsInteger());
        }
    }
}
