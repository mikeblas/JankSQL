using System.Text;

namespace JankSQL.Engines
{
    // represents a table in a CSV engine
    public class DynamicCSVTable : IEngineSource, IEngineDestination
    {
        private readonly string filename;
        private readonly string tableName;
        private IEngine engine;

        // list of column names
        private FullColumnName[]? columnNames;

        // list of types
        private ExpressionOperandType[]? columnTypes;

        // list of lines; each line is a list of values
        private List<ExpressionOperand[]> values;

        public DynamicCSVTable(string filename, string tableName, IEngine engine)
        {
            this.filename = filename;
            this.values = new List<ExpressionOperand[]>();
            this.tableName = tableName;
            this.engine = engine;
        }

        ExpressionOperandType[] GetColumnTypes(FullTableName tableName)
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

                DynamicCSVTable sysColumns = engine.GetSysColumns();

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

            columnTypes = GetColumnTypes(FullTableName.FromTableName(tableName));

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

        public int ColumnCount { get { return columnNames!.Length; } }


        public ExpressionOperand[] Row(int n)
        {
            return values[n];
        }

        public FullColumnName ColumnName(int n)
        {
            return columnNames![n];
        }

        public int ColumnIndex(string columnName)
        {
            FullColumnName fcnMatch = FullColumnName.FromColumnName(columnName);
            for (int i = 0; i < columnNames!.Length; i++)
            {
                if (columnNames[i].Equals(fcnMatch))
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
            string? firstLine;
            using (var reader = new StreamReader(fileStream))
            {
                firstLine = reader.ReadLine();
                if (firstLine == null)
                {
                    throw new ExecutionException($"table in {filename} doesn't have header row");
                }
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



        public void InsertRow(ExpressionOperand[] row)
        {
            if (row.Length != columnNames!.Length)
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


        public int DeleteRows(List<int> rowIndexesToDelete)
        {
            int deletedCount = 0;

            // rename the file
            string newFileName = filename + ".bak";
            File.Delete(newFileName);
            File.Move(filename, newFileName);

            var lines = File.ReadLines(newFileName);
            int lineNumber = 0;

            using StreamWriter writer = new(filename);

            columnTypes = GetColumnTypes(FullTableName.FromTableName(tableName));

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

