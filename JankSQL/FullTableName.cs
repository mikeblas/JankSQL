using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class FullTableName
    {
        string? linkedServerName;
        string? schemaName;
        string? databaseName;
        string tableName;

        internal static FullTableName FromTableNameContext(TSqlParser.Table_nameContext context)
        {
            var r = new FullTableName(Program.GetEffectiveName(context.table.GetText()));
            r.databaseName = (context.database != null) ? Program.GetEffectiveName(context.database.GetText()) : null;
            r.schemaName = (context.schema != null) ? Program.GetEffectiveName(context.schema.GetText()) : null;
            return r;
        }

        internal static FullTableName FromFullTableNameContext(TSqlParser.Full_table_nameContext context)
        {
            var r = new FullTableName(Program.GetEffectiveName(context.table.GetText()));
            r.databaseName = (context.database != null) ? Program.GetEffectiveName(context.database.GetText()) : null;
            r.schemaName = (context.schema != null) ? Program.GetEffectiveName(context.schema.GetText()) : null;
            r.linkedServerName = (context.linkedServer != null) ? Program.GetEffectiveName(context.schema.GetText()) : null;
            return r;
        }

        FullTableName(string tableName)
        {
            this.tableName = tableName;
        }

        public override int GetHashCode()
        {
            int hash = 19;

            if (linkedServerName != null)
                hash = hash * 31 + linkedServerName.GetHashCode();
            if (databaseName != null)
                hash = hash * 31 + databaseName.GetHashCode();
            if (schemaName != null)
                hash = hash * 31 + schemaName.GetHashCode();
            if (tableName != null)
                hash = hash * 31 + tableName.GetHashCode();

            return hash;
        }

        public override string ToString()
        {
            //REVIEW: is this right? could be that serverName != null but schemaName == null, and ...
            string ret = "";
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

    }
}
