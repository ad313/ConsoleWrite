using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleWrite
{
    public class ConsoleWriteTable
    {
        public IList<object> Headers { get; set; } = new List<object>();

        public IList<object[]> Rows { get; protected set; } = new List<object[]>();

        public ConsoleWriteTable AddHeader(params string[] headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            foreach (var name in headers)
                Headers.Add(name ?? string.Empty);

            return this;
        }

        public ConsoleWriteTable AddRow(params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            Rows.Add(values);
            return this;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // longest line is greater of formatted columnHeader and longest row
            //var longestLine = ColumnLengths().Sum() + (columnLengths.Count * 3 + 1);

            var divider = " ";
            foreach (var length in columnLengths)
            {
                divider += "+" + string.Join("", Enumerable.Repeat("-", length + 2));
            }

            divider += "+";

            builder.Clear();
            builder.AppendLine(divider);

            var header = GetString(columnLengths, Headers);
            builder.AppendLine(header);

            var rowString = Rows.Select(row => GetString(columnLengths, row)).ToList();
            foreach (var row in rowString)
            {
                builder.AppendLine(divider);
                builder.AppendLine(row);
            }

            builder.AppendLine(divider);

            Console.WriteLine(builder.ToString());

            return builder.ToString();
        }

        private List<int> ColumnLengths()
        {
            var columnLengths = Headers
                .Select((t, i) => Rows.Select(x => x[i])
                    .Union(new[] { Headers[i] })
                    .Where(x => x != null)
                    .Select(x => GetTextWidth(x.ToString())).Max())
                .ToList();
            return columnLengths;
        }

        private string GetNumberAlignment(int i)
        {
            return "-";
        }

        private string GetString(List<int> columnLengths, IList<object> row, string delimiter = "|")
        {
            var sb = new StringBuilder();
            for (var i = 0; i < columnLengths.Count; i++)
            {
                var length = columnLengths[i];
                var value = row[i]?.ToString() ?? string.Empty;

                var drawLength = length - GetTextWidth(value);
                if (drawLength > 0)
                {
                    if (i == 0)
                        sb.Append($" {delimiter}");

                    sb.Append($" {value}{GetWhiteSpace(drawLength + 1) + "|"}");

                    if (i == columnLengths.Count - 1)
                        sb.Append(" ");
                }
                else
                {
                    if (i == 0)
                        sb.Append($" {delimiter}");

                    sb.Append($" {value} |");

                    if (i == columnLengths.Count - 1)
                        sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        private string GetWhiteSpace(int length)
        {
            if (length <= 0)
                return string.Empty;

            if (length == 1)
                return " ";

            return Enumerable.Range(0, length / 2).Select(d => GetOneWhiteSpace()).Aggregate((s, a) => s + a) + (length % 2 > 0 ? " " : "");
        }

        private string GetOneWhiteSpace()
        {
            return "　";
        }

        public static int GetTextWidth(string value)
        {
            if (value == null)
                return 0;

            return value.ToCharArray().Sum(c => c > 127 ? 2 : 1);
        }
    }
}