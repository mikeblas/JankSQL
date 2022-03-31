namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using JankSQL;
    using Engines = JankSQL.Engines;

    public class UpdateTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod]
        public void TestUpdateExpression()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE MyTable SET Population = Population * 1.2;");

            Assert.IsNotNull(ecUpdate);
            Assert.AreEqual(0, ecUpdate.TotalErrors);

            ExecuteResult resultsUpdate = ecUpdate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsUpdate.ExecuteStatus, resultsUpdate.ErrorMessage);
            Assert.IsNull(resultsUpdate.ResultSet);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(3, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("Population"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[popIndex].AsInteger();
            }
        }


        [TestMethod]
        public void TestUpdateSameExpression()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE is_even = 1;");

            Assert.IsNotNull(ecUpdate);
            Assert.AreEqual(0, ecUpdate.TotalErrors);

            ExecuteResult resultsUpdate = ecUpdate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsUpdate.ExecuteStatus, resultsUpdate.ErrorMessage);
            Assert.IsNull(resultsUpdate.ResultSet);


            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM ten;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(10, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int evenIndex= resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
            }

        }
    }
}


