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
            CreateStudentsTable();

            string select =
                "SELECT Students.StudentName, Students.Score, Students.Class " +
                "  FROM Students " +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Students.Class AND X.TopScore = Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            CheckStudentResults(resultSelect);
        }

        [Test]
        public void TestActualStudentGradesAmbiguous()
        {
            // this is expected to fail because "Class" in "X.Class = Class" is ambiguous;
            // there is also "Sutdents.Class".

            CreateStudentsTable();

            string select =
                "SELECT Students.StudentName, Students.Score, Students.Class " +
                "  FROM Students " +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Class AND X.TopScore = Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(resultSelect);
        }

        [Test]
        public void TestActualStudentGradesAliases()
        {
            CreateStudentsTable();

            string select =
                "SELECT Y.StudentName, Y.Score, Y.Class " +
                "  FROM Students Y" +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Y.Class AND X.TopScore = Y.Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            resultSelect.ResultSet.Dump();

            CheckStudentResults(resultSelect);
        }


        [Test]
        public void TestActualStudentGradesInnerAliases()
        {
            CreateStudentsTable();

            string select =
                "SELECT Students.StudentName, Students.Score, Students.Class " +
                "  FROM Students " +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students GROUP BY Class) X " +
                "    ON X.Class = Students.Class AND X.TopScore = Students.Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            resultSelect.ResultSet.Dump();

            CheckStudentResults(resultSelect);
        }

        [Test]
        public void TestActualStudentGradesInnerAggregateAliases()
        {
            CreateStudentsTable();

            string select =
                "SELECT Y.StudentName, Y.Score, Y.Class " +
                "  FROM Students Y" +
                "  JOIN ( SELECT XX.Class, MAX(XX.Score) TopScore FROM Students AS XX GROUP BY XX.Class) X " +
                "    ON X.Class = Y.Class AND X.TopScore = Y.Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            resultSelect.ResultSet.Dump();

            CheckStudentResults(resultSelect);
        }

        [Test]
        public void TestActualStudentGradesInnerAggregateAliasDefault()
        {
            CreateStudentsTable();

            string select =
                "SELECT Y.StudentName, Y.Score, Y.Class " +
                "  FROM Students Y" +
                "  JOIN ( SELECT Class, MAX(Score) TopScore FROM Students AS XX GROUP BY Class) X " +
                "    ON X.Class = Y.Class AND X.TopScore = Y.Score; ";

            var ecSelect = Parser.ParseSQLFileFromString(select);

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            resultSelect.ResultSet.Dump();

            CheckStudentResults(resultSelect);
        }


        private void CreateStudentsTable()
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
        }

        private static void CheckStudentResults(ExecuteResult resultSelect)
        {
            JankAssert.RowsetExistsWithShape(resultSelect, 3, 4);
            resultSelect.ResultSet.Dump();

            //REVIEW: probably should have made a set with records in it, but ...
            bool sawJohn = false;
            bool sawMark = false;
            bool sawBill = false;
            bool sawMaria = false;
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                string studentName = resultSelect.ResultSet.Row(i)[0].AsString();
                int studentClass = resultSelect.ResultSet.Row(i)[2].AsInteger();
                int studentScore = resultSelect.ResultSet.Row(i)[1].AsInteger();

                if (studentName == "Mark")
                {
                    Assert.That(sawMark, Is.False);
                    sawMark = true;
                    Assert.That(studentClass, Is.EqualTo(7));
                    Assert.That(studentScore, Is.EqualTo(894));
                }
                else if (studentName == "Bill")
                {
                    Assert.That(sawBill, Is.False);
                    sawBill = true;
                    Assert.That(studentClass, Is.EqualTo(7));
                    Assert.That(studentScore, Is.EqualTo(894));
                }
                else if (studentName == "Maria")
                {
                    Assert.That(sawMaria, Is.False);
                    sawMaria = true;
                    Assert.That(studentClass, Is.EqualTo(8));
                    Assert.That(studentScore, Is.EqualTo(678));

                }
                else if (studentName == "John")
                {
                    Assert.That(sawJohn, Is.False);
                    sawJohn = true;
                    Assert.That(studentClass, Is.EqualTo(9));
                    Assert.That(studentScore, Is.EqualTo(899));
                }
            }

            Assert.That(sawMark, Is.True);
            Assert.That(sawBill, Is.True);
            Assert.That(sawMaria, Is.True);
            Assert.That(sawJohn, Is.True);
        }
    }
}
