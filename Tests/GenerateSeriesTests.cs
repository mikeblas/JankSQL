namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class GenerateSeriesTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestGenerateSeries()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value FROM GENERATE_SERIES(1, 100)");
            // var ec = Parser.ParseSQLFileFromString("SELECT value FROM OPENXML(39, 'FOOEY');");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 100);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestGenerateSeriesAsAlias()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT Z.value FROM GENERATE_SERIES(1, 100) AS Z");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 100);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestGenerateSeriesAutoReverse()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value FROM GENERATE_SERIES(100, 1);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 100);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestGenerateSeriesStep()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value FROM GENERATE_SERIES(1, 100, 3);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 34);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestGenerateSeriesStepEmptyCrossed()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value FROM GENERATE_SERIES(100, 1, 3);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestGenerateSeriesStepEmptyStepped()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value FROM GENERATE_SERIES(1, 10, 33);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();
        }
    }
}
