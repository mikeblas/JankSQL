using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        private int depth = 0;

        readonly ExecutionContext executionContext = new();
        SelectContext? selectContext;
        PredicateContext? predicateContext;

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

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");
            if (predicateContext == null)
                throw new InternalErrorException("Expected a PredicateContext");

            selectContext.PredicateContext = predicateContext;
            predicateContext = null;

            executionContext.ExecuteContexts.Add(selectContext);
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            base.EnterSelect_statement(context);

            selectContext = new SelectContext(context);
            predicateContext = new PredicateContext();

        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");

            selectContext.SelectListContext = new SelectListContext(context);
        }

        public override void ExitSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.ExitSelect_list(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");
            if (selectContext.SelectListContext == null)
                throw new InternalErrorException("Expected a SelectListContext");

            foreach (var elem in context.select_list_elem())
            {
                FullColumnName? fcn = null;
                Expression? x;

                if (elem.column_elem() != null)
                {
                    ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromContext(elem.column_elem().full_column_name()));
                    x = new();
                    x.Add(n);

                    fcn = FullColumnName.FromContext(elem.column_elem().full_column_name());
                }
                else if (elem.expression_elem() != null)
                {
                    Console.WriteLine("NNN: Got expression");

                    if (elem.expression_elem().as_column_alias() != null)
                    {
                        fcn = FullColumnName.FromColumnName(elem.expression_elem().as_column_alias().GetText());
                        // throw new NotImplementedException("Can't handle AS alias");
                    }

                    x = GobbleExpression(elem.expression_elem().expression());
                }
                else if (elem.asterisk() != null)
                {
                    Console.WriteLine("asterisk!");
                    continue;
                    //TODO: what should this do?
                }
                else
                {
                    Console.WriteLine("Don't know this type");
                    continue;
                }

                if (fcn != null)
                    selectContext.SelectListContext.AddRowsetColumnName(fcn);
                else
                    selectContext.SelectListContext.AddUnknownRowsetColumnName();

                Console.WriteLine($"NNN:   {String.Join(" ", x)}");
                selectContext.EndSelectListExpressionList(x);
            }
        }

        Expression GobbleExpression(TSqlParser.ExpressionContext expr)
        {
            Expression x = new();
            List<object> stack = new();
            stack.Add(expr);

            while (stack.Count > 0)
            {
                // object rule = stack.Pop();
                object rule = stack[^1];
                stack.RemoveAt(stack.Count - 1);
                if (rule is TSqlParser.Primitive_expressionContext)
                {
                    TSqlParser.Primitive_expressionContext context = (TSqlParser.Primitive_expressionContext)(rule);
                    if (context.constant().FLOAT() is not null)
                    {
                        string str = context.constant().FLOAT().GetText();
                        Console.WriteLine($"constant: '{str}'");
                        bool isNegative = context.constant().sign() is not null;
                        ExpressionNode n = ExpressionOperand.DecimalFromString(isNegative, str);
                        x.Add(n);
                    }
                    else if (context.constant().DECIMAL() is not null)
                    {
                        string str = context.constant().DECIMAL().GetText();
                        Console.WriteLine($"constant: '{str}'");
                        bool isNegative = context.constant().sign() is not null;

                        ExpressionOperandType t = ExpressionOperand.IntegerOrDecimal(str);
                        ExpressionNode n;

                        if (t == ExpressionOperandType.INTEGER)
                            n = ExpressionOperand.IntegerFromString(isNegative, str);
                        else
                            n = ExpressionOperand.DecimalFromString(isNegative, str);

                        x.Add(n);
                    }
                    else if (context.constant().STRING() is not null)
                    {
                        // up to us to decide if its NVARCHAR or not
                        string str = context.constant().STRING().GetText();
                        if (str[0] == 'N')
                        {
                            Console.WriteLine($"constant: '{context.constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.NVARCHARFromStringContext(str);
                            x.Add(n);
                        }
                        else
                        {
                            Console.WriteLine($"constant: '{context.constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.VARCHARFromStringContext(str);
                            x.Add(n);
                        }
                    }
                }
                else if (rule is TSqlParser.Function_callContext)
                {
                    TSqlParser.Function_callContext context = (TSqlParser.Function_callContext)rule;
                    Console.WriteLine("Function_callContext");
                    string? functionNane = null;
                    int firstTop = stack.Count;

                    for (int i = context.ChildCount - 1; i >= 0; i--)
                    {
                        IParseTree childContext = context.GetChild(i);

                        if (childContext is TSqlParser.Scalar_function_nameContext scalarContext)
                        {
                            functionNane = scalarContext.func_proc_name_server_database_schema().func_proc_name_database_schema().func_proc_name_schema().procedure.GetText();
                            ExpressionNode n = new ExpressionOperator(functionNane);
                            stack.Insert(firstTop, n);
                        }
                        else if (childContext is TSqlParser.ExpressionContext)
                        {
                            stack.Add(childContext);
                        }
                        else if (childContext is TSqlParser.Expression_listContext exprContext)
                        {
                            for (int e = exprContext.expression().Length - 1; e >= 0; e--)
                            {
                                stack.Add(exprContext.expression()[e]);
                            }
                        }
                        else if (childContext is TSqlParser.Aggregate_windowed_functionContext awfc)
                        {
                            AggregateContext agg = GobbleAggregateFunctionContext(awfc);

                            // throw new NotImplementedException("can't yet handle AWFC in expresion");
                            ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromColumnName("AGGREGATION_OUTPUT"));
                            x.Add(n);
                        }
                        else
                        {
                            throw new NotImplementedException("Unknown function_call type");
                        }
                    }

                }
                else if (rule is TSqlParser.ExpressionContext xContext)
                {
                    if (xContext.op != null)
                    {
                        Console.WriteLine($"operator: '{xContext.op.Text}'");
                        ExpressionNode n = new ExpressionOperator(xContext.op.Text);
                        stack.Add(n);
                    }

                    if (xContext.expression().Length > 1)
                        stack.Add(xContext.expression()[1]);
                    if (xContext.expression().Length > 0)
                        stack.Add(xContext.expression()[0]);

                    if (xContext.primitive_expression() != null)
                        stack.Add(xContext.primitive_expression());
                    if (xContext.bracket_expression() != null)
                        stack.Add(xContext.bracket_expression().expression());

                    if (xContext.function_call() != null)
                        stack.Add(xContext.function_call());

                    if (xContext.full_column_name() != null)
                        stack.Add(xContext.full_column_name());

                }
                else if (rule is ExpressionNode xNode)
                {
                    x.Add(xNode);
                }
                else if (rule is TSqlParser.Full_column_nameContext fullColumn)
                {
                    ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromContext(fullColumn));
                    x.Add(n);
                }
                else
                {
                    Console.WriteLine($"don't know {rule}");
                }
            }

            return x;
        }



        public override void ExitJoin_part([NotNull] TSqlParser.Join_partContext context)
        {
            base.ExitJoin_part(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");
            if (predicateContext == null)
                throw new InternalErrorException("Expected a PredicateContext");


            // figure out which join type
            if (context.cross_join() != null)
            {
                // CROSS Join!

                FullTableName otherTableName = FullTableName.FromTableNameContext(context.cross_join().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                Console.WriteLine($"CROSS JOIN On {otherTableName}");

                JoinContext jc = new(JoinType.CROSS_JOIN, otherTableName);
                PredicateContext pcon = new();
                selectContext.AddJoin(jc, pcon);
            }
            else if (context.join_on() != null)
            {
                Expression x = GobbleSearchCondition(context.join_on().search_condition());
                PredicateContext pcon = new();
                pcon.EndPredicateExpressionList(x);

                // ON join
                FullTableName otherTableName = FullTableName.FromTableNameContext(context.join_on().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name());
                Console.WriteLine($"INNER JOIN On {otherTableName}");

                JoinContext jc = new(JoinType.INNER_JOIN, otherTableName);
                selectContext.AddJoin(jc, pcon);
            }
            else 
            {
                throw new NotImplementedException("unsupported JOIN type enountered");
            }
        }

    }
}

