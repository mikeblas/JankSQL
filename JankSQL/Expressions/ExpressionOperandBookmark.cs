﻿namespace JankSQL
{
    public class ExpressionOperandBookmark : ExpressionOperand
    {
        ExpressionOperand[] tuple;
        internal ExpressionOperandBookmark(ExpressionOperand[] tuple)
            : base(ExpressionOperandType.NVARCHAR)
        {
            this.tuple = tuple;
        }

        internal static ExpressionOperandBookmark FromInteger(int mark)
        {
            ExpressionOperand bmk = ExpressionOperand.IntegerFromInt(mark);
            var ret = new ExpressionOperandBookmark(new ExpressionOperand[] { bmk });
            return ret;
        }

        public override object Clone()
        {
            return new ExpressionOperandBookmark(tuple);
        }

        public override string ToString()
        {
            return $"Bookmark({tuple})";
        }

        public override double AsDouble()
        {
            throw new NotImplementedException();
        }

        public override string AsString()
        {
            throw new NotImplementedException();
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override int AsInteger()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }
        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(ExpressionOperandBookmark? other)
        {
            if (other == null)
                throw new ArgumentNullException("obj");
            if (other.tuple.Length != tuple.Length)
                throw new ArgumentException($"can't compare bookmarks of different lengths; this is {tuple.Length} other is {other.tuple.Length}");

            int index = 0;
            int ret = 0;
            while (ret == 0 && index < tuple.Length)
            {
                ret = tuple[index].CompareTo(other.tuple[index]);
            }
            return ret;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            ExpressionOperandBookmark o = (ExpressionOperandBookmark)other;
            if (o.tuple.Length != tuple.Length)
                throw new ArgumentException($"can't compare bookmarks of different lengths; this is {tuple.Length} other is {o.tuple.Length}");

            int index = 0;
            int ret = 0;
            while (ret == 0 && index < tuple.Length)
            {
                ret = tuple[index].CompareTo(o.tuple[index]);
            }
            return ret;
        }

        public ExpressionOperand[] Tuple => tuple;
    }
}
