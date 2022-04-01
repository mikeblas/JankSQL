

namespace Tests
{
    using JankSQL;
    using Engines = JankSQL.Engines;
    using static JankSQL.ExpressionOperandType;

    internal class TestHelpers
    {
        static internal void InjectTableMyTable(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("mytable")
                .WithColumnNames(new string[] { "keycolumn", "city_name", "state_code", "population" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, VARCHAR, VARCHAR, DECIMAL })
                .WithRow(new object[] { 1, "Monroeville", "PA", 25000 })
                .WithRow(new object[] { 2, "Sammamish", "WA", 37000 })
                .WithRow(new object[] { 3, "New York", "NY", 11500000 })
                .Build();

            engine.InjectTestTable(tt);
        }


        static internal void InjectTableTen(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("ten")
                .WithColumnNames(new string[] { "number_id", "number_name", "is_even" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, VARCHAR, INTEGER })
                .WithRow(new object[] { 1, "one",   0 })
                .WithRow(new object[] { 2, "two",   1 })
                .WithRow(new object[] { 3, "three", 0 })
                .WithRow(new object[] { 4, "four",  1 })
                .WithRow(new object[] { 5, "five",  0 })
                .WithRow(new object[] { 6, "six",   1 })
                .WithRow(new object[] { 7, "seven", 0 })
                .WithRow(new object[] { 8, "eight", 1 })
                .WithRow(new object[] { 9, "nine",  0 })
                .WithRow(new object[] { 0, "zero",  1 })
                .Build();

            engine.InjectTestTable(tt);
        }


        static internal void InjectTableStates(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("states")
                .WithColumnNames(new string[] { "state_code", "state_name" })
                .WithColumnTypes(new ExpressionOperandType[] { VARCHAR, VARCHAR })
                .WithRow(new object[] { "PA", "Pennsylvania" })
                .WithRow(new object[] { "AK", "Arkansas" })
                .WithRow(new object[] { "HI", "Hawaii" })
                .WithRow(new object[] { "WA", "Washington" })
                .WithRow(new object[] { "MA", "Massachusetts" })
                .WithRow(new object[] { "CT", "Connecticut" })
                .WithRow(new object[] { "NY", "New York" })
                .WithRow(new object[] { "VT", "Vermont" })
                .Build();

            engine.InjectTestTable(tt);
        }


        static internal void InjectTableThree(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("three")
                .WithColumnNames(new string[] { "number_id", "number_name" })
                .WithColumnTypes(new ExpressionOperandType[] { DECIMAL, VARCHAR })
                .WithRow(new object[] { 1, "one" })
                .WithRow(new object[] { 2, "two" })
                .WithRow(new object[] { 3, "three" })
                .Build();

            engine.InjectTestTable(tt);
        }


        static internal void InjectTableKeyOrdering(Engines.IEngine engine)
        {
            // order matches the description text when key is (K2, K3, K1)

            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("three")
                .WithColumnNames(new string[] { "K1", "K2", "K3", "Description" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, INTEGER, INTEGER, VARCHAR })
                .WithRow(new object[] { 0, 0, 0, "first" })
                .WithRow(new object[] { 1, 0, 0, "second" })
                .WithRow(new object[] { 0, 0, 1, "third" })
                .WithRow(new object[] { 1, 0, 1, "fourth" })
                .WithRow(new object[] { 0, 1, 0, "fifth" })
                .WithRow(new object[] { 1, 1, 0, "sixth" })
                .WithRow(new object[] { 0, 1, 1, "seventh" })
                .WithRow(new object[] { 1, 1, 1, "eighth" })
                .Build();

            engine.InjectTestTable(tt);
        }
    }
}

