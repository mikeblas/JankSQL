﻿namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using JankSQL;
    using Engines = JankSQL.Engines;

    public class ExecuteTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER((10/2), 15/5) FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                Assert.AreEqual(125, result.ResultSet.Row(i)[0].AsDouble());
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionTwoExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5, 92 * 6 FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                Assert.AreEqual(3+5, result.ResultSet.Row(i)[0].AsDouble());
                Assert.AreEqual(92*6, result.ResultSet.Row(i)[1].AsDouble());
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionThreeExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 355/113, 867-5309, (123 + 456 - 111) / 3 FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");


            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                Assert.AreEqual(355 / 113, result.ResultSet.Row(i)[0].AsInteger());
                Assert.AreEqual(867 - 5309, result.ResultSet.Row(i)[1].AsInteger());
                Assert.AreEqual((123 + 456 - 111) / 3, result.ResultSet.Row(i)[2].AsInteger());
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectStar()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectStarIsNotNull()
        {
            // insert a row with NULL in population
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name) VALUES (53, 'West Hartford');");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultInsert.ExecuteStatus);
            Assert.IsNotNull(resultInsert.ResultSet);
            resultInsert.ResultSet.Dump();

            // select back where population isn't null
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM mytable WHERE Population IS NOT NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectStarIsNull()
        {
            // insert a row with NULL in population
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name) VALUES (53, 'West Hartford');");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultInsert.ExecuteStatus);
            Assert.IsNotNull(resultInsert.ResultSet);

            // select back where population isn't null
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE Population IS NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population] FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestCompoundSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population]*2, [population] FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestCompoundSelectListQualified()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [mytable].[city_name], [mytable].[population], [population]*2 FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectListExpressionDivide()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [population] / [keycolumn] FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectListExpressionDivideQualifiedAliased()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [population] / [mytable].[keycolumn] AS [TheRatio] FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
                Assert.AreEqual(8, result.ResultSet.Row(i)[0].AsInteger());
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionAdditionAliased()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 AS [MySum] FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
                Assert.AreEqual(8, result.ResultSet.Row(i)[0].AsInteger());
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionParenthesis()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 2*(6+4) FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
                Assert.AreEqual(20, result.ResultSet.Row(i)[0].AsInteger());
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionSquareRoot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2) FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
                Assert.AreEqual(1.41421356, result.ResultSet.Row(i)[0].AsDouble(), 0.00000001);
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionPower()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(5, 3) FROM [mytable];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
                Assert.AreEqual(125, result.ResultSet.Row(i)[0].AsInteger());
        }

        [TestMethod, Timeout(1000)]
        public void TestTwoResults()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'This'; SELECT 'That';");

            ExecuteResult[] results = ec.Execute(engine);
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Length, "expected two results");
            for (int i = 0; i < results.Length; i++)
            {
                Assert.IsNotNull(results[i]);
                Assert.IsNotNull(results[i].ResultSet, results[i].ErrorMessage);
                results[i].ResultSet!.Dump();
                Assert.AreEqual(1, results[i].ResultSet!.RowCount, "rowcount mismatch");
                Assert.AreEqual(1, results[i].ResultSet!.ColumnCount, "column count mismatch");

                if (i == 0)
                    Assert.AreEqual("This", results[i].ResultSet!.Row(0)[0].AsString());
                else if (i == 1)
                    Assert.AreEqual("That", results[i].ResultSet!.Row(0)[0].AsString());
            }
        }


        [TestMethod, Timeout(1000)]
        public void TestPredicateFunction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] > POWER(2500, 2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");
        }
    }
}


