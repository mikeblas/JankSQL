
namespace Tests
{
    using JankSQL;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Diagnostics.CodeAnalysis;

    internal class JankAssert
    {

        public static void RowsetExistsWithShape(ExecuteResult executeResult, int expectedColumns, int expectedRows)
        {
            if (executeResult.ResultSet == null)
                throw new AssertFailedException($"expected a non-null result set. Error message was {executeResult.ErrorMessage}");

            if (executeResult.ResultSet.ColumnCount != expectedColumns)
                throw new AssertFailedException($"expected {expectedColumns}, found {executeResult.ResultSet.ColumnCount}");

            if (executeResult.ResultSet.RowCount != expectedRows)
                throw new AssertFailedException($"expected {expectedRows}, found {executeResult.ResultSet.RowCount}");
        }
    }
}
