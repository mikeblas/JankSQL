
using JankSQL;
using Engines = JankSQL.Engines;

namespace Tests
{
    internal class TestHelpers
    {
        static internal void InjectMyTable(Engines.IEngine engine)
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
    }
}
