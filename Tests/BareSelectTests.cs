using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Assert.AreEqual(3+5, result.ResultSet.Row(0)[0].AsDouble());
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

            Assert.AreEqual(-32, result.ResultSet.Row(0)[0].AsDouble());
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

            Assert.AreEqual(-32 * -133, result.ResultSet.Row(0)[0].AsDouble());
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

            Assert.AreEqual(8, result.ResultSet.Row(0)[0].AsDouble());

            Assert.AreEqual(3 + 5, result.ResultSet.Row(0)[0].AsDouble());
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

            Assert.AreEqual("hello", result.ResultSet.Row(0)[0].AsString());
            Assert.AreEqual("goodbye", result.ResultSet.Row(0)[1].AsString());
            Assert.AreEqual("Bob's Burgers", result.ResultSet.Row(0)[2].AsString());
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

            Assert.AreEqual("Hello, world", result.ResultSet.Row(0)[0].AsString());
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
            Assert.AreEqual("Hello, world, good day!", result.ResultSet.Row(0)[0].AsString());
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

            Assert.AreEqual(295, result.ResultSet.Row(0)[0].AsDouble());

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

            Assert.AreEqual(305, result.ResultSet.Row(0)[0].AsDouble());
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

            Assert.AreEqual(-295, result.ResultSet.Row(0)[0].AsDouble());
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

            Assert.AreEqual(305, result.ResultSet.Row(0)[0].AsDouble());
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
        public void TestFunctionWhereTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE SQRT(2) < SQRT(3);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWhereFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE SQRT(2) > SQRT(3);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) > POWER(10, 3);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) < POWER(10, 3);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerConstantTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 100;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerExpressionTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 10 * 10;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerExpressionFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 327 * 5525;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerConstantFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 8675309 = POWER(10, 2);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
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
            int[] nums = { -200, 300, 5, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.AreEqual(ExpressionOperandType.INTEGER, row[n].NodeType);
                Assert.AreEqual(nums[n], row[n].AsDouble());
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
            double[] nums = { 200, 300.1, 5.182837, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.AreEqual(ExpressionOperandType.DECIMAL, row[n].NodeType);
                Assert.AreEqual(nums[n], row[n].AsDouble(), 0.00000001);
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(3, 3);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(27, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWERTimes()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(3, 3) * 2;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(54, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionSQRT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2);");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(1.41421356, result.ResultSet.Row(0)[0].AsDouble(), 0.00000001);
        }

    }
}
