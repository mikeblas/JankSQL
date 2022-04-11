namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

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

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultDelete.ExecuteStatus, resultDelete.ErrorMessage);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 2);
            resultSelect.ResultSet.Dump();

            int keyColIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("keycolumn"));
            List<int> keys = new ();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
                keys.Add(resultSelect.ResultSet.Row(i)[keyColIndex].AsInteger());

            Assert.IsTrue(keys.Contains(1), "Wrong row deleted");
            Assert.IsTrue(keys.Contains(3), "Wrong row deleted");
            Assert.IsFalse(keys.Contains(2), "expected row not deleted");
        }

        [Test]
        public void TestInsertThree()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.NotNull(resultsCreate.ErrorMessage);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100), (2, 'larry', 200), (3, 'curly', 300);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select them back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM TransientTestTable;");

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

            Assert.IsTrue(someIntegers.Contains(1));
            Assert.IsTrue(someIntegers.Contains(2));
            Assert.IsTrue(someIntegers.Contains(3));

            Assert.IsTrue(moreIntegers.Contains(300));
            Assert.IsTrue(moreIntegers.Contains(200));
            Assert.IsTrue(moreIntegers.Contains(100));
        }

        [Test]
        public void TestFailInsertThreeNotAllColumns()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.NotNull(resultsCreate.ErrorMessage);

            // insert some rows, but the last one doesn't have all columns
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100), (2, 'larry', 200), (3, 'curly');");

            // should've had a semantic error
            Assert.IsTrue(ecInsert.HadSemanticError, "expected semantic error");
        }

        [Test]
        public void TestInsertOne()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.NotNull(resultsCreate.ErrorMessage);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultsInsert);

            // select them back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM TransientTestTable;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 3, 1);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
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

            Assert.IsTrue(someIntegers.Contains(1));
            Assert.IsTrue(moreIntegers.Contains(100));
        }

        [Test]
        public void TestInsertExpression()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES(51+2, 'West ' + 'Hartford', 'CT', SQRT(12) * POWER(10, 4));");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

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

            Assert.AreEqual("West Hartford", resultSelect.ResultSet.Row(0)[cityIndex].AsString());
            Assert.AreEqual("CT", resultSelect.ResultSet.Row(0)[stateIndex].AsString());
            Assert.AreEqual(34641.016, resultSelect.ResultSet.Row(0)[popIndex].AsDouble(), 0.01);
        }

        [Test]
        public void TestInsertExpressionNull()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES(51+2, 'West ' + 'Hartford', 'CT', NULL);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

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


            Assert.AreEqual("West Hartford", resultSelect.ResultSet.Row(0)[cityIndex].AsString());
            Assert.AreEqual("CT", resultSelect.ResultSet.Row(0)[stateIndex].AsString());
            Assert.IsTrue(resultSelect.ResultSet.Row(0)[popIndex].RepresentsNull);
        }

        [Test]
        public void TestInsertExpressionAssumedNull()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code) VALUES (51+2, 'West ' + 'Hartford', 'CT');");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

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


            Assert.AreEqual("West Hartford", resultSelect.ResultSet.Row(0)[cityIndex].AsString());
            Assert.AreEqual("CT", resultSelect.ResultSet.Row(0)[stateIndex].AsString());
            Assert.IsTrue(resultSelect.ResultSet.Row(0)[popIndex].RepresentsNull);
        }

        [Test]
        public void TestInsertExpressionAllAssumedNull()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code) VALUES (51+2, 'West ' + 'Hartford', 'CT');");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

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

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

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

            Assert.IsTrue(resultSelect.ResultSet.Row(0)[cityIndex].RepresentsNull);
            Assert.IsTrue(resultSelect.ResultSet.Row(0)[stateIndex].RepresentsNull);
            Assert.IsTrue(resultSelect.ResultSet.Row(0)[popIndex].RepresentsNull);
        }

        [Test]
        public void TestInsertNoList()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable VALUES (53, 'West Hartford', 'CT', 34641);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

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

            Assert.AreEqual("West Hartford", resultSelect.ResultSet.Row(0)[cityIndex].AsString());
            Assert.AreEqual("CT", resultSelect.ResultSet.Row(0)[stateIndex].AsString());
            Assert.AreEqual(34641, resultSelect.ResultSet.Row(0)[popIndex].AsDouble(), 0.01);
        }

        [Test]
        public void TestFailInsertBadColumns()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, wrongcolumnName, state_code, population) VALUES(53, 'West Hartford', 'CT', 325743);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.FAILED, resultInsert.ExecuteStatus);
        }


        [Test]
        public void TestFailRepeatedColumns()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, state_code, state_code, population) VALUES(53, 'West Hartford', 'CT', 325743);");

            Assert.IsTrue(ecInsert.HadSemanticError, "expected semantic error");
        }

        [Test]
        public void TestFailInsertTooManyValues()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES (53, 'West Hartford', 'CT', 325743, 'Grapefruit');");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.FAILED, resultInsert.ExecuteStatus);
        }

        [Test]
        public void TestFailInsertTooFewValues()
        {
            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO MyTable (keycolumn, city_name, state_code, population) VALUES (53, 'West Hartford');");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.FAILED, resultInsert.ExecuteStatus);
        }


        [Test]
        public void TestDeleteNoPredicate()
        {
            // delete all rows (no predicate)
            var ecDelete = Parser.ParseSQLFileFromString("DELETE FROM [mytable];");

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultDelete.ExecuteStatus, resultDelete.ErrorMessage);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 0);
        }

        [Test]
        public void TestDeleteTruePredicate()
        {
            // delete all rows (identity predicate)
            var ecDelete = Parser.ParseSQLFileFromString("DELETE FROM [mytable] WHERE  1=1;");

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultDelete.ExecuteStatus, resultDelete.ErrorMessage);

            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 4, 0);
        }
    }
}

