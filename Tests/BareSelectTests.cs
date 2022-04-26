namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;
    using Tuple = JankSQL.Tuple;

    abstract public class BareSelectTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 3 + 5);
        }


        [Test]
        public void TestAdditionBinds()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT @Left + @Right;");

            for (int left = -5; left <= 5; left++)
            {
                for (int right = -5; right <= 5; right++)
                {
                    ec.SetBindValue("@Left", left);
                    ec.SetBindValue("@Right", right);

                    ExecuteResult result = ec.ExecuteSingle(engine);

                    JankAssert.RowsetExistsWithShape(result, 1, 1);

                    JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, left + right);
                }
            }
        }


        [Test]
        public void TestMultiplicationBinds()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT @Left * @Right;");

            for (int left = -60; left <= 60; left += 3)
            {
                for (int right = -50; right <= 50; right += 5)
                {
                    ec.SetBindValue("@Left", left);
                    ec.SetBindValue("@Right", right);

                    ExecuteResult result = ec.ExecuteSingle(engine);

                    JankAssert.RowsetExistsWithShape(result, 1, 1);

                    JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, left * right);
                }
            }
        }

        [Test]
        public void TestAdditionWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3 + NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }


        [Test]
        public void TestJanapeseString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '〒105-0011 東京都港区芝公園４丁目２−8';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "〒105-0011 東京都港区芝公園４丁目２−8");
        }

        [Test]
        public void TestJanapeseNString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'〒105-0011 東京都港区芝公園４丁目２−8';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "〒105-0011 東京都港区芝公園４丁目２−8");
        }

        [Test]
        public void TestNegativeNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, -32);
        }


        [Test]
        public void TestNegativeNumberMultiply()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32 * -133;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, -32 * -133);
        }

        [Test]
        public void TestNegativeNumberMultiplyWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32 * NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestAdditionWhere()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 8);
        }


        [Test]
        public void TestAdditionWhereBinds()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT @Left + @Right WHERE 1=@Flag;");

            int flag = 0;
            for (int left = -5; left <= 5; left++)
            {
                for (int right = -5; right <= 5; right++)
                {
                    flag = (flag + 1) % 3;

                    ec.SetBindValue("@Left", left);
                    ec.SetBindValue("@Right", right);
                    ec.SetBindValue("@Flag", flag);

                    ExecuteResult result = ec.ExecuteSingle(engine);

                    if (flag == 1)
                    {
                        JankAssert.RowsetExistsWithShape(result, 1, 1);
                        JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, left + right);
                    }
                    else
                        JankAssert.RowsetExistsWithShape(result, 1, 0);
                }
            }
        }


        [Test]
        public void TestAdditionWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestAdditionWithNullWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3 + NULL WHERE 1=1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'hello', 'goodbye', 'Bob''s Burgers';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "hello");
            JankAssert.ValueMatchesString(result.ResultSet, 1, 0, "goodbye");
            JankAssert.ValueMatchesString(result.ResultSet, 2, 0, "Bob's Burgers");
        }

        [Test]
        public void TestThreeStringsAndNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'hello', 'goodbye', NULL, 'Bob''s Burgers';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "hello");
            JankAssert.ValueMatchesString(result.ResultSet, 1, 0, "goodbye");
            JankAssert.ValueMatchesString(result.ResultSet, 3, 0, "Bob's Burgers");

            JankAssert.ValueIsNull(result.ResultSet, 2, 0);
        }

        [Test]
        public void TestConcatenateTwoStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Hello, world");
        }

        [Test]
        public void TestConcatenateTwoStringsWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world' + NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestConcatenateNullWithStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT NULL + 'Hello' + ', world';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestAddNullWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT NULL + NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestConcatenateThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world' + ', good day!';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Hello, world, good day!");
        }


        [Test]
        public void TestStringMinusNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '300' - 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 295);
        }

        [Test]
        public void TestStringPlusNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT '300' + 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 305);
        }


        [Test]
        public void TestNumberMinusString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 - '300';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, -295);
        }

        [Test]
        public void TestNumberPlusString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 + '300';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 305);
        }


        [Test]
        public void TestGreaterThan()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 5 > 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }

        [Test]
        public void TestLessThan()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 2 < 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestNumberGreaterString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 300 > '5';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestStringLessNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE '300' < 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestStringGreaterNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE '300' > 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestNumberLessString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 5 < '300';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestFunctionWhereTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE SQRT(2) < SQRT(3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestFunctionWhereFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE SQRT(2) > SQRT(3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestFunctionWherePowerFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) > POWER(10, 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestFunctionWherePowerTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) < POWER(10, 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestFunctionWherePowerConstantTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 100;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }

        [Test]
        public void TestFunctionWherePowerExpressionTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 10 * 10;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }

        [Test]
        public void TestFunctionWherePowerExpressionFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE POWER(10, 2) = 327 * 5525;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestFunctionWherePowerConstantFalse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE 8675309 = POWER(10, 2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestNumberIntegers()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -200, 300, 5, 0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();

            int[] nums = { -200, 300, 5, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
                JankAssert.ValueMatchesInteger(result.ResultSet, n, 0, nums[n]);
        }

        [Test]
        public void TestNumberIntegersWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -200, 300, NULL, 5, 0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 5, 1);
            result.ResultSet.Dump();

            Tuple row = result.ResultSet.Row(0);
            int?[] nums = { -200, 300, null, 5, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                if (nums[n] == null)
                    Assert.IsTrue(row[n].RepresentsNull);
                else
                    JankAssert.ValueMatchesInteger(result.ResultSet, n, 0, (int)nums[n]!);
            }
        }

        [Test]
        public void TestNumberDecimals()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 200., 300.1, 5.182837, .0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();

            Tuple row = result.ResultSet.Row(0);
            double[] nums = { 200, 300.1, 5.182837, 0 };
            for (int n = 0; n < result.ResultSet.ColumnCount; n++)
            {
                Assert.IsFalse(row[n].RepresentsNull);
                Assert.AreEqual(ExpressionOperandType.DECIMAL, row[n].NodeType);
                Assert.AreEqual(nums[n], row[n].AsDouble(), 0.00000001);
            }
        }

        [Test]
        public void TestNumberDecimalsWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 200., 300.1, NULL, 5.182837, .0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 5, 1);
            result.ResultSet.Dump();

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


        [Test]
        public void TestFunctionPI()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT PI();");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, 3.1415926, 0.0000001);
        }

        [Test]
        public void TestFailFunctionPIWithArg()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT PI(200);");
            Assert.IsTrue(ec.HadSemanticError, "expected a semantic error");
        }

        [Test]
        public void TestFunctionPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(9, 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 729);
        }


        [Test]
        public void TestFunctionPOWERTimes()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(9, 3) * 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 729 * 2);
        }


        [Test]
        public void TestFunctionPOWERNullTimes()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(NULL, 3) * 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [Test]
        public void TestFunctionPOWERTimesNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(9, 3) * NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [Test]
        public void TestFunctionSQRT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, 1.41421356, 0.00000001);
        }

        [Test]
        public void TestNegateFunctionSQRT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT - SQRT(2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, -1.41421356, 0.00000001);
        }


        [Test]
        public void TestFunctionSQRTNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(NULL);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [Test]
        public void TestFunctionPOWERFunction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(POWER(3, 2), 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 729);
        }

        [Test]
        public void TestFunctionFunctionPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(10, POWER(2, 3));");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 100_000_000);
        }

        [Test]
        public void TestFunctionSqrtPiPOWER()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(SQRT(PI()), 2);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, 3.1415926, 0.0000001);
        }

        [Test]
        public void TestFailMissingOperator()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3 5;");
            Assert.IsTrue(ec.TotalErrors > 0, "Expected an error");
        }


        [Test]
        public void TestFailMissingOperand()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+;");
            Assert.IsTrue(ec.TotalErrors > 0, "Expected an error");
        }

        [Test]
        public void TestFailMissingFunctionParameter()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(2)");
            Assert.IsTrue(ec.HadSemanticError, "expected semantic error");
        }

        [Test]
        public void TestFailBogusFunctionName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT BOGUS(2)");
            Assert.IsTrue(ec.HadSemanticError, "expected semantic error");
        }

        [Test]
        public void TestSelectIsNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, 35);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 35);
        }

        [Test]
        public void TestSelectIsNullNotNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(93, 35);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 93);
        }

        [Test]
        public void TestSelectIsNullNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, NULL);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }


        [Test]
        public void TestSelectIsNullFunction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, POWER(10, 3));");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 1000);
        }


        [Test]
        public void TestSelectIsNullExpression()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT ISNULL(NULL, 250 + 10 - 3);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 250 + 10 - 3);
        }

        [Test]
        public void TestModulo()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 17 % 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 17 % 5);
        }

        [Test]
        public void TestModuloDoubles()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 17.5 % 5.1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, 17.5 % 5.1, 0.00001);
        }


        [Test]
        public void TestModuloNegativeLeft()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -17 % 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, -17 % 5);
        }

        [Test]
        public void TestModuloNegativeRight()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 17 % -5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 17 % -5);
        }

        [Test]
        public void TestModuloString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 17 % '-5';");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 17 % -5);
        }

        [Test]
        public void TestModuloNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 17 % NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [Test]
        public void TestDivision()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 30/10;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 30 / 10);
        }

        [Test]
        public void TestDivisionWithNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 30/NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsTrue(result.ResultSet.Row(0)[0].RepresentsNull);
        }

        [Test]
        public void TestIsNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE NULL IS NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestIsNotNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Yes' WHERE NULL IS NOT NULL;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestIIFTrue()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT IIF(3 = 3, 'Yes', 'No');");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }

        [Test]
        public void TestIIFTrueExpression()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT IIF(POWER(10, 2) = 100, 'Yes', 'No');");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "Yes");
        }


        [Test]
        public void TestIIFFalse ()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT IIF(3 = 5, 'Yes', 'No');");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "No");
        }

        [Test]
        public void TestIIFFalseExpression()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT IIF(POWER(10, 2) = 333, 'Yes', 'No');");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "No");
        }

        [Test]
        public void TestLenString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT LEN('hello');");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 5);
        }


        [Test]
        public void TestLenStringConcat()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT LEN('hello' + 'world');");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 10);
        }


        [Test]
        public void TestLenInteger()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT LEN(325);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 3);
        }

        [Test]
        public void TestLenDecimal()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT LEN(32.3423);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 7);
        }

        [Test]
        public void TestLenNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT LEN(NULL);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestDateTimeFromString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('1963-11-22 12:30:00' AS DATETIME);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 22, 12, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeFromStringIntegerAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('1963-11-22 12:30:00' AS DATETIME) + 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 27, 12, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeIntegerAdditionFromString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5 + CAST('1963-11-22 12:30:00' AS DATETIME);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 27, 12, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeFromStringDecimalAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('1963-11-22 12:30:00' AS DATETIME) + 5.3;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 27, 19, 42, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeDecimalAdditionFromString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 5.3 + CAST('1963-11-22 12:30:00' AS DATETIME);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 27, 19, 42, 0, DateTimeKind.Utc));
        }


        // ---

        [Test]
        public void TestDateTimeFromStringIntegerSubtraction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('1963-11-22 12:30:00' AS DATETIME) - 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 17, 12, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeIntegerSubtractionFromString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 1050000 - CAST('1963-11-22 12:30:00' AS DATETIME);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(912, 11, 30, 11, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeFromStringDecimalSubtraction()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('1963-11-22 12:30:00' AS DATETIME) - 5.3;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1963, 11, 17, 5, 18, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateTimeDecimalSubtractionFromString()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 1050000.3 - CAST('1963-11-22 12:30:00' AS DATETIME);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(912, 11, 30, 18, 42, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateAddDays()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT DATEADD(day, 1, CAST('1900-12-31 13:22' AS DATETIME))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1901, 1, 1, 13, 22, 0, DateTimeKind.Utc));
        }

        [Test]
        public void TestDateAddHours()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT DATEADD(hour, 1, CAST('1900-12-31 13:22' AS DATETIME))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1900, 12, 31, 14, 22, 0, DateTimeKind.Utc));
        }


        [Test]
        public void TestDateAddYears()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT DATEADD(year, 1, CAST('1900-12-31 13:22' AS DATETIME))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1901, 12, 31, 13, 22, 0, DateTimeKind.Utc));
        }


        [Test]
        public void TestDateAddMonths()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT DATEADD(month, 1, CAST('1900-12-31 13:22' AS DATETIME))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDateTime(result.ResultSet, 0, 0, new DateTime(1901, 1, 31, 13, 22, 0, DateTimeKind.Utc));
        }
    }
}
