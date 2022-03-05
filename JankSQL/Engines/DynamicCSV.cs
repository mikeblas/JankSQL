using System.Text;

namespace JankSQL.Engines
{
    public class DynamicCSV : IEngineSource, IEngineDestination
    {
        private readonly string filename;
        private readonly string tableName;

        // list of column names
        private FullColumnName[] columnNames;

        // list of types
        private ExpressionOperandType[] columnTypes;

        // list of lines; each line is a list of values
        private List<ExpressionOperand[]> values;

        public DynamicCSV(string filename, string tableName)
        {
            this.filename = filename;
            this.values = new List<ExpressionOperand[]>();
            this.tableName = tableName;
        }

        static ExpressionOperandType[] GetColumnTypes(FullTableName tableName)
        {
            ExpressionOperandType[] ret;

            // we'd infinitely recurse if we had to look up columns for sys_columns by looking up sys_columns ...
            if (tableName.TableName.Equals("sys_columns", StringComparison.InvariantCultureIgnoreCase))
            {
                List<ExpressionOperandType> types = new();

                // table_name
                types.Add(ExpressionOperandType.VARCHAR);

                // column_name
                types.Add(ExpressionOperandType.VARCHAR);

                // column_type
                types.Add(ExpressionOperandType.VARCHAR);

                // index
                types.Add(ExpressionOperandType.INTEGER);

                ret = types.ToArray();
            }
            else
            {
                DynamicCSV sysColumns = new DynamicCSV("sys_columns.csv", "sys_columns");
                sysColumns.Load();

                // table_name,column_name,column_type,index
                int tableNameIndex = sysColumns.ColumnIndex("table_name");
                int columnNameIndex = sysColumns.ColumnIndex("column_name");
                int typeIndex = sysColumns.ColumnIndex("column_type");
                int indexIndex = sysColumns.ColumnIndex("index");

                List<int> matchingRows = new();

                for (int n = 0; n < sysColumns.RowCount; n++)
                {
                    if (sysColumns.Row(n)[tableNameIndex].AsString().Equals(tableName.TableName, StringComparison.InvariantCultureIgnoreCase))
                        matchingRows.Add(n);
                }

                // build the array
                ret = new ExpressionOperandType[matchingRows.Count];

                for (int i = 0; i < matchingRows.Count; i++)
                {
                    int n = matchingRows[i];
                    ExpressionOperandType operandType;
                    if (!ExpressionNode.TypeFromString(sysColumns.Row(n)[typeIndex].AsString(), out operandType))
                    {
                        throw new ExecutionException($"unknown type {sysColumns.Row(n)[typeIndex].AsString()} in table {tableName.TableName}");
                    }

                    int index = sysColumns.Row(n)[indexIndex].AsInteger();

                    ret[index] = operandType;
                }

            }

            return ret;
        }

        public void Load()
        {
            var lines = File.ReadLines(filename);
            int lineNumber = 0;

            columnTypes = DynamicCSV.GetColumnTypes(FullTableName.FromTableName(tableName));

            foreach (var line in lines)
            {
                string[] fields = line.Split(",");

                if (lineNumber == 0)
                {
                    columnNames = new FullColumnName[fields.Length];
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, fields[i]);
                        columnNames[i] = fcn;
                    }
                }
                else
                {
                    ExpressionOperand[] newRow = new ExpressionOperand[fields.Length];

                    for (int i = 0; i < fields.Length; i++)
                    {
                        switch (columnTypes[i])
                        {
                            case ExpressionOperandType.DECIMAL:
                                newRow[i] = new ExpressionOperandDecimal(Double.Parse(fields[i]));
                                break;

                            case ExpressionOperandType.VARCHAR:
                                newRow[i] = new ExpressionOperandVARCHAR(fields[i]);
                                break;

                            case ExpressionOperandType.NVARCHAR:
                                newRow[i] = new ExpressionOperandNVARCHAR(fields[i]);
                                break;

                            case ExpressionOperandType.INTEGER:
                                newRow[i] = new ExpressionOperandInteger(Int32.Parse(fields[i]));
                                break;

                            default:
                                throw new NotImplementedException();

                        }
                    }

                    values.Add(newRow);
                }

                lineNumber++;
            }
        }

        public int RowCount { get { return values.Count; } }

        public int ColumnCount { get { return columnNames.Length; } }


        public ExpressionOperand[] Row(int n)
        {
            return values[n];
        }

        public FullColumnName ColumnName(int n)
        {
            return columnNames[n];
        }

        public int ColumnIndex(string columnName)
        {
            FullColumnName fcnMatch = FullColumnName.FromColumnName(columnName);
            for (int i = 0; i < columnNames.Length; i++)
            {
                if (columnNames[i].Equals(fcnMatch))
                // if (fcnMatch.Equals(columnNames[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public void TruncateTable()
        {
            // get the first line of the file
            var fileStream = new FileStream(filename, FileMode.Open);
            string firstLine;
            using (var reader = new StreamReader(fileStream))
            {
                firstLine = reader.ReadLine();
            }
            fileStream.Close();

            // delete the file
            File.Delete(filename);

            // re-create it with that line
            using StreamWriter writer = new(filename);
            writer.WriteLine(firstLine);
            writer.Flush();
            writer.Close();

            Load();
        }

        static public string? FileFromSysTables(Engines.DynamicCSV sysTables, string effectiveTableName)
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

        public void InsertRow(ExpressionOperand[] row)
        {
            if (row.Length != columnNames.Length)
                throw new ExecutionException($"table {tableName}: can't insert row with {row.Length} columns, need {columnNames.Length} columns");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < row.Length; i++)
            {
                string col = row[i].AsString();
                if (i > 0)
                    sb.Append(",");
                sb.Append(col);
            }

            using (StreamWriter sw = File.AppendText(this.filename))
            {
                sw.WriteLine(sb.ToString());
                // Console.WriteLine($"Table writer: {sb.ToString()}");
            }
        }

        internal static void CreateTable(FullTableName tableName, List<FullColumnName> columnNames, List<ExpressionOperandType> columnTypes)
        {
            // guess file name
            string fileName = tableName.TableName.Replace("[", "").Replace("]", "") + ".csv";

            // see if table doesn't exist
            DynamicCSV sysTables = new DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            string? foundFileName = FileFromSysTables(sysTables, tableName.TableName);
            if (foundFileName != null)
            {
                throw new ExecutionException($"Table named {tableName} already exists");
            }

            //make sure file doesn't exist, too
            int idxFile = sysTables.ColumnIndex("file_name");
            int idxName = sysTables.ColumnIndex("table_name");
            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxFile].AsString().Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    string otherTableName = sysTables.Row(i)[idxName].AsString();
                    throw new ExecutionException($"File name {fileName} already exists for table {otherTableName}");
                }
            }

            // create file
            FullColumnName fcn;
            string types = String.Join(',', columnTypes);
            using StreamWriter file = new(fileName);
            file.WriteLine(types);
            file.Close();

            // add row to sys_tables
            ExpressionOperand[] newRow = new ExpressionOperand[2];
            newRow[idxFile] = ExpressionOperand.NVARCHARFromString(fileName);
            newRow[idxName] = ExpressionOperand.NVARCHARFromString(tableName.TableName);

            sysTables.InsertRow(newRow);

            // add rows to sys_columns
            DynamicCSV sysColumns = new DynamicCSV("sys_columns.csv", "sys_columns");
            sysColumns.Load();

            int idxColumnName = sysColumns.ColumnIndex("column_name");
            int idxIdx = sysColumns.ColumnIndex("index");
            int idxTableName = sysColumns.ColumnIndex("table_name");
            int idxType = sysColumns.ColumnIndex("column_type");

            for (int i = 0; i < columnNames.Count; i++)
            {
                ExpressionOperand[] columnRow = new ExpressionOperand[columnNames.Count];

                columnRow[idxIdx] = ExpressionOperand.IntegerFromInt(i);
                columnRow[idxTableName] = ExpressionOperand.NVARCHARFromString(tableName.TableName);
                columnRow[idxColumnName] = ExpressionOperand.NVARCHARFromString(columnNames[i].ColumnNameOnly());
                columnRow[idxType] = ExpressionOperand.VARCHARFromString(columnTypes[i].ToString());

                sysColumns.InsertRow(columnRow);
            }
        }

        internal static void DropTable(FullTableName tableName)
        {
            // delete the file
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            string? fileName = Engines.DynamicCSV.FileFromSysTables(sysTables, tableName.TableName);
            if (fileName == null)
                throw new ExecutionException($"Table {tableName} does not exist");

            File.Delete(fileName);

            // remove entries from sys_columns
            Engines.DynamicCSV sysColumns = new Engines.DynamicCSV("sys_columns.csv", "sys_columns");
            sysColumns.Load();
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

        internal int DeleteRows(List<int> rowIndexesToDelete)
        {
            int deletedCount = 0;

            // rename the file
            string newFileName = filename + ".bak";
            File.Delete(newFileName);
            File.Move(filename, newFileName);

            var lines = File.ReadLines(newFileName);
            int lineNumber = 0;

            using StreamWriter writer = new(filename);

            columnTypes = DynamicCSV.GetColumnTypes(FullTableName.FromTableName(tableName));

            foreach (var line in lines)
            {
                lineNumber++;

                bool keep = true;
                if (lineNumber > 1)
                {
                    // line number is an ordinal, rows to delete contains indexes
                    // line number 2 is index 0
                    if (rowIndexesToDelete.IndexOf(lineNumber - 2) != -1)
                        keep = false;
                }

                if (keep)
                    writer.WriteLine(line);
                else
                    deletedCount++;
            }

            writer.Flush();
            writer.Close();

            File.Delete(newFileName);
            Load();
            return deletedCount;
        }
    }

}

