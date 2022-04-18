namespace JankSQL.Engines
{
    using System.Collections.Immutable;

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

        public void CreateTable(FullTableName tableName, IImmutableList<FullColumnName> columnNames, IImmutableList<ExpressionOperandType> columnTypes)
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
                ExpressionOperand.VARCHARFromString(tableName.TableName),
                ExpressionOperand.VARCHARFromString(string.Empty),
            };
            sysTables.InsertRow(tablesRow);

            // add rows for the sys_columns
            for (int nameIndex = 0; nameIndex < columnNames.Count; nameIndex++)
            {
                Tuple columnRow = new ()
                {
                    ExpressionOperand.VARCHARFromString(tableName.TableName),
                    ExpressionOperand.VARCHARFromString(columnNames[nameIndex].ColumnNameOnly()),
                    ExpressionOperand.VARCHARFromString(columnTypes[nameIndex].ToString()), // type
                    ExpressionOperand.IntegerFromInt(nameIndex), // ordinal
                };

                sysColumns.InsertRow(columnRow);
            }


            inMemoryTables.Add(tableName.TableName, table);
        }

        public void CreateIndex(FullTableName tableName, string indexName, bool isUnique, IEnumerable<(string columnName, bool isDescending)> columnInfos)
        {
            // make sure no duplicate columns in the index
            HashSet<string> columnNames = new ();
            foreach (var (columnName, isDescending) in columnInfos)
            {
                if (columnNames.Contains(columnName))
                    throw new ArgumentException($"repeated index column {columnName}");
                columnNames.Add(columnName);
            }

            // we did get a non-empty list, right?
            if (columnNames.Count == 0)
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
                ExpressionOperand.VARCHARFromString(tableName.TableName),
                ExpressionOperand.VARCHARFromString(indexName),
                ExpressionOperand.VARCHARFromString(isUnique ? "U" : "N"),
            };
            GetSysIndexes().InsertRow(indexesRow);
            sysIndexes.Dump();

            Tuple indexColumnsRow;
            int index = 0;
            foreach (var (columnName, isDescending) in columnInfos)
            {
                indexColumnsRow = new ()
                {
                    ExpressionOperand.VARCHARFromString(tableName.TableName),
                    ExpressionOperand.VARCHARFromString(indexName),
                    ExpressionOperand.IntegerFromInt(index),
                    ExpressionOperand.VARCHARFromString(columnName),
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
            ExpressionOperandBookmark tableBookmark = new (Tuple.FromSingleValue(tableName.TableName, ExpressionOperandType.VARCHAR));
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

        public IEngineTable InjectTestTable(TestTable testTable)
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

            return table;
        }

        internal BTreeTable? GetEngineBTreeTable(FullTableName tableName)
        {
            inMemoryTables.TryGetValue(tableName.TableName, out BTreeTable? table);
            return table;
        }

        private static BTreeTable CreateSysColumns()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER };

            FullColumnName[] keyNames = new[] { FullColumnName.FromColumnName("table_name"), FullColumnName.FromColumnName("column_name") };
            FullColumnName[] valueNames = new[] { FullColumnName.FromColumnName("column_type"), FullColumnName.FromColumnName("index") };

            BTreeTable table = new ("sys_columns", keyTypes, keyNames, valueTypes, valueNames);
            Tuple row;

            // --- columns for sys_tables
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_tables"),
                ExpressionOperand.VARCHARFromString("table_name"),
                ExpressionOperand.VARCHARFromString("VARCHAR"),
                ExpressionOperand.IntegerFromInt(1),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_tables"),
                ExpressionOperand.VARCHARFromString("file_name"),
                ExpressionOperand.VARCHARFromString("VARCHAR"),
                ExpressionOperand.IntegerFromInt(2),
            };
            table.InsertRow(row);

            // -- columns for sys_columns
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("table_name"),
                ExpressionOperand.VARCHARFromString("VARCHAR"),
                ExpressionOperand.IntegerFromInt(1),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("column_name"),
                ExpressionOperand.VARCHARFromString("VARCHAR"),
                ExpressionOperand.IntegerFromInt(2),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("column_type"),
                ExpressionOperand.VARCHARFromString("VARCHAR"),
                ExpressionOperand.IntegerFromInt(3),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("index"),
                ExpressionOperand.VARCHARFromString("INTEGER"),
                ExpressionOperand.IntegerFromInt(4),
            };
            table.InsertRow(row);

            return table;
        }

        private static BTreeTable CreateSysTables()
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.VARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.VARCHAR };

            FullColumnName[] keyNames = new[] { FullColumnName.FromColumnName("table_name") };
            FullColumnName[] valueNames = new[] { FullColumnName.FromColumnName("file_name") };

            BTreeTable table = new ("sys_tables", keyTypes, keyNames, valueTypes, valueNames);

            Tuple row;

            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_tables"),
                ExpressionOperand.VARCHARFromString(string.Empty),
            };
            table.InsertRow(row);

            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString(string.Empty),
            };
            table.InsertRow(row);

            return table;
        }

        private static BTreeTable CreateSysIndexes()
        {
            // key is: table_name, index_name
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR };

            // values are: index_type
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.VARCHAR };

            FullColumnName[] keyNames = new[]
            {
                FullColumnName.FromColumnName("table_name"),
                FullColumnName.FromColumnName("index_name"),
            };
            FullColumnName[] valueNames = new[]
            {
                FullColumnName.FromColumnName("index_type"),
            };

            BTreeTable table = new ("sys_indexes", keyTypes, keyNames, valueTypes, valueNames);

            Tuple row;

            // --- for sys_tables
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_tables"),
                ExpressionOperand.VARCHARFromString("sys_tables_pk"),
                ExpressionOperand.VARCHARFromString("U"),
            };
            table.InsertRow(row);

            // --- for sys_columns
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("sys_columns_pk"),
                ExpressionOperand.VARCHARFromString("U"),
            };
            table.InsertRow(row);

            // --- for sys_indexes
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexes"),
                ExpressionOperand.VARCHARFromString("sys_indexes_pk"),
                ExpressionOperand.VARCHARFromString("U"),
            };
            table.InsertRow(row);

            // --- for sys_columns
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.VARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.VARCHARFromString("U"),
            };
            table.InsertRow(row);

            return table;
        }

        private static BTreeTable CreateSysIndexColumns()
        {
            // key is: table_name, index_name, index
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER };

            // values are: column_name
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.VARCHAR };

            FullColumnName[] keyNames = new[]
            {
                FullColumnName.FromColumnName("table_name"),
                FullColumnName.FromColumnName("index_name"),
                FullColumnName.FromColumnName("index"),
            };

            FullColumnName[] valueNames = new[]
            {
                FullColumnName.FromColumnName("column_name"),
            };

            BTreeTable table = new ("sys_indexcolumns", keyTypes, keyNames, valueTypes, valueNames);

            Tuple row;

            // --- for sys_tables, the key is just the table name
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_tables"),
                ExpressionOperand.VARCHARFromString("sys_tables_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.VARCHARFromString("table_name"),
            };
            table.InsertRow(row);

            // --- for sys_columns, the key is (table_name, column_name)
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("sys_columns_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.VARCHARFromString("table_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_columns"),
                ExpressionOperand.VARCHARFromString("sys_columns_pk"),
                ExpressionOperand.IntegerFromInt(2),
                ExpressionOperand.VARCHARFromString("column_name"),
            };
            table.InsertRow(row);

            // --- for sys_indexes, the key is the table name and the index name
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexes"),
                ExpressionOperand.VARCHARFromString("sys_indexes_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.VARCHARFromString("table_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexes"),
                ExpressionOperand.VARCHARFromString("sys_indexes_pk"),
                ExpressionOperand.IntegerFromInt(2),
                ExpressionOperand.VARCHARFromString("index_name"),
            };
            table.InsertRow(row);


            // --- for sys_indexcolumns, the key is the tablename, index name, and index
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.VARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.IntegerFromInt(1),
                ExpressionOperand.VARCHARFromString("table_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.VARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.IntegerFromInt(2),
                ExpressionOperand.VARCHARFromString("index_name"),
            };
            table.InsertRow(row);
            row = new Tuple()
            {
                ExpressionOperand.VARCHARFromString("sys_indexcolumns"),
                ExpressionOperand.VARCHARFromString("sys_indexcolumns_pk"),
                ExpressionOperand.IntegerFromInt(3),
                ExpressionOperand.VARCHARFromString("index"),
            };
            table.InsertRow(row);

            return table;
        }
    }
}

