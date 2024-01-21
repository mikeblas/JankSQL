namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;
    using JankSQL.Operators;

    abstract public class UpdateTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestUpdateExpression()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE MyTable SET Population = Population * 1.2;");

            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult resultUpdate = ecUpdate.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultUpdate, 3);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT population FROM MyTable;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 1, 3);
            resultSelect.ResultSet.Dump();

            HashSet<int> expected =
            [
                44400, 30000, 13800000,
            ];

            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("Population"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[popIndex].AsInteger();

                Assert.That(expected, Contains.Item(number), $"expected to find {number}");
                expected.Remove(number);
            }

            Assert.That(expected.Count, Is.Zero, "Expected all values to be found");
        }


        [Test]
        public void TestFailUpdateExpressionSource()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE MyTable SET Population = badcolumn * 1.2;");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult result = ecUpdate.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(result);
        }

        [Test]
        public void TestFailUpdateExpressionTarget()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE MyTable SET badcolumn = Population * 1.2;");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult result = ecUpdate.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(result);
        }

        [Test]
        public void TestFailUpdateExpressionBadWhere()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE MyTable SET Population = Population * 1.2 WHERE badcolumn = 392;");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult result = ecUpdate.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(result);
        }



        [Test]
        public void TestUpdateSameExpressionNoMatches()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE is_even = 1 AND SQRT(10) > 10;");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult resultUpdate = ecUpdate.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultUpdate, 0);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT is_even, number_id FROM ten;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 2, 10);
            resultSelect.ResultSet.Dump();

            int evenIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                if (number % 2 == 0)
                    Assert.That(even, Is.EqualTo(1));
                else
                    Assert.That(even, Is.EqualTo(0));
            }
        }

        [Test]
        public void TestUpdateSameExpression()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE is_even = 1;");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult resultUpdate = ecUpdate.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultUpdate, 5);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT is_even, number_id FROM ten;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 2, 10);
            resultSelect.ResultSet.Dump();

            int evenIndex= resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                if (number % 2 == 0)
                    Assert.That(even, Is.EqualTo(9));
                else
                    Assert.That(even, Is.EqualTo(0));
            }
        }

        [Test]
        public void TestUpdateSameExpressionCompound()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE is_even = 1 AND (number_name = 'four' OR number_name = 'six');");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult resultUpdate = ecUpdate.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultUpdate, 2);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT number_name, is_even, number_id FROM ten;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 3, 10);
            resultSelect.ResultSet.Dump();

            int evenIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            int nameIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_name"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                string name = resultSelect.ResultSet.Row(i)[nameIndex].AsString();

                if (name == "four" || name == "six")
                    Assert.That(even, Is.EqualTo(9));
                else
                {
                    if (number % 2 == 0)
                        Assert.That(even, Is.EqualTo(1));
                    else
                        Assert.That(even, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void TestUpdateSameNOTExpression()
        {
            var ecUpdate = Parser.ParseSQLFileFromString("UPDATE ten SET is_even = 9 WHERE NOT is_even = 1;");
            JankAssert.SuccessfulParse(ecUpdate);

            ExecuteResult resultUpdate = ecUpdate.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultUpdate, 5);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT is_even, number_id FROM ten;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 2, 10);
            resultSelect.ResultSet.Dump();

            int evenIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("is_even"));
            int numberIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_id"));
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                int number = resultSelect.ResultSet.Row(i)[numberIndex].AsInteger();
                int even = resultSelect.ResultSet.Row(i)[evenIndex].AsInteger();
                if (number % 2 == 0)
                    Assert.That(even, Is.EqualTo(1));
                else
                    Assert.That(even, Is.EqualTo(9));
            }
        }
    }
}

