namespace JankSQL.Engines
{
    using System.Collections;

    internal class DynamicCSVRowEnumerator : IEnumerator<RowWithBookmark>
    {
        private readonly IEnumerator<Tuple> valuesEnumerator;
        private readonly IEnumerator<ExpressionOperandBookmark> bookmarksEnumerator;

        internal DynamicCSVRowEnumerator(IEnumerator<Tuple> valuesEnumerator, IEnumerator<ExpressionOperandBookmark> bookmarksEnumerator)
        {
            this.valuesEnumerator = valuesEnumerator;
            this.bookmarksEnumerator = bookmarksEnumerator;
        }

        public RowWithBookmark Current
        {
            get
            {
                /*
                ExpressionOperand[] rowResult = new ExpressionOperand[valuesEnumerator.Current.Length];
                Array.Copy(valuesEnumerator.Current, rowResult, valuesEnumerator.Current.Length);
                ExpressionOperandBookmark bookmarkResult = bookmarksEnumerator.Current;
                return new RowWithBookmark(rowResult, bookmarkResult);
                */

                ExpressionOperandBookmark bookmarkResult = bookmarksEnumerator.Current;
                return new RowWithBookmark(valuesEnumerator.Current, bookmarkResult);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            valuesEnumerator.Dispose();
            bookmarksEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            bool v = valuesEnumerator.MoveNext();
            bool b = bookmarksEnumerator.MoveNext();

            if ((v == true && b == false) || (v == false && b == true))
                throw new InvalidOperationException("Enumerators out of sync");
            return v;
        }

        public void Reset()
        {
            valuesEnumerator.Reset();
            bookmarksEnumerator.Reset();
        }
    }
}
