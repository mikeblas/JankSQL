
namespace JankSQL.Engines
{

    internal enum OpenPolicy
    {
        ExistingOnly,   // fail if not found
        Always,         // always open -- create if not there
        Obliterate,     // erase if exists
    }

    internal class DynamicCSVEngine : IEngine
    {
        readonly string basePath;
        readonly string sysTablesPath;
        readonly string sysColumnsPath;

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
                    throw new ArgumentException();
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

        DynamicCSVEngine(string basePath, string sysTablesPath, string sysColsPath)
        {
            this.basePath = basePath;
            this.sysTablesPath = sysTablesPath;
            this.sysColumnsPath = sysColsPath;
        }

        static void CreateDatabase(string basePath)
        {
            Directory.CreateDirectory(basePath);
            CreateSystemCatalog(basePath);
        }

        static void RemoveDatabase(string basePath)
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

        static (string sysTablesPath, string sysColsPath) GetCatalogPaths(string basePath)
        {
            string sysTablesPath = Path.Combine(basePath, "sys_tables.csv");
            string sysColsPath = Path.Combine(basePath, "sys_columns.csv");

            return (sysTablesPath, sysColsPath);
        }

        static void CreateSystemCatalog(string rootPath)
        {
            (string sysTablesPath, string sysColsPath) = GetCatalogPaths(rootPath);

            string[] sysTablesStrings = new string[] {
                "table_name,file_name",
                $"sys_tables,{sysTablesPath}",
                $"sys_columns,{sysColsPath}",
            };

            string[] sysColsStrings = new string[] {
                "table_name,column_name,column_type,index",
                "sys_tables, table_name, NVARCHAR,0",
                "sys_tables,file_name,NVARCHAR,1",
                "sys_columns,table_name,NVARCHAR,0",
                "sys_columns,column_name,NVARCHAR,1",
                "sys_coluns,column_type,NVARCHAR,2",
                "sys_colmns,index,INTEGER,3"
            };

            File.WriteAllLines(sysTablesPath, sysTablesStrings);
            File.WriteAllLines(sysColsPath, sysColsStrings);
        }


        public void CreateTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes)
        {
            // guess file name
            string fileName = tableName.TableName.Replace("[", "").Replace("]", "") + ".csv";
            string fullPath = Path.Combine(basePath, fileName);

            // see if table doesn't exist
            IEngineTable sysTables = GetSysTables();

            string? foundFileName = FileFromSysTables(sysTables, tableName.TableName);
            if (foundFileName != null)
            {
                throw new ExecutionException($"Table named {tableName} already exists");
            }

            // make sure file doesn't exist, too
            int idxFile = sysTables.ColumnIndex("file_name");
            int idxName = sysTables.ColumnIndex("table_name");
            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxFile].AsString().Equals(fullPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    string otherTableName = sysTables.Row(i)[idxName].AsString();
                    throw new ExecutionException($"File name {fullPath} already exists for table {otherTableName}");
                }
            }

            // create file
            string types = String.Join(',', columnTypes);
            using StreamWriter file = new(fullPath);
            file.WriteLine(types);
            file.Close();

            // add row to sys_tables
            ExpressionOperand[] newRow = new ExpressionOperand[2];
            newRow[idxFile] = ExpressionOperand.NVARCHARFromString(fullPath);
            newRow[idxName] = ExpressionOperand.NVARCHARFromString(tableName.TableName);

            sysTables.InsertRow(newRow);

            // add rows to sys_columns
            IEngineTable sysColumns = GetSysColumns();

            int idxColumnName = sysColumns.ColumnIndex("column_name");
            int idxIdx = sysColumns.ColumnIndex("index");
            int idxTableName = sysColumns.ColumnIndex("table_name");
            int idxType = sysColumns.ColumnIndex("column_type");

            for (int i = 0; i < columnNames.Count; i++)
            {
                ExpressionOperand[] columnRow = new ExpressionOperand[sysColumns.ColumnCount];

                columnRow[idxIdx] = ExpressionOperand.IntegerFromInt(i);
                columnRow[idxTableName] = ExpressionOperand.NVARCHARFromString(tableName.TableName);
                columnRow[idxColumnName] = ExpressionOperand.NVARCHARFromString(columnNames[i].ColumnNameOnly());
                columnRow[idxType] = ExpressionOperand.VARCHARFromString(columnTypes[i].ToString());

                sysColumns.InsertRow(columnRow);
            }
        }

        public IEngineTable GetSysTables()
        {
            DynamicCSVTable sysTables = new DynamicCSVTable(sysTablesPath, "sys_tables", this);
            sysTables.Load();
            return sysTables;
        }

        public IEngineTable GetSysColumns()
        {
            DynamicCSVTable sysColumns = new DynamicCSVTable(sysColumnsPath, "sys_columns", this);
            sysColumns.Load();
            return sysColumns;
        }

        public void DropTable(FullTableName tableName)
        {
            // delete the file
            IEngineTable sysTables = GetSysTables();

            string? fileName = DynamicCSVEngine.FileFromSysTables(sysTables, tableName.TableName);
            if (fileName == null)
                throw new ExecutionException($"Table {tableName} does not exist");

            File.Delete(fileName);

            // remove entries from sys_columns
            IEngineTable sysColumns = GetSysColumns();
            int tableNameIndex = sysColumns.ColumnIndex("table_name");

            List<int> rowIndexesToDelete = new();

            for (int i = 0; i < sysColumns.RowCount; i++)
            {
                if (sysColumns.Row(i)[tableNameIndex].AsString().Equals(tableName.TableName, StringComparison.InvariantCultureIgnoreCase))
                    rowIndexesToDelete.Add(i);
            }

            sysColumns.DeleteRows(rowIndexesToDelete);

            // remove from sys_tables
            rowIndexesToDelete = new();
            int idxName = sysTables.ColumnIndex("table_name");

            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxName].AsString().Equals(tableName.TableName, StringComparison.InvariantCultureIgnoreCase))
                    rowIndexesToDelete.Add(i);
            }

            sysTables.DeleteRows(rowIndexesToDelete);
        }

        static public string? FileFromSysTables(IEngineTable sysTables, string effectiveTableName)
        {
            // is this source table in there?
            int idxName = sysTables.ColumnIndex("table_name");
            int idxFile = sysTables.ColumnIndex("file_name");

            int foundRow = -1;
            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxName].AsString().Equals(effectiveTableName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundRow = i;
                    break;
                }
            }
            if (foundRow == -1)
                return null;

            return sysTables.Row(foundRow)[idxFile].AsString();
        }


        public IEngineTable? GetEngineTable(FullTableName tableName)
        {
            // get systables
            IEngineTable sysTables = GetSysTables();

            // get the file name for our table
            string? effectiveTableFileName = FileFromSysTables(sysTables, tableName.TableName);

            if (effectiveTableFileName == null)
            {
                return null;
            }
            else
            {
                // found the source table, so load it
                DynamicCSVTable table = new DynamicCSVTable(effectiveTableFileName, tableName.TableName, this);
                table.Load();
                return table;
            }
        }


    }
}


