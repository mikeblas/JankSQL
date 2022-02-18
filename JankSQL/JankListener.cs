using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public class JankListener : TSqlParserBaseListener
    {
        private int depth = 0;

        ExecutionContext executionContext = new ExecutionContext();
        SelectListContext selectListContext = null;

        internal ExecutionContext ExecutionContext { get { return executionContext; } }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            var s = new string(' ', depth);
            base.EnterEveryRule(context);
            Console.WriteLine($"{s}{context.GetType().Name}, {context.GetText()}");
            depth++;
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            var s = new string(' ', depth);
            base.ExitEveryRule(context);
            Console.WriteLine($"{s}{context.GetType().Name}, {context.GetText()}");
            depth--;
        }

        public override void ExitSelect_statement(TSqlParser.Select_statementContext context)
        {
            SelectContext selectContext = new SelectContext(context, selectListContext);
            selectListContext = null;
            executionContext.SelectContext = selectContext;
        }

        public override void EnterSelect_statement_standalone([NotNull] TSqlParser.Select_statement_standaloneContext context)
        {
            base.EnterSelect_statement_standalone(context);

        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);
            selectListContext = new SelectListContext(context);
        }


        public override void ExitExpression_elem([NotNull] TSqlParser.Expression_elemContext context)
        {
            /*
            Console.WriteLine($"operator = {context.expression().op.Text}");
            ExpressionNode x = new ExpressionNode(context.expression().op.Text);
            expressionList.Add(x);
            */

            foreach (ExpressionNode n in selectListContext.ExpressionList)
            {
                Console.Write($"[{n.ToString()}] ");
            }
            Console.WriteLine();
            selectListContext.EndExpressionList();
            // expressionList.Clear();

            base.ExitExpression_elem(context);
        }

        public override void ExitExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            Console.WriteLine($"operator = {context.op}");
            if (context.op != null)
            {
                ExpressionNode x = new ExpressionOperator(context.op.Text);
                selectListContext.ExpressionList.Add(x);
            }
            base.ExitExpression(context);
        }

        public override void ExitPrimitive_expression([NotNull] TSqlParser.Primitive_expressionContext context)
        {
            Console.WriteLine($"constant = {context.constant().DECIMAL()}");

            ExpressionNode x = ExpressionOperand.DecimalFromString(context.constant().DECIMAL().GetText());
            selectListContext.ExpressionList.Add(x);

            base.ExitPrimitive_expression(context);
        }

        public override void ExitSCALAR_FUNCTION([NotNull] TSqlParser.SCALAR_FUNCTIONContext context)
        {
            ExpressionNode x = new ExpressionOperator(context.scalar_function_name().GetText());
            selectListContext.ExpressionList.Add(x);

            base.ExitSCALAR_FUNCTION(context);

        }

        public override void ExitFull_column_name([NotNull] TSqlParser.Full_column_nameContext context)
        {
            ExpressionNode x = new ExpressionOperandFromColumn(Program.GetEffectiveName(context.column_name.GetText()));
            selectListContext.ExpressionList.Add(x);

            base.ExitFull_column_name(context);
        }

        public override void ExitSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
            string? rowsetColumnName = null;

            if (selectListContext.CurrentAlias != null)
            {
                rowsetColumnName = selectListContext.CurrentAlias;
            }
            else if (context.expression_elem() != null && context.expression_elem().column_alias() != null)
            {
                rowsetColumnName = context.expression_elem().column_alias().GetText();
            }
             else if (context.column_elem() != null)
            {
                rowsetColumnName = context.column_elem().full_column_name().GetText();
            }

            if (rowsetColumnName != null)
                selectListContext.AddRowsetColumnName(Program.GetEffectiveName(rowsetColumnName));
            else
                selectListContext.AddUnknownRowsetColumnName();

            base.ExitSelect_list_elem(context);
        }

        public override void ExitAs_column_alias([NotNull] TSqlParser.As_column_aliasContext context)
        {
            selectListContext.CurrentAlias = context.column_alias().GetText();
            Console.WriteLine($"alias == {context.column_alias().GetText()}");
            base.ExitAs_column_alias(context);
        }

        public override void ExitCase_expression(TSqlParser.Case_expressionContext context)
        {
            var elseMsg = context.elseExpr.GetText();
            Console.WriteLine($"Case ELSE expression: {elseMsg}");
        }

        public override void ExitDrop_table(TSqlParser.Drop_tableContext context)
        {
            var idNames = context.table_name().id_().Select(e => e.GetText());
            Console.WriteLine($"You're trying to delete {string.Join(".", idNames)}");
        }

        public override void ExitCreate_table(TSqlParser.Create_tableContext context)
        {
            var cdtcs = context.column_def_table_constraints();
            var cdtc = cdtcs.column_def_table_constraint();

            foreach(var c in cdtc)
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

        public ResultSet Execute()
        {
            return executionContext.Execute();
        }
    }
}


