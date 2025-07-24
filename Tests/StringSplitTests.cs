namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class StringSplitTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestStringSplit()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value FROM STRING_SPLIT('Porsche.Mercedes-Benz.Ferrari', '.')");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 3);
            result.ResultSet.Dump();

            JankAssert.StringColumnMatchesSet(result.ResultSet, 0, new HashSet<string>() { "Porsche", "Mercedes-Benz", "Ferrari" });
        }

        [Test]
        public void TestStringSplitAlias()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT Z.value FROM STRING_SPLIT('Porsche.Mercedes-Benz.Ferrari', '.') AS Z");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 3);
            result.ResultSet.Dump();

            JankAssert.StringColumnMatchesSet(result.ResultSet, 0, new HashSet<string>() { "Porsche", "Mercedes-Benz", "Ferrari" });
        }

        [Test]
        public void TestStringSplitOrdinalNoOrdinal()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM STRING_SPLIT('Porsche.Mercedes-Benz.Ferrari', '.', 0)");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 3);
            result.ResultSet.Dump();

            JankAssert.StringColumnMatchesSet(result.ResultSet, 0, new HashSet<string>() { "Porsche", "Mercedes-Benz", "Ferrari" });
        }

        [Test]
        public void TestStringSplitOrdinal()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT value, ordinal FROM STRING_SPLIT('Porsche.Mercedes-Benz.Ferrari', '.', 1)");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 3);
            result.ResultSet.Dump();

            JankAssert.StringColumnMatchesSet(result.ResultSet, 0, new HashSet<string>() { "Porsche", "Mercedes-Benz", "Ferrari" });
        }

        [Test]
        public void TestStringSplitOrdinalAlias()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT Z.value, Z.ordinal FROM STRING_SPLIT('Porsche.Mercedes-Benz.Ferrari', '.', 1) AS Z");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 3);
            result.ResultSet.Dump();

            JankAssert.StringColumnMatchesSet(result.ResultSet, 0, new HashSet<string>() { "Porsche", "Mercedes-Benz", "Ferrari" });
        }

    }
}
