
namespace JankSQL
{
    internal interface IRowValueAccessor
    {
        ExpressionOperand GetValue(FullColumnName fcn);
        void SetValue(FullColumnName fcn, ExpressionOperand value);
    }
}
