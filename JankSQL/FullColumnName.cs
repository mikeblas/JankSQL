namespace JankSQL
{
    public class FullColumnName: IComparable, IComparable<FullColumnName>
    {
        private readonly string columnName;

        private readonly string? serverName;
        private readonly string? schemaName;

        private FullColumnName(string columnName)
        {
            this.columnName = columnName;
        }

        private FullColumnName(string? serverName, string? schemaName, string? tableName, string columnName)
        {
            this.serverName = serverName;
            this.schemaName = schemaName;
            this.TableNameOnly = tableName;
            this.columnName = columnName;
        }

        public string? TableNameOnly { get; }

        public override bool Equals(object? o)
        {
            if (o is not FullColumnName other)
                return false;

            // InvariantCultureIgnoreCase so that identifier names can be localized
            if (other.serverName != null && !other.serverName.Equals(this.serverName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (other.schemaName != null && !other.schemaName.Equals(this.schemaName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (other.TableNameOnly != null && !other.TableNameOnly.Equals(this.TableNameOnly, StringComparison.InvariantCultureIgnoreCase))
                return false;

            bool ret = other.columnName.Equals(this.columnName, StringComparison.InvariantCultureIgnoreCase);
            return ret;
        }

        public override int GetHashCode()
        {
            int hash = 19;

            if (serverName != null)
                hash = (hash * 31) + serverName.GetHashCode();
            if (schemaName != null)
                hash = (hash * 31) + schemaName.GetHashCode();
            if (TableNameOnly != null)
                hash = (hash * 31) + TableNameOnly.GetHashCode();
            hash = (hash * 31) + columnName.GetHashCode();

            return hash;
        }

        public string ColumnNameOnly()
        {
            return columnName;
        }

        public override string ToString()
        {
            //REVIEW: is this right? could be that serverName != null but schemaName == null, and ...
            string ret = string.Empty;
            if (serverName != null)
                ret += $"[{serverName}]";
            if (schemaName != null)
            {
                if (ret.Length > 0)
                    ret += ".";
                ret += $"[{schemaName}]";
            }

            if (TableNameOnly != null)
            {
                if (ret.Length > 0)
                    ret += ".";
                ret += $"[{TableNameOnly}]";
            }

            if (ret.Length > 0)
                ret += ".";
            ret += $"[{columnName}]";

            return ret;
        }


        internal static FullColumnName FromContext(TSqlParser.Full_column_nameContext context)
        {
            string columnName = ParseHelpers.StringFromIDContext(context.id_());

            string? serverName = null;
            string? schemaName = null;
            string? tableName = null;

            if (context.full_table_name() != null)
            {
                serverName = ParseHelpers.PossibleStringFromIDContext(context.full_table_name().server);
                schemaName = ParseHelpers.PossibleStringFromIDContext(context.full_table_name().schema);
                tableName = ParseHelpers.PossibleStringFromIDContext(context.full_table_name().table);
            }

            return new FullColumnName(serverName, schemaName, tableName, columnName);
        }

        internal static FullColumnName FromIDContext(TSqlParser.Id_Context context)
        {
            var r = new FullColumnName(ParseHelpers.StringFromIDContext(context));
            return r;
        }

        internal static FullColumnName FromColumnName(string columnName)
        {
            var r = new FullColumnName(columnName);
            return r;
        }

        internal static FullColumnName FromTableColumnName(string tableName, string columnName)
        {
            var r = new FullColumnName(null, null, tableName, columnName);
            return r;
        }

        internal FullColumnName ApplyTableAlias(string newTableName)
        {
            FullColumnName fcnNew = new (serverName, schemaName, newTableName, columnName);
            return fcnNew;
        }

        public int CompareTo(object? obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            FullColumnName other = obj as FullColumnName;
            if (other == null)
                throw new ArgumentException("Can't compare to other type");

            if (this == other)
                return 0;

            return CompareTo(other);
        }

        public int CompareTo(FullColumnName? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (this == other)
                return 0;

            int r;
            r = SafeStringCompare(other.serverName, this.serverName);
            if (r != 0)
                return r;

            r = SafeStringCompare(other.schemaName, this.schemaName);
            if (r != 0)
                return r;

            r = SafeStringCompare(other.tableName, this.tableName);
            if (r != 0)
                return r;

            r = SafeStringCompare(other.columnName, this.columnName);
            return r;
        }

        protected static int SafeStringCompare(string? left, string? right)
        {
            if (left == null && right == null)
                return 0;

            if (left == null)
                return 1;
            if (right == null)
                return -1;

            return string.Compare(left, right, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
