using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace FixedPawnGenerate
{
    internal class TablePrinter
    {
        private List<string> headers;

        private List<List<string>> rows;

        public float Padding = 10f;

        public TablePrinter(List<string> headers)
        {
            this.headers = headers;
            this.rows = new List<List<string>>();
        }

        public void AddRow(List<string> row)
        {
            if (row.Count != headers.Count)
            {
                Log.Error($"Row count {row.Count} does not match header count {headers.Count}");
            }
            rows.Add(row);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            List<float> maxRowWidths = new List<float>();

            //计算行数的位数
            int numberCount = rows.Count.ToString().Length;

            float numberWidth = Text.CalcSize("[" + new string('0', numberCount) + "]").x + Padding;

            for (int i = 0; i < headers.Count; i++)
            {
                float headerWidth = Text.CalcSize(headers[i]).x;
                maxRowWidths.Add(headerWidth);
            }


            // Calculate column widths
            foreach (var row in rows)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    float cellWidth = Text.CalcSize(row[i]).x;
                    maxRowWidths[i] = Math.Max(maxRowWidths[i], cellWidth);
                }
            }

            //build header
            sb.Append(PadRight("", numberWidth));
            for (int i = 0; i < headers.Count; i++)
            {
                string header = headers[i];
                sb.Append(PadRight(header, maxRowWidths[i] + Padding));
            }
            sb.Append("\n");

            //build rows
            int rowIndex = 1;
            foreach (var row in rows)
            {
                sb.Append(PadRight($"[{NumberToString(rowIndex++, numberCount)}]", numberWidth));
                for (int i = 0; i < headers.Count; i++)
                {
                    sb.Append(PadRight(row[i], maxRowWidths[i] + Padding));
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }


        private string PadRight(string str, float width)
        {
            string result = str;

            while (Text.CalcSize(result).x < width)
            {
                result += " ";
            }

            return result;
        }

        private string NumberToString(int number, int totalDigits)
        {
            string result = number.ToString();
            while (result.Length < totalDigits)
            {
                result = "0" + result;
            }
            return result;
        }
    }
}
