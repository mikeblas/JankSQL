namespace JankSQL
{
    using System.Diagnostics;
    using JankSQL.Expressions;

    [DebuggerDisplay("Expression = {ToString()}")]
    internal class Expression : List<ExpressionNode>, IEquatable<Expression>
    {
        internal Expression()
        {
            ContainsAggregate = false;
        }

        internal bool ContainsAggregate { get; set; }

        public override string ToString()
        {
            return string.Join(", ", this);
        }

        public virtual bool Equals(Expression? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (ReferenceEquals(this, other))
                return true;

            if (this.Count != other.Count)
                return false;

            for (int i = 0; i < this.Count; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Expression);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal ExpressionOperand Evaluate(IRowValueAccessor? accessor)
        {
            Stack<ExpressionOperand> stack = new Stack<ExpressionOperand>();

            do
            {
                foreach (ExpressionNode n in this)
                {
                    if (n is ExpressionOperand expressionOperand)
                        stack.Push(expressionOperand);
                    else if (n is ExpressionOperator expressionOperator)
                    {
                        // it's an operator
                        ExpressionOperand r = expressionOperator.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionFunction expressionFunction)
                    {
                        // a function to evaluate
                        ExpressionOperand r = expressionFunction.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionOperandFromColumn columnOperand)
                    {
                        // value from a column
                        if (accessor == null)
                            throw new ExecutionException("Not in a row context to evaluate {this}");
                        stack.Push(accessor.GetValue(columnOperand.ColumnName));
                    }
                    else if (n is ExpressionComparisonOperator comparisonOperator)
                    {
                        // comparison operator
                        ExpressionOperand r = comparisonOperator.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionIsNullOperator nullOperator)
                    {
                        // nullness operator
                        ExpressionOperand r = nullOperator.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionBooleanOperator booleanOperator)
                    {
                        ExpressionOperand r = booleanOperator.Evaluate(stack);
                        stack.Push(r);
                    }
                    else if (n is ExpressionBetweenOperator betweenOperator)
                    {
                        // a [NOT] BETWEEN b AND c
                        ExpressionOperand r = betweenOperator.Evaluate(stack);
                        stack.Push(r);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Not prepared for ExpressionNode {n}");
                    }

                }
            }
            while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand)stack.Pop();

            // Console.WriteLine($"Evaluated {this} ==> [{result}]");

            return result;
        }
    }
}
