using System;
using System.Collections.Generic;
using System.Text;

namespace LunarParser.JSON
{
    public static class JSONWriter
    {
        private static void Append(DataNode node, StringBuilder sb, bool genBounds = true)
        {
            if (node.Name != null)
            {
                sb.Append('"');
                sb.Append(node.Name);
                sb.Append("\" : ");
            }

            if (node.Value != null)
            {
                sb.Append("\"");
                sb.Append(EscapeJSON(node.Value));
                sb.Append('"');
            }
            else
            {
                if (node.Name != null || genBounds)
                {
                    sb.Append(node.Kind == NodeKind.Array ? '[' : '{');
                }

                if (node.Children != null)
                {
                    int index = 0;
                    foreach (var entry in node.Children)
                    {
                        if (index > 0)
                        {
                            sb.Append(',');
                        }

                        Append(entry, sb, node.Kind == NodeKind.Array);

                        index++;
                    }
                }

                if (node.Name != null || genBounds)
                {
                    sb.Append(node.Kind == NodeKind.Array ? ']' : '}');
                }
            }
        }

        public static string WriteToString(DataNode node)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            Append(node, sb);
            sb.Append('}');
            return sb.ToString();
        }

        public static string EscapeJSON(string s)
        {
            if (s == null || s.Length == 0)
            {
                return "";
            }

            char c = '\0';
            int i;
            int len = s.Length;
            StringBuilder sb = new StringBuilder(len + 4);
            String t;

            for (i = 0; i < len; i += 1)
            {
                c = s[i];
                switch (c)
                {
                    case '\\':
                    case '"':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '/':
                        sb.Append('\\');
                        sb.Append(c);
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    default:
                        if (c < ' ')
                        {
                            t = "000" + String.Format("X", c);
                            sb.Append("\\u" + t.Substring(t.Length - 4));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

    }
}
