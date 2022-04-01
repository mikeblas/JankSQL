﻿namespace Tests
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

            var ecSelect = Parser.ParseSQLFileFromString("SELECT population FROM MyTable;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(3, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            HashSet<int> expected = new ()
            {
                44400, 30000, 13800000,
            };

            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("Population"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[popIndex].AsInteger();

                Assert.IsTrue(expected.Contains(number), $"expected to find {number}");
                expected.Remove(number);
            }

            Assert.IsTrue(expected.Count == 0, "Expected all values to be found");
        }


        [TestMethod]
        public void TestUpdateSameExpressionNoMatches()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE is_even = 1 AND SQRT(10) > 10;");

            Assert.IsNotNull(ecUpdate);
            Assert.AreEqual(0, ecUpdate.TotalErrors);

            ExecuteResult resultsUpdate = ecUpdate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsUpdate.ExecuteStatus, resultsUpdate.ErrorMessage);
            Assert.IsNull(resultsUpdate.ResultSet);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT is_even, number_id FROM ten;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(10, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int evenIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                if (number % 2 == 0)
                    Assert.AreEqual(1, even);
                else
                    Assert.AreEqual(0, even);
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

            var ecSelect = Parser.ParseSQLFileFromString("SELECT is_even, number_id FROM ten;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(10, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int evenIndex= resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                if (number % 2 == 0)
                    Assert.AreEqual(9, even);
                else
                    Assert.AreEqual(0, even);
            }
        }

        [TestMethod]
        public void TestUpdateSameExpressionCompound()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE is_even = 1 AND (number_name = 'four' OR number_name = 'six');");

            Assert.IsNotNull(ecUpdate);
            Assert.AreEqual(0, ecUpdate.TotalErrors);

            ExecuteResult resultsUpdate = ecUpdate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsUpdate.ExecuteStatus, resultsUpdate.ErrorMessage);
            Assert.IsNull(resultsUpdate.ResultSet);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT number_name, is_even, number_id FROM ten;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(10, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int evenIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            int nameIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_name"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                string name = resultSelect.ResultSet.Row(i)[nameIndex].AsString();

                if (name == "four" || name == "six")
                    Assert.AreEqual(9, even);
                else
                {
                    if (number % 2 == 0)
                        Assert.AreEqual(1, even);
                    else
                        Assert.AreEqual(0, even);
                }
            }
        }

        [TestMethod]
        public void TestUpdateSameNOTExpression()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE NOT is_even = 1;");

            Assert.IsNotNull(ecUpdate);
            Assert.AreEqual(0, ecUpdate.TotalErrors);

            ExecuteResult resultsUpdate = ecUpdate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsUpdate.ExecuteStatus, resultsUpdate.ErrorMessage);
            Assert.IsNull(resultsUpdate.ResultSet);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT is_even, number_id FROM ten;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(10, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int evenIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                if (number % 2 == 0)
                    Assert.AreEqual(1, even);
                else
                    Assert.AreEqual(9, even);
            }
        }
    }
}

