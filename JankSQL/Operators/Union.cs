namespace JankSQL.Operators
{
    using System.Collections.Generic;

    using JankSQL.Contexts;
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Union : IComponentOutput
    {
        private IComponentOutput leftInput;
        private IComponentOutput rightInput;
        private UnionType unionType;

        private ResultSet? leftRows = null;
        private ResultSet? rightRows = null;

        public Union(UnionType unionType, IComponentOutput leftInput, IComponentOutput rightInput)
        {
            this.leftInput = leftInput;
            this.rightInput = rightInput;
            this.unionType = unionType;
        }

        public ResultSet GetRows(IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (rightRows == null)
                rightRows = rightInput.GetRows(engine, outerAccessor, max, bindValues);
            if (leftRows == null)
                leftRows = leftInput.GetRows(engine, outerAccessor, max, bindValues);

            if (leftRows.ColumnCount != rightRows.ColumnCount)
                throw new SemanticErrorException($"UNION operator has {leftRows.ColumnCount} columns on the left and {rightRows.ColumnCount} columns on the right");


            ResultSet rsOutput = ResultSet.NewWithShape(rightRows);

            if (rightRows.IsEOF && leftRows.IsEOF)
                rsOutput.MarkEOF();
            else
            {
                if (!leftRows.IsEOF)
                {
                    for (int i = 0; i < leftRows.RowCount; i++)
                        rsOutput.AddRowFrom(leftRows, i);
                    leftRows = null;
                }

                if (!rightRows.IsEOF)
                {
                    for (int i = 0; i < rightRows.RowCount; i++)
                        rsOutput.AddRowFrom(rightRows, i);
                    rightRows = null;
                }
            }
 
            return rsOutput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
