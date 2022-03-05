using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Load()
        {
            var lines = File.ReadLines(filename);
            int lineNumber = 0;

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
                else if (lineNumber == 1)
                {
                    columnTypes = new ExpressionOperandType[fields.Length];
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        switch (fields[i])
                        {
                            case "DECIMAL":
                                columnTypes[i] = ExpressionOperandType.DECIMAL;
                                break;

                            case "INTEGER":
                                columnTypes[i] = ExpressionOperandType.INTEGER;
                                break;

                            case "VARCHAR":
                                columnTypes[i] = ExpressionOperandType.VARCHAR;
                                break;

                            case "NVARCHAR":
                                columnTypes[i] = ExpressionOperandType.NVARCHAR;
                                break;
                        }
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
            // get the first two lines of the file
            var fileStream = new FileStream(filename, FileMode.Open);
            string[] lines = new string[2];
            using (var reader = new StreamReader(fileStream))
            {
                for (int i = 0; i < 2; i++)
                    lines[i] = reader.ReadLine();
            }
            fileStream.Close();

            // delete the file
            File.Delete(filename);

            // re-create it with those two lines
            using StreamWriter writer = new(filename);
            for (int i = 0; i < 2; i++)
                writer.WriteLine(lines[i]);
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
            DynamicCSV sysTables = new DynamicCSV("sys_tables.csv", "systables");
            sysTables.Load();

            string foundFileName = FileFromSysTables(sysTables, tableName.TableName);
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
            string columns = String.Join(',', columnNames.Select(x => x.ColumnNameOnly()));
            string types = String.Join(',', columnTypes);
            using StreamWriter file = new(fileName);
            file.WriteLine(columns);
            file.WriteLine(types);
            file.Close();

            // add row to sys_tables
            ExpressionOperand[] newRow = new ExpressionOperand[2];
            newRow[idxFile] = ExpressionOperand.NVARCHARFromString(fileName);
            newRow[idxName] = ExpressionOperand.NVARCHARFromString(tableName.TableName);

            sysTables.InsertRow(newRow);
        }
    }
}

