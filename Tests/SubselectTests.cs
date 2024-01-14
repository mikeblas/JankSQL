namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class SubselectTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestLessThanSubselect()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id " +
                "  FROM ten " +
                " WHERE number_id < (SELECT MAX(keycolumn) FROM mytable);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 3);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 0, 1, 2 });
        }


        [Test]
        public void TestLessThanSubselectWhere()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id " +
                "  FROM ten " +
                " WHERE number_id < (SELECT MAX(keycolumn) FROM mytable WHERE ten.is_even = 0);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 1);
        }


        [Test]
        public void TestLessThanSubselectWhereCompound()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id " +
                "  FROM ten " +
                " WHERE number_id < (SELECT MAX(keycolumn) FROM mytable WHERE ten.is_even = 0 AND keycolumn = 3);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 1);
        }


        [Test]
        public void TestWhereInList()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id from ten WHERE number_id IN (3, 5, 7);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 3);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 3, 5, 7 });
        }

        [Test]
        public void TestWhereInListSelfRefColumn()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id from ten WHERE number_id IN (3, 5, 7, 1 + is_even);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 5);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 3, 5, 7, 1, 2 });
        }

        [Test]
        public void TestWhereNotInList()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id from ten WHERE number_id NOT IN (3, 5, 7);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 7);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 0, 1, 2, 4, 6, 8, 9 });
        }

        [Test]
        public void TestWhereInSubselect()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id FROM ten WHERE number_id IN (SELECT keycolumn FROM mytable);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 3);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 1, 2, 3 });
        }

        [Test]
        public void TestWhereNotInSubselect()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id FROM ten WHERE number_id NOT IN (SELECT keycolumn FROM mytable);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 7);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 0, 4, 5, 6, 7, 8, 9 });
        }

        [Test]
        public void TestNestedSevenTimes()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * FROM " +
                "( " +
                "    SELECT * FROM " +
                "    ( " +
                "        SELECT * FROM " +
                "        ( " +
                "            SELECT * FROM " +
                "            ( " +
                "                SELECT * FROM " +
                "                ( " +
                "                    SELECT * FROM " +
                "                    ( " +
                "                        SELECT * FROM " +
                "                        ( " +
                "                            SELECT number_id " +
                "                            FROM ten " +
                "                            ORDER BY number_id DESC " +
                "                        ) AS T1 " +
                "                    ) AS T2 " +
                "                ) AS T3 " +
                "            ) AS T4 " +
                "        ) AS T5 " +
                "    ) AS T6 " +
                ") AS T7");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            JankAssert.IntegerColumnMatchesSet(result.ResultSet, 0, new HashSet<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }
    }
}
