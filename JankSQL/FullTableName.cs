namespace JankSQL
{
    public class FullTableName
    {
        private readonly string tableName;

        private string? linkedServerName;
        private string? schemaName;
        private string? databaseName;

        internal static FullTableName FromTableNameContext(TSqlParser.Table_nameContext context)
        {
            var r = new FullTableName(GetEffectiveName(context.table.GetText()));
            r.databaseName = (context.database != null) ? GetEffectiveName(context.database.GetText()) : null;
            r.schemaName = (context.schema != null) ? GetEffectiveName(context.schema.GetText()) : null;
            return r;
        }

        internal static FullTableName FromFullTableNameContext(TSqlParser.Full_table_nameContext context)
        {
            var r = new FullTableName(GetEffectiveName(context.table.GetText()));
            r.databaseName = (context.database != null) ? GetEffectiveName(context.database.GetText()) : null;
            r.schemaName = (context.schema != null) ? GetEffectiveName(context.schema.GetText()) : null;
            r.linkedServerName = (context.linkedServer != null) ? GetEffectiveName(context.linkedServer.GetText()) : null;
            return r;
        }

        internal static FullTableName FromTableName(string tableName)
        {
            return new FullTableName(tableName);
        }

        internal string TableName { get { return tableName;  } }

        FullTableName(string tableName)
        {
            this.tableName = tableName;
        }

        public override int GetHashCode()
        {
            int hash = 19;

            if (linkedServerName != null)
                hash = (hash * 31) + linkedServerName.GetHashCode();
            if (databaseName != null)
                hash = (hash * 31) + databaseName.GetHashCode();
            if (schemaName != null)
                hash = (hash * 31) + schemaName.GetHashCode();
            if (tableName != null)
                hash = (hash * 31) + tableName.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            //REVIEW: is this right? could be that serverName != null but schemaName == null, and ...
            string ret = string.Empty;
            if (linkedServerName != null)
                ret += $"[{linkedServerName}]";

            if (databaseName != null)
            {
                if (ret.Length > 0)
                    ret += ".";
                ret += $"[{databaseName}]";
            }

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
