using System;
using System.Linq;
using System.Text;

namespace LunarLabs.Parser.XML
{

    public class XMLWriter

    {
        public static string WriteToString(DataNode node, bool expand = false, bool escape = false)
        {
            StringBuilder builder = new StringBuilder();

            WriteNode(builder, node, 0, expand, escape);

            return builder.ToString();
        }

        private static void WriteNode(StringBuilder buffer, DataNode node, int tabs, bool expand, bool escape)
        {
            for (int i = 0; i < tabs; i++)
            {
                buffer.Append('\t');
            }
            buffer.Append('<');
            buffer.Append(node.Name);

            int skippedChildren = 0;
            int processedChildren = 0;

            foreach (DataNode child in node.Children)
            {
                if (expand || child.Children.Any())
                {
                    skippedChildren++;
                    continue;
                }

                buffer.Append(' ');
                buffer.Append(child.Name);
                buffer.Append('=');
                buffer.Append('"');
                buffer.Append(EscapeXML(child.Value, escape));
                buffer.Append('"');

                processedChildren++;
            }

            if (processedChildren > 0)
            {
                buffer.Append(' ');
            }

            var finished = processedChildren == node.ChildCount && node.Value == null;

            if (finished)
            {
                buffer.Append('/');
            }
            buffer.Append('>');

            if (finished)
            {
                if (!expand)
                {
                    buffer.AppendLine();
                }
                return;
            }
            
            if (processedChildren < node.ChildCount)
            {
                buffer.AppendLine();

                foreach (DataNode child in node.Children)
                {
                    if (!expand && !child.Children.Any())
                    {
                        continue;
                    }

                    WriteNode(buffer, child, tabs + 1, expand, escape);
                }

                for (int i = 0; i < tabs; i++)
                {
                    buffer.Append('\t');
                }
            }

            if (node.Value != null)
            {
                buffer.Append(EscapeXML(node.Value, escape));
            }

            buffer.Append('<');
            buffer.Append('/');
            buffer.Append(node.Name);
            buffer.Append('>');
            buffer.AppendLine();
        }

        private static string EscapeXML(string content, bool escape)
        {
            if (!escape)
            {
                return content;
            }

            var sb = new StringBuilder();
            foreach (var ch in content)
            {
                switch (ch)
                {
                    case '\'': sb.Append("&apos;"); break;
                    case '"': sb.Append("&quot;"); break;
                    case '<': sb.Append("&lt;"); break;
                    case '>': sb.Append("&gt;"); break;
                    case '&': sb.Append("&amp;"); break;

                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }

}