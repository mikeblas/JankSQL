namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;
    using JankSQL.Operators;

    abstract public class InsertDeleteTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestDelete()
        {
            // delete one row
            var ecDelete = Parser.ParseSQLFileFromString("DELETE FROM [mytable] WHERE keycolumn = 2;");
            JankAssert.SuccessfulParse(ecDelete);

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultDelete, 1);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 2);
            resultSelect.ResultSet.Dump();

            int keyColIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("keycolumn"));
            List<int> keys = new ();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
                keys.Add(resultSelect.ResultSet.Row(i)[keyColIndex].AsInteger());

            Assert.That(keys, Has.Member(1), "Wrong row deleted");
            Assert.That(keys, Has.Member(3), "Wrong row deleted");
            Assert.That(keys, Has.No.Member(2), "expected row not deleted");
        }

        [Test]
        public void TestInsertThree()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");
            JankAssert.SuccessfulParse(ecCreate);

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString(
                "INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES " +
                "(1, 'moe', 100),   " +
                "(2, 'larry', 200), " +
                "(3, 'curly', 300); ");

            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select them back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM TransientTestTable;");
            JankAssert.SuccessfulParse(ecCreate);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 3, 3);
            resultSelect.ResultSet.Dump();

            int someIntegerIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("someinteger"));
            int anotherIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("anotherone"));
            List<int> someIntegers = new ();
            List<int> moreIntegers = new ();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                someIntegers.Add(resultSelect.ResultSet.Row(i)[someIntegerIndex].AsInteger());
                moreIntegers.Add(resultSelect.ResultSet.Row(i)[anotherIndex].AsInteger());
            }

            Assert.That(someIntegers, Has.Member(1));
            Assert.That(someIntegers, Has.Member(2));
            Assert.That(someIntegers, Has.Member(3));

            Assert.That(moreIntegers, Has.Member(300));
            Assert.That(moreIntegers, Has.Member(200));
            Assert.That(moreIntegers, Has.Member(100));
        }

        [Test]
        public void TestFailInsertThreeNotAllColumns()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");
            JankAssert.SuccessfulParse(ecCreate);

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            // insert some rows, but the last one doesn't have all columns
            var ecInsert = Parser.ParseSQLFileFromString(
                "INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES " +
                "(1, 'moe', 100),   " +
                "(2, 'larry', 200), " +
                "(3, 'curly'); ");

            // should've had a semantic error
            Assert.That(ecInsert.HadSemanticError, Is.True, "expected semantic error");
        }

        [Test]
        public void TestInsertOne()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");
            JankAssert.SuccessfulParse(ecCreate);

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100);");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select them back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM TransientTestTable;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 3, 1);
            resultSelect.ResultSet.Dump();

            int someIntegerIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("someinteger"));
            int anotherIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("anotherone"));
            List<int> someIntegers = new ();
            List<int> moreIntegers = new ();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                someIntegers.Add(resultSelect.ResultSet.Row(i)[someIntegerIndex].AsInteger());
                moreIntegers.Add(resultSelect.ResultSet.Row(i)[anotherIndex].AsInteger());
            }

            Assert.That(someIntegers, Has.Member(1));
            Assert.That(moreIntegers, Has.Member(100));
        }

        [Test]
        public void TestInsertExpression()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES(51+2, 'West ' + 'Hartford', 'CT', SQRT(12) * POWER(10, 4));");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select it back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable WHERE keycolumn = 53;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 1);
            resultSelect.ResultSet.Dump();

            int cityIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("city_name"));
            int stateIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_code"));
            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("population"));

            Assert.That(resultSelect.ResultSet.Row(0)[cityIndex].AsString(), Is.EqualTo("West Hartford"));
            Assert.That(resultSelect.ResultSet.Row(0)[stateIndex].AsString(), Is.EqualTo("CT"));
            Assert.That(resultSelect.ResultSet.Row(0)[popIndex].AsDouble(), Is.EqualTo(34641.016).Within(0.01));
        }

        [Test]
        public void TestInsertExpressionNull()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES(51+2, 'West ' + 'Hartford', 'CT', NULL);");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select it back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable WHERE keycolumn = 53;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 1);
            resultSelect.ResultSet.Dump();

            int cityIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("city_name"));
            int stateIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_code"));
            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("population"));

            Assert.That(resultSelect.ResultSet.Row(0)[cityIndex].AsString(), Is.EqualTo("West Hartford"));
            Assert.That(resultSelect.ResultSet.Row(0)[stateIndex].AsString(), Is.EqualTo("CT"));
            Assert.That(resultSelect.ResultSet.Row(0)[popIndex].RepresentsNull, Is.True);
        }

        [Test]
        public void TestInsertExpressionAssumedNull()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code) VALUES (51+2, 'West ' + 'Hartford', 'CT');");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select it back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable WHERE keycolumn = 53;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 1);
            resultSelect.ResultSet.Dump();

            int cityIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("city_name"));
            int stateIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_code"));
            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("population"));

            Assert.That(resultSelect.ResultSet.Row(0)[cityIndex].AsString(), Is.EqualTo("West Hartford"));
            Assert.That(resultSelect.ResultSet.Row(0)[stateIndex].AsString(), Is.EqualTo("CT"));
            Assert.That(resultSelect.ResultSet.Row(0)[popIndex].RepresentsNull, Is.True);
        }

        [Test]
        public void TestInsertExpressionAllAssumedNull()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code) VALUES (51+2, 'West ' + 'Hartford', 'CT');");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select it back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 4);
            resultSelect.ResultSet.Dump();
        }

        [Test]
        public void TestInsertExplicitNULL()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES (51+2, NULL, NULL, NULL);");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select it back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable WHERE keycolumn = 53;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 1);
            resultSelect.ResultSet.Dump();

            int cityIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("city_name"));
            int stateIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_code"));
            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("population"));

            Assert.That(resultSelect.ResultSet.Row(0)[cityIndex].RepresentsNull, Is.True);
            Assert.That(resultSelect.ResultSet.Row(0)[stateIndex].RepresentsNull, Is.True);
            Assert.That(resultSelect.ResultSet.Row(0)[popIndex].RepresentsNull, Is.True);
        }

        [Test]
        public void TestInsertNoList()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable VALUES (53, 'West Hartford', 'CT', 34641);");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select it back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM MyTable WHERE keycolumn = 53;");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 1);
            resultSelect.ResultSet.Dump();

            int cityIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("city_name"));
            int stateIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_code"));
            int popIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("population"));

            Assert.That(resultSelect.ResultSet.Row(0)[cityIndex].AsString(), Is.EqualTo("West Hartford"));
            Assert.That(resultSelect.ResultSet.Row(0)[stateIndex].AsString(), Is.EqualTo("CT"));
            Assert.That(resultSelect.ResultSet.Row(0)[popIndex].AsDouble(), Is.EqualTo(34641).Within(0.01));
        }

        [Test]
        public void TestFailInsertBadColumns()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, wrongcolumnName, state_code, population) VALUES(53, 'West Hartford', 'CT', 325743);");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(resultInsert);
        }


        [Test]
        public void TestFailRepeatedColumns()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, state_code, state_code, population) VALUES(53, 'West Hartford', 'CT', 325743);");

            Assert.That(ecInsert.HadSemanticError, Is.True, "expected semantic error");
        }

        [Test]
        public void TestFailInsertTooManyValues()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES (53, 'West Hartford', 'CT', 325743, 'Grapefruit');");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(resultInsert);
        }

        [Test]
        public void TestFailInsertTooFewValues()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES (53, 'West Hartford');");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(resultInsert);
        }

        [Test]
        public void TestFailInsertBadTableName()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO BadTableName (keycolumn, city_name) VALUES (53, 'West Hartford');");
            JankAssert.SuccessfulParse(ecInsert);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(resultInsert);
        }

        [Test]
        public void TestDeleteNoPredicate()
        {
            // delete all rows (no predicate)
            var ecDelete = Parser.ParseSQLFileFromString("DELETE FROM [mytable];");
            JankAssert.SuccessfulParse(ecDelete);

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultDelete, 3);

            Assert.That(resultDelete.ExecuteStatus, Is.EqualTo(ExecuteStatus.SUCCESSFUL), resultDelete.ErrorMessage);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 0);
        }

        [Test]
        public void TestDeleteTruePredicate()
        {
            // delete all rows (identity predicate)
            var ecDelete = Parser.ParseSQLFileFromString("DELETE FROM [mytable] WHERE  1=1;");
            JankAssert.SuccessfulParse(ecDelete);

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultDelete, 3);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");
            JankAssert.SuccessfulParse(ecSelect);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 0);
        }
    }
}

