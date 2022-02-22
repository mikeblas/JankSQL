using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    internal interface IComponentOutput
    {
        ResultSet GetRows(int max);

    }

    internal class TableSource : IComponentOutput
    {
        Engines.DynamicCSV source;
        int currentRow = 0;

        internal TableSource(Engines.DynamicCSV source)
        {
            this.source = source;
            this.source.Load();
        }

        internal void Describe()
        {
        }

        public ResultSet GetRows(int max)
        {
            ResultSet rs = new ResultSet();
            List<string> columnNames = new List<string>();
            for (int n = 0; n < source.ColumnCount; n++)
                columnNames.Add(source.ColumnName(n));
            rs.SetColumnNames(columnNames);

            while (currentRow < source.RowCount && rs.RowCount < max)
            {
                ExpressionOperand[] thisRow = new ExpressionOperand[source.ColumnCount];

                for (int i = 0; i < source.ColumnCount; i++)
                {
                    thisRow[i] = ExpressionOperand.NVARCHARFromString(source.Row(currentRow)[i]);
                }

                rs.AddRow(thisRow);
                currentRow++;
            }

            return rs;
        }
    }

    internal class Filter : IComponentOutput
    {
        IComponentOutput myInput;
        List<List<ExpressionNode>> predicateExpressionLists;

        internal Filter(IComponentOutput input, List<List<ExpressionNode>> predicateExpressionLists)
        {
            this.Input = input;
            this.Predicates = predicateExpressionLists;
        }

        internal Filter()
        {

        }

        internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal List<List<ExpressionNode>> Predicates { set { predicateExpressionLists = value; } }

        public ResultSet GetRows(int max)
        {
            ResultSet rsInput = myInput.GetRows(max);

            ResultSet rsOuptut = ResultSet.NewWithShape(rsInput);


            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // evaluate the where clauses, if any
                bool predicatePassed = true;
                foreach (var p in predicateExpressionLists)
                {
                    ExpressionOperand result = SelectListContext.Execute(p, rsInput, i);

                    if (!result.IsTrue())
                    {
                        predicatePassed = false;
                        break;
                    }
                }

                if (!predicatePassed)
                    continue;

                rsOuptut.AddRowFrom(rsInput, i);
            }


            return rsOuptut;
        }
    }


    internal class Select
    {
        IComponentOutput myInput;
        TSqlParser.Select_list_elemContext[] selectListContexts;
        SelectListContext selectList;

        internal IComponentOutput Input { get { return myInput; } set { myInput = value; } }

        internal Select(TSqlParser.Select_list_elemContext[] selectListContexts, SelectListContext selectList)
        {
            this.selectListContexts = selectListContexts;
            this.selectList = selectList;
        }

        internal ResultSet GetRows(int max)
        {
            ResultSet rsInput = myInput.GetRows(max);

            // get an effective column list ...
            List<string> effectiveColumns = new List<string>();
            int resultSetColumnIndex = 0;
            foreach (var c in selectListContexts)
            {
                if (c.asterisk() != null)
                {
                    Console.WriteLine("Asterisk!");
                    for (int i = 0; i < rsInput.ColumnCount; i++)
                    {
                        effectiveColumns.Add(rsInput.GetColumnName(i));
                        ExpressionNode x = new ExpressionOperandFromColumn(rsInput.GetColumnName(i));
                        List<ExpressionNode> xlist = new List<ExpressionNode>();
                        xlist.Add(x);
                        selectList.AddSelectListExpressionList(xlist);
                        /*
                        ExpressionList.Add(x);
                        EndSelectListExpressionList();
                        */
                    }
                }
                else
                {
                    effectiveColumns.Add(selectList.RowsetColumnName(resultSetColumnIndex++));
                }
            }


            ResultSet rsOutput = new ResultSet();
            rsOutput.SetColumnNames(effectiveColumns);

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                // add the row to the result set
                int exprIndex = 0;
                int rsIndex = 0;

                ExpressionOperand[] thisRow = rsInput.Row(i);
                ExpressionOperand[] rowResults = new ExpressionOperand[effectiveColumns.Count];
                foreach (string columnName in effectiveColumns)
                {
                    int idx = rsInput.ColumnIndex(columnName);
                    if (idx == -1 || true)
                    {
                        ExpressionOperand result = selectList.Execute(exprIndex, rsInput, i);
                        rowResults[rsIndex] = result;
                        exprIndex++;
                    }
                    else
                    {
                        rowResults[rsIndex] = thisRow[idx];
                    }
                    rsIndex++;
                }

                rsOutput.AddRow(rowResults);
            }


            return rsOutput;
        }
    }
}