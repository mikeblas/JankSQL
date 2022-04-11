namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class DDLTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestCreateInsertTruncateDropTable()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultCreate.ExecuteStatus, resultCreate.ErrorMessage);
            Assert.NotNull(resultCreate.ErrorMessage);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString(
                "INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES " +
                "(1, 'moe', 100),   " +
                "(2, 'larry', 200), " + 
                "(3, 'curly', 300); ");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsInsert.ExecuteStatus, resultsInsert.ErrorMessage);
            Assert.Throws<InvalidOperationException>(() => { var x = resultsInsert.ResultSet; } );

            // truncate the table
            var ecTruncate = Parser.ParseSQLFileFromString("TRUNCATE TABLE [TransientTestTable];");

            Assert.IsNotNull(ecTruncate);
            Assert.AreEqual(0, ecTruncate.TotalErrors);

            ExecuteResult resultTruncate = ecTruncate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultTruncate.ExecuteStatus, resultTruncate.ErrorMessage);
            Assert.NotNull(resultTruncate.ErrorMessage);

            // drop the table
            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE TransientTestTable;");

            Assert.IsNotNull(ecDrop);
            Assert.AreEqual(0, ecDrop.TotalErrors);

            ExecuteResult resultDrop = ecDrop.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultDrop.ExecuteStatus, resultDrop.ErrorMessage);
            Assert.NotNull(resultDrop.ErrorMessage);
        }


        [Test]
        public void TestTruncateTableBadName()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE [BadTableName];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }


        [Test]
        public void TestFailDropTableBadName()
        {
            var ec = Parser.ParseSQLFileFromString("DROP TABLE [BadTableName];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }


        [Test]
        public void TestCreateDropTable()
        {
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100));");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.NotNull(resultsCreate.ErrorMessage);

            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE TransientTestTable;");

            Assert.IsNotNull(ecDrop);
            Assert.AreEqual(0, ecDrop.TotalErrors);

            ExecuteResult resultsDrop = ecDrop.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultsDrop.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.NotNull(resultsDrop.ErrorMessage);
        }
    }
}

