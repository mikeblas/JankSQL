using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        private int depth = 0;

        ExecutionContext executionContext = new ExecutionContext();
        SelectContext? selectContext;

        Expression currentExpression = new();
        List<Expression> currentExpressionList;
        List<List<Expression>> currentExpressionListList;

        internal ExecutionContext ExecutionContext { get { return executionContext; } }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            base.EnterEveryRule(context);

            var s = new string(' ', depth);
            Console.WriteLine($"+{s}{context.GetType().Name}, {context.GetText()}");
            depth++;
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            base.ExitEveryRule(context);

            var s = new string(' ', depth);
            Console.WriteLine($"-{s}{context.GetType().Name}, {context.GetText()}");
            depth--;
        }

        public override void ExitSelect_statement(TSqlParser.Select_statementContext context)
        {
            base.ExitEveryRule(context);

            executionContext.ExecuteContexts.Add(selectContext);
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.EnterSelect_statement(context);

            selectContext = new SelectContext(context);
        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);

            selectContext.SelectListContext = new SelectListContext(context);
            currentExpressionList = new();
            currentExpressionListList = new();
        }

        public override void ExitExpression_elem([NotNull] TSqlParser.Expression_elemContext context)
        {
            base.ExitExpression_elem(context);

            Console.Write("Expression_element: ");
            foreach (ExpressionNode n in currentExpression)
            {
                Console.Write($"[{n.ToString()}] ");
            }
            Console.WriteLine();
        }

        public override void ExitExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.ExitExpression(context);

            if (context.op != null)
            {
                Console.WriteLine($"operator: '{context.op.Text}'");
                ExpressionNode x = new ExpressionOperator(context.op.Text);
                currentExpressionList[^1].Add(x);
            }
            else
                Console.WriteLine($"operator is null");


            currentExpressionList.Add(currentExpression);
            currentExpression = new();
        }

        public override void EnterExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.EnterExpression(context);
        }

        public override void EnterExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            base.EnterExpression_list(context);

            currentExpressionList = new();
        }

        public override void ExitExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            base.ExitExpression_list(context);

            currentExpressionListList.Add(currentExpressionList);
        }

        public override void ExitPrimitive_expression([NotNull] TSqlParser.Primitive_expressionContext context)
        {
            base.ExitPrimitive_expression(context);

            if (context.constant().FLOAT() is not null)
            {
                string str = context.constant().FLOAT().GetText();
                Console.WriteLine($"constant: '{str}'");
                bool isNegative = context.constant().sign() is not null;
                ExpressionNode x = ExpressionOperand.DecimalFromString(isNegative, str);
                currentExpression.Add(x);
            }
            else if (context.constant().DECIMAL() is not null)
            {
                string str = context.constant().DECIMAL().GetText();
                Console.WriteLine($"constant: '{str}'");
                bool isNegative = context.constant().sign() is not null;

                ExpressionOperandType t = ExpressionOperand.IntegerOrDecimal(str);
                ExpressionNode x;

                if (t == ExpressionOperandType.INTEGER)
                    x = ExpressionOperand.IntegerFromString(isNegative, str);
                else
                    x = ExpressionOperand.DecimalFromString(isNegative, str);

                currentExpression.Add(x);

            }
            else if (context.constant().STRING() is not null)
            {
                // up to us to decide if its NVARCHAR or not
                string str = context.constant().STRING().GetText();
                if (str[0] == 'N')
                {
                    Console.WriteLine($"constant: '{context.constant().STRING()}'");
                    ExpressionNode x = ExpressionOperand.NVARCHARFromStringContext(str);
                    currentExpression.Add(x);
                }
                else
                {
                    Console.WriteLine($"constant: '{context.constant().STRING()}'");
                    ExpressionNode x = ExpressionOperand.VARCHARFromStringContext(str);
                    currentExpression.Add(x);
                }
            }
        }

        public override void ExitComparison_operator([NotNull] TSqlParser.Comparison_operatorContext context)
        {
            base.ExitComparison_operator(context);
        }

        public override void ExitSCALAR_FUNCTION([NotNull] TSqlParser.SCALAR_FUNCTIONContext context)
        {
            base.ExitSCALAR_FUNCTION(context);

            ExpressionNode x = new ExpressionOperator(context.scalar_function_name().GetText());
            currentExpressionList[^1].Add(x);
        }

        public override void ExitSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
            base.ExitSearch_condition(context);

            Expression total = new();
            foreach (var x in currentExpressionList)
                total.AddRange(x);
            currentExpressionList = new();
            currentExpression = new();
            
            if (context.AND() != null)
            {
                Console.WriteLine("Got AND");
                ExpressionNode x = ExpressionBooleanOperator.GetAndOperator();
                total.Add(x);
                selectContext.EndAndCombinePredicateExpressionList(2, total);
            }
            else if (context.OR() != null)
            {
                Console.WriteLine("Got OR");
                ExpressionNode x = ExpressionBooleanOperator.GetOrOperator();
                total.Add(x);
                selectContext.EndAndCombinePredicateExpressionList(2, total);
            }
            else if (context.NOT(0) != null)
            {
                int n = 0;
                do
                {
                    Console.WriteLine("Got NOT");
                    ExpressionNode x = ExpressionBooleanOperator.GetNotOperator();
                    total.Add(x);
                    if (total.Count == 1)
                    {
                        selectContext.EndAndCombinePredicateExpressionList(1, total);
                    }
                    else
                    {
                        selectContext.EndPredicateExpressionList(total);
                    }
                } while (context.NOT(++n) != null);
            }
            else
            {
                Console.WriteLine("Got neither");
                selectContext.EndPredicateExpressionList(total);
            }
        }

        public override void ExitFull_column_name([NotNull] TSqlParser.Full_column_nameContext context)
        {
            base.ExitFull_column_name(context);

            ExpressionNode x = new ExpressionOperandFromColumn(FullColumnName.FromContext(context));
            currentExpression.Add(x);
        }

        public override void ExitSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
            base.ExitSelect_list_elem(context);

            // if this is an asterisk, it doesn't get an expression
            if (context.asterisk() == null)
            {
                FullColumnName? fcn = null;

                if (selectContext.SelectListContext.CurrentAlias != null)
                {
                    fcn = FullColumnName.FromColumnName(selectContext.SelectListContext.CurrentAlias);
                }
                else if (context.expression_elem() != null && context.expression_elem().column_alias() != null)
                {
                    fcn = FullColumnName.FromColumnName(context.expression_elem().column_alias().GetText());
                }
                else if (context.column_elem() != null)
                {
                    fcn = FullColumnName.FromContext(context.column_elem().full_column_name());
                    currentExpressionList.Add(currentExpression);
                    currentExpression = new();
                }

                if (fcn != null)
                    selectContext.SelectListContext.AddRowsetColumnName(fcn);
                else
                    selectContext.SelectListContext.AddUnknownRowsetColumnName();


                Expression total = new();
                foreach (var x in currentExpressionList)
                    total.AddRange(x);

                selectContext.EndSelectListExpressionList(total);
                currentExpression = new();
                currentExpressionList = new();
            }
        }

        public override void ExitPredicate([NotNull] TSqlParser.PredicateContext context)
        {
            base.ExitPredicate(context);

            Console.WriteLine($"Predicate comparison: '{context.comparison_operator().GetText()}'");
            ExpressionNode x = new ExpressionComparisonOperator(context.comparison_operator().GetText());
            Expression xl = new();
            xl.Add(x);
            currentExpressionList.Add(xl);
        }

        public override void ExitAs_column_alias([NotNull] TSqlParser.As_column_aliasContext context)
        {
            base.ExitAs_column_alias(context);

            selectContext.SelectListContext.CurrentAlias = context.column_alias().GetText();
            Console.WriteLine($"alias == {context.column_alias().GetText()}");
        }


        public override void ExitJoin_part([NotNull] TSqlParser.Join_partContext context)
        {
            base.ExitJoin_part(context);

            // figure out which join type
            if (context.cross_join() != null)
            {
                // CROSS Join!
                string str = context.cross_join().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name().id_()[0].GetText();
                Console.WriteLine($"CROSS JOIN On {str}");

                JoinContext jc = new JoinContext(JoinType.CROSS_JOIN, str);
                selectContext.AddJoin(jc);
            }
            else if (context.join_on() != null)
            {
                // ON join
                string str = context.join_on().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name().id_()[0].GetText();
                Console.WriteLine($"INNER JOIN On {str}");

                JoinContext jc = new JoinContext(JoinType.INNER_JOIN, str);
                selectContext.AddJoin(jc);
            }
            else 
            {
                Console.WriteLine("Don't know this join type");
            }
        }

    }
}

