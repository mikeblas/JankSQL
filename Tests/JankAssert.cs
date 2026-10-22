
namespace Tests
{
    using JankSQL;

    using NUnit.Framework;

    public static class JankAssert
    {
        public static void RowsetExistsWithShape(ExecuteResult executeResult, int expectedColumns, int expectedRows)
        {
            if (executeResult.ResultSet == null)
                throw new AssertionException($"expected a non-null result set. Error message was {executeResult.ErrorMessage}");

            List<string> messages = [];

            if (executeResult.ResultSet.ColumnCount != expectedColumns)
                messages.Add($"expected {expectedColumns} columns, found {executeResult.ResultSet.ColumnCount}");

            if (executeResult.ResultSet.RowCount != expectedRows)
                messages.Add($"expected {expectedRows} rows, found {executeResult.ResultSet.RowCount}");

            if (messages.Count > 0)
            {
                executeResult.ResultSet.Dump();
                throw new AssertionException(string.Join(';', messages));
            }
        }

        public static void ValueMatchesString(ResultSet rs, int column, int row, string expectedValue)
        {
            if (rs == null)
                throw new AssertionException("expected a non-null result set");

            if (rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected non-null string value at column {column}, row {row}");

            if (rs.Row(row)[column].NodeType != ExpressionOperandType.VARCHAR)
                throw new AssertionException($"expected string value at column {column}, row {row}, found {rs.Row(row)[column].NodeType}");

            Assert.That(rs.Row(row)[column].AsString(), Is.EqualTo(expectedValue));
        }

        public static void ValueMatchesInteger(ResultSet rs, int column, int row, int expectedValue)
        {
            if (rs == null)
                throw new AssertionException("expected a non-null result set");

            if (rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected non-null integer value at column {column}, row {row}");

            if (rs.Row(row)[column].NodeType != ExpressionOperandType.INTEGER)
                throw new AssertionException($"expected integer value at column {column}, row {row}, found {rs.Row(row)[column].NodeType}");

            Assert.That(rs.Row(row)[column].AsInteger(), Is.EqualTo(expectedValue));
        }

        public static void ValueMatchesDateTime(ResultSet rs, int column, int row, DateTime expectedValue)
        {
            if (rs == null)
                throw new AssertionException("expected a non-null result set");

            if (rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected non-null DateTime value at column {column}, row {row}");

            if (rs.Row(row)[column].NodeType != ExpressionOperandType.DATETIME)
                throw new AssertionException($"expected DateTime value at column {column}, row {row}, found {rs.Row(row)[column].NodeType}");

            Assert.That(rs.Row(row)[column].AsDateTime(), Is.EqualTo(expectedValue));
        }

        public static void ValueIsNull(ResultSet rs, int column, int row)
        {
            if (rs == null)
                throw new AssertionException("expected a non-null result set");

            if (!rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected null at column {column}, row {row}; instead found {rs.Row(row)[column]}");
        }

        public static void ValueMatchesDecimal(ResultSet rs, int column, int row, double expectedValue, double tolerance)
        {
            if (rs == null)
                throw new AssertionException("expected a non-null result set");

            if (rs.Row(row)[column].RepresentsNull)
                throw new AssertionException($"expected non-null integer value at column {column}, row {row}");

            if (rs.Row(row)[column].NodeType != ExpressionOperandType.DECIMAL)
                throw new AssertionException($"expected decimal value at column {column}, row {row}, found {rs.Row(row)[column].NodeType}");

            Assert.That(rs.Row(row)[column].AsDouble(), Is.EqualTo(expectedValue).Within(tolerance));
        }

        public static void SuccessfulParse(ExecutableBatch ec)
        {
            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.EqualTo(0));
        }


        public static void SuccessfulNoResultSet(ExecuteResult er)
        {
            Assert.That(er.ExecuteStatus, Is.EqualTo(ExecuteStatus.SUCCESSFUL));
            Assert.Throws<InvalidOperationException>(() => { var _ = er.ResultSet; });
        }


        public static void SuccessfulWithMessageNoResultSet(ExecuteResult er)
        {
            Assert.That(er.ExecuteStatus, Is.EqualTo(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE));
            Assert.Throws<InvalidOperationException>(() => { var _ = er.ResultSet; });
        }


        public static void SuccessfulRowsAffected(ExecuteResult er, int rowsExpected)
        {
            // Assert.That(er.ExecuteStatus, Is.EqualTo(ExecuteStatus.SUCCESSFUL));
            if (er.ExecuteStatus != ExecuteStatus.SUCCESSFUL)
            {
                Assert.Fail($"expected SUCCESSFUL, but result was {er.ExecuteStatus} with message \"{er.ErrorMessage}\"");
            }
            Assert.That(er.ErrorMessage, Is.Null);
            Assert.That(er.RowsAffected, Is.EqualTo(rowsExpected));
        }


        public static void IntegerColumnMatchesSet(ResultSet rs, int columnIndex, ISet<int> expectedSet)
        {
            for (int i = 0; i < rs.RowCount; i++)
            {
                int val = rs.Row(i)[columnIndex].AsInteger();
                if (!expectedSet.Remove(val))
                    Assert.Fail($"unexpected value {val} returned");
            }

            Assert.That(expectedSet.Count, Is.EqualTo(0), $"not all values were found in the expected set: {expectedSet} missing");
        }

        public static void StringColumnMatchesSet(ResultSet rs, int columnIndex, ISet<string> expectedSet)
        {
            for (int i = 0; i < rs.RowCount; i++)
            {
                string val = rs.Row(i)[columnIndex].AsString();
                if (!expectedSet.Remove(val))
                    Assert.Fail($"unexpected value {val} returned");
            }

            Assert.That(expectedSet.Count, Is.EqualTo(0), $"not all values were found in the expected set: {expectedSet} missing");
        }

        public static void FailureWithMessage(ExecuteResult er)
        {
            Assert.That(er.ExecuteStatus, Is.EqualTo(ExecuteStatus.FAILED));
            Assert.That(er.ErrorMessage, Is.Not.Null);

            // throws exception since no ResultSet is available
            Assert.Throws<InvalidOperationException>(() => { var x = er.ResultSet; });
        }
    }
}
