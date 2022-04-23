namespace JankSQL.Engines
{
    using System.Collections;
    using System.IO;
    using CSharpTest.Net;
    using CSharpTest.Net.Collections;
    using CSharpTest.Net.Serialization;

    using JankSQL.Expressions;

    internal class TupleSerializer : ISerializer<Tuple>
    {
        private static readonly ISerializer<byte> ByteSerialzier = PrimitiveSerializer.Byte;
        // private static readonly ISerializer<int> IntSerializer = PrimitiveSerializer.Int32;

        public Tuple ReadFrom(Stream stream)
        {
            // byte: number of columns
            byte columnCount = TupleSerializer.ByteSerialzier.ReadFrom(stream);
            Tuple ret = Tuple.CreateEmpty(columnCount);

            // [#cols]: byte: ExpressionOperandTypes per column
            for (int i = 0; i < columnCount; i++)
                ret.Values[i] = ExpressionOperand.CreateFromByteStream(stream);

            return ret;
        }

        public void WriteTo(Tuple value, Stream stream)
        {
            // byte: number of columns
            TupleSerializer.ByteSerialzier.WriteTo((byte)value.Count, stream);

            // [#cols]: ExpresionOperandType values
            for (int i = 0; i < value.Count; i++)
                value.Values[i].WriteToByteStream(stream);
        }
    }
}
