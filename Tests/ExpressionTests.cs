
using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class ExpressionTests
    {

        /// <summary>
        /// We expect Expression.Equals to compare two expressions, in order, by value
        /// </summary>
        [TestMethod]
        public void TestExpressionObjectEqualsMethod()
        {
            Expression exp1 = new ();
            exp1.Add(new ExpressionOperandFromColumn(FullColumnName.FromColumnName("SomeColumn")));
            exp1.Add(ExpressionOperand.IntegerFromInt(3));
            exp1.Add(new ExpressionOperator("+"));

            Expression exp2 = new ();
            exp2.Add(new ExpressionOperandFromColumn(FullColumnName.FromColumnName("SomeColumn")));
            exp2.Add(ExpressionOperand.IntegerFromInt(3));
            exp2.Add(new ExpressionOperator("+"));

            Assert.IsTrue(exp1.Equals(exp2));
        }


        /// <summary>
        /// Some expressions aren't equal
        /// </summary>
        [TestMethod]
        public void TestExpressionObjectEqualsNotMethod()
        {
            Expression exp1 = new ();
            exp1.Add(ExpressionOperand.IntegerFromInt(3));
            exp1.Add(new ExpressionOperandFromColumn(FullColumnName.FromColumnName("SomeColumn")));
            exp1.Add(new ExpressionOperator("+"));

            Expression exp2 = new ();
            exp2.Add(new ExpressionOperandFromColumn(FullColumnName.FromColumnName("SomeColumn")));
            exp2.Add(ExpressionOperand.IntegerFromInt(3));
            exp2.Add(new ExpressionOperator("+"));

            Assert.IsTrue(! exp1.Equals(exp2));
        }

    }
}
