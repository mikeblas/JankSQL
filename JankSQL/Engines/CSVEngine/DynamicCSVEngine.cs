namespace JankSQL.Engines
{
    using System.Collections.Immutable;

    using JankSQL.Expressions;

    public enum OpenPolicy
    {
        ExistingOnly,   // fail if not found
        Always,         // always open -- create if not there
        Obliterate,     // erase if exists
    }

    public class DynamicCSVEngine : IEngine
    {
        private readonly string basePath;
        private readonly string sysTablesPath;
        private readonly string sysColumnsPath;

        protected DynamicCSVEngine(string basePath, string sysTablesPath, string sysColsPath)
        {
            this.basePath = basePath;
            this.sysTablesPath = sysTablesPath;
            this.sysColumnsPath = sysColsPath;
        }

        public static DynamicCSVEngine Open(string basePath, OpenPolicy policy)
        {
            DynamicCSVEngine engine;
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

        public static DynamicCSVEngine OpenExistingOnly(string basePath)
        {
            if (!Directory.Exists(basePath))
                throw new FileNotFoundException($"CSV database directory {basePath} not found");

            (string sysTablesPath, string sysColsPath) = GetCatalogPaths(basePath);

            if (!File.Exists(sysColsPath))
                throw new FileNotFoundException($"CSV SysColumns file {sysColsPath} not found");

            if (!File.Exists(sysTablesPath))
                throw new FileNotFoundException($"CSV SysTables file {sysTablesPath} not found");

            return new DynamicCSVEngine(basePath, sysTablesPath, sysColsPath);
        }

        public static DynamicCSVEngine OpenAlways(string basePath)
        {
            DynamicCSVEngine? engine = null;
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

        public static DynamicCSVEngine OpenObliterate(string basePath)
        {
            RemoveDatabase(basePath);
            CreateDatabase(basePath);
            return OpenExistingOnly(basePath);
        }

        public static string? FileFromSysTables(IEngineTable sysTables, string effectiveTableName)
        {
            // is this source table in there?
            int idxName = sysTables.ColumnIndex("table_name");
            int idxFile = sysTables.ColumnIndex("file_name");

            foreach (var row in sysTables)
            {
                if (row.RowData[idxName].AsString().Equals(effectiveTableName, StringComparison.InvariantCultureIgnoreCase))
                    return row.RowData[idxFile].AsString();
            }

            return null;
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

        public void Dispose()
        {
            // nothing to dispose
            GC.SuppressFinalize(this);
        }

        public void Commit()
        {
            // nothing to commit
        }

        public void Rollback()
        {
            // nothing to rollback
        }

        public void CreateTable(FullTableName tableName, IImmutableList<FullColumnName> columnNames, IImmutableList<ExpressionOperandType> columnTypes)
        {
            // guess file name
            string fileName = tableName.TableNameOnly.Replace("[", string.Empty).Replace("]", string.Empty) + ".csv";
            string fullPath = Path.Combine(basePath, fileName);

            // see if table doesn't exist
            IEngineTable sysTables = GetSysTables();

            string? foundFileName = FileFromSysTables(sysTables, tableName.TableNameOnly);
            if (foundFileName != null)
            {
                throw new ExecutionException($"Table named {tableName} already exists");
            }

            // make sure file doesn't exist, too
            int idxFile = sysTables.ColumnIndex("file_name");
            int idxName = sysTables.ColumnIndex("table_name");
            foreach (var row in sysTables)
            {
                if (row.RowData[idxFile].AsString().Equals(fullPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    string otherTableName = row.RowData[idxName].AsString();
                    throw new ExecutionException($"File name {fullPath} already exists for table {otherTableName}");
                }
            }

            // create file
            using StreamWriter file = new (fullPath);
            string types = string.Join(',', columnTypes);
            string names = string.Join(',', columnNames.Select(c => c.ColumnNameOnly()).ToArray());
            file.WriteLine(names);
            file.Close();

            // add row to sys_tables
            Tuple newRow = Tuple.CreateEmpty(sysTables.ColumnCount);
            newRow[idxFile] = ExpressionOperand.VARCHARFromString(fullPath);
            newRow[idxName] = ExpressionOperand.VARCHARFromString(tableName.TableNameOnly);

            sysTables.InsertRow(newRow);

            // add rows to sys_columns
            IEngineTable sysColumns = GetSysColumns();

            int idxColumnName = sysColumns.ColumnIndex("column_name");
            int idxIdx = sysColumns.ColumnIndex("index");
            int idxTableName = sysColumns.ColumnIndex("table_name");
            int idxType = sysColumns.ColumnIndex("column_type");

            int columnNameIndex = 0;
            IEnumerator<ExpressionOperandType> columnTypeEnumerator = columnTypes.GetEnumerator();
            foreach (var columnName in columnNames)
            {
                columnTypeEnumerator.MoveNext();
                Tuple columnRow = Tuple.CreateEmpty(sysColumns.ColumnCount);

                columnRow[idxIdx] = ExpressionOperand.IntegerFromInt(columnNameIndex);
                columnRow[idxTableName] = ExpressionOperand.VARCHARFromString(tableName.TableNameOnly);
                columnRow[idxColumnName] = ExpressionOperand.VARCHARFromString(columnName.ColumnNameOnly());
                columnRow[idxType] = ExpressionOperand.VARCHARFromString(columnTypeEnumerator.Current.ToString());

                sysColumns.InsertRow(columnRow);

                columnNameIndex++;
            }
        }

        public void CreateIndex(FullTableName tableName, string indexName, bool isUnique, IEnumerable<(string columnName, bool isDescending)> columnInfos)
        {
            throw new NotImplementedException();
        }

        public IEngineTable GetSysTables()
        {
            DynamicCSVTable sysTables = new (sysTablesPath, "sys_tables", this);
            sysTables.Load();
            return sysTables;
        }

        public IEngineTable GetSysColumns()
        {
            DynamicCSVTable sysColumns = new (sysColumnsPath, "sys_columns", this);
            sysColumns.Load();
            return sysColumns;
        }

        public IEngineTable GetSysIndexes()
        {
            throw new NotImplementedException();
        }

        public IEngineTable GetSysIndexColumns()
        {
            throw new NotImplementedException();
        }

        public void DropTable(FullTableName tableName)
        {
            // delete the file
            IEngineTable sysTables = GetSysTables();

            string? fileName = DynamicCSVEngine.FileFromSysTables(sysTables, tableName.TableNameOnly);
            if (fileName == null)
                throw new ExecutionException($"Table {tableName} does not exist");

            File.Delete(fileName);

            // remove entries from sys_columns
            IEngineTable sysColumns = GetSysColumns();
            int tableNameIndex = sysColumns.ColumnIndex("table_name");

            List<ExpressionOperandBookmark> rowIndexesToDelete = new ();
            foreach (var row in sysColumns)
            {
                if (row.RowData[tableNameIndex].AsString().Equals(tableName.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
                {
                    rowIndexesToDelete.Add(row.Bookmark);
                }
            }

            sysColumns.DeleteRows(rowIndexesToDelete);

            // remove from sys_tables
            rowIndexesToDelete.Clear();
            int idxName = sysTables.ColumnIndex("table_name");

            foreach (var row in sysTables)
            {
                if (row.RowData[idxName].AsString().Equals(tableName.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
                {
                    rowIndexesToDelete.Add(row.Bookmark);
                }
            }

            sysTables.DeleteRows(rowIndexesToDelete);
        }

        public IEngineTable? GetEngineTable(FullTableName tableName)
        {
            // get systables
            IEngineTable sysTables = GetSysTables();

            // get the file name for our table
            string? effectiveTableFileName = FileFromSysTables(sysTables, tableName.TableNameOnly);

            if (effectiveTableFileName == null)
                return null;
            else
            {
                // found the source table, so load it
                DynamicCSVTable table = new (effectiveTableFileName, tableName.TableNameOnly, this);
                table.Load();
                return table;
            }
        }

        public IEngineTable InjectTestTable(TestTableDefinition testTable)
        {
            // create the table ...
            CreateTable(testTable.TableName, testTable.ColumnNames, testTable.ColumnTypes);

            // then insert each of the given rows
            // get the file name for our table
            IEngineTable sysTables = GetSysTables();
            string? effectiveTableFileName = FileFromSysTables(sysTables, testTable.TableName.TableNameOnly);
            if (effectiveTableFileName == null)
                throw new InvalidOperationException();

            DynamicCSVTable table = new (effectiveTableFileName, testTable.TableName.TableNameOnly, this);
            table.Load();

            foreach (var row in testTable.Rows)
                table.InsertRow(row);

            return table;
        }

        protected static void CreateDatabase(string basePath)
        {
            Directory.CreateDirectory(basePath);
            CreateSystemCatalog(basePath);
        }

        protected static (string sysTablesPath, string sysColsPath) GetCatalogPaths(string basePath)
        {
            string sysTablesPath = Path.Combine(basePath, "sys_tables.csv");
            string sysColsPath = Path.Combine(basePath, "sys_columns.csv");

            return (sysTablesPath, sysColsPath);
        }

        protected static void CreateSystemCatalog(string rootPath)
        {
            (string sysTablesPath, string sysColsPath) = GetCatalogPaths(rootPath);

            string[] sysTablesStrings = new string[]
            {
                "table_name,file_name",
                $"sys_tables,{sysTablesPath}",
                $"sys_columns,{sysColsPath}",
            };

            string[] sysColsStrings = new string[]
            {
                "table_name,column_name,column_type,index",
                "sys_tables, table_name,VARCHAR,0",
                "sys_tables,file_name,VARCHAR,1",
                "sys_columns,table_name,VARCHAR,0",
                "sys_columns,column_name,VARCHAR,1",
                "sys_columns,column_type,VARCHAR,2",
                "sys_columns,index,INTEGER,3",
            };

            File.WriteAllLines(sysTablesPath, sysTablesStrings);
            File.WriteAllLines(sysColsPath, sysColsStrings);
        }

    }
}
