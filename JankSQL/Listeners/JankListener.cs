using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace JankSQL
{
    public partial class JankListener : TSqlParserBaseListener
    {
        private int depth = 0;

        ExecutionContext executionContext = new ExecutionContext();
        SelectContext? selectContext;
        PredicateContext? predicateContext;

        Expression currentExpression = new();
        List<Expression>? currentExpressionList;
        List<List<Expression>>? currentExpressionListList;

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
            currentExpressionList = new();
            currentExpressionListList = new();

        }

        public override void ExitSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.ExitSelect_list(context);

            List<List<Expression>> xList = new();
            foreach (var elem in context.select_list_elem())
            {
                if (elem.column_elem() != null)
                {
                    ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromContext(elem.column_elem().full_column_name()));
                    Expression x = new();
                    x.Add(n);
                    List<Expression> xl = new();
                    xl.Add(x);
                    xList.Add(xl);
                }
                else if (elem.expression_elem() != null)
                {
                    Console.WriteLine("NNN: Got expression");

                    Expression x = GobbleExpression(elem.expression_elem());
                    List<Expression> xl = new();
                    xl.Add(x);
                    xList.Add(xl);
                }
                else
                {
                    Console.WriteLine("Don't know this type");
                }
            }

            foreach (var x in xList)
            {
                Console.WriteLine($"NNN:   {String.Join(" ", x)}");
                selectContext.EndSelectListExpressionList(x[0]);
            }
        }

        Expression GobbleExpression(TSqlParser.Expression_elemContext expr)
        {
            Expression x = new();
            List<object> stack = new();
            stack.Add(expr.expression());
            // stack.Push(expr.expression());

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

                        if (childContext is TSqlParser.Scalar_function_nameContext)
                        {
                            TSqlParser.Scalar_function_nameContext scalarContext = (TSqlParser.Scalar_function_nameContext)(childContext);
                            functionNane = scalarContext.func_proc_name_server_database_schema().func_proc_name_database_schema().func_proc_name_schema().procedure.GetText();
                            ExpressionNode n = new ExpressionOperator(functionNane);
                            stack.Insert(firstTop, n);
                        }
                        else if (childContext is TSqlParser.ExpressionContext)
                        {
                            stack.Add(childContext);
                        }
                        else if (childContext is TSqlParser.Expression_listContext)
                        {
                            TSqlParser.Expression_listContext exprContext = (TSqlParser.Expression_listContext)(childContext);

                            for (int e = exprContext.expression().Length - 1; e >= 0; e--)
                            {
                                stack.Add(exprContext.expression()[e]);
                            }
                        }
                    }

                }
                else if (rule is TSqlParser.ExpressionContext)
                {
                    TSqlParser.ExpressionContext context = (TSqlParser.ExpressionContext)(rule);

                    if (context.op != null)
                    {
                        Console.WriteLine($"operator: '{context.op.Text}'");
                        ExpressionNode n = new ExpressionOperator(context.op.Text);
                        stack.Add(n);
                    }

                    if (context.expression().Length > 1)
                        stack.Add(context.expression()[1]);
                    if (context.expression().Length > 0)
                        stack.Add(context.expression()[0]);

                    if (context.primitive_expression() != null)
                        stack.Add(context.primitive_expression());
                    if (context.bracket_expression() != null)
                        stack.Add(context.bracket_expression().expression());

                    if (context.function_call() != null)
                        stack.Add(context.function_call());

                    if (context.full_column_name() != null)
                        stack.Add(context.full_column_name());

                }
                else if (rule is ExpressionNode)
                {
                    ExpressionNode n = (ExpressionNode)rule;
                    x.Add(n);
                }
                else if (rule is TSqlParser.Full_column_nameContext)
                {
                    TSqlParser.Full_column_nameContext context = (TSqlParser.Full_column_nameContext)(rule);
                    ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromContext(context));
                    x.Add(n);
                }
                else
                {
                    Console.WriteLine($"don't know {rule}");
                }
            }

            return x;
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

        int expressionDepth = 0;
        public override void ExitExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.ExitExpression(context);

            if (context.op != null)
            {
                Console.WriteLine($"operator: '{context.op.Text}'");
                ExpressionNode x = new ExpressionOperator(context.op.Text);
                currentExpression.Add(x);
            }
            else
                Console.WriteLine($"operator is null");

            expressionDepth--;

            if (expressionDepth == 0)
            {
                if (currentExpressionList == null)
                    throw new InternalErrorException("Expected a ExpressionList");

                currentExpressionList.Add(currentExpression);
                currentExpression = new();
            }
            Console.WriteLine($"Expression depth: {expressionDepth}");
        }

        public override void EnterExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.EnterExpression(context);

            expressionDepth++;
            Console.WriteLine($"Expression depth: {expressionDepth}");
        }

        int expressionListDepth = 0;

        public override void EnterExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            base.EnterExpression_list(context);
            expressionListDepth++;
            Console.WriteLine($"ExpressionList depth: {expressionListDepth}");
        }

        public override void ExitExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            base.ExitExpression_list(context);

            expressionListDepth--;
            if (expressionListDepth == 0)
            {
                if (currentExpressionListList == null)
                    throw new InternalErrorException("Expected a ExpressionListList");
                if (currentExpressionList == null)
                    throw new InternalErrorException("Expected a ExpressionList");

                currentExpressionListList.Add(currentExpressionList);
                currentExpressionList = new();
            }

            Console.WriteLine($"ExpressionList depth: {expressionListDepth}");
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

            ExpressionNode n = new ExpressionOperator(context.scalar_function_name().GetText());
            currentExpression.Add(n);
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

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");
            if (selectContext.SelectListContext == null)
                throw new InternalErrorException("Expected a SelectListContext");
            if (currentExpressionList == null)
                throw new InternalErrorException("Expected a ExpressionList");

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

                if (false)
                {
                    Expression total = new();
                    foreach (var x in currentExpressionList)
                        total.AddRange(x);

                    selectContext.EndSelectListExpressionList(total);
                    currentExpression = new();
                    currentExpressionList = new();
                }
            }
        }

        public override void ExitPredicate([NotNull] TSqlParser.PredicateContext context)
        {
            base.ExitPredicate(context);

            if (currentExpressionList == null)
                throw new InternalErrorException("Expected a ExpressionList");

            Console.WriteLine($"Predicate comparison: '{context.comparison_operator().GetText()}'");
            ExpressionNode x = new ExpressionComparisonOperator(context.comparison_operator().GetText());
            Expression xl = new();
            xl.Add(x);
            currentExpressionList.Add(xl);
        }

        public override void ExitAssignment_operator([NotNull] TSqlParser.Assignment_operatorContext context)
        {
            base.ExitAssignment_operator(context);

            if (currentExpressionList == null)
                throw new InternalErrorException("Expected a ExpressionList");

            Console.WriteLine($"Assignment operator: '{context.GetText()}'");
            ExpressionAssignmentOperator op = new ExpressionAssignmentOperator(context.GetText());
            Expression xl = new();
            xl.Add(op);
            currentExpressionList.Add(xl);
        }

        public override void ExitAs_column_alias([NotNull] TSqlParser.As_column_aliasContext context)
        {
            base.ExitAs_column_alias(context);

            if (selectContext == null)
                throw new InternalErrorException("Expected a SelectContext");
            if (selectContext.SelectListContext == null)
                throw new InternalErrorException("Expected a SelectListContext");

            selectContext.SelectListContext.CurrentAlias = context.column_alias().GetText();
            Console.WriteLine($"alias == {context.column_alias().GetText()}");
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
                string str = context.cross_join().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name().id_()[0].GetText();
                Console.WriteLine($"CROSS JOIN On {str}");

                JoinContext jc = new JoinContext(JoinType.CROSS_JOIN, str);
                selectContext.AddJoin(jc, predicateContext);
                predicateContext = new PredicateContext();
            }
            else if (context.join_on() != null)
            {
                // ON join
                string str = context.join_on().table_source().table_source_item_joined().table_source_item().table_name_with_hint().table_name().id_()[0].GetText();
                Console.WriteLine($"INNER JOIN On {str}");

                JoinContext jc = new JoinContext(JoinType.INNER_JOIN, str);
                selectContext.AddJoin(jc, predicateContext);
                predicateContext = new PredicateContext();
            }
            else 
            {
                throw new NotImplementedException("unsupported JOIN type enountered");
            }
        }

    }
}

