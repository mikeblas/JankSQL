﻿namespace JankSQL
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;
    using Antlr4.Runtime.Tree;

    using JankSQL.Contexts;
    using JankSQL.Expressions;

    public partial class JankListener : TSqlParserBaseListener
    {
        private readonly ExecutionContext executionContext = new ();
        private readonly bool quiet = false;

        private int depth = 0;

        internal JankListener()
        {
            quiet = false;
        }

        internal JankListener(bool quiet)
        {
            this.quiet = quiet;
        }

        internal ExecutionContext ExecutionContext
        {
            get { return executionContext; }
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            base.EnterEveryRule(context);

            if (!quiet)
            {
                var s = new string(' ', depth);
                Console.WriteLine($"+{s}{context.GetType().Name}, {context.GetText()}");
            }

            depth++;
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            base.ExitEveryRule(context);

            if (!quiet)
            {
                var s = new string(' ', depth);
                Console.WriteLine($"-{s}{context.GetType().Name}, {context.GetText()}");
            }

            depth--;
        }

        internal Expression GobbleExpression(TSqlParser.ExpressionContext expr)
        {
            Expression x = new ();
            List<object> stack = new ();
            stack.Add(expr);

            while (stack.Count > 0)
            {
                object rule = stack[^1];
                stack.RemoveAt(stack.Count - 1);
                if (rule is TSqlParser.Primitive_expressionContext primitiveContext)
                {
                    if (primitiveContext.NULL_() != null)
                    {
                        Console.WriteLine($"constant: NULL");
                        ExpressionNode n = ExpressionOperand.NullLiteral();
                        x.Add(n);
                    }
                    else if (primitiveContext.constant().FLOAT() != null)
                    {
                        string str = primitiveContext.constant().FLOAT().GetText();
                        if (!quiet)
                            Console.WriteLine($"constant: '{str}'");
                        bool isNegative = primitiveContext.constant().sign() != null;
                        ExpressionNode n = ExpressionOperand.DecimalFromString(isNegative, str);
                        x.Add(n);
                    }
                    else if (primitiveContext.constant().DECIMAL() != null)
                    {
                        string str = primitiveContext.constant().DECIMAL().GetText();
                        if (!quiet)
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
                            if (!quiet)
                                Console.WriteLine($"constant: '{primitiveContext.constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.NVARCHARFromStringContext(str);
                            x.Add(n);
                        }
                        else
                        {
                            if (!quiet)
                                Console.WriteLine($"constant: '{primitiveContext.constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.VARCHARFromStringContext(str);
                            x.Add(n);
                        }
                    }
                }
                else if (rule is TSqlParser.Function_callContext functionCallContext)
                {
                    Console.WriteLine($"Function_callContext with {functionCallContext.ChildCount} children");
                    string? functionName;
                    int firstTop = stack.Count;

                    // why doesn't Function_callContext have any useful members?
                    // we must go after child nodes with GetChild() directly

                    IParseTree childContext = functionCallContext.GetChild(0);
                    if (childContext is TSqlParser.Scalar_function_nameContext scalarContext)
                    {
                        // get a function call
                        functionName = scalarContext.func_proc_name_server_database_schema().func_proc_name_database_schema().func_proc_name_schema().procedure.GetText();
                        ExpressionFunction? n = ExpressionFunction.FromFunctionName(functionName);

                        //TODO: how are listener errors to be raised?
                        if (n == null)
                            throw new ExecutionException($"function {functionName} not implemented");
                        stack.Insert(firstTop, n);

                        // and its argument list
                        if (functionCallContext.GetChild(2) is TSqlParser.Expression_listContext exprContext)
                        {
                            //TODO: how are listener errors to be raised?
                            if (n.ExpectedParameters != exprContext.expression().Length)
                                throw new ExecutionException($"function {n} expects {n.ExpectedParameters} parameters, received {exprContext.expression().Length}");

                            for (int e = exprContext.expression().Length - 1; e >= 0; e--)
                                stack.Add(exprContext.expression()[e]);
                        }
                        else
                        {
                            //TODO: how are listener errors to be raised?
                            if (n.ExpectedParameters != 0)
                                throw new ExecutionException($"function {n} expects {n.ExpectedParameters} parameters, received none");
                        }
                    }
                    else if (childContext is TSqlParser.Aggregate_windowed_functionContext awfc)
                    {
                        if (selectContext == null)
                            throw new InternalErrorException("Expected a SelectContext");

                        AggregateContext agg = GobbleAggregateFunctionContext(awfc);
                        selectContext.AddAggregate(agg);

                        if (agg.ExpressionName == null)
                            throw new InternalErrorException("Expected named expression in aggregation");

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
    }
}

