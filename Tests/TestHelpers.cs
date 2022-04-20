namespace Tests
{
    using JankSQL;
    using Engines = JankSQL.Engines;
    using static JankSQL.ExpressionOperandType;
    using System.Diagnostics;

    public class TestHelpers
    {
        static public void InjectTableMyTable(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("mytable")
                .WithColumnNames(new string[] { "keycolumn", "city_name", "state_code", "population" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, VARCHAR, VARCHAR, DECIMAL })
                .WithRow(new object[] { 1, "Monroeville", "PA",     25_000 })
                .WithRow(new object[] { 2, "Sammamish",   "WA",     37_000 })
                .WithRow(new object[] { 3, "New York",    "NY", 11_500_000 })
                .Build();

            engine.InjectTestTable(tt);
        }


        static public void InjectTableTen(Engines.IEngine engine)
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


        static public void InjectTableStates(Engines.IEngine engine)
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


        static public void InjectTableThree(Engines.IEngine engine)
        {
            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("three")
                .WithColumnNames(new string[] { "number_id", "number_name" })
                .WithColumnTypes(new ExpressionOperandType[] { DECIMAL, VARCHAR })
                .WithRow(new object[] { 1, "one"   })
                .WithRow(new object[] { 2, "two"   })
                .WithRow(new object[] { 3, "three" })
                .Build();

            engine.InjectTestTable(tt);
        }


        static public void InjectTableKeyOrdering(Engines.IEngine engine)
        {
            // order matches the description text when key is (K2, K3, K1)

            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("three")
                .WithColumnNames(new string[] { "K1", "K2", "K3", "Description" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, INTEGER, INTEGER, VARCHAR })
                .WithRow(new object[] { 0, 0, 0, "first"   })
                .WithRow(new object[] { 1, 0, 0, "second"  })
                .WithRow(new object[] { 0, 0, 1, "third"   })
                .WithRow(new object[] { 1, 0, 1, "fourth"  })
                .WithRow(new object[] { 0, 1, 0, "fifth"   })
                .WithRow(new object[] { 1, 1, 0, "sixth"   })
                .WithRow(new object[] { 0, 1, 1, "seventh" })
                .WithRow(new object[] { 1, 1, 1, "eighth"  })
                .Build();

            engine.InjectTestTable(tt);
        }


        static public void InjectTableKiloLeft(Engines.IEngine engine)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("kiloLeft")
                .WithColumnNames(new string[] { "number_id", "number_name", "is_even" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, VARCHAR, INTEGER })
                .Build();

            Engines.IEngineTable kiloLeft = engine.InjectTestTable(tt);

            for (int n = 1; n <= 1000; n++)
            {
                Tuple row = Tuple.CreateEmpty(3);
                row[0] = ExpressionOperand.IntegerFromInt(n);
                row[1] = ExpressionOperand.VARCHARFromString(IntToRoman(n));
                row[2] = ExpressionOperand.IntegerFromInt(n % 2 == 0 ? 1 : 0);
                kiloLeft.InsertRow(row);
            }

            sw.Stop();
            Console.WriteLine($"Injected table kiloLeft in {sw.Elapsed.TotalMilliseconds} ms");
        }


        static public void InjectTableKiloRight(Engines.IEngine engine)
        {
            Stopwatch sw = Stopwatch.StartNew();

            Engines.TestTable tt = Engines.TestTableBuilder.NewBuilder()
                .WithTableName("kiloRight")
                .WithColumnNames(new string[] { "number_id", "number_name", "is_even" })
                .WithColumnTypes(new ExpressionOperandType[] { INTEGER, VARCHAR, INTEGER })
                .Build();

            Engines.IEngineTable kiloLeft = engine.InjectTestTable(tt);

            for (int n = 1; n <= 1000; n++)
            {
                Tuple row = Tuple.CreateEmpty(3);
                row[0] = ExpressionOperand.IntegerFromInt(n);
                row[1] = ExpressionOperand.VARCHARFromString(IntToRoman(n));
                row[2] = ExpressionOperand.IntegerFromInt(n % 2 == 0 ? 1 : 0);
                kiloLeft.InsertRow(row);
            }

            sw.Stop();
            Console.WriteLine($"Injected table kiloRight in {sw.Elapsed.TotalMilliseconds} ms");
        }


        /// <summary>
        /// Convert an integer to a roman numeral. 
        /// Due to StackOverflow: https://stackoverflow.com/questions/71583420/c-sharp-convert-integer-into-roman-numeral-and-number-in-words
        /// </summary>
        /// <param name="num">integer to convert</param>
        /// <returns>string with roman numeral representation</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        static private string IntToRoman(int num)
        {
            if ((num < 0) || (num > 39999)) throw new ArgumentOutOfRangeException(nameof(num));
            if (num >= 10000) return "m" + IntToRoman(num - 10000);
            if (num >= 9000) return "Mm" + IntToRoman(num - 9000);
            if (num >= 5000) return "v" + IntToRoman(num - 5000);
            if (num >= 4000) return "Mv" + IntToRoman(num - 4000);
            if (num >= 1000) return "M" + IntToRoman(num - 1000);
            if (num >= 900) return "CM" + IntToRoman(num - 900);
            if (num >= 500) return "D" + IntToRoman(num - 500);
            if (num >= 400) return "CD" + IntToRoman(num - 400);
            if (num >= 100) return "C" + IntToRoman(num - 100);
            if (num >= 90) return "XC" + IntToRoman(num - 90);
            if (num >= 50) return "L" + IntToRoman(num - 50);
            if (num >= 40) return "XL" + IntToRoman(num - 40);
            if (num >= 10) return "X" + IntToRoman(num - 10);
            if (num >= 9) return "IX" + IntToRoman(num - 9);
            if (num >= 5) return "V" + IntToRoman(num - 5);
            if (num >= 4) return "IV" + IntToRoman(num - 4);
            if (num > 1) return "I" + IntToRoman(num - 1);
            if (num == 1) return "I";
            return string.Empty;
        }
    }
}

