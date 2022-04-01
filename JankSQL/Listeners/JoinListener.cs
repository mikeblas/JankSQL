namespace JankSQL
{
    using Antlr4.Runtime.Misc;
    using JankSQL.Contexts;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitJoin_part([NotNull] TSqlParser.Join_partContext context)
        {
            base.ExitJoin_part(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");

            // figure out which join type
            if (context.cross_join() != null)
            {
                // CROSS Join!

                FullTableName otherTableName = FullTableName.FromTableNameContext(context.cross_join().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                Console.WriteLine($"CROSS JOIN On {otherTableName}");

                JoinContext jc = new (JoinType.CROSS_JOIN, otherTableName);
                PredicateContext pcon = new ();
                selectContext.AddJoin(jc, pcon);
            }
            else if (context.join_on() != null)
            {
                Expression x = GobbleSearchCondition(context.join_on().search_condition());
                PredicateContext pcon = new (x);

                // ON join
                FullTableName otherTableName = FullTableName.FromTableNameContext(context.join_on().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                Console.WriteLine($"INNER JOIN On {otherTableName}");

                JoinContext jc = new (JoinType.INNER_JOIN, otherTableName);
                selectContext.AddJoin(jc, pcon);
            }
            else
            {
                throw new NotImplementedException("unsupported JOIN type enountered");
            }
        }

    }
}

