﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class BareSelectTests
    {
        [TestMethod, Timeout(1000)]
        public void TestAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestNegativeNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestNegativeNumberMultiply()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32 * -133;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestAdditionWhere()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=1;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestAdditionWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=0;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'hello', 'goodbye', 'Bob''s Burgers';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestConcatenateTwoStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestConcatenateThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world' + ', good day!';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestStringMinusNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '300' - 5;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestStringPlusNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '300' + 5;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestNumberMinusString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 - '300';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestNumberPlusString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 + '300';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestNumberGreaterString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 300 > '5';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestStringLessNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE '300' < 5;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestStringGreaterNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE '300' > 5;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestNumberLessString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 5 < '300';");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }



        [TestMethod, Timeout(1000)]
        public void TestNumberIntegers()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -200, 300, 5, 0;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");

            ExpressionOperand[] row = result.ResultSet.Row(0);
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.AreEqual(ExpressionOperandType.INTEGER, row[n].NodeType);
            }
        }



        [TestMethod, Timeout(1000)]
        public void TestNumberDecimals()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 200., 300.1, 5.182837, .0;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");

            ExpressionOperand[] row = result.ResultSet.Row(0);
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.AreEqual(ExpressionOperandType.DECIMAL, row[n].NodeType);
            }
        }

    }
}
