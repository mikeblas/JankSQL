using Antlr4.Runtime.Misc;

namespace JankSQL.Listeners
{
    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitCreate_table([NotNull] TSqlParser.Create_tableContext context)
        {
            base.ExitCreate_table(context);

            var cdtcs = context.column_def_table_constraints();
            var cdtc = cdtcs.column_def_table_constraint();

            foreach (var c in cdtc)
            {
                var cd = c.column_definition();
                var id = cd.id_();
                var dt = cd.data_type();
                var id0 = id[0];

                if (dt.unscaled_type is not null)
                {
                    string typeName = (dt.unscaled_type.ID() is not null) ? dt.unscaled_type.ID().ToString() : dt.unscaled_type.keyword().GetText();

                    Console.Write($"{id0.ID()}, {typeName} ");
                    if (typeName.Equals("INTEGER", StringComparison.OrdinalIgnoreCase) || typeName.Equals("INT", StringComparison.OrdinalIgnoreCase))
                        Console.WriteLine("It's an integer!");
                }
                else
                {
                    Console.Write($"{id0.ID()}, {dt.ext_type.keyword().GetText()} ");

                    // null or not, if it's VARCHAR or not.
                    var dktvc = dt.ext_type.keyword().VARCHAR();
                    var dktnvc = dt.ext_type.keyword().NVARCHAR();
                    // TSqlParser.VARCHAR

                    if (dt.prec is not null)
                    {
                        Console.Write($"({dt.scale.Text}, {dt.prec.Text}) ");
                    }
                    else
                    {
                        Console.Write($"({dt.scale.Text}) ");
                    }
                }

                if (cd.null_notnull() == null || cd.null_notnull().NULL_() == null)
                    Console.WriteLine("NULL");
                else
                    Console.WriteLine("NOT NULL");
            }
        }

    }
}
