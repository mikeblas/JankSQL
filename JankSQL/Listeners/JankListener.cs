namespace JankSQL
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;

    using JankSQL.Contexts;
    using ExecutionContext = JankSQL.Contexts.ExecutionContext;

    public partial class JankListener : TSqlParserBaseListener
    {
        private readonly ExecutionContext executionContext = new ();

        private int depth = 0;

        private SelectContext? selectContext;
        private PredicateContext? predicateContext;

        internal ExecutionContext ExecutionContext
        {
            get { return executionContext; }
        }

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
                    x = new ();
                    x.Add(n);

                    fcn = FullColumnName.FromContext(elem.column_elem().full_column_name());
                }
                else if (elem.expression_elem() != null)
                {
                    if (elem.expression_elem().as_column_alias() != null)
                    {
                        fcn = FullColumnName.FromColumnName(elem.expression_elem().as_column_alias().GetText());
                    }

                    x = GobbleExpression(elem.expression_elem().expression());
                }
                else if (elem.asterisk() != null)
                {
                    Console.WriteLine("asterisk!");
                    continue;
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

                Console.WriteLine($"SelectListElement:   {string.Join(" ", x)}");
                selectContext.AddSelectListExpressionList(x);
            }
        }


        internal Expression GobbleExpression(TSqlParser.ExpressionContext expr)
        {
            Expression x = new ();
            List<object> stack = new ();
            stack.Add(expr);

            while (stack.Count > 0)
            {
                // object rule = stack.Pop();
                object rule = stack[^1];
                stack.RemoveAt(stack.Count - 1);
                if (rule is TSqlParser.Primitive_expressionContext primitiveContext)
                {
                    if (primitiveContext.constant().FLOAT() != null)
                    {
                        string str = primitiveContext.constant().FLOAT().GetText();
                        Console.WriteLine($"constant: '{str}'");
                        bool isNegative = primitiveContext.constant().sign() != null;
                        ExpressionNode n = ExpressionOperand.DecimalFromString(isNegative, str);
                        x.Add(n);
                    }
                    else if (primitiveContext.constant().DECIMAL() != null)
                    {
                        string str = primitiveContext.constant().DECIMAL().GetText();
                        Console.WriteLine($"constant: '{str}'");
                        bool isNegative = primitiveContext.constant().sign() != null;

                        ExpressionOperandType t = ExpressionOperand.IntegerOrDecimal(str);
                        ExpressionNode n;

                        if (t == ExpressionOperandType.INTEGER)
                            n = ExpressionOperand.IntegerFromString(isNegative, str);
                        else
                            n = ExpressionOperand.DecimalFromString(isNegative, str);

                        x.Add(n);
                    }
                    else if (primitiveContext.constant().STRING() != null)
                    {
                        // up to us to decide if its NVARCHAR or not
                        string str = primitiveContext.constant().STRING().GetText();
                        if (str[0] == 'N')
                        {
                            Console.WriteLine($"constant: '{primitiveContext.constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.NVARCHARFromStringContext(str);
                            x.Add(n);
                        }
                        else
                        {
                            Console.WriteLine($"constant: '{primitiveContext.constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.VARCHARFromStringContext(str);
                            x.Add(n);
                        }
                    }
                }
                else if (rule is TSqlParser.Function_callContext functionCallContext)
                {
                    Console.WriteLine($"Function_callContext with {functionCallContext.ChildCount} children");
                    string? functionNane;
                    int firstTop = stack.Count;

                    // why doesn't Function_callContext have any useful members?
                    // we must go after child nodes with GetChild() directly

                    IParseTree childContext = functionCallContext.GetChild(0);
                    if (childContext is TSqlParser.Scalar_function_nameContext scalarContext)
                    {
                        // get a function call
                        functionNane = scalarContext.func_proc_name_server_database_schema().func_proc_name_database_schema().func_proc_name_schema().procedure.GetText();
                        ExpressionNode n = new ExpressionOperator(functionNane);
                        stack.Insert(firstTop, n);

                        // and its argument list
                        if (functionCallContext.GetChild(2) is TSqlParser.Expression_listContext exprContext)
                        {
                            for (int e = exprContext.expression().Length - 1; e >= 0; e--)
                            {
                                stack.Add(exprContext.expression()[e]);
                            }
                        }
                    }
                    else if (childContext is TSqlParser.Aggregate_windowed_functionContext awfc)
                    {
                        if (selectContext == null)
                            throw new InternalErrorException("Expected a SelectContext");

                        AggregateContext agg = GobbleAggregateFunctionContext(awfc);
                        selectContext.AddAggregate(agg);

                        // throw new NotImplementedException("can't yet handle AWFC in expresion");
                        ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromColumnName(agg.ExpressionName));
                        x.Add(n);
                        x.ContainsAggregate = true;
                    }
                    else
                    {
                        Console.WriteLine($"functionCallContext: skpping {childContext.GetText()}");
                        // throw new NotImplementedException("Unknown function_call type");
                    }
                }
                else if (rule is TSqlParser.ExpressionContext xContext)
                {
                    if (xContext.op != null)
                    {
                        Console.WriteLine($"expressionContext: '{xContext.op.Text}'");
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

                JoinContext jc = new (JoinType.CROSS_JOIN, otherTableName);
                PredicateContext pcon = new ();
                selectContext.AddJoin(jc, pcon);
            }
            else if (context.join_on() != null)
            {
                Expression x = GobbleSearchCondition(context.join_on().search_condition());
                PredicateContext pcon = new ();
                pcon.EndPredicateExpressionList(x);

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

        public override void EnterOrder_by_clause([NotNull] TSqlParser.Order_by_clauseContext context)
        {
            base.EnterOrder_by_clause(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");

            OrderByContext obc = new ();

            foreach (var expr in context.order_by_expression())
            {
                Expression obx = GobbleExpression(expr.expression());
                Console.Write($"   {string.Join(",", obx.Select(x => "[" + x + "]"))} ");
                if (expr.DESC() != null)
                    Console.WriteLine("DESC");
                else
                    Console.WriteLine("ASC");

                obc.AddExpression(obx, expr.DESC() == null);
            }

            selectContext.OrderByContext = obc;
        }
    }
}

