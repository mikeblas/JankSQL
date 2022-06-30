
namespace JankSQL
{

    using Tests;

    internal class ScratchWork
    {
        public static void Main()
        {
            Test3();
        }

        public static void Test3()
        {
            var engine = Engines.BTreeEngine.CreateInMemory();
            TestHelpers.InjectTableKiloLeft(engine);
            TestHelpers.InjectTableKiloRight(engine);

            string select =
                "SELECT * FROM KiloLeft UNION ALL SELECT * FROM KiloRight";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ecSelect.Dump();

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);

            resultSelect.ResultSet.Dump();
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

            resultSelect.ResultSet.Dump();
        }
    }
}
