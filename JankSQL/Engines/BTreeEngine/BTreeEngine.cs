
namespace JankSQL.Engines
{
    public class BTreeEngine : IEngine
    {

        BTreeTable sysColumns;
        BTreeTable sysTables;

        public static BTreeEngine CreateInMemory()
        {
            return new BTreeEngine();
        }

        BTreeEngine()
        {
            sysColumns = CreateSysColumns();
            sysTables = CreateSysTables();
        }

        public void CreateTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes)
        {
            throw new NotImplementedException();
        }

        public void DropTable(FullTableName tableName)
        {
            throw new NotImplementedException();
        }

        public IEngineTable? GetEngineTable(FullTableName tableName)
        {
            throw new NotImplementedException();
        }

        public IEngineTable GetSysColumns()
        {
            throw new NotImplementedException();
        }

        public IEngineTable GetSysTables()
        {
            throw new NotImplementedException();
        }

        public void InjectTestTable(TestTable testTable)
        {
            throw new NotImplementedException();
        }

        static BTreeTable CreateSysColumns()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.NVARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.INTEGER };

            List<FullColumnName> keyNames = new();
            List<FullColumnName> valueNames = new();

            keyNames.Add(FullColumnName.FromColumnName("table_name"));
            keyNames.Add(FullColumnName.FromColumnName("column_name"));
            valueNames.Add(FullColumnName.FromColumnName("column_type"));
            valueNames.Add(FullColumnName.FromColumnName("index"));

            BTreeTable table = new BTreeTable(keyTypes, keyNames, valueTypes, valueNames);
            ExpressionOperand[] row;

            // --- columns for sys_tables
            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("table_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(1)
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("file_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(2)
            };
            table.InsertRow(row);

            // -- columns for sys_columns
            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("table_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(1)
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("column_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(2)
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("column_type"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(3)
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("index"),
                ExpressionOperand.NVARCHARFromString("INTEGER"),
                ExpressionOperand.IntegerFromInt(4)
            };
            table.InsertRow(row);

            return table;
        }

        static BTreeTable CreateSysTables()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR };

            List<FullColumnName> keyNames = new();
            List<FullColumnName> valueNames = new();

            keyNames.Add(FullColumnName.FromColumnName("table_name"));
            valueNames.Add(FullColumnName.FromColumnName("file_name"));

            BTreeTable table = new BTreeTable(keyTypes, keyNames, valueTypes, valueNames);

            ExpressionOperand[] row;

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("")
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("")
            };
            table.InsertRow(row);

            return table;
        }

    }
}

