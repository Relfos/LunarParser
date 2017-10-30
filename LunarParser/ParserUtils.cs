using System;
using System.Collections.Generic;
using System.Text;

namespace LunarParser
{
    internal static class ParserUtils
    {
        public static void GetColumnAndLine(string text, int offset, out int col, out int line)
        {
            line = 1;
            col = 0;

            for (int i=0; i<=offset; i++)
            {
                if (i>=text.Length )
                {
                    return;
                }

                var c = text[i];
                if (c == '\n')
                {
                    col = 0;
                    line++;
                }

                col++;
            }
        }

        public static string GetOffsetError(string text, int offset)
        {
            int col, line;

            GetColumnAndLine(text, offset, out col, out line);

            return $"at line {line}, column {col}";
        }

        public static bool IsNumeric(this string text) {
            double val;
            return double.TryParse(text, out val);
        }
    }
}
