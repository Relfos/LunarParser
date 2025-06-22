using System;
using System.Linq;
using System.Text;

namespace LunarLabs.Parser.XML
{

    public class XMLWriter

    {
        public static string WriteToString(DataNode node, bool expand = false, bool escape = false, bool allowEmptyNames = false)
        {
            StringBuilder builder = new StringBuilder();

            WriteNode(builder, node, 0, expand, escape, allowEmptyNames);

            return builder.ToString();
        }

        private static void WriteNode(StringBuilder buffer, DataNode node, int tabs, bool expand, bool escape, bool allowEmptyNames)
        {
            if (!allowEmptyNames && string.IsNullOrEmpty(node.Name))
            {
                throw new Exception("Node cannot have empty name");
            }

            for (int i = 0; i < tabs; i++)
            {
                buffer.Append('\t');
            }

            buffer.Append('<');
            buffer.Append(node.Name);

            int processedChildren = 0;

            if (!expand)
            {
                foreach (DataNode child in node.Children)
                {
                    if (child.Children.Any())
                    {
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
            }

            var finished = processedChildren == node.ChildCount && node.Value == null;

            if (finished)
            {
                buffer.Append('/');
            }
            buffer.Append('>');

            if (finished)
            {
                buffer.AppendLine();
                return;
            }

            if (node.Children.Any())
            {
                buffer.AppendLine();

                foreach (DataNode child in node.Children)
                {
                    if (!expand && !child.Children.Any())
                    {
                        continue;
                    }

                    WriteNode(buffer, child, tabs + 1, expand, escape, allowEmptyNames);
                }

                if (node.Value != null)
                {
                    if (node.Value.Trim().Length > 0)
                    {
                        throw new Exception("Nodes with values cannot have child nodes");
                    }
                }
            }
            else
            {
                buffer.Append(EscapeXML(node.Value, escape));
            }

            if (node.Children.Any())
            {
                for (int i = 0; i < tabs; i++)
                {
                    buffer.Append('\t');
                }
            }

            buffer.Append('<');
            buffer.Append('/');
            buffer.Append(node.Name);
            buffer.Append('>');

            buffer.AppendLine();
        }


        /*
        private static void WriteNode2(StringBuilder buffer, DataNode node, int tabs, bool expand, bool escape)
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
        }*/

        private static string EscapeXML(string content, bool escape)
        {
            if (!escape || string.IsNullOrEmpty(content))
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