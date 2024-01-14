﻿
namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using JankSQL.Expressions;

    [TestFixture]
    public class ExpressionTests
    {

        /// <summary>
        /// We expect Expression.Equals to compare two expressions, in order, by value.
        /// Prove that "SomeColumn+3" is the same as "SomeColumn+3".
        /// </summary>
        [Test]
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

            Assert.That(exp1, Is.EqualTo(exp2));
            // Assert.IsTrue(exp1.Equals(exp2));
        }


        /// <summary>
        /// Some expressions aren't equal; "3+SomeColumn" is not the same as "SomeColumn+3"
        /// </summary>
        [Test]
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


            Assert.That(exp1, Is.Not.EqualTo(exp2));
            // Assert.IsTrue(! exp1.Equals(exp2));
        }
    }
}
