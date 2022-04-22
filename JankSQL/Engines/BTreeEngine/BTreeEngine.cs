﻿namespace JankSQL.Engines
{
    using System.Collections.Immutable;
    using CSharpTest.Net.Collections;
    using JankSQL.Expressions;

    public class BTreeEngine : IEngine
    {
        private readonly Dictionary<string, BTreeTable> inMemoryTables = new (StringComparer.InvariantCultureIgnoreCase);

        private BTreeTable? sysColumns;
        private BTreeTable? sysTables;
        private BTreeTable? sysIndexes;
        private BTreeTable? sysIndexColumns;

        private string? basePath;

        protected BTreeEngine()
        {
            sysColumns = CreateSysColumns(null);
            InitializeSysColumns(sysColumns);

            sysTables = CreateSysTables(null);
            InitializeSysTables(sysTables);

            sysIndexes = CreateSysIndexes(null);
            InitializeSysIndexes(sysIndexes);

            sysIndexColumns = CreateSysIndexColumns(null);
            InitializeSysIndexColumns(sysIndexColumns);

            inMemoryTables["sys_tables"] = sysTables;
            inMemoryTables["sys_indexes"] = sysIndexes;
            inMemoryTables["sys_indexcolumns"] = sysIndexColumns;
            inMemoryTables["sys_columns"] = sysColumns;

            sysIndexes.Dump();
            sysIndexColumns.Dump();

            this.basePath = null;
        }

        protected BTreeEngine(string basePath, string sysTablesPath, string sysColumnsPath, string sysIndexesPath, string sysIndexColumnsPath)
        {
            this.basePath = basePath;

            BPlusTree<Tuple, Tuple>.OptionsV2? sysColumnsOptions = new (new TupleSerializer(), new TupleSerializer());
            sysColumnsOptions.FileName = sysColumnsPath;
            sysColumnsOptions.CreateFile = CreatePolicy.Never;
            sysColumns = CreateSysColumns(sysColumnsOptions);
            sysColumns.Commit();

            BPlusTree<Tuple, Tuple>.OptionsV2? sysTablesOptions = new (new TupleSerializer(), new TupleSerializer());
            sysTablesOptions.FileName = sysTablesPath;
            sysTablesOptions.CreateFile = CreatePolicy.Never;
            sysTables = CreateSysTables(sysTablesOptions);

            BPlusTree<Tuple, Tuple>.OptionsV2? sysIndexesOptions = new (new TupleSerializer(), new TupleSerializer());
            sysIndexesOptions.FileName = sysIndexesPath;
            sysIndexesOptions.CreateFile = CreatePolicy.Never;
            sysIndexes = CreateSysIndexes(sysIndexesOptions);

            BPlusTree<Tuple, Tuple>.OptionsV2? sysIndexColumnsOptions = new (new TupleSerializer(), new TupleSerializer());
            sysIndexColumnsOptions.FileName = sysIndexColumnsPath;
            sysIndexColumnsOptions.CreateFile = CreatePolicy.Never;
            sysIndexColumns = CreateSysIndexColumns(sysIndexColumnsOptions);

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

        public static BTreeEngine OpenDiskBased(string basePath, OpenPolicy policy)
        {
            BTreeEngine engine;
            switch (policy)
            {
                case OpenPolicy.ExistingOnly:
                    engine = OpenExistingOnly(basePath);
                    break;

                case OpenPolicy.Always:
                    engine = OpenExistingOnly(basePath);
                    break;

                case OpenPolicy.Obliterate:
                    engine = OpenObliterate(basePath);
                    break;

                default:
                    throw new ArgumentException($"can't handle OpenPolicy {policy}");
            }

            return engine;
        }

        public static BTreeEngine OpenExistingOnly(string basePath)
        {
            if (!Directory.Exists(basePath))
                throw new FileNotFoundException($"database directory {basePath} not found");

            (string sysTablesPath, string sysColsPath, string sysIndexesPath, string sysIndexesColumnsPath) = GetCatalogPaths(basePath);

            if (!File.Exists(sysColsPath))
                throw new FileNotFoundException($"SysColumns file {sysColsPath} not found");

            if (!File.Exists(sysTablesPath))
                throw new FileNotFoundException($"SysTables file {sysTablesPath} not found");

            if (!File.Exists(sysIndexesPath))
                throw new FileNotFoundException($"SysIndexes file {sysIndexesPath} not found");

            if (!File.Exists(sysIndexesColumnsPath))
                throw new FileNotFoundException($"SysIndexesColumns file {sysIndexesColumnsPath} not found");

            return new BTreeEngine(basePath, sysTablesPath, sysColsPath, sysIndexesPath, sysIndexesColumnsPath);
        }

        public static BTreeEngine OpenAlways(string basePath)
        {
            BTreeEngine? engine = null;
            try
            {
                engine = OpenExistingOnly(basePath);
            }
            catch (FileNotFoundException)
            {
                // it's okay; we're meant to handle this
            }

            if (engine == null)
                engine = OpenObliterate(basePath);

            return engine;
        }

        public static BTreeEngine OpenObliterate(string basePath)
        {
            RemoveDatabase(basePath);
            CreateDatabase(basePath);
            return OpenExistingOnly(basePath);
        }


        public static void RemoveDatabase(string basePath)
        {
            try
            {
                Directory.Delete(basePath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // that's OK -- already gone!
            }
        }

        protected static void CreateDatabase(string basePath)
        {
            Directory.CreateDirectory(basePath);
            CreateSystemCatalog(basePath);
        }

        protected static void CreateSystemCatalog(string basePath)
        {
            (string sysTablesPath, string sysColumnsPath, string sysIndexesPath, string sysIndexColumnsPath) = GetCatalogPaths(basePath);

            BPlusTree<Tuple, Tuple>.OptionsV2? sysColumnsOptions = new (new TupleSerializer(), new TupleSerializer());
            sysColumnsOptions.FileName = sysColumnsPath;
            sysColumnsOptions.CreateFile = CreatePolicy.Always;
            using BTreeTable sysColumns = CreateSysColumns(sysColumnsOptions);
            InitializeSysColumns(sysColumns);
            sysColumns.Commit();

            BPlusTree<Tuple, Tuple>.OptionsV2? sysTablesOptions = new (new TupleSerializer(), new TupleSerializer());
            sysTablesOptions.FileName = sysTablesPath;
            sysTablesOptions.CreateFile = CreatePolicy.Always;
            using BTreeTable sysTables = CreateSysTables(sysTablesOptions);
            InitializeSysTables(sysTables);
            sysTables.Commit();

            BPlusTree<Tuple, Tuple>.OptionsV2? sysIndexesOptions = new (new TupleSerializer(), new TupleSerializer());
            sysIndexesOptions.FileName = sysIndexesPath;
            sysIndexesOptions.CreateFile = CreatePolicy.Always;
            using BTreeTable sysIndexes = CreateSysIndexes(sysIndexesOptions);
            InitializeSysIndexes(sysIndexes);
            sysIndexes.Commit();

            BPlusTree<Tuple, Tuple>.OptionsV2? sysIndexColumnsOptions = new (new TupleSerializer(), new TupleSerializer());
            sysIndexColumnsOptions.FileName = sysIndexColumnsPath;
            sysIndexColumnsOptions.CreateFile = CreatePolicy.Always;
            using BTreeTable sysIndexColumns = CreateSysIndexColumns(sysIndexColumnsOptions);
            InitializeSysIndexColumns(sysIndexColumns);
            sysIndexColumns.Commit();
        }

        protected static (string sysTablesPath, string sysColsPath, string sysIndexesPath, string sysIndexesColumnsPath) GetCatalogPaths(string basePath)
        {
            string sysTablesPath = Path.Combine(basePath, "sys_tables.jank");
            string sysColsPath = Path.Combine(basePath, "sys_columns.jank");
            string sysIndexesPath = Path.Combine(basePath, "sys_indexes.jank");
            string sysIndexesColumnsPath = Path.Combine(basePath, "sys_indexcolumns.jank");

            return (sysTablesPath, sysColsPath, sysIndexesPath, sysIndexesColumnsPath);
        }

        public void Dispose()
        {
            foreach (var table in inMemoryTables.Values)
                table.Dispose();
            inMemoryTables.Clear();
            sysColumns = null;
            sysTables = null;
            sysIndexes = null;
            sysIndexColumns = null;
        }

        public void CreateTable(FullTableName tableName, IImmutableList<FullColumnName> columnNames, IImmutableList<ExpressionOperandType> columnTypes)
        {
            if (columnNames.Count == 0)
                throw new ArgumentException("Must have at least one column name");
            if (columnNames.Count != columnTypes.Count)
                throw new ArgumentException($"Must have at types for each column; got {columnNames.Count} names and {columnTypes.Count} types");

            // create the table
            BTreeTable table = new (tableName.TableNameOnly, columnTypes.ToArray(), columnNames, null);

            // add a row to sys_tables
            Tuple tablesRow = new ()
            {
                ExpressionOperand.VARCHARFromString(tableName.TableNameOnly),
                ExpressionOperand.VARCHARFromString(string.Empty),
            };
            sysTables.InsertRow(tablesRow);

            // add rows for the sys_columns
            for (int nameIndex = 0; nameIndex < columnNames.Count; nameIndex++)
            {
                Tuple columnRow = new ()
                {
                    ExpressionOperand.VARCHARFromString(tableName.TableNameOnly),
                    ExpressionOperand.VARCHARFromString(columnNames[nameIndex].ColumnNameOnly()),
                    ExpressionOperand.VARCHARFromString(columnTypes[nameIndex].ToString()), // type
                    ExpressionOperand.IntegerFromInt(nameIndex), // ordinal
                };

                sysColumns.InsertRow(columnRow);
            }


            inMemoryTables.Add(tableName.TableNameOnly, table);
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
                if (row.RowData[tableNameIndex].AsString().Equals(tableName.TableNameOnly, StringComparison.OrdinalIgnoreCase) &&
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
                if (row.RowData[columnsTableNameIndex].AsString().Equals(tableName.TableNameOnly, StringComparison.OrdinalIgnoreCase))
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
                ExpressionOperand.VARCHARFromString(tableName.TableNameOnly),
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
                    ExpressionOperand.VARCHARFromString(tableName.TableNameOnly),
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
            if (!inMemoryTables.ContainsKey(tableName.TableNameOnly))
                throw new ExecutionException($"table {tableName} does not exist");

            inMemoryTables.Remove(tableName.TableNameOnly);

            // delete from sys_tables
            ExpressionOperandBookmark tableBookmark = new (Tuple.FromSingleValue(tableName.TableNameOnly, ExpressionOperandType.VARCHAR));
            List<ExpressionOperandBookmark> tableMark = new () { tableBookmark };
            sysTables.DeleteRows(tableMark);

            // delete from sys_columns
            List<ExpressionOperandBookmark> columnRows = new ();
            int tableIndex = sysColumns.ColumnIndex("table_name");
            int columnIndex = sysColumns.ColumnIndex("column_name");

            foreach (var row in sysColumns)
            {
                if (row.RowData[tableIndex].AsString().Equals(tableName.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
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

            table.Commit();
            return table;
        }

        internal BTreeTable? GetEngineBTreeTable(FullTableName tableName)
        {
            inMemoryTables.TryGetValue(tableName.TableNameOnly, out BTreeTable? table);
            return table;
        }

        private static BTreeTable CreateSysColumns(BPlusTree<Tuple, Tuple>.OptionsV2? options)
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.VARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.VARCHAR, ExpressionOperandType.INTEGER };

            FullColumnName[] keyNames = new[] { FullColumnName.FromColumnName("table_name"), FullColumnName.FromColumnName("column_name") };
            FullColumnName[] valueNames = new[] { FullColumnName.FromColumnName("column_type"), FullColumnName.FromColumnName("index") };

            BTreeTable table = new ("sys_columns", keyTypes, keyNames, valueTypes, valueNames, options);

            return table;
        }

        private static void InitializeSysColumns(BTreeTable table)
        {
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
        }

        private static BTreeTable CreateSysTables(BPlusTree<Tuple, Tuple>.OptionsV2? options)
        {
            ExpressionOperandType[] keyTypes = new[] { ExpressionOperandType.VARCHAR };
            ExpressionOperandType[] valueTypes = new[] { ExpressionOperandType.VARCHAR };

            FullColumnName[] keyNames = new[] { FullColumnName.FromColumnName("table_name") };
            FullColumnName[] valueNames = new[] { FullColumnName.FromColumnName("file_name") };

            BTreeTable table = new ("sys_tables", keyTypes, keyNames, valueTypes, valueNames, options);
            return table;
        }

        private static void InitializeSysTables(BTreeTable table)
        {
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
        }

        private static BTreeTable CreateSysIndexes(BPlusTree<Tuple, Tuple>.OptionsV2? options)
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

            BTreeTable table = new ("sys_indexes", keyTypes, keyNames, valueTypes, valueNames, options);
            return table;
        }

        private static void InitializeSysIndexes(BTreeTable table)
        {

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
        }

        private static BTreeTable CreateSysIndexColumns(BPlusTree<Tuple, Tuple>.OptionsV2? options)
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

            BTreeTable table = new ("sys_indexcolumns", keyTypes, keyNames, valueTypes, valueNames, options);
            return table;
        }

        private static void InitializeSysIndexColumns(BTreeTable table)
        {
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
        }
    }
}

