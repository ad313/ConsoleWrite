using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ConsoleWrite
{
    public class ConsoleWriteTable
    {
        public IList<object> Headers { get; private set; } = new List<object>();

        public ConsoleColor? ConsoleColor { get; private set; }

        public IList<object[]> Rows { get; protected set; } = new List<object[]>();

        public static ConsoleWriteTable From<T>(List<T> values)
        {
            var type = typeof(T);

            var columns = GetColumns(type.GetProperties());
            var table = AddHeader(columns);

            foreach (var propertyValues in values.Select(value => columns.Select(column => GetColumnValue(value, column, type))))
                table.AddRow(propertyValues.ToArray());

            return table;
        }

        public static ConsoleWriteTable AddHeader(params string[] headers)
        {
            if (headers == null || !headers.Any())
                throw new ArgumentNullException(nameof(headers));

            var table = new ConsoleWriteTable();

            foreach (var name in headers)
                table.Headers.Add(name ?? string.Empty);

            return table;
        }

        public ConsoleWriteTable AddRow(params object[] values)
        {
            if (values == null || !values.Any())
                throw new ArgumentNullException(nameof(values));

            if (!Headers.Any())
                throw new ArgumentException("Please Add Header first");

            if (Headers.Count > values.Length)
            {
                for (var i = 0; i < Headers.Count - values.Length; i++)
                {
                    values = values.Append(string.Empty).ToArray();
                }
            }

            Rows.Add(values);
            return this;
        }

        public ConsoleWriteTable SetHeaderColor(ConsoleColor color)
        {
            ConsoleColor = color;
            return this;
        }
        
        public StringBuilder ToString(TextAlign textAlign = TextAlign.Left, string delimiter = "|")
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            var divider = CreateDivider(columnLengths);
            builder.AppendLine(divider);

            var header = GetString(columnLengths, Headers, delimiter, textAlign);
            builder.AppendLine(header);
            builder.AppendLine(divider);

            var rowString = Rows.Select(row => GetString(columnLengths, row, delimiter, textAlign)).ToList();
            foreach (var row in rowString)
            {
                builder.AppendLine(row);
                builder.AppendLine(divider);
            }

            return builder;
        }

        public void Write(TextAlign textAlign = TextAlign.Left, string delimiter = "|")
        {
            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            var divider = CreateDivider(columnLengths);
            var header = GetString(columnLengths, Headers, delimiter, textAlign);
            if (ConsoleColor != null)
            {
                Console.ForegroundColor = ConsoleColor.Value;
                Console.WriteLine(divider);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine(divider);
            }

            Console.WriteLine(header);

            var rowString = Rows.Select(row => GetString(columnLengths, row, delimiter, textAlign)).ToList();
            foreach (var row in rowString)
            {
                Console.WriteLine(divider);
                Console.WriteLine(row);
            }

            Console.WriteLine(divider);
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

        private string CreateDivider(List<int> columnLengths)
        {
            var divider = " ";
            foreach (var length in columnLengths)
            {
                divider += "+" + string.Join("", Enumerable.Repeat("-", length + 2));
            }

            divider += "+";
            return divider;
        }

        private string GetString(List<int> columnLengths, IList<object> row, string delimiter, TextAlign textAlign)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < columnLengths.Count; i++)
            {
                var totalLength = columnLengths[i];
                var value = row[i]?.ToString() ?? string.Empty;

                var whiteSpaceLength = totalLength - GetTextWidth(value);
                if (whiteSpaceLength > 0)
                {
                    if (i == 0)
                        sb.Append($" {delimiter}");

                    sb.Append($"{GetFirstWhiteSpace(whiteSpaceLength, textAlign)}{value}{GetLastWhiteSpace(whiteSpaceLength, textAlign)}{delimiter}");

                    if (i == columnLengths.Count - 1)
                        sb.Append(" ");
                }
                else
                {
                    if (i == 0)
                        sb.Append($" {delimiter}");

                    sb.Append($" {value} {delimiter}");

                    if (i == columnLengths.Count - 1)
                        sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        private string GetFirstWhiteSpace(int length, TextAlign textAlign)
        {
            switch (textAlign)
            {
                case TextAlign.Left:
                    return " ";
                case TextAlign.Center:
                    var l = (length + 2) / 2;
                    return GetWhiteSpace(l);
                case TextAlign.Right:
                    return GetWhiteSpace(length + 1);
                default:
                    throw new ArgumentOutOfRangeException(nameof(textAlign), textAlign, null);
            }
        }

        private string GetLastWhiteSpace(int length, TextAlign textAlign)
        {
            switch (textAlign)
            {
                case TextAlign.Left:
                    return GetWhiteSpace(length + 1);
                case TextAlign.Center:
                    var l = (length + 2) / 2;
                    return GetWhiteSpace(length + 2 - l);
                case TextAlign.Right:
                    return " ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(textAlign), textAlign, null);
            }
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

        private static string[] GetColumns(PropertyInfo[] properties)
        {
            return properties.Select(x => x.Name).ToArray();
        }

        private static object GetColumnValue(object target, string column, Type type)
        {
            return type.GetProperty(column)?.GetValue(target, null);
        }

        public static int GetTextWidth(string value)
        {
            if (value == null)
                return 0;

            return value.ToCharArray().Sum(c => c > 127 ? 2 : 1);
        }
    }

    public enum TextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }
}