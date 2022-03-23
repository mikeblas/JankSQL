namespace JankSQL.Engines
{
    public class BTreeEngine : IEngine
    {
        private readonly BTreeTable sysColumns;
        private readonly BTreeTable sysTables;

        private readonly Dictionary<string, BTreeTable> inMemoryTables = new (StringComparer.InvariantCultureIgnoreCase);

        protected BTreeEngine()
        {
            sysColumns = CreateSysColumns();
            sysTables = CreateSysTables();
        }

        public static BTreeEngine CreateInMemory()
        {
            return new BTreeEngine();
        }

        public void CreateTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes)
        {
            if (columnNames.Count == 0)
                throw new ArgumentException("Must have at least one column name");
            if (columnNames.Count != columnTypes.Count)
                throw new ArgumentException($"Must have at types for each column; got {columnNames.Count} names and {columnTypes.Count} types");

            // create the table
            BTreeTable table = new (tableName.TableName, columnTypes.ToArray(), columnNames);

            // add a row to sys_tables
            ExpressionOperand[] tablesRow = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString(tableName.TableName),
                ExpressionOperand.NVARCHARFromString(string.Empty),
            };
            sysTables.InsertRow(tablesRow);

            // add rows for the sys_columns
            for (int i = 0; i < columnNames.Count; i++)
            {
                ExpressionOperand[] columnRow = new ExpressionOperand[]
                {
                    ExpressionOperand.NVARCHARFromString(tableName.TableName),
                    ExpressionOperand.NVARCHARFromString(columnNames[i].ColumnNameOnly()),
                    ExpressionOperand.NVARCHARFromString(columnTypes[i].ToString()), // type
                    ExpressionOperand.IntegerFromInt(i), // ordinal
                };
                sysColumns.InsertRow(columnRow);
            }

            inMemoryTables.Add(tableName.TableName, table);
        }

        public void DropTable(FullTableName tableName)
        {
            // delete the file (remove from map)
            if (!inMemoryTables.ContainsKey(tableName.TableName))
                throw new ExecutionException($"table {tableName} does not exist");

            inMemoryTables.Remove(tableName.TableName);

            // delete from sys_tables
            ExpressionOperand tableKey = ExpressionOperand.NVARCHARFromString(tableName.TableName);
            ExpressionOperandBookmark tableBookmark = new ExpressionOperandBookmark(new ExpressionOperand[] { tableKey });
            List<ExpressionOperandBookmark> tableMark = new () { tableBookmark };
            sysTables.DeleteRows(tableMark);

            // delete from sys_columns
            List<ExpressionOperandBookmark> columnRows = new ();
            int tableIndex = sysColumns.ColumnIndex("table_name");
            int columnIndex = sysColumns.ColumnIndex("column_name");

            foreach (var row in sysColumns)
            {
                if (row.RowData[tableIndex].AsString().Equals(tableName.TableName, StringComparison.InvariantCultureIgnoreCase))
                {
                    var k = new ExpressionOperand[] { row.RowData[tableIndex], row.RowData[columnIndex] };
                    ExpressionOperandBookmark columnMark = new ExpressionOperandBookmark(k);
                    columnRows.Add(columnMark);
                }
            }

            sysColumns.DeleteRows(columnRows);

            sysColumns.Dump();
            sysTables.Dump();
        }

        public IEngineTable? GetEngineTable(FullTableName tableName)
        {
            inMemoryTables.TryGetValue(tableName.TableName, out BTreeTable? table);
            return table;
        }

        public IEngineTable GetSysColumns()
        {
            return sysColumns;
        }

        public IEngineTable GetSysTables()
        {
            return sysTables;
        }

        public void InjectTestTable(TestTable testTable)
        {
            // create the table ...
            CreateTable(testTable.TableName, testTable.ColumnNames, testTable.ColumnTypes);

            // then insert each of the given rows
            // get the file name for our table
            IEngineTable? table = GetEngineTable(testTable.TableName);
            if (table == null)
            {
                throw new InvalidOperationException();
            }

            foreach (var row in testTable.Rows)
            {
                table.InsertRow(row);
            }
        }

        internal static BTreeTable CreateSysColumns()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.NVARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.INTEGER };

            List<FullColumnName> keyNames = new ();
            List<FullColumnName> valueNames = new ();

            keyNames.Add(FullColumnName.FromColumnName("table_name"));
            keyNames.Add(FullColumnName.FromColumnName("column_name"));
            valueNames.Add(FullColumnName.FromColumnName("column_type"));
            valueNames.Add(FullColumnName.FromColumnName("index"));

            BTreeTable table = new BTreeTable("sys_columns", keyTypes, keyNames, valueTypes, valueNames);
            ExpressionOperand[] row;

            // --- columns for sys_tables
            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("table_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(1),
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("file_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(2),
            };
            table.InsertRow(row);

            // -- columns for sys_columns
            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("table_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(1),
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("column_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(2),
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("column_type"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(3),
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("index"),
                ExpressionOperand.NVARCHARFromString("INTEGER"),
                ExpressionOperand.IntegerFromInt(4),
            };
            table.InsertRow(row);

            return table;
        }

        static BTreeTable CreateSysTables()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR };

            List<FullColumnName> keyNames = new ();
            List<FullColumnName> valueNames = new ();

            keyNames.Add(FullColumnName.FromColumnName("table_name"));
            valueNames.Add(FullColumnName.FromColumnName("file_name"));

            BTreeTable table = new BTreeTable("sys_tables", keyTypes, keyNames, valueTypes, valueNames);

            ExpressionOperand[] row;

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString(string.Empty),
            };
            table.InsertRow(row);

            row = new ExpressionOperand[]
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString(string.Empty),
            };
            table.InsertRow(row);

            return table;
        }

    }
}

