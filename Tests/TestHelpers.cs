﻿
using JankSQL;
using Engines = JankSQL.Engines;

namespace Tests
{
    internal class TestHelpers
    {
        static internal void InjectTableMyTable(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("mytable")
                .WithColumnNames(new string[] { "keycolumn", "city_name", "state_code", "population" })
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.INTEGER, ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER })
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
                .WithColumnNames(new string[] { "number_id", "number_name" })
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.INTEGER, ExpressionOperandType.VARCHAR })
                .WithRow(new object[] { 1, "one" })
                .WithRow(new object[] { 2, "two" })
                .WithRow(new object[] { 3, "three" })
                .WithRow(new object[] { 4, "four" })
                .WithRow(new object[] { 5, "fiave" })
                .WithRow(new object[] { 6, "six" })
                .WithRow(new object[] { 7, "seven" })
                .WithRow(new object[] { 8, "eight" })
                .WithRow(new object[] { 9, "nine" })
                .WithRow(new object[] { 0, "zero" })

                .Build();

            engine.InjectTestTable(tt);
        }


        static internal void InjectTableStates(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("states")
                .WithColumnNames(new string[] { "state_code", "state_name" })
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR })
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
                .WithColumnTypes(new ExpressionOperandType[] { ExpressionOperandType.DECIMAL, ExpressionOperandType.VARCHAR })
                .WithRow(new object[] { 1, "one" })
                .WithRow(new object[] { 2, "two" })
                .WithRow(new object[] { 3, "three" })
                .Build();

            engine.InjectTestTable(tt);
        }
    }
}

