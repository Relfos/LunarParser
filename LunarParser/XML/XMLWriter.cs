using System;
using System.Linq;
using System.Text;

namespace LunarLabs.Parser.XML
{

    public class XMLWriter
    {
        public static string WriteToString(DataNode node, bool expand = false)
        {
            StringBuilder builder = new StringBuilder();

            WriteNode(builder, node, 0, expand);

            return builder.ToString();
        }

        private static void WriteNode(StringBuilder buffer, DataNode node, int tabs, bool expand)
        {
            for (int i = 0; i < tabs; i++)
            {
                buffer.Append('\t');
            }
            buffer.Append('<');
            buffer.Append(node.Name);

            int cc = 0;
            int cs = 0;

            foreach (DataNode child in node.Children)
            {
                if (expand || child.Children.Any())
                {
                    cc++;
                    continue;
                }

                buffer.Append(' ');
                buffer.Append(child.Name);
                buffer.Append('=');
                buffer.Append('"');
                buffer.Append(child.Value);
                buffer.Append('"');

                cs++;
            }

            if (cs > 0)
            {
                buffer.Append(' ');
            }

            var finished = cc == 0 && (!expand || node.Value == null);

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

            if (cc == 0 && expand && node.Value != null)
            {
                buffer.Append(node.Value);
            }
            else
            {
                buffer.AppendLine();

                foreach (DataNode child in node.Children)
                {
                    if (!expand && !child.Children.Any())
                    {
                        continue;
                    }

                    WriteNode(buffer, child, tabs + 1, expand);
                }

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

    }

}