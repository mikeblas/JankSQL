namespace JankSQL
{
    public sealed class FullTableName
    {
        private readonly string? linkedServerName;
        private readonly string? databaseName;
        private readonly string? schemaName;

        private FullTableName(string tableName)
        {
            this.TableNameOnly = tableName;
        }

        private FullTableName(string? linkedServerName, string? databaseName, string? schemaName, string tableName)
        {
            this.linkedServerName = linkedServerName;
            this.databaseName = databaseName;
            this.schemaName = schemaName;

            this.TableNameOnly = tableName;
        }

        internal string TableNameOnly { get; }

        public override int GetHashCode()
        {
            int hash = 19;

            if (linkedServerName != null)
                hash = (hash * 31) + linkedServerName.GetHashCode();
            if (databaseName != null)
                hash = (hash * 31) + databaseName.GetHashCode();
            if (schemaName != null)
                hash = (hash * 31) + schemaName.GetHashCode();
            if (TableNameOnly != null)
                hash = (hash * 31) + TableNameOnly.GetHashCode();

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

            if (TableNameOnly != null)
            {
                if (ret.Length > 0)
                    ret += ".";
                ret += $"[{TableNameOnly}]";
            }

            return ret;
        }


        internal static FullTableName FromTableNameContext(TSqlParser.Table_nameContext context)
        {
            string? databaseName = ParseHelpers.PossibleStringFromIDContext(context.database);
            string? schemaName = ParseHelpers.PossibleStringFromIDContext(context.schema);
            string tableName = ParseHelpers.StringFromIDContext(context.table);

            var r = new FullTableName(null, databaseName, schemaName, tableName);
            return r;
        }

        internal static FullTableName? FromPossibleTableNameContext(TSqlParser.Table_nameContext? context)
        {
            if (context == null)
                return null;
            return FromTableNameContext(context);
        }

        internal static FullTableName FromFullTableNameContext(TSqlParser.Full_table_nameContext context)
        {
            string? databaseName = ParseHelpers.PossibleStringFromIDContext(context.database);
            string? schemaName = ParseHelpers.PossibleStringFromIDContext(context.schema);
            string? linkedServerName = ParseHelpers.PossibleStringFromIDContext(context.linkedServer);
            string tableName = ParseHelpers.StringFromIDContext(context.table);

            var r = new FullTableName(linkedServerName, databaseName, schemaName, tableName);
            return r;
        }

        internal static FullTableName? FromTableAliasContext(TSqlParser.As_table_aliasContext? context)
        {
            if (context == null)
                return null;

            var r = new FullTableName(ParseHelpers.StringFromIDContext(context.table_alias().id_()));
            return r;
        }

        internal static FullTableName FromTableName(string tableName)
        {
            return new FullTableName(tableName);
        }
    }
}
