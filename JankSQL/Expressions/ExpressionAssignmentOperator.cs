
namespace JankSQL
{
    internal class ExpressionAssignmentOperator : ExpressionNode
    {
        internal string str;

        internal ExpressionAssignmentOperator(string str)
        {
            this.str = str;
        }

        public override String ToString()
        {
            return str;
        }
    }
}
