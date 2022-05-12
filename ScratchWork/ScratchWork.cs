
namespace JankSQL
{
    using JankSQL.Expressions;
    using Tests;

    internal class ScratchWork
    {
        public static void Main()
        {
            Test5();

            // Test2();
        }

        public static void Test5()
        {
            var engine = Engines.BTreeEngine.CreateInMemory();

            TestHelpers.InjectTableFiveIndexPopulated(engine);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));

            // JustOne has a single column; get it where it equals 3

            var comparisonOperators = new List<ExpressionComparisonOperator>()
            {
                new ExpressionComparisonOperator("=")
            };

            var predicate = new Expression
            {
                ExpressionOperand.IntegerFromInt(3),
            };

            List<Expression> predicates = new();
            predicates.Add(predicate);

            var idx = t!.PredicateIndex("JustOne", comparisonOperators, predicates);

            foreach (var indexRow in idx)
            {
                Tuple tableRow = t.RowFromBookmark(indexRow.Bookmark);
                Console.WriteLine($"  key = {indexRow.RowData[0]} --> {tableRow}");
            }
        }

        public static void Test2()
        {
            var engine = Engines.BTreeEngine.CreateInMemory();
            TestHelpers.InjectTableTen(engine);
            TestHelpers.InjectTableMyTable(engine);

            string creates = @"
CREATE TABLE students (
    StudentID INTEGER,
    StudentName VARCHAR(255) NOT NULL,
    score INTEGER NOT NULL,
    class INTEGER NOT NULL    
);

INSERT INTO students(StudentID, StudentName, score, class) VALUES(1, 'Mark', 894, 7);
INSERT INTO students(StudentID, StudentName, score, class) VALUES(2, 'Bill', 894, 7);
INSERT INTO students(StudentID, StudentName, score, class) VALUES(3, 'Maria', 678, 8);
INSERT INTO students(StudentID, StudentName, score, class) VALUES(4, 'David', 733, 9);
INSERT INTO students(StudentID, StudentName, score, class) VALUES(5, 'John', 899, 9);
INSERT INTO students(StudentID, StudentName, score, class) VALUES(6, 'Rob', 802, 9);";


            var ecCreate = Parser.ParseSQLFileFromString(creates);

            ExecuteResult resultCreate = ecCreate.ExecuteSingle(engine);
            JankAssert.SuccessfulWithMessageNoResultSet(resultCreate);

            /*
            string select =
                "SELECT Y.StudentName, Y.Score, Y.Class " +
                "  FROM Students Y" +
                "  JOIN ( SELECT Class, MAX(XX.Score) TopScore FROM Students AS XX GROUP BY Class) X " +
                "    ON X.Class = Y.Class AND X.TopScore = Y.Score; ";

            // select = "SELECT XX.Class, MAX(XX.Score) TopScore FROM Students AS XX GROUP BY XX.Class";

            string select =
                "SELECT Y.StudentName, Y.Score, Y.Class " +
                "  FROM Students Y " +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students XX GROUP BY Class) X " +
                "    ON X.Class = Y.Class AND X.TopScore = Y.Score; ";
            */

            string select =
                "SELECT Class, MAX(XX.Score) TopScore FROM Students AS XX GROUP BY Class";

            select =
                "SELECT XX.Class, MAX(XX.Score) TopScore FROM Students AS XX GROUP BY Class";


            select = "SELECT DATEDIFF(hour, CAST('2022-04-25 12:35' AS DATETIME), CAST('2022-04-27 16:45' AS DATETIME))";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);

            TestHelpers.InjectTableFiveIndex(engine);

            var fiveTable = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            if (fiveTable == null)
                throw new InternalErrorException("couldn't get table");

            List<(string, bool)> filterColumns = new ();
            string? str;

            filterColumns.Add(("Col1", true));
            filterColumns.Add(("Col2", true));
            str = fiveTable.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");

            filterColumns.Clear();
            filterColumns.Add(("Col1", true));
            filterColumns.Add(("Col5", true));
            str = fiveTable.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");

            filterColumns.Clear();
            filterColumns.Add(("Col3", true));
            filterColumns.Add(("Col2", true));
            str = fiveTable.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");

            filterColumns.Clear();
            filterColumns.Add(("Col4", true));
            filterColumns.Add(("Col5", true));
            filterColumns.Add(("Col1", false));
            filterColumns.Add(("Col2", false));
            str = fiveTable.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");


            resultSelect.ResultSet.Dump();
        }
    }
}
