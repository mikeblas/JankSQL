﻿namespace JankSQL.Expressions
{
    internal class ExpressionCaseOperator : ExpressionNode
    {
        private readonly List<Expression> whens;
        private readonly List<Expression> thens;
        private readonly Expression? elseExpression;

        internal ExpressionCaseOperator(List<Expression> whens, List<Expression> thens, Expression? elseExpression)
        {
            this.whens = whens;
            this.thens = thens;
            this.elseExpression = elseExpression;

            if (whens.Count != thens.Count)
                throw new SemanticErrorException($"must have the same number of expressions");
        }

        public override string ToString()
        {
            return "CASE Operator";
        }

        internal ExpressionOperand Evaluate(IRowValueAccessor accessor, Stack<ExpressionOperand> stack)
        {
            ExpressionOperand? result = null;

            // evaluate each when to find truth ...
            for (int i = 0; i < whens.Count; i++)
            {
                ExpressionOperand whenResult = whens[i].Evaluate(accessor);

                if (whenResult.IsTrue())
                {
                    result = thens[i].Evaluate(accessor);
                    break;
                }
            }

            // got a match?
            if (result == null)
            {
                // no match -- if no ELSE, then NULL
                if (elseExpression == null)
                    result = ExpressionOperand.NullLiteral();
                else
                    result = elseExpression.Evaluate(accessor);
            }

            // return what we discovered
            return result;
        }
    }
}

    