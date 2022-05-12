namespace JankSQL.Engines
{
    using System.Text;
    using JankSQL.Expressions;

    /// <summary>
    /// represents a table in a CSV engine.
    ///
    /// Handling of data types here is a bit weak:
    ///
    /// 1) Strings aren't escaped, so a string containing a comma will break the format.
    /// 2) Null values are stored as an empty field (,,) for any data type. This is indescernable from an empty string.
    ///
    /// </summary>
    public class DynamicCSVTable : IEngineTable
    {
        private readonly string filename;
        private readonly string tableName;
        private readonly IEngine engine;

        // list of lines; each line is a list of values
        private readonly List<Tuple> values;
        private readonly List<ExpressionOperandBookmark> bookmarks;

        // list of column names
        private FullColumnName[]? columnNames;

        // list of types
        private ExpressionOperandType[]? columnTypes;

        public DynamicCSVTable(string filename, string tableName, IEngine engine)
        {
            this.filename = filename;
            this.values = new List<Tuple>();
            this.bookmarks = new List<ExpressionOperandBookmark>();
            this.tableName = tableName;
            this.engine = engine;
        }

        public int ColumnCount
        {
            get { return columnNames!.Length; }
        }

        public void Load()
        {
            // ReadLines returns an iterator, which is disposable, but doesn't
            // explicitly implement IDispoasable so a direct "using' wont work to dispose the file handle
            // if the file isn't read until the end -- which it might not be, in the case of an exception.
            var lines = File.ReadLines(filename);
            using IDisposable disposable = (IDisposable)lines;

            int lineNumber = 0;

            ExpressionOperandType[] dictionaryColumnTypes = GetColumnTypes(FullTableName.FromTableName(tableName));
            columnTypes = new ExpressionOperandType[dictionaryColumnTypes.Length + 1];
            Array.Copy(dictionaryColumnTypes, columnTypes, dictionaryColumnTypes.Length);
            columnTypes[dictionaryColumnTypes.Length] = ExpressionOperandType.INTEGER;

            foreach (var line in lines)
            {
                string[] fileFields = line.Split(",");
                if (lineNumber == 0)
                {
                    string[] fields = new string[fileFields.Length];
                    Array.Copy(fileFields, fields, fileFields.Length);

                    columnNames = new FullColumnName[fields.Length];
                    for (int i = 0; i < fields.Length; ++i)
                    {
                        FullColumnName fcn = FullColumnName.FromTableColumnName(tableName, fields[i]);
                        columnNames[i] = fcn;
                    }
                }
                else
                {
                    Tuple newRow = Tuple.CreateEmpty(fileFields.Length);

                    for (int i = 0; i < fileFields.Length; i++)
                    {
                        if (fileFields[i].Length == 0)
                            newRow[i] = ExpressionOperand.NullLiteral();
                        else
                        {
                            switch (columnTypes[i])
                            {
                                case ExpressionOperandType.DECIMAL:
                                    newRow[i] = new ExpressionOperandDecimal(double.Parse(fileFields[i]));
                                    break;

                                case ExpressionOperandType.VARCHAR:
                                    newRow[i] = new ExpressionOperandVARCHAR(fileFields[i]);
                                    break;

                                case ExpressionOperandType.INTEGER:
                                    newRow[i] = new ExpressionOperandInteger(int.Parse(fileFields[i]));
                                    break;

                                case ExpressionOperandType.DATETIME:
                                    newRow[i] = new ExpressionOperandDateTime(DateTime.Parse(fileFields[i]).ToUniversalTime());
                                    break;

                                default:
                                    throw new NotImplementedException($"Can't support type {columnTypes[i]}");
                            }
                        }
                    }

                    values.Add(newRow);
                    ExpressionOperandBookmark bm = new (Tuple.FromSingleValue(lineNumber));
                    bookmarks.Add(bm);
                }

                lineNumber++;
            }
        }

        public IndexAccessor Index(string indexName)
        {
            throw new NotImplementedException();
        }

        public IndexAccessor? PredicateIndex(string indexName, IEnumerable<ExpressionComparisonOperator> comparisons, IEnumerable<Expression> expressions)
        {
            throw new NotImplementedException();
        }

        public Tuple RowFromBookmark(ExpressionOperandBookmark bmk)
        {
            throw new NotImplementedException();
        }

        public FullColumnName ColumnName(int n)
        {
            return columnNames![n];
        }

        public string? BestIndex(IEnumerable<(string columnName, bool isEquality)> accessColumns)
        {
            return null;
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
            using var fileStream = new FileStream(filename, FileMode.Open);
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
            using StreamWriter writer = new (filename);
            writer.WriteLine(firstLine);
            writer.Flush();
            writer.Close();

            Load();
        }

        public void InsertRow(Tuple row)
        {
            if (row.Length != columnNames!.Length)
                throw new ExecutionException($"table {tableName}: can't insert row with {row.Length} columns, need {columnNames.Length} columns");

            StringBuilder sb = new ();

            for (int i = 0; i < row.Length; i++)
            {
                //TODO: make a good representation of NULL in CSV
                string col;
                if (row[i].RepresentsNull)
                    col = string.Empty;
                else
                    col = row[i].AsString();
                if (i > 0)
                    sb.Append(',');
                sb.Append(col);
            }

            using StreamWriter sw = File.AppendText(this.filename);
            sw.WriteLine(sb.ToString());
        }


        public int DeleteRows(List<ExpressionOperandBookmark> bookmarksToDelete)
        {
            if (bookmarksToDelete.Count == 0)
                return 0;

            if (bookmarksToDelete[0].Tuple.Length != 1)
                throw new ArgumentException("CSV bookmarks have a single field");
            if (bookmarksToDelete[0].Tuple[0].NodeType != ExpressionOperandType.INTEGER)
                throw new ArgumentException("Bogus tuple type for CSV");

            // convert generic bookmarks to our integers
            List<int> rowIndexesToDelete = new ();
            foreach (var mark in bookmarksToDelete)
            {
                rowIndexesToDelete.Add(mark.Tuple[0].AsInteger());
            }

            int deletedCount = 0;

            // rename the file
            string newFileName = filename + ".bak";
            File.Delete(newFileName);
            File.Move(filename, newFileName);

            // ReadLines returns an iterator, which is disposable, but doesn't
            // explicitly implement IDispoasable so a direct "using' wont work to dispose the file handle
            // if the file isn't read until the end -- which it might not be, in the case of an exception.
            var lines = File.ReadLines(newFileName);
            using IDisposable disposable = (IDisposable)lines;
            int lineNumber = 0;

            using StreamWriter writer = new (filename);

            columnTypes = GetColumnTypes(FullTableName.FromTableName(tableName));

            foreach (var line in lines)
            {
                lineNumber++;

                bool keep = true;
                if (lineNumber > 1)
                {
                    // line number is an ordinal, rows to delete contains indexes
                    // line number 2 is index 0
                    if (rowIndexesToDelete.IndexOf(lineNumber - 1) != -1)
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

        public IEnumerator<RowWithBookmark> GetEnumerator()
        {
            DynamicCSVRowEnumerator e = new (values.GetEnumerator(), bookmarks.GetEnumerator());
            return e;
        }

        public void Commit()
        {
            // nothing here, since the implenentation auto-commits
        }

        public void Rollback()
        {
            // nothing here, since the implenentation auto-commits
        }

        private ExpressionOperandType[] GetColumnTypes(FullTableName tableName)
        {
            ExpressionOperandType[] ret;

            // we'd infinitely recurse if we had to look up columns for sys_columns by looking up sys_columns ...
            if (tableName.TableNameOnly.Equals("sys_columns", StringComparison.InvariantCultureIgnoreCase))
            {
                List<ExpressionOperandType> types = new ();

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
                IEngineTable sysColumns = engine.GetSysColumns();

                // table_name,column_name,column_type,index
                int tableNameIndex = sysColumns.ColumnIndex("table_name");
                int columnNameIndex = sysColumns.ColumnIndex("column_name");
                int typeIndex = sysColumns.ColumnIndex("column_type");
                int indexIndex = sysColumns.ColumnIndex("index");

                int matchCount = 0;
                foreach (var row in sysColumns)
                {
                    if (row.RowData[tableNameIndex].AsString().Equals(tableName.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
                        matchCount++;
                }

                ret = new ExpressionOperandType[matchCount];

                foreach (var row in sysColumns)
                {
                    if (row.RowData[tableNameIndex].AsString().Equals(tableName.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!ExpressionNode.TypeFromString(row.RowData[typeIndex].AsString(), out ExpressionOperandType operandType))
                            throw new ExecutionException($"unknown type {row.RowData[typeIndex].AsString()} in table {tableName.TableNameOnly}");

                        int index = row.RowData[indexIndex].AsInteger();
                        ret[index] = operandType;
                    }
                }
            }

            return ret;
        }
    }
}

