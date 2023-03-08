﻿namespace JankSQL
{
    public class FullColumnName
    {
        private readonly string columnName;

        private readonly string? serverName;
        private readonly string? schemaName;
        private readonly string? tableName;

        private FullColumnName(string columnName)
        {
            this.columnName = columnName;
        }

        private FullColumnName(string? serverName, string? schemaName, string? tableName, string columnName)
        {
            this.serverName = serverName;
            this.schemaName = schemaName;
            this.tableName = tableName;
            this.columnName = columnName;
        }

        public string? TableNameOnly
        {
            get { return tableName; }
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
                hash = (hash * 31) + serverName.GetHashCode();
            if (schemaName != null)
                hash = (hash * 31) + schemaName.GetHashCode();
            if (tableName != null)
                hash = (hash * 31) + tableName.GetHashCode();
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
            FullColumnName fcnNew = new FullColumnName(serverName, schemaName, newTableName, columnName);
            return fcnNew;
        }
    }
}
