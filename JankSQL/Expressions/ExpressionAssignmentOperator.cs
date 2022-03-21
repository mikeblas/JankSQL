namespace JankSQL
{
    internal class ExpressionAssignmentOperator : ExpressionNode
    {
        private readonly string str;

        internal ExpressionAssignmentOperator(string str)
        {
            this.str = str;
        }

        public override string ToString()
        {
            return str;
        }
    }
}
