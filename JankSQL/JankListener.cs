using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace JankSQL
{
    public class JankListener : TSqlParserBaseListener
    {
        private int depth = 0;

        ExecutionContext executionContext = new ExecutionContext();
        SelectContext? selectContext;
        InsertContext? insertContext;

        Expression currentExpression = new();
        List<Expression> currentExpressionList;
        List<List<Expression>> currentExpressionListList;

        internal ExecutionContext ExecutionContext { get { return executionContext; } }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            var s = new string(' ', depth);
            base.EnterEveryRule(context);
            Console.WriteLine($"+{s}{context.GetType().Name}, {context.GetText()}");
            depth++;
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            var s = new string(' ', depth);
            base.ExitEveryRule(context);
            Console.WriteLine($"-{s}{context.GetType().Name}, {context.GetText()}");
            depth--;
        }

        public override void ExitSelect_statement(TSqlParser.Select_statementContext context)
        {
            executionContext.ExecuteContexts.Add(selectContext);
        }

        public override void EnterSelect_statement([NotNull] TSqlParser.Select_statementContext context)
        {
            selectContext = new SelectContext(context);
            base.EnterSelect_statement(context);
        }

        public override void EnterSelect_list([NotNull] TSqlParser.Select_listContext context)
        {
            base.EnterSelect_list(context);
            selectContext.SelectListContext = new SelectListContext(context);
            currentExpressionList = new();
            currentExpressionListList = new();
        }

        public override void EnterInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            insertContext = new InsertContext(context);
            base.EnterInsert_statement(context);
        }


        public override void ExitExpression_elem([NotNull] TSqlParser.Expression_elemContext context)
        {
            foreach (ExpressionNode n in currentExpression)
            {
                Console.Write($"[{n.ToString()}] ");
            }
            Console.WriteLine();

            base.ExitExpression_elem(context);
        }

        public override void ExitExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            Console.WriteLine($"operator: '{context.op}'");
            if (context.op != null)
            {
                ExpressionNode x = new ExpressionOperator(context.op.Text);
                currentExpression.Add(x);
            }

            currentExpressionList.Add(currentExpression);
            currentExpression = new();

            base.ExitExpression(context);
        }

        public override void EnterExpression([NotNull] TSqlParser.ExpressionContext context)
        {
            base.EnterExpression(context);
        }

        public override void EnterExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
            currentExpressionList = new();
            base.EnterExpression_list(context);
        }

        public override void ExitPrimitive_expression([NotNull] TSqlParser.Primitive_expressionContext context)
        {
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


            base.ExitPrimitive_expression(context);
        }

        public override void ExitComparison_operator([NotNull] TSqlParser.Comparison_operatorContext context)
        {
            base.ExitComparison_operator(context);
        }

        public override void ExitSCALAR_FUNCTION([NotNull] TSqlParser.SCALAR_FUNCTIONContext context)
        {
            ExpressionNode x = new ExpressionOperator(context.scalar_function_name().GetText());
            currentExpression.Add(x);

            base.ExitSCALAR_FUNCTION(context);
        }

        public override void ExitSearch_condition([NotNull] TSqlParser.Search_conditionContext context)
        {
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

            base.ExitSearch_condition(context);
        }

        public override void ExitFull_column_name([NotNull] TSqlParser.Full_column_nameContext context)
        {
            ExpressionNode x = new ExpressionOperandFromColumn(FullColumnName.FromContext(context));
            currentExpression.Add(x);

            base.ExitFull_column_name(context);
        }

        public override void ExitSelect_list_elem([NotNull] TSqlParser.Select_list_elemContext context)
        {
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

                selectContext.EndSelectListExpressionList(currentExpressionList[0]);
                currentExpression = new();
                currentExpressionList = new();
            }

            base.ExitSelect_list_elem(context);
        }

        public override void ExitPredicate([NotNull] TSqlParser.PredicateContext context)
        {
            Console.WriteLine($"Predicate comparison: '{context.comparison_operator().GetText()}'");
            ExpressionNode x = new ExpressionComparisonOperator(context.comparison_operator().GetText());
            Expression xl = new();
            xl.Add(x);
            currentExpressionList.Add(xl);
            base.ExitPredicate(context);
        }

        public override void ExitAs_column_alias([NotNull] TSqlParser.As_column_aliasContext context)
        {
            selectContext.SelectListContext.CurrentAlias = context.column_alias().GetText();
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

        public override void ExitTruncate_table([NotNull] TSqlParser.Truncate_tableContext context)
        {
            string tableName = context.table_name().id_()[0].GetText();

            TruncateTableContext c = new TruncateTableContext(tableName);

            executionContext.ExecuteContexts.Add(c);

            base.ExitTruncate_table(context);
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


        public override void ExitJoin_part([NotNull] TSqlParser.Join_partContext context)
        {
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

            base.ExitJoin_part(context);
        }

        public override void ExitInsert_column_name_list([NotNull] TSqlParser.Insert_column_name_listContext context)
        {
            base.ExitInsert_column_name_list(context);

            List<FullColumnName> columns = new();

            foreach (var col in context.insert_column_id())
            {
                Console.WriteLine(col.id_()[0].GetText());
                columns.Add(FullColumnName.FromColumnName(col.id_()[0].GetText()));
            }

            insertContext.TargetColumns = columns;
        }


        public override void ExitInsert_statement([NotNull] TSqlParser.Insert_statementContext context)
        {
            base.ExitInsert_statement(context);

            Console.WriteLine($"INTO {context.ddl_object().full_table_name()}");
            insertContext.TableName = context.ddl_object().full_table_name().GetText();

            executionContext.ExecuteContexts.Add(insertContext);
        }


        public override void ExitInsert_with_table_hints([NotNull] TSqlParser.Insert_with_table_hintsContext context)
        {
            base.ExitInsert_with_table_hints(context);
        }


        public override void EnterTable_value_constructor([NotNull] TSqlParser.Table_value_constructorContext context)
        {
            base.EnterTable_value_constructor(context);
            currentExpressionListList = new();
        }

        public override void ExitTable_value_constructor([NotNull] TSqlParser.Table_value_constructorContext context)
        {
            base.ExitTable_value_constructor(context);

            insertContext.AddExpressionLists(currentExpressionListList);
            currentExpressionList = new();
        }

        public override void ExitExpression_list([NotNull] TSqlParser.Expression_listContext context)
        {
         
            base.ExitExpression_list(context);
            currentExpressionListList.Add(currentExpressionList);
        }

    }
}

