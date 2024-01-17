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

            Assert.That(ecCreate, Is.Not.Null);
            Assert.That(ecCreate.TotalErrors, Is.EqualTo(0));

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString(
                "INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES " +
                "(1, 'Moe', 100),   " +
                "(2, 'Larry', 200), " +
                "(3, 'Curly', 300); ");

            Assert.That(ecInsert, Is.Not.Null);
            Assert.That(ecInsert.TotalErrors, Is.EqualTo(0));

            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulRowsAffected(resultInsert, 3);

            // truncate the table
            var ecTruncate = Parser.ParseSQLFileFromString("TRUNCATE TABLE [TransientTestTable];");

            Assert.That(ecTruncate, Is.Not.Null);
            Assert.That(ecTruncate.TotalErrors, Is.EqualTo(0));

            ExecuteResult resultTruncate = ecTruncate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultTruncate);

            // drop the table
            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE TransientTestTable;");

            Assert.That(ecDrop, Is.Not.Null);
            Assert.That(ecDrop.TotalErrors, Is.Zero);

            ExecuteResult resultDrop = ecDrop.ExecuteSingle(engine);

            JankAssert.SuccessfulWithMessageNoResultSet(resultDrop);
        }


        [Test]
        public void TestFailTruncateTableBadName()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE [BadTableName];");

            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.Zero);

            ExecuteResult result = ec.ExecuteSingle(engine);

            JankAssert.FailureWithMessage(result);
        }


        [Test]
        public void TestFailDropTableBadName()
        {
            var ec = Parser.ParseSQLFileFromString("DROP TABLE [BadTableName];");

            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.Zero);

            ExecuteResult result = ec.ExecuteSingle(engine);

            JankAssert.FailureWithMessage(result);
        }


        [Test]
        public void TestCreateDropTable()
        {
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100));");

            Assert.That(ecCreate, Is.Not.Null);
            Assert.That(ecCreate.TotalErrors, Is.Zero);

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE TransientTestTable;");

            Assert.That(ecDrop, Is.Not.Null);
            Assert.That(ecDrop.TotalErrors, Is.EqualTo(0));

            ExecuteResult resultDrop = ecDrop.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultDrop);
        }

        [Test]
        public void TestCreateIDNames()
        {
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE \"monkey\" ([name] \"INTEGER\", integer [INTEGER]);");

            Assert.That(ecCreate, Is.Not.Null);
            Assert.That(ecCreate.TotalErrors, Is.EqualTo(0));

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE \"monkey\";");

            Assert.That(ecDrop, Is.Not.Null);
            Assert.That(ecDrop.TotalErrors, Is.EqualTo(0));

            ExecuteResult resultDrop = ecDrop.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultDrop);
        }

    }
}

