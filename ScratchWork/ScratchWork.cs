﻿
namespace JankSQL
{

    using Tests;

    internal class ScratchWork
    {
        public static void Main()
        {
            Test2();
        }

        public static void Test2()
        {
            var engine = Engines.BTreeEngine.CreateInMemory();

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
                "SELECT Students.StudentName, Students.Score, Students.Class " +
                "  FROM Students " +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Class AND X.TopScore = Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);

            resultSelect.ResultSet.Dump();
        }
    }
}
