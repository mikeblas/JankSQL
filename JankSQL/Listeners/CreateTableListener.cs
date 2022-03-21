namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitCreate_table([NotNull] TSqlParser.Create_tableContext context)
        {
            base.ExitCreate_table(context);

            var tableName = FullTableName.FromTableNameContext(context.table_name());
            List<FullColumnName> columnNames = new ();
            List<ExpressionOperandType> columnTypes = new ();

            var cdtcs = context.column_def_table_constraints();
            var cdtc = cdtcs.column_def_table_constraint();

            foreach (var c in cdtc)
            {
                var cd = c.column_definition();
                var dt = cd.data_type();
                var id0 = cd.id_()[0];

                if (dt.unscaled_type is not null)
                {
                    string typeName = (dt.unscaled_type.ID() != null) ? dt.unscaled_type.ID().GetText() : dt.unscaled_type.keyword().GetText();

                    if (typeName == null)
                    {
                        throw new ExecutionException($"No typename found for column {id0.ID()}");
                    }

                    Console.Write($"{id0.ID()}, {typeName} ");
                    ExpressionOperandType columnType;
                    if (!ExpressionNode.TypeFromString(typeName, out columnType))
                        throw new ExecutionException($"Unknown column type {typeName} on column {id0.ID()}");

                    columnNames.Add(FullColumnName.FromColumnName(id0.ID().GetText()));
                    columnTypes.Add(columnType);
                }
                else
                {
                    string typeName = dt.ext_type.keyword().GetText();
                    Console.Write($"{id0.ID()}, {typeName} ");

                    ExpressionOperandType columnType;
                    if (!ExpressionNode.TypeFromString(dt.ext_type.keyword().GetText(), out columnType))
                        throw new ExecutionException($"Unknown column type {typeName} on column {id0.ID()}");

                    // null or not, if it's VARCHAR or not.
                    var dktvc = dt.ext_type.keyword().VARCHAR();
                    var dktnvc = dt.ext_type.keyword().NVARCHAR();

                    if (dktvc is not null)
                    {
                        columnNames.Add(FullColumnName.FromColumnName(id0.ID().GetText()));
                        columnTypes.Add(ExpressionOperandType.VARCHAR);
                    }
                    else if (dktnvc is not null)
                    {
                        columnNames.Add(FullColumnName.FromColumnName(id0.ID().GetText()));
                        columnTypes.Add(ExpressionOperandType.NVARCHAR);
                    }
                    else
                        throw new ExecutionException($"Unknown scaled column type {typeName} on column {id0.ID()}");

                    if (dt.prec is not null)
                        Console.Write($"({dt.scale.Text}, {dt.prec.Text}) ");
                    else
                        Console.Write($"({dt.scale.Text}) ");
                }

                if (cd.null_notnull() == null || cd.null_notnull().NULL_() == null)
                    Console.WriteLine("NULL");
                else
                    Console.WriteLine("NOT NULL");
            }

            CreateTableContext createContext = new CreateTableContext(tableName, columnNames, columnTypes);

            executionContext.ExecuteContexts.Add(createContext);
        }
    }
}
