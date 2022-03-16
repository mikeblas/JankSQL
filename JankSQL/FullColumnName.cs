using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public class FullColumnName
    {
        string? serverName;
        string? schemaName;
        string? tableName;
        string columnName;

        FullColumnName(string columnName)
        {
            this.columnName = columnName;
        }

        public static FullColumnName FromContext(TSqlParser.Full_column_nameContext context)
        {
            var r = new FullColumnName(Program.GetEffectiveName(context.column_name.GetText()));
            r.serverName = (context.server != null) ? Program.GetEffectiveName(context.server.GetText()) : null;
            r.schemaName = (context.schema != null) ? Program.GetEffectiveName(context.schema.GetText()) : null;
            r.tableName = (context.tablename != null) ? Program.GetEffectiveName(context.tablename.GetText()) : null;
            return r;
        }

        public static FullColumnName FromColumnName(string columnName)
        {
            var r = new FullColumnName(Program.GetEffectiveName(columnName));
            return r;
        }

        public static FullColumnName FromTableColumnName(string tableName, string columnName)
        {
            var r = new FullColumnName(Program.GetEffectiveName(columnName));
            r.tableName = Program.GetEffectiveName(tableName);
            return r;
        }

        public override bool Equals(object? o)
        {
            var other = o as FullColumnName;
            if (other == null)
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
    }
}
