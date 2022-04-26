namespace JankSQL.Expressions
{
    internal class ExpressionOperandDateTime : ExpressionOperand, IComparable<ExpressionOperandDateTime>, IEquatable<ExpressionOperandDateTime>
    {
        private readonly bool isNull;
        private DateTime dt;

        internal ExpressionOperandDateTime(DateTime dt)
            : base(ExpressionOperandType.DATETIME)
        {
            this.dt = dt;
            isNull = false;
        }


        internal ExpressionOperandDateTime(DateTime dt, bool isNull)
            : base(ExpressionOperandType.DATETIME)
        {
            this.dt = dt;
            this.isNull = isNull;
        }

        public override bool RepresentsNull
        {
            get { return isNull; }
        }

        public override object Clone()
        {
            return new ExpressionOperandDateTime(dt, isNull);
        }

        public override string ToString()
        {
            if (isNull)
                return "DateTime(NULL)";
            return $"DateTime({dt:yyyy-MM-ddTHH:mm:ssZ})";
        }

        public override double AsDouble()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null DATETIME to double");

            // convert unit to days
            return dt.Ticks / (double)TimeSpan.TicksPerDay;
        }

        public override string AsString()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null DATETIME to string");
            return $"{dt:yyyy-MM-ddTHH:mm:ssZ}";
        }

        public override int AsInteger()
        {
            if (isNull)
                throw new InvalidOperationException("can't convert null DATETIME to integer");

            // convert unit to days
            return (int)(dt.Ticks / TimeSpan.TicksPerDay);
        }

        public override DateTime AsDateTime()
        {
            return dt;
        }

        public override bool IsTrue()
        {
            throw new NotImplementedException();
        }

        public override bool OperatorEquals(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

            if (other.NodeType == ExpressionOperandType.DATETIME)
            {
                return other.AsDateTime() == AsDateTime();
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL )
            {
                return other.AsDouble() == AsDouble();
            }
            else if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                return other.AsInteger() == AsInteger();
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDateTime() == other.AsDateTime();
            }

            return false;
        }

        public override bool OperatorGreaterThan(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

            if (other.NodeType == ExpressionOperandType.DATETIME)
            {
                return other.AsDateTime() > AsDateTime();
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                return AsDouble() > other.AsDouble();
            }
            else if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsInteger() > other.AsInteger();
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDateTime() > other.AsDateTime();
            }
            else
            {
                throw new NotImplementedException($"DateTime GreaterThan {other.NodeType}");
            }
        }

        public override bool OperatorLessThan(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return false;

            if (other.NodeType == ExpressionOperandType.DATETIME)
            {
                return other.AsDateTime() < AsDateTime();
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                return AsDouble() < other.AsDouble();
            }
            else if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                return AsInteger() < other.AsInteger();
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                return AsDateTime() < other.AsDateTime();
            }
            else
            {
                throw new NotImplementedException($"DateTime LessThan {other.NodeType}");
            }
        }


        public override ExpressionOperand OperatorPlus(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

            if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                long l = dt.Ticks + (other.AsInteger() * TimeSpan.TicksPerDay);
                var result = new DateTime(l, DateTimeKind.Utc);
                return new ExpressionOperandDateTime(result);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                long l = dt.Ticks + (long)(other.AsDouble() * TimeSpan.TicksPerDay);
                var result = new DateTime(l, DateTimeKind.Utc);
                return new ExpressionOperandDateTime(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                int oint = other.AsInteger();
                int result = AsInteger() + oint;
                return new ExpressionOperandInteger(result);
            }
            else
            {
                throw new NotImplementedException($"DateTime OperatorPlus {other.NodeType}");
            }
        }

        public override ExpressionOperand OperatorMinus(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

            if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                long l = dt.Ticks - (other.AsInteger() * TimeSpan.TicksPerDay);
                var result = new DateTime(l, DateTimeKind.Utc);
                return new ExpressionOperandDateTime(result);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                long l = dt.Ticks - (long)(other.AsDouble() * TimeSpan.TicksPerDay);
                var result = new DateTime(l, DateTimeKind.Utc);
                return new ExpressionOperandDateTime(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                int oint = other.AsInteger();
                int result = AsInteger() - oint;
                return new ExpressionOperandInteger(result);
            }
            else
            {
                throw new NotImplementedException($"DateTime OperatorMinus {other.NodeType}");
            }
        }


        public override ExpressionOperand OperatorSlash(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

            if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                int result = AsInteger() / other.AsInteger();
                return new ExpressionOperandInteger(result);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                double result = AsDouble() / other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new NotImplementedException($"DateTime OperatorSlash {other.NodeType}");
            }
        }


        public override ExpressionOperand OperatorTimes(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

            if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                int result = AsInteger() * other.AsInteger();
                return new ExpressionOperandInteger(result);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                double result = AsDouble() * other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else
            {
                throw new NotImplementedException($"DateTime OperatorTimes {other.NodeType}");
            }
        }

        public override ExpressionOperand OperatorModulo(ExpressionOperand other)
        {
            if (RepresentsNull || other.RepresentsNull)
                return new ExpressionOperandInteger(0, true);

            if (other.NodeType == ExpressionOperandType.INTEGER)
            {
                int result = AsInteger() % other.AsInteger();
                return new ExpressionOperandInteger(result);
            }
            else if (other.NodeType == ExpressionOperandType.DECIMAL)
            {
                double result = AsDouble() % other.AsDouble();
                return new ExpressionOperandDecimal(result);
            }
            else if (other.NodeType == ExpressionOperandType.VARCHAR)
            {
                int oint = other.AsInteger();
                int result = AsInteger() % oint;
                return new ExpressionOperandInteger(result);
            }
            else
            {
                throw new NotImplementedException($"DateTime OperatorModulo {other.NodeType}");
            }
        }


        public override void AddToSelf(ExpressionOperand other)
        {
            throw new NotImplementedException("Can't increment a DATETIME");
        }

        public override ExpressionOperand OperatorUnaryMinus()
        {
            throw new SemanticErrorException("Can't negate a DATETIME");
        }

        public override ExpressionOperand OperatorUnaryPlus()
        {
            return new ExpressionOperandDateTime(dt);
        }

        public override ExpressionOperand OperatorUnaryTilde()
        {
            throw new SemanticErrorException("DATETIME has no bitwise NOT");
        }

        public int CompareTo(ExpressionOperandDateTime? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (isNull && other.isNull)
                return 0;
            if (isNull && !other.isNull)
                return -1;
            if (!isNull && other.isNull)
                return 1;

            int result = dt.CompareTo(other.dt);
            return result;
        }

        public override int CompareTo(ExpressionOperand? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            ExpressionOperandInteger o = (ExpressionOperandInteger)other;

            return CompareTo(o);
        }

        public bool Equals(ExpressionOperandDateTime? other)
        {
            return CompareTo(other) == 0;
        }

        public override bool Equals(object? o)
        {
            if (o is not ExpressionOperandDateTime other)
                return false;
            return this.Equals(other);
        }

        public override int GetHashCode()
        {
            if (isNull)
                return 8675309;
            return dt.GetHashCode();
        }

        internal static ExpressionOperandDateTime FromByteStream(Stream stream)
        {
            byte[] rep = new byte[8];
            stream.Read(rep, 0, rep.Length);

            long ticks = BitConverter.ToInt64(rep, 0);
            return new ExpressionOperandDateTime(new DateTime(ticks, DateTimeKind.Utc));
        }

        internal override void WriteToByteStream(Stream stream)
        {
            WriteTypeAndNullness(stream);

            // then ourselves
            byte[] rep = BitConverter.GetBytes(dt.Ticks);
            stream.Write(rep);
        }
    }
}

