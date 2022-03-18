
namespace JankSQL
{
    internal class Aggregation : IComponentOutput
    {
        List<AggregationOperatorType> operatorTypes;
        List<Expression> expressions;

        List<Expression> groupByExpressions;

        IComponentOutput myInput;
        bool inputExhausted;

        internal Aggregation(IComponentOutput input, List<AggregationOperatorType> operators, List<Expression> expressions,
            List<Expression> groupByExpressions)
        {
            this.operatorTypes = operators;
            this.expressions = expressions;
            this.groupByExpressions = groupByExpressions;
            myInput = input;
            inputExhausted = false;
        }

        public ResultSet? GetRows(int max)
        {
            if (!inputExhausted)
                ReadInput();

            throw new NotImplementedException();
        }

        void ReadInput()
        {



            inputExhausted = false;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}

