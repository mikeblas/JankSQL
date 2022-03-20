using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;
using Engines = JankSQL.Engines;


namespace Tests
{
    public class OrderByTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod]
        public void TestOrderByOneStringDefault()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(10, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            string previous = result.ResultSet.Row(0)[0].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[0].AsString();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [TestMethod]
        public void TestOrderByOneStringAsc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name ASC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(10, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            string previous = result.ResultSet.Row(0)[0].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[0].AsString();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }


        [TestMethod]
        public void TestOrderByOneStringDesc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name desc;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(10, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            string previous = result.ResultSet.Row(0)[0].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[0].AsString();
                Assert.IsTrue(previous.CompareTo(current) >= 0, $"expected {previous} >= {current}");
                previous = current;
            }
        }


        [TestMethod]
        public void TestOrderByOneIntegerDefault()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten ORDER BY number_id;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(10, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }


        [TestMethod]
        public void TestOrderByOneIntegerAsc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten ORDER BY number_id ASC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(10, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }



        [TestMethod]
        public void TestOrderByOneIntegerDesc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten ORDER BY number_id DESC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(10, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) >= 0, $"expected {previous} >= {current}");
                previous = current;
            }
        }
    }
}
