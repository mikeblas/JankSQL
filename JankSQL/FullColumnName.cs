
namespace JankSQL
{
    public class FullColumnName
    {
        string? serverName;
        string? schemaName;
        string? tableName;
        readonly string columnName;

        FullColumnName(string columnName)
        {
            this.columnName = columnName;
        }

        public static FullColumnName FromContext(TSqlParser.Full_column_nameContext context)
        {
            var r = new FullColumnName(GetEffectiveName(context.column_name.GetText()));
            r.serverName = (context.server != null) ? GetEffectiveName(context.server.GetText()) : null;
            r.schemaName = (context.schema != null) ? GetEffectiveName(context.schema.GetText()) : null;
            r.tableName = (context.tablename != null) ? GetEffectiveName(context.tablename.GetText()) : null;
            return r;
        }

        public static FullColumnName FromColumnName(string columnName)
        {
            var r = new FullColumnName(GetEffectiveName(columnName));
            return r;
        }

        public static FullColumnName FromTableColumnName(string tableName, string columnName)
        {
            var r = new FullColumnName(GetEffectiveName(columnName));
            r.tableName = GetEffectiveName(tableName);
            return r;
        }

        public override bool Equals(object? o)
        {
            if (o is not FullColumnName other)
                return false;

            if (other.serverName != null && !other.serverName.Equals(this.serverName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (other.schemaName != null && !other.schemaName.Equals(this.schemaName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            if (other.tableName != null && !other.tableName.Equals(this.tableName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            bool ret = other.columnName.Equals(this.columnName, StringComparison.InvariantCultureIgnoreCase);
            return ret;
        }

        public override int GetHashCode()
        {
            int hash = 19;

            if (serverName != null)
                hash = hash * 31 + serverName.GetHashCode();
            if (schemaName != null)
                hash = hash * 31 + schemaName.GetHashCode();
            if (tableName != null)
                hash = hash * 31 + tableName.GetHashCode();
            hash = hash * 31 + columnName.GetHashCode();

            return hash;
        }

        public string ColumnNameOnly()
        {
            return columnName;
        }

        public override string ToString()
        {
            //REVIEW: is this right? could be that serverName != null but schemaName == null, and ...
            string ret = "";
            if (serverName != null)
                ret += $"[{serverName}]";
            if (schemaName != null)
            {
                if (ret.Length > 0)
                    ret += ".";
                ret += $"[{schemaName}]";
            }
            if (tableName != null)
            {
                if (ret.Length > 0)
                    ret += ".";
                ret += $"[{tableName}]";
            }

            if (ret.Length > 0)
                ret += ".";
            ret += $"[{columnName}]";

            return ret;
        }

        private static string GetEffectiveName(string objectName)
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges
            if (objectName[0] != '[' || objectName[^1] != ']')
                return objectName;

            return objectName[1..^1];
        }
    }
}
