using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL.Engines
{

    public class DynamicCSV : IEngineSource
    {
        private string filename;
        private string tableName;

        // list of column names
        private FullColumnName[] columnNames;

        // list of types
        private ExpressionNodeType[] columnTypes;

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
                    columnTypes = new ExpressionNodeType[fields.Length];
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        switch (fields[i])
                        {
                            case "DECIMAL":
                                columnTypes[i] = ExpressionNodeType.DECIMAL;
                                break;

                            case "INTEGER":
                                columnTypes[i] = ExpressionNodeType.INTEGER;
                                break;

                            case "VARCHAR":
                                columnTypes[i] = ExpressionNodeType.VARCHAR;
                                break;

                            case "NVARCHAR":
                                columnTypes[i] = ExpressionNodeType.NVARCHAR;
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
                            case ExpressionNodeType.DECIMAL:
                                newRow[i] = new ExpressionOperandDecmial(Double.Parse(fields[i]));
                                break;

                            case ExpressionNodeType.VARCHAR:
                                newRow[i] = new ExpressionOperandVARCHAR(fields[i]);
                                break;

                            case ExpressionNodeType.NVARCHAR:
                                newRow[i] = new ExpressionOperandNVARCHAR(fields[i]);
                                break;

                            case ExpressionNodeType.INTEGER:
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
    }
}

