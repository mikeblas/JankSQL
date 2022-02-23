using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL.Engines
{
    public class DynamicCSV
    {
        private string filename;

        // list of column names
        private FullColumnName[] columnNames = null;

        // list of lines; each line is a list of values
        private List<string[]> values;

        public DynamicCSV(string filename)
        {
            this.filename = filename;
            this.values = new List<string[]>();
        }

        public void Load()
        {
            var lines = File.ReadLines(filename);
            bool firstLine = true;

            foreach (var line in lines)
            {
                string[] fields = line.Split(",");

                if (firstLine)
                {
                    columnNames = new FullColumnName[fields.Length];
                    for (int i = 0; i < fields.Length; ++i)
                    { 
                        FullColumnName fcn = FullColumnName.FromColumnName(fields[i]);
                        columnNames[i] = fcn;
                    }
                    firstLine = false;
                }
                else
                {
                    values.Add(fields);
                }
            }

        }

        public int RowCount { get { return values.Count; } }

        public int ColumnCount { get { return columnNames.Length; } }


        public string[] Row(int n)
        {
            return values[n];
        }

        internal FullColumnName ColumnName(int n)
        {
            return columnNames[n];
        }

        public int ColumnIndex(string columnName)
        {
            FullColumnName fcnMatch = FullColumnName.FromColumnName(columnName);
            for (int i = 0; i < columnNames.Length; i++)
            {
                if (fcnMatch.Equals(columnNames[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}

