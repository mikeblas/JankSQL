﻿
namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using JankSQL;
    using Engines = JankSQL.Engines;

    public class ExecuteWhereTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereGreater()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] > 30000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereLess()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] < 30000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualNone()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 30000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualSome()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 25000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualsMathA()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 12500 * 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualsMathB()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] * 2 = 50000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereBangEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] != 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereLessGreaterNotEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] <> 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereOR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 37000 OR [keycolumn] = 1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereAND()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 25000 AND [keycolumn] = 1;");

            ec.Dump();

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereComplexAND()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 25000 AND [keycolumn] = 5-4;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        // 
        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTParens()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT ([population] = 37000);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTCompoundParens()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT ([population] = 37000 OR [keycolumn] = 1);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");

        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTMultiParens()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT(NOT(NOT ([population] = 37000)));");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT [population] = 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTMulti()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT NOT NOT NOT NOT [population] = 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTCompound()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT [population] = 37000 OR [keycolumn] = 1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTCompound3Rows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT [population] = 37000 OR [keycolumn] = 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

    }
}
