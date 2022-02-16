using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public class MyParserListener : TSqlParserBaseListener
    {
        private int depth = 0;

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
            var expressions = context.query_expression();
            var querySpecs = expressions.query_specification();
            var sourceTable = querySpecs.table_sources().table_source().First().GetText();
            Console.WriteLine($"ExitSelect_Statement: {sourceTable}");

            string effectiveName = Program.GetEffectiveName(sourceTable);


            // get systables
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv");
            sysTables.Load();

            // is this source table in there?
            int idxName = sysTables.ColumnIndex("table_name");
            int idxFile = sysTables.ColumnIndex("file_name");

            int foundRow = -1;
            for (int i = 0; i < sysTables.RowCount; i++)
            {
                if (sysTables.Row(i)[idxName].Equals(effectiveName, StringComparison.InvariantCultureIgnoreCase))
                {
                    foundRow = i;
                    break;
                }
            }

            if (foundRow == -1)
                Console.WriteLine($"Table {effectiveName} does not exist");
            else
            {
                // load that table
                Engines.DynamicCSV table = new Engines.DynamicCSV(sysTables.Row(foundRow)[1]);
                table.Load();

                // get an effective column list ...
                List<string> effectiveColumns = new List<string>();
                foreach (var c in querySpecs.select_list().select_list_elem())
                {
                    if (c.asterisk() != null)
                    {
                        Console.WriteLine("Asterisk!");
                        for (int i = 0; i < table.ColumnCount; i++)
                        {
                            effectiveColumns.Add(table.ColumnName(i));

                        }
                    }
                    else if (c.column_elem() != null)
                    {
                        Console.WriteLine($"column element! {c.column_elem().full_column_name().column_name.SQUARE_BRACKET_ID()}");
                        effectiveColumns.Add(Program.GetEffectiveName(c.column_elem().full_column_name().column_name.SQUARE_BRACKET_ID().GetText()));
                    }
                }

                for (int i = 0; i < table.RowCount; i++)
                {
                    string[] thisRow = table.Row(i);
                    bool first = true;
                    foreach(string columnName in effectiveColumns)
                    {
                        int idx = table.ColumnIndex(columnName);
                        if (!first)
                            Console.Write(", ");
                        first = false;
                        Console.Write($"{thisRow[idx]}");
                    }
                    Console.WriteLine();
                }

                // for each row, for each column list ...
                // querySpecs.select_list().select_list_elem();

            }
        }

        List<ExpressionNode> expressionList = new List<ExpressionNode>();

        public override void ExitExpression_elem([NotNull] TSqlParser.Expression_elemContext context)
        {
            /*
            Console.WriteLine($"operator = {context.expression().op.Text}");
            ExpressionNode x = new ExpressionNode(context.expression().op.Text);
            expressionList.Add(x);
            */

            foreach (ExpressionNode n in expressionList)
            {
                Console.Write($"[{n.ToString()}] ");
            }
            Console.WriteLine();
            // expressionList.Clear();

            Stack<ExpressionNode> stack = new Stack<ExpressionNode>();

            do
            {
                foreach (ExpressionNode n in expressionList)
                {
                    if (n is ExpressionOperand)
                        stack.Push(n);
                    else
                    {
                        // it's an operator
                        ExpressionOperator oper = (ExpressionOperator)n;
                        ExpressionOperand r = oper.Evaluate(stack);
                        stack.Push(r);
                    }

                }
            } while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand) stack.Pop();
            Console.WriteLine($"==> [{result}]");

            base.ExitExpression_elem(context);
        }

        public override void ExitExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            Console.WriteLine($"operator = {context.op}");
            if (context.op != null)
            {
                ExpressionNode x = new ExpressionOperator(context.op.Text);
                expressionList.Add(x);
            }
            base.ExitExpression(context);
        }

        public override void ExitPrimitive_expression([NotNull] TSqlParser.Primitive_expressionContext context)
        {
            Console.WriteLine($"constant = {context.constant().DECIMAL()}");

            ExpressionNode x = ExpressionOperand.DecimalFromString(context.constant().DECIMAL().GetText());
            expressionList.Add(x);

            base.ExitPrimitive_expression(context);
        }

        public override void ExitSCALAR_FUNCTION([NotNull] TSqlParser.SCALAR_FUNCTIONContext context)
        {
            ExpressionNode x = new ExpressionOperator(context.scalar_function_name().GetText());
            expressionList.Add(x);

            base.ExitSCALAR_FUNCTION(context);
        }

        public override void EnterBracket_expression([NotNull] TSqlParser.Bracket_expressionContext context)
        {
            /*
            ExpressionNode x = new ExpressionOperator("(");
            expressionList.Add(x);
            */

            base.EnterBracket_expression(context);
        }

        public override void ExitSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
            expressionList.Clear();
            base.ExitSelect_list_elem(context);
        }

        public override void ExitBracket_expression([NotNull] TSqlParser.Bracket_expressionContext context)
        {
            /*
            ExpressionNode y = new ExpressionOperator(")");
            expressionList.Add(y);
            */

            base.ExitBracket_expression(context);
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
                    string? typeName = (dt.unscaled_type.ID() is not null) ? dt.unscaled_type.ID().ToString() : dt.unscaled_type.keyword().GetText();

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


