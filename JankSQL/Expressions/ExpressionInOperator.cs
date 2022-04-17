namespace JankSQL.Expressions
{
    using JankSQL.Contexts;

    internal class ExpressionInOperator : ExpressionNode
    {
        private readonly List<Expression>? targets;
        private readonly SelectContext? selectContext;
        private readonly bool notIn;

        internal ExpressionInOperator(bool notIn, List<Expression> targets)
        {
            this.targets = targets;
            this.notIn = notIn;
        }

        internal ExpressionInOperator(bool notIn, SelectContext selectContext)
        {
            this.selectContext = selectContext;
            this.notIn = notIn;
        }

        public override string ToString()
        {
            return "IN Operator";
        }

        internal ExpressionOperand Evaluate(Engines.IEngine engine, IRowValueAccessor accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            bool result;
            if (targets != null)
                result = EvaluateTargets(engine, accessor, stack, bindValues);
            else
                result = EvaluateSubselect(engine, accessor, stack, bindValues);

            // return what we discovered
            ExpressionOperand r = new ExpressionOperandBoolean(result);
            return r;
        }

        protected bool EvaluateTargets(Engines.IEngine engine, IRowValueAccessor accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            bool result = false;

            ExpressionOperand left = stack.Pop();

            // see if we find one that matches
            for (int i = 0; i < targets!.Count; i++)
            {
                ExpressionOperand target = targets[i].Evaluate(accessor, engine, bindValues);
                if (left.OperatorEquals(target))
                {
                    result = true;
                    break;
                }
            }

            // invert?
            if (notIn)
                result = !result;

            return result;
        }

        protected bool EvaluateSubselect(Engines.IEngine engine, IRowValueAccessor accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            selectContext!.Reset();
            ExecuteResult queryResult = selectContext.Execute(engine, accessor, bindValues);

            // no rows means we can't match
            if (queryResult.ResultSet.RowCount == 0)
                return false;

            if (queryResult.ResultSet.ColumnCount != 1)
                throw new SemanticErrorException($"subselect returned {queryResult.ResultSet.ColumnCount} columns, must only return 1 column");

            // otherwise, see if there is a match
            ExpressionOperand left = stack.Pop();
            bool result = false;

            for (int i = 0; i < queryResult.ResultSet.RowCount; i++)
            {
                ExpressionOperand target = queryResult.ResultSet.Row(i)[0];
                if (left.OperatorEquals(target))
                {
                    result = true;
                    break;
                }
            }

            // invert?
            if (notIn)
                result = !result;

            return result;
        }
    }
}
