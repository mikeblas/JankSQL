namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using JankSQL;
    using Engines = JankSQL.Engines;
    using Tuple = JankSQL.Tuple;

    public class BareSelectTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod, Timeout(1000)]
        public void TestAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(3+5, result.ResultSet.Row(0)[0].AsDouble());
        }

        [TestMethod, Timeout(1000)]
        public void TestAdditionWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3 + NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }


        [TestMethod, Timeout(1000)]
        public void TestNegativeNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(-32, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestNegativeNumberMultiply()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32 * -133;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(-32 * -133, result.ResultSet.Row(0)[0].AsDouble());
        }

        [TestMethod, Timeout(1000)]
        public void TestNegativeNumberMultiplyWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32 * NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestAdditionWhere()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(3 + 5, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestAdditionWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestAdditionWithNullWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3 + NULL WHERE 1=0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'hello', 'goodbye', 'Bob''s Burgers';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual("hello", result.ResultSet.Row(0)[0].AsString());

            Assert.IsFalse(result.ResultSet.Row(0)[1].RepresentsNull);
            Assert.AreEqual("goodbye", result.ResultSet.Row(0)[1].AsString());

            Assert.IsFalse(result.ResultSet.Row(0)[2].RepresentsNull);
            Assert.AreEqual("Bob's Burgers", result.ResultSet.Row(0)[2].AsString());
        }

        [TestMethod, Timeout(1000)]
        public void TestThreeStringsAndNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'hello', 'goodbye', NULL, 'Bob''s Burgers';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual("hello", result.ResultSet.Row(0)[0].AsString());

            Assert.IsFalse(result.ResultSet.Row(0)[1].RepresentsNull);
            Assert.AreEqual("goodbye", result.ResultSet.Row(0)[1].AsString());

            Assert.IsTrue(result.ResultSet.Row(0)[2].RepresentsNull);

            Assert.IsFalse(result.ResultSet.Row(0)[3].RepresentsNull);
            Assert.AreEqual("Bob's Burgers", result.ResultSet.Row(0)[3].AsString());
        }

        [TestMethod, Timeout(1000)]
        public void TestConcatenateTwoStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual("Hello, world", result.ResultSet.Row(0)[0].AsString());
        }

        [TestMethod, Timeout(1000)]
        public void TestConcatenateTwoStringsWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world' + NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestConcatenateNullWithStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT NULL + 'Hello' + ', world';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestAddNullWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT NULL + NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestConcatenateThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world' + ', good day!';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual("Hello, world, good day!", result.ResultSet.Row(0)[0].AsString());
        }


        [TestMethod, Timeout(1000)]
        public void TestStringMinusNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '300' - 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(295, result.ResultSet.Row(0)[0].AsDouble());
        }

        [TestMethod, Timeout(1000)]
        public void TestStringPlusNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '300' + 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(305, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestNumberMinusString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 - '300';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(-295, result.ResultSet.Row(0)[0].AsDouble());
        }

        [TestMethod, Timeout(1000)]
        public void TestNumberPlusString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 + '300';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(305, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestNumberGreaterString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 300 > '5';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestStringLessNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE '300' < 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestStringGreaterNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE '300' > 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestNumberLessString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 5 < '300';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWhereTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE SQRT(2) < SQRT(3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWhereFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE SQRT(2) > SQRT(3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) > POWER(10, 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) < POWER(10, 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerConstantTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 100;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerExpressionTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 10 * 10;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerExpressionFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 327 * 5525;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionWherePowerConstantFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 8675309 = POWER(10, 2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(0, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestNumberIntegers()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -200, 300, 5, 0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");

            Tuple row = result.ResultSet.Row(0);
            int[] nums = { -200, 300, 5, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.IsFalse(row[n].RepresentsNull);
                Assert.AreEqual(ExpressionOperandType.INTEGER, row[n].NodeType);
                Assert.AreEqual(nums[n], row[n].AsInteger());
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestNumberIntegersWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -200, 300, NULL, 5, 0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(5, result.ResultSet.ColumnCount, "column count mismatch");

            Tuple row = result.ResultSet.Row(0);
            int?[] nums = { -200, 300, null, 5, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                if (nums[n] == null)
                    Assert.IsTrue(row[n].RepresentsNull);
                else
                {
                    Assert.IsFalse(row[n].RepresentsNull);
                    Assert.AreEqual(ExpressionOperandType.INTEGER, row[n].NodeType);
                    Assert.AreEqual(nums[n], row[n].AsInteger());
                }
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestNumberDecimals()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 200., 300.1, 5.182837, .0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, result.ResultSet.ColumnCount, "column count mismatch");

            Tuple row = result.ResultSet.Row(0);
            double[] nums = { 200, 300.1, 5.182837, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.IsFalse(row[n].RepresentsNull);
                Assert.AreEqual(ExpressionOperandType.DECIMAL, row[n].NodeType);
                Assert.AreEqual(nums[n], row[n].AsDouble(), 0.00000001);
            }
        }

        [TestMethod, Timeout(1000)]
        public void TestNumberDecimalsWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 200., 300.1, NULL, 5.182837, .0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(5, result.ResultSet.ColumnCount, "column count mismatch");

            Tuple row = result.ResultSet.Row(0);
            double?[] nums = { 200, 300.1,  null, 5.182837, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                if (nums[n] == null)
                    Assert.IsTrue(row[n].RepresentsNull);
                else
                {
                    Assert.IsFalse(row[n].RepresentsNull);
                    Assert.AreEqual(ExpressionOperandType.DECIMAL, row[n].NodeType);
                    Assert.AreEqual((double) nums[n]!, row[n].AsDouble(), 0.00000001);
                }
            }
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionPI()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT PI();");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(3.1415926, result.ResultSet.Row(0)[0].AsDouble(), 0.0000001);
        }

        [TestMethod, Timeout(1000)]
        public void TestFailFunctionPIWithArg()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT PI(200);");
            Assert.IsTrue(ec.HadSemanticError, "expected a semantic error");
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(9, 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(729, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWERTimes()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(9, 3) * 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(729 * 2, result.ResultSet.Row(0)[0].AsDouble());
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWERNullTimes()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(NULL, 3) * 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWERTimesNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(9, 3) * NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionSQRT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(1.41421356, result.ResultSet.Row(0)[0].AsDouble(), 0.00000001);
        }

        [TestMethod, Timeout(1000)]
        public void TestNegateFunctionSQRT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT - SQRT(2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(-1.41421356, result.ResultSet.Row(0)[0].AsDouble(), 0.00000001);
        }


        [TestMethod, Timeout(1000)]
        public void TestFunctionSQRTNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(NULL);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionPOWERFunction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(POWER(3, 2), 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(729, result.ResultSet.Row(0)[0].AsDouble());
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionFunctionPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(10, POWER(2, 3));");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(100000000, result.ResultSet.Row(0)[0].AsDouble());
        }

        [TestMethod, Timeout(1000)]
        public void TestFunctionSqrtPiPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(SQRT(PI()), 2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(3.1415926, result.ResultSet.Row(0)[0].AsDouble(), 0.0000001);
        }

        [TestMethod, Timeout(1000)]
        public void TestFailMissingOperator()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3 5;");
            Assert.IsTrue(ec.TotalErrors > 0, "Expected an error");
        }


        [TestMethod, Timeout(1000)]
        public void TestFailMissingOperand()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+;");
            Assert.IsTrue(ec.TotalErrors > 0, "Expected an error");
        }

        [TestMethod, Timeout(1000)]
        public void TestFailMissingFunctionParameter()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(2)");
            Assert.IsTrue(ec.HadSemanticError, "expected semantic error");
        }

        [TestMethod, Timeout(1000)]
        public void TestFailBogusFunctionName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT BOGUS(2)");
            Assert.IsTrue(ec.HadSemanticError, "expected semantic error");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectIsNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, 35);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(35, result.ResultSet.Row(0)[0].AsInteger());
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectIsNullNotNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(93, 35);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(93, result.ResultSet.Row(0)[0].AsInteger());
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectIsNullNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, NULL);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectIsNullFunction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, POWER(10, 3));");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(1000, result.ResultSet.Row(0)[0].AsInteger());
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectIsNullExpression()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, 250 + 10 - 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            Assert.AreEqual(250 + 10 - 3, result.ResultSet.Row(0)[0].AsInteger());
        }

    }
}
