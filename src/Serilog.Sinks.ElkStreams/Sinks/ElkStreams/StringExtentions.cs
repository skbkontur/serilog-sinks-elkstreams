using System;
using System.Text.RegularExpressions;

namespace Serilog.Sinks.ElkStreams
{
    static class StringExtentions
    {
        const string Pattern = "{0}[truncated {1}]";

        public static string Truncate(this string source, int length)
        {
            if (source.Length <= length)
            {
                return source;
            }

            var x = source.Length - length;
            var y = Pattern.Length - 6 + GetNumberOfDigits(x);
            var truncatedCount = x + y + (GetNumberOfDigits(x + y) > GetNumberOfDigits(x) ? 1 : 0);
            return string.Format(Pattern, source.Substring(0, source.Length - truncatedCount), truncatedCount);
        }

        public static string RemoveGuids(this string source)
        {
            return GuidRegex.Replace(source, string.Empty);
        }

        static int GetNumberOfDigits(int i) => (int)Math.Floor(Math.Log10(i) + 1);
        private static readonly Regex GuidRegex = new Regex("[0-9a-f]{8}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{12}", RegexOptions.Compiled);
    }
}