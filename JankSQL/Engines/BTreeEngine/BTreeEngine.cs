namespace JankSQL.Engines
{
    using JankSQL.Expressions;

    public class BTreeEngine : IEngine
    {
        private readonly BTreeTable sysColumns;
        private readonly BTreeTable sysTables;
        private readonly BTreeTable sysIndexes;
        private readonly BTreeTable sysIndexColumns;

        private readonly Dictionary<string, BTreeTable> inMemoryTables = new (StringComparer.InvariantCultureIgnoreCase);

        protected BTreeEngine()
        {
            sysColumns = CreateSysColumns();
            sysTables = CreateSysTables();
            sysIndexes = CreateSysIndexes();
            sysIndexColumns = CreateSysIndexColumns();

            inMemoryTables["sys_tables"] = sysTables;
            inMemoryTables["sys_indexes"] = sysIndexes;
            inMemoryTables["sys_indexcolumns"] = sysIndexColumns;
            inMemoryTables["sys_columns"] = sysColumns;

            sysIndexes.Dump();
            sysIndexColumns.Dump();
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
            Tuple tablesRow = new ()
            {
                ExpressionOperand.NVARCHARFromString(tableName.TableName),
                ExpressionOperand.NVARCHARFromString(string.Empty),
            };
            sysTables.InsertRow(tablesRow);

            // add rows for the sys_columns
            for (int i = 0; i < columnNames.Count; i++)
            {
                Tuple columnRow = new ()
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

        public void CreateIndex(FullTableName tableName, string indexName, bool isUnique, List<(string columnName, bool isDescending)> columnInfos)
        {
            if (columnInfos.Count == 0)
                throw new ArgumentException("Must have at least one column in this index");

            // make sure the table exists
            BTreeTable? table = GetEngineBTreeTable(tableName);
            if (table == null)
                throw new ArgumentException($"Table {tableName} does not exist");

            // make sure the index doesn't already exist
            int tableNameIndex = sysIndexes.ColumnIndex("table_name");
            int indexNameIndex = sysIndexes.ColumnIndex("index_name");
            foreach (var row in sysIndexes)
            {
                if (row.RowData[tableNameIndex].AsString().Equals(tableName.TableName, StringComparison.OrdinalIgnoreCase) &&
                    row.RowData[indexNameIndex].AsString().Equals(indexName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ExecutionException($"index {indexName} already exists on table {tableName}");
                }
            }

            // make sure no duplicate columns in the table
            HashSet<string> columnNames = new ();
            for (int i = 0; i < columnInfos.Count; i++)
            {
                if (columnNames.Contains(columnInfos[i].columnName))
                    throw new ArgumentException($"repeated index column {columnInfos[i].columnName}");
                columnNames.Add(columnInfos[i].columnName);
            }

            // make sure those columns exist in the table
            int columnsTableNameIndex = sysColumns.ColumnIndex("table_name");
            int columnsColumnNameIndex = sysColumns.ColumnIndex("column_name");
            int columnsIndexIndex = sysColumns.ColumnIndex("index");
            Dictionary<string, int> columnNameToIndex = new (StringComparer.InvariantCultureIgnoreCase);
            foreach (var row in sysColumns)
            {
                if (row.RowData[columnsTableNameIndex].AsString().Equals(tableName.TableName, StringComparison.OrdinalIgnoreCase))
                    columnNameToIndex.Add(row.RowData[columnsColumnNameIndex].AsString(), row.RowData[columnsIndexIndex].AsInteger());
            }

            foreach (var columnName in columnNames)
            {
                if (!columnNameToIndex.ContainsKey(columnName))
                    throw new ExecutionException($"column {columnName} does not exist in table {tableName}");
            }

            // actually create the index
            table.AddIndex(indexName, isUnique, columnInfos);

            // a new row for Sysindexes about this index
            Tuple indexesRow = new ()
            {
                ExpressionOperand.NVARCHARFromString(tableName.TableName),
                ExpressionOperand.NVARCHARFromString(indexName),
                ExpressionOperand.NVARCHARFromString(isUnique ? "U" : "N"),
            };
            GetSysIndexes().InsertRow(indexesRow);
            sysIndexes.Dump();

            Tuple indexColumnsRow;
            int index = 0;
            foreach (var (columnName, isDescending) in columnInfos)
            {
                indexColumnsRow = new ()
                {
                    ExpressionOperand.NVARCHARFromString(tableName.TableName),
                    ExpressionOperand.NVARCHARFromString(indexName),
                    ExpressionOperand.IntegerFromInt(index),
                    ExpressionOperand.NVARCHARFromString(columnName),
                };
                GetSysIndexColumns().InsertRow(indexColumnsRow);
                index++;
            }

            sysIndexes.Dump();
            sysIndexColumns.Dump();
        }


        public void DropTable(FullTableName tableName)
        {
            // delete the file (remove from map)
            if (!inMemoryTables.ContainsKey(tableName.TableName))
                throw new ExecutionException($"table {tableName} does not exist");

            inMemoryTables.Remove(tableName.TableName);

            // delete from sys_tables
            ExpressionOperandBookmark tableBookmark = new (Tuple.FromSingleValue(tableName.TableName, ExpressionOperandType.NVARCHAR));
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
                    var k = Tuple.FromOperands(row.RowData[tableIndex], row.RowData[columnIndex]);
                    ExpressionOperandBookmark columnMark = new (k);
                    columnRows.Add(columnMark);
                }
            }

            sysColumns.DeleteRows(columnRows);

            sysColumns.Dump();
            sysTables.Dump();
        }

        public IEngineTable? GetEngineTable(FullTableName tableName)
        {
            return GetEngineBTreeTable(tableName);
        }

        public IEngineTable GetSysColumns()
        {
            return sysColumns;
        }

        public IEngineTable GetSysTables()
        {
            return sysTables;
        }

        public IEngineTable GetSysIndexes()
        {
            return sysIndexes;
        }

        public IEngineTable GetSysIndexColumns()
        {
            return sysIndexColumns;
        }

        public void InjectTestTable(TestTable testTable)
        {
            // create the table as a heap ...
            CreateTable(testTable.TableName, testTable.ColumnNames, testTable.ColumnTypes);

            // then insert each of the given rows
            // get the file name for our table
            IEngineTable? table = GetEngineTable(testTable.TableName);
            if (table == null)
                throw new InvalidOperationException();

            foreach (var row in testTable.Rows)
                table.InsertRow(row);
        }

        internal BTreeTable? GetEngineBTreeTable(FullTableName tableName)
        {
            inMemoryTables.TryGetValue(tableName.TableName, out BTreeTable? table);
            return table;
        }

        private static BTreeTable CreateSysColumns()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.NVARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.INTEGER };

            List<FullColumnName> keyNames = new ();
            List<FullColumnName> valueNames = new ();

            keyNames.Add(FullColumnName.FromColumnName("table_name"));
            keyNames.Add(FullColumnName.FromColumnName("column_name"));
            valueNames.Add(FullColumnName.FromColumnName("column_type"));
            valueNames.Add(FullColumnName.FromColumnName("index"));

            BTreeTable table = new ("sys_columns", keyTypes, keyNames, valueTypes, valueNames);
            Tuple row;

            // --- columns for sys_tables
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("table_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(1),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("file_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(2),
            };
            table.InsertRow(row);

            // -- columns for sys_columns
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("table_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(1),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("column_name"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(2),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("column_type"),
                ExpressionOperand.NVARCHARFromString("NVARCHAR"),
                ExpressionOperand.IntegerFromInt(3),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("index"),
                ExpressionOperand.NVARCHARFromString("INTEGER"),
                ExpressionOperand.IntegerFromInt(4),
            };
            table.InsertRow(row);

            return table;
        }

        private static BTreeTable CreateSysTables()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR };

            List<FullColumnName> keyNames = new ();
            List<FullColumnName> valueNames = new ();

            keyNames.Add(FullColumnName.FromColumnName("table_name"));
            valueNames.Add(FullColumnName.FromColumnName("file_name"));

            BTreeTable table = new ("sys_tables", keyTypes, keyNames, valueTypes, valueNames);

            Tuple row;

            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString(string.Empty),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString(string.Empty),
            };
            table.InsertRow(row);

            return table;
        }

        private static BTreeTable CreateSysIndexes()
        {
            // key is: table_name, index_name
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.NVARCHAR };

            // values are: index_type
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR };

            List<FullColumnName> keyNames = new ()
            {
                FullColumnName.FromColumnName("table_name"),
                FullColumnName.FromColumnName("index_name"),
            };
            List<FullColumnName> valueNames = new ()
            {
                FullColumnName.FromColumnName("index_type"),
            };

            BTreeTable table = new ("sys_indexes", keyTypes, keyNames, valueTypes, valueNames);

            Tuple row;

            // --- for sys_tables
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("sys_tables_pk"),
                ExpressionOperand.NVARCHARFromString("U"),
            };
            table.InsertRow(row);

            // --- for sys_columns
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("sys_columns_pk"),
                ExpressionOperand.NVARCHARFromString("U"),
            };
            table.InsertRow(row);

            // --- for sys_indexes
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexes"),
                ExpressionOperand.NVARCHARFromString("sys_indexes_pk"),
                ExpressionOperand.NVARCHARFromString("U"),
            };
            table.InsertRow(row);

            // --- for sys_columns
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.NVARCHARFromString("U"),
            };
            table.InsertRow(row);

            return table;
        }

        private static BTreeTable CreateSysIndexColumns()
        {
            // key is: table_name, index_name, index
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.NVARCHAR, ExpressionOperandType.NVARCHAR, ExpressionOperandType.INTEGER };

            // values are: column_name
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.NVARCHAR };

            List<FullColumnName> keyNames = new ()
            {
                FullColumnName.FromColumnName("table_name"),
                FullColumnName.FromColumnName("index_name"),
                FullColumnName.FromColumnName("index"),
            };
            List<FullColumnName> valueNames = new ()
            {
                FullColumnName.FromColumnName("column_name"),
            };

            BTreeTable table = new ("sys_indexcolumns", keyTypes, keyNames, valueTypes, valueNames);

            Tuple row;

            // --- for sys_tables, the key is just the table name
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_tables"),
                ExpressionOperand.NVARCHARFromString("sys_tables_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.NVARCHARFromString("table_name"),
            };
            table.InsertRow(row);

            // --- for sys_columns, the key is (table_name, column_name)
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("sys_columns_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.NVARCHARFromString("table_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_columns"),
                ExpressionOperand.NVARCHARFromString("sys_columns_pk"),
                ExpressionOperand.IntegerFromInt(2),
                ExpressionOperand.NVARCHARFromString("column_name"),
            };
            table.InsertRow(row);

            // --- for sys_indexes, the key is the table name and the index name
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexes"),
                ExpressionOperand.NVARCHARFromString("sys_indexes_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.NVARCHARFromString("table_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexes"),
                ExpressionOperand.NVARCHARFromString("sys_indexes_pk"),
                ExpressionOperand.IntegerFromInt(2),
                ExpressionOperand.NVARCHARFromString("index_name"),
            };
            table.InsertRow(row);


            // --- for sys_indexcolumns, the key is the tablename, index name, and index
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.NVARCHARFromString("table_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.IntegerFromInt(2),
                ExpressionOperand.NVARCHARFromString("index_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.NVARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.IntegerFromInt(3),
                ExpressionOperand.NVARCHARFromString("index"),
            };
            table.InsertRow(row);

            return table;
        }
    }
}

