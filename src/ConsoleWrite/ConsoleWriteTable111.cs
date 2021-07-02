using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleWrite
{
    public class ConsoleWriteTable111
    {
        public IList<object> Headers { get; set; }

        public IList<object[]> Rows { get; protected set; }

        public ConsoleTableOptions Options { get; protected set; }

        public Type[] ColumnTypes { get; private set; }

        public IList<string> Formats { get; private set; }

        public static HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };

        public ConsoleWriteTable111(params string[] columns)
            : this(new ConsoleTableOptions { Columns = new List<string>(columns) })
        {
        }

        public ConsoleWriteTable111(ConsoleTableOptions options)
        {
            Options = options ?? throw new ArgumentNullException("options");
            Rows = new List<object[]>();
            Headers = new List<object>(options.Columns);
        }

        public ConsoleWriteTable111 AddColumn(IEnumerable<string> names)
        {
            foreach (var name in names)
                Headers.Add(name);
            return this;
        }

        public ConsoleWriteTable111 AddRow(params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (!Headers.Any())
                throw new Exception("Please set the columns first");

            if (Headers.Count != values.Length)
                throw new Exception(
                    $"The number columns in the row ({Headers.Count}) does not match the values ({values.Length})");

            Rows.Add(values);
            return this;
        }

        public ConsoleWriteTable111 Configure(Action<ConsoleTableOptions> action)
        {
            action(Options);
            return this;
        }

        public static ConsoleWriteTable111 From<T>(IEnumerable<T> values)
        {
            var table = new ConsoleWriteTable111
            {
                ColumnTypes = GetColumnsType<T>().ToArray()
            };

            var columns = GetColumns<T>();

            table.AddColumn(columns);

            foreach (
                var propertyValues
                in values.Select(value => columns.Select(column => GetColumnValue<T>(value, column)))
            ) table.AddRow(propertyValues.ToArray());

            return table;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // set right alinment if is a number
            var columnAlignment = Enumerable.Range(0, Headers.Count)
                .Select(GetNumberAlignment)
                .ToList();

            // create the string format with padding ; just use for maxRowLength
            var format = Enumerable.Range(0, Headers.Count)
                .Select(i => " | {" + i + "," + columnAlignment[i] + columnLengths[i] + "}")
                .Aggregate((s, a) => s + a) + " |";

            SetFormats(ColumnLengths(), columnAlignment);

            // find the longest formatted line
            var maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
            var columnHeaders = string.Format(Formats[0], Headers.ToArray());

            // longest line is greater of formatted columnHeader and longest row
            var longestLine = Math.Max(maxRowLength, columnHeaders.Length);

            // add each row
            var results = Rows.Select((row, i) => string.Format(Formats[i + 1], row)).ToList();

            // create the divider
            var divider = " " + string.Join("", Enumerable.Repeat("-", longestLine + 2)) + " ";

            builder.AppendLine(divider);
            builder.AppendLine(columnHeaders);

            foreach (var row in results)
            {
                builder.AppendLine(divider);
                builder.AppendLine(row);
            }

            builder.AppendLine(divider);

            if (Options.EnableCount)
            {
                builder.AppendLine("");
                builder.AppendFormat(" Count: {0}", Rows.Count);
            }



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

            return builder.ToString();
        }


        private string GetString(List<int> columnLengths, IList<object> row, string delimiter = "|")
        {
            var sb = new StringBuilder();
            for (var i = 0; i < columnLengths.Count; i++)
            {
                var length = columnLengths[i];
                var value = row[i]?.ToString() ?? string.Empty;

                var drawLength = length - (GetTextWidth(value));
                if (drawLength > 0)
                {
                    if (i == 0)
                    {
                        sb.Append($" {delimiter} {value}{GetWhiteSpace(drawLength) + " |"}");
                    }
                    else if (i == columnLengths.Count - 1)
                    {
                        sb.Append($"　{value}{GetWhiteSpace(drawLength) + "　| "}");
                    }
                    else
                    {
                        sb.Append($"　{value}{GetWhiteSpace(drawLength) + " |"}");
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        sb.Append($" {delimiter} {value} |");
                    }
                    else if (i == columnLengths.Count - 1)
                    {
                        sb.Append($"　{value}　| ");
                    }
                    else
                    {
                        sb.Append($"　{value} |");
                    }
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

            return Enumerable.Range(0, length / 2).Select(d => "　").Aggregate((s, a) => s + a) + (length % 2 > 0 ? " " : "");
        }


        private void SetFormats(List<int> columnLengths, List<string> columnAlignment)
        {
            var allLines = new List<object[]>();
            allLines.Add(Headers.ToArray());
            allLines.AddRange(Rows);

            Formats = allLines.Select(d =>
            {
                return Enumerable.Range(0, Headers.Count)
                    .Select(i =>
                    {
                        var value = d[i]?.ToString() ?? "";
                        var length = columnLengths[i] - (GetTextWidth(value) - value.Length);
                        return " | {" + i + "," + columnAlignment[i] + length + "}";
                    })
                    .Aggregate((s, a) => s + a) + " |";
            }).ToList();
        }

        public static int GetTextWidth(string value)
        {
            if (value == null)
                return 0;

            var length = value.ToCharArray().Sum(c => c > 127 ? 2 : 1);
            return length;
        }

        public string ToMarkDownString()
        {
            return ToMarkDownString('|');
        }

        private string ToMarkDownString(char delimiter)
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // create the string format with padding
            var format = Format(columnLengths, delimiter);

            // find the longest formatted line
            var columnHeaders = string.Format(Formats[0].TrimStart(), Headers.ToArray());

            // add each row
            var results = Rows.Select((row, i) => string.Format(Formats[i + 1].TrimStart(), row)).ToList();

            // create the divider
            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");

            builder.AppendLine(columnHeaders);
            builder.AppendLine(divider);
            results.ForEach(row => builder.AppendLine(row));

            return builder.ToString();
        }

        public string ToMinimalString()
        {
            return ToMarkDownString(char.MinValue);
        }

        public string ToStringAlternative()
        {
            var builder = new StringBuilder();

            // find the longest column by searching each row
            var columnLengths = ColumnLengths();

            // create the string format with padding
            var format = Format(columnLengths);

            // find the longest formatted line
            var columnHeaders = string.Format(Formats[0].TrimStart(), Headers.ToArray());

            // add each row
            var results = Rows.Select((row, i) => string.Format(Formats[i + 1].TrimStart(), row)).ToList();

            // create the divider
            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");
            var dividerPlus = divider.Replace("|", "+");

            builder.AppendLine(dividerPlus);
            builder.AppendLine(columnHeaders);

            foreach (var row in results)
            {
                builder.AppendLine(dividerPlus);
                builder.AppendLine(row);
            }
            builder.AppendLine(dividerPlus);

            return builder.ToString();
        }

        private string Format(List<int> columnLengths, char delimiter = '|')
        {
            // set right alinment if is a number
            var columnAlignment = Enumerable.Range(0, Headers.Count)
                .Select(GetNumberAlignment)
                .ToList();

            SetFormats(columnLengths, columnAlignment);

            var delimiterStr = delimiter == char.MinValue ? string.Empty : delimiter.ToString();
            var format = (Enumerable.Range(0, Headers.Count)
                .Select(i => " " + delimiterStr + " {" + i + "," + columnAlignment[i] + columnLengths[i] + "}")
                .Aggregate((s, a) => s + a) + " " + delimiterStr).Trim();
            return format;
        }

        private string GetNumberAlignment(int i)
        {
            return Options.NumberAlignment == Alignment.Right
                    && ColumnTypes != null
                    && NumericTypes.Contains(ColumnTypes[i])
                ? ""
                : "-";
        }

        private List<int> ColumnLengths()
        {
            var columnLengths = Headers
                .Select((t, i) => Rows.Select(x => x[i])
                    .Union(new[] { Headers[i] })
                    .Where(x => x != null)
                    .Select(x => x.ToString().ToCharArray().Sum(c => c > 127 ? 2 : 1)).Max())
                .ToList();
            return columnLengths;
        }

        public void Write(Format format = ConsoleWrite.Format.Default)
        {
            switch (format)
            {
                case ConsoleWrite.Format.Default:
                    //Options.OutputTo.WriteLine(ToString());
                    Console.WriteLine(ToString());
                    break;
                case ConsoleWrite.Format.MarkDown:
                    Options.OutputTo.WriteLine(ToMarkDownString());
                    break;
                case ConsoleWrite.Format.Alternative:
                    Options.OutputTo.WriteLine(ToStringAlternative());
                    break;
                case ConsoleWrite.Format.Minimal:
                    Options.OutputTo.WriteLine(ToMinimalString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static IEnumerable<string> GetColumns<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name).ToArray();
        }

        private static object GetColumnValue<T>(object target, string column)
        {
            return typeof(T).GetProperty(column).GetValue(target, null);
        }

        private static IEnumerable<Type> GetColumnsType<T>()
        {
            return typeof(T).GetProperties().Select(x => x.PropertyType).ToArray();
        }
    }

    public class ConsoleTableOptions
    {
        public IEnumerable<string> Columns { get; set; } = new List<string>();
        public bool EnableCount { get; set; } = true;

        /// <summary>
        /// Enable only from a list of objects
        /// </summary>
        public Alignment NumberAlignment { get; set; } = Alignment.Left;

        /// <summary>
        /// The <see cref="TextWriter"/> to write to. Defaults to <see cref="Console.Out"/>.
        /// </summary>
        public TextWriter OutputTo { get; set; } = Console.Out;
    }

    public enum Format
    {
        Default = 0,
        MarkDown = 1,
        Alternative = 2,
        Minimal = 3
    }

    public enum Alignment
    {
        Left,
        Right
    }
}
