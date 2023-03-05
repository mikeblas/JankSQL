namespace JankSQL
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Misc;

    using JankSQL.Contexts;
    using JankSQL.Expressions;
    using JankSQL.Expressions.Functions;

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

        /// <summary>
        /// Do some tracing at rule entry. We'll write a numbered line with some
        /// indentation to see the parse tree.
        /// </summary>
        /// <param name="context">ParserRuleContext for the current visit context.</param>
        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            base.EnterEveryRule(context);

            if (!quiet)
            {
                var s = new string(' ', depth);
                Console.WriteLine($"+{depth,2}|{s}{context.GetType().Name}, {context.GetText()}");
            }

            depth++;
        }

        /// <summary>
        /// Do some tracing at rule exit, too. Lines up with the entry tracing.
        /// </summary>
        /// <param name="context">ParserRuleContext for the current visit context.</param>
        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            base.ExitEveryRule(context);

            if (!quiet)
            {
                var s = new string(' ', depth);
                Console.WriteLine($"+{depth,2}|{s}{context.GetType().Name}, {context.GetText()}");
            }

            depth--;
        }

        internal Expression GobbleSelectExpression(TSqlParser.ExpressionContext expr, SelectContext selectContext)
        {
            return GobbleExpressionImpl(expr, selectContext);
        }

        internal Expression GobbleExpression(TSqlParser.ExpressionContext expr)
        {
            return GobbleExpressionImpl(expr, null);
        }


        internal Expression GobbleExpressionImpl(TSqlParser.ExpressionContext expr, SelectContext? selectContext)
        {
            // the expression we're building
            Expression x = new ();

            // stack of TSqlParser objects we're considering as we build out the expression
            List<ParserRuleContext> stack = new ()
            {
                expr,
            };

            while (stack.Count > 0)
            {
                ParserRuleContext rule = stack[^1];
                stack.RemoveAt(stack.Count - 1);
                if (rule is TSqlParser.Primitive_expressionContext primitiveContext)
                {
                    if (primitiveContext.NULL_() != null)
                    {
                        Console.WriteLine($"constant: NULL");
                        ExpressionNode n = ExpressionOperand.NullLiteral();
                        x.Insert(0, n);
                    }
                    else if (primitiveContext.LOCAL_ID() != null)
                    {
                        string bindName = primitiveContext.LOCAL_ID().GetText();
                        Console.WriteLine($"found bind variable named {bindName}");
                        ExpressionNode n = new ExpressionBindOperator(bindName);
                        x.Insert(0, n);
                    }
                    else if (primitiveContext.primitive_constant().FLOAT() != null)
                    {
                        string str = primitiveContext.primitive_constant().FLOAT().GetText();
                        if (!quiet)
                            Console.WriteLine($"decimal constant: '{str}'");
                        bool isNegative = primitiveContext.primitive_constant().MINUS() != null;
                        ExpressionNode n = ExpressionOperand.DecimalFromString(isNegative, str);
                        x.Insert(0, n);
                    }
                    else if (primitiveContext.primitive_constant().DECIMAL() != null)
                    {
                        string str = primitiveContext.primitive_constant().DECIMAL().GetText();
                        if (!quiet)
                            Console.WriteLine($"integer constant: '{str}'");
                        bool isNegative = primitiveContext.primitive_constant().MINUS() != null;

                        ExpressionOperandType t = ExpressionOperand.IntegerOrDecimal(str);
                        ExpressionNode n;

                        if (t == ExpressionOperandType.INTEGER)
                            n = ExpressionOperand.IntegerFromString(isNegative, str);
                        else
                            n = ExpressionOperand.DecimalFromString(isNegative, str);

                        x.Insert(0, n);
                    }
                    else if (primitiveContext.primitive_constant().STRING() != null)
                    {
                        // up to us to decide if its NVARCHAR or not
                        string str = primitiveContext.primitive_constant().STRING().GetText();
                        if (str[0] == 'N')
                        {
                            if (!quiet)
                                Console.WriteLine($"constant: '{primitiveContext.primitive_constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.VARCHARFromStringContext(str);
                            x.Insert(0, n);
                        }
                        else
                        {
                            if (!quiet)
                                Console.WriteLine($"constant: '{primitiveContext.primitive_constant().STRING()}'");
                            ExpressionNode n = ExpressionOperand.VARCHARFromStringContext(str);
                            x.Insert(0, n);
                        }
                    }
                }
                else if (rule is TSqlParser.Function_callContext functionCallContext)
                {
                    // function_call uses labels for any of about seven alternatives.
                    // ANTLR doesn't emit any features for the Function_callContext class, and instead
                    // uses derived classes for each of the alternatives. The only way to find the
                    // satisfying rule appears to be test for object type.

                    if (functionCallContext is TSqlParser.BUILT_IN_FUNCContext bifContext)
                    {
                        HandleBuiltInFunction(x, stack, bifContext.built_in_functions());
                    }
                    else if (functionCallContext is TSqlParser.SCALAR_FUNCTIONContext scalarFunctionContext)
                    {
                        // get a function call
                        string functionName = scalarFunctionContext.scalar_function_name().func_proc_name_server_database_schema().func_proc_name_database_schema().func_proc_name_schema().procedure.GetText();
                        ExpressionFunction? n = ExpressionFunction.FromFunctionName(functionName);

                        if (n == null)
                            throw new SemanticErrorException($"function {functionName} not implemented");
                        x.Insert(0, n);

                        // and its argument list
                        if (functionCallContext.GetChild(2) is TSqlParser.Expression_listContext exprContext)
                        {
                            if (n.ExpectedParameters != exprContext.expression().Length)
                                throw new SemanticErrorException($"function {n} expects {n.ExpectedParameters} parameters, received {exprContext.expression().Length}");

                            for (int i = 0; i < exprContext.expression().Length; i++)
                                stack.Add(exprContext.expression()[i]);
                        }
                        else
                        {
                            if (n.ExpectedParameters != 0)
                                throw new SemanticErrorException($"function {n} expects {n.ExpectedParameters} parameters, received none");
                        }
                    }
                    else if (functionCallContext is TSqlParser.AGGREGATE_WINDOWED_FUNCContext awfs)
                    {
                        if (selectContext == null)
                            throw new InternalErrorException("Expected a SelectContext");

                        AggregateContext agg = GobbleAggregateFunctionContext(awfs.aggregate_windowed_function());
                        selectContext.AddAggregate(agg);

                        if (agg.ExpressionName == null)
                            throw new InternalErrorException("Expected named expression in aggregation");

                        ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromColumnName(agg.ExpressionName));
                        x.Insert(0, n);
                        x.ContainsAggregate = true;
                    }
                    else if (functionCallContext is TSqlParser.FREE_TEXTContext)
                    {
                        throw new NotImplementedException("Free-Text search functions are not supported");
                    }
                    else if (functionCallContext is TSqlParser.RANKING_WINDOWED_FUNCContext)
                    {
                        throw new NotImplementedException("Ranking Windowed functions are not supported");
                    }
                    else if (functionCallContext is TSqlParser.PARTITION_FUNCContext)
                    {
                        throw new NotImplementedException("Partition functions are not supported");
                    }
                    else if (functionCallContext is TSqlParser.ANALYTIC_WINDOWED_FUNCContext)
                    {
                        throw new NotImplementedException("Analytic windowed functions are not supported");
                    }
                    else
                    {
                        Console.WriteLine($"functionCallContext: skipping {functionCallContext}");
                    }
                }
                else if (rule is TSqlParser.ExpressionContext xContext)
                {
                    if (xContext.op != null)
                    {
                        // binary operator
                        Console.WriteLine($"expressionContext: '{xContext.op.Text}'");
                        ExpressionNode n = new ExpressionOperator(xContext.op.Text);
                        x.Insert(0, n);
                        stack.Add(xContext.expression()[0]);
                        stack.Add(xContext.expression()[1]);
                    }
                    else if (xContext.primitive_expression() != null)
                    {
                        // primitive expression, like NULL or a constant literal
                        stack.Add(xContext.primitive_expression());
                    }
                    else if (xContext.bracket_expression() != null)
                    {
                        // bracket expression, which is just an expression in brackets ...
                        // but could also be a sub-query
                        if (xContext.bracket_expression().subquery() != null)
                        {
                            SelectContext subSelect = GobbleSelectStatement(xContext.bracket_expression().subquery().select_statement());
                            ExpressionNode n = new ExpressionSubselectOperator(subSelect);
                            x.Insert(0, n);
                            // throw new NotImplementedException("Sub-queries are not yet implemented");
                        }
                        else
                        {
                            stack.Add(xContext.bracket_expression().expression());
                        }
                    }
                    else if (xContext.function_call() != null)
                    {
                        // some type of function call
                        stack.Add(xContext.function_call());
                    }
                    else if (xContext.full_column_name() != null)
                    {
                        // reference to a column name
                        stack.Add(xContext.full_column_name());
                    }
                    else if (xContext.unary_operator_expression() != null)
                    {
                        // unary operators, like minus in "-SQRT(2)"
                        ExpressionNode n = ExpressionUnaryOperator.GetUnaryOperator(xContext.unary_operator_expression());
                        x.Insert(0, n);
                        stack.Add(xContext.unary_operator_expression().expression());
                    }
                    else if (xContext.case_expression() != null)
                    {
                        ExpressionNode n = GobbleCaseExpression(xContext.case_expression());
                        x.Insert(0, n);
                    }
                    else
                    {
                        throw new InternalErrorException("Some unexpected expression type");
                    }
                }
                else if (rule is TSqlParser.Full_column_nameContext fullColumn)
                {
                    ExpressionNode n = new ExpressionOperandFromColumn(FullColumnName.FromContext(fullColumn));
                    x.Insert(0, n);
                }
                else if (rule is TSqlParser.Search_conditionContext searchCondition)
                {
                    Expression n = GobbleSearchCondition(searchCondition);
                    x.InsertRange(0, n);
                }
                else
                {
                    Console.WriteLine($"don't know {rule}");
                }
            }

            return x;
        }

        internal ExpressionOperandType GobbleDataType(TSqlParser.Data_typeContext context)
        {
            ExpressionOperandType ot;

            if (context.unscaled_type is not null)
            {
                string typeName = (context.unscaled_type.ID() != null) ? context.unscaled_type.ID().GetText() : context.unscaled_type.keyword().GetText();

                if (typeName == null)
                    throw new ExecutionException($"No type name found");

                Console.Write($"{typeName} ");
                if (!ExpressionNode.TypeFromString(typeName, out ot))
                    throw new ExecutionException($"Unknown column type {typeName}");
            }
            else
            {
                string typeName = context.ext_type.keyword().GetText();
                Console.Write($"{typeName} ");

                ExpressionOperandType columnType;
                if (!ExpressionNode.TypeFromString(context.ext_type.keyword().GetText(), out columnType))
                    throw new ExecutionException($"Unknown column type {typeName}");

                // null or not, if it's VARCHAR or not.
                var dktvc = context.ext_type.keyword().VARCHAR();
                var dktnvc = context.ext_type.keyword().NVARCHAR();

                if (dktvc != null || dktnvc != null)
                    ot = ExpressionOperandType.VARCHAR;
                else
                    throw new ExecutionException($"Unknown scaled column type {typeName}");
            }

            return ot;
        }

        // here's a dictionary of functions by string name to the classes which work them
        private static readonly Dictionary<Type, Func<ExpressionFunction>> FunctionDict = new ()
        {
            { typeof(TSqlParser.GETDATEContext),    () => new FunctionGetDate() },
            { typeof(TSqlParser.LENContext),        () => new FunctionLEN()     },
            { typeof(TSqlParser.PIContext),         () => new FunctionPI()      },
            { typeof(TSqlParser.POWERContext),      () => new FunctionPOWER()   },
            { typeof(TSqlParser.SQRTContext),       () => new FunctionSQRT()    },
            { typeof(TSqlParser.ISNULLContext),     () => new FunctionIsNull()  },
//          { typeof(TSqlParser.CASTContext),       () => new FunctionCast()    },
            { typeof(TSqlParser.IIFContext),        () => new FunctionIIF()     },
//          { typeof(TSqlParser.DATEADDContext),    () => new FunctionDateAdd() },
//          { typeof(TSqlParser.DATEDIFFContext),   () => new FunctionDateDiff() },
        };


        internal void HandleBuiltInFunction(Expression x, List<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            if (FunctionDict.ContainsKey(bifContext.GetType()))
            {
                var r = FunctionDict[bifContext.GetType()].Invoke();

                x.Insert(0, r);

                r.SetFromBuiltInFunctionsContext(stack, bifContext);

            }
            else if (bifContext is TSqlParser.CASTContext castContext)
            {
                // CAST
                ExpressionOperandType opType = GobbleDataType(castContext.data_type());

                ExpressionFunction f = new Expressions.Functions.FunctionCast(opType);
                x.Insert(0, f);

                stack.Add(castContext.expression());
            }
            else if (bifContext is TSqlParser.DATEADDContext dateAddContext)
            {
                // DATEADD(datepart_12, number, date)
                ExpressionFunction f = new Expressions.Functions.FunctionDateAdd(dateAddContext.dateparts_12().GetText());
                x.Insert(0, f);

                stack.Add(dateAddContext.number);
                stack.Add(dateAddContext.date);
            }
            else if (bifContext is TSqlParser.DATEDIFFContext dateDiffContext)
            {
                // DATEDIFF(dateparts_12, date_first, date_second)
                ExpressionFunction f = new Expressions.Functions.FunctionDateDiff(dateDiffContext.dateparts_12().GetText());
                x.Insert(0, f);

                stack.Add(dateDiffContext.date_first);
                stack.Add(dateDiffContext.date_second);
            }
            else
            {
                throw new NotImplementedException($"Unknown built-in function {bifContext.GetText()}");
            }
        }
    }
}

