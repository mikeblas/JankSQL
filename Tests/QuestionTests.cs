namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class QuestionTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestActualStudentGrades()
        {
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

            string select =
                "SELECT Students.StudentName, Students.Score, Students.Class "+
                "  FROM Students " +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Class AND X.TopScore = Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);

            resultSelect.ResultSet.Dump();

        }


        [Test]
        public void TestActualStudentGradesAliases()
        {
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

            string select =
                "SELECT Y.StudentName, Y.Score, Y.Class " +
                "  FROM Students Y" +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Y.Class AND X.TopScore = Y.Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);

            resultSelect.ResultSet.Dump();

        }

    }
}
