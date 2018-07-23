using System;
using System.Linq;
using System.Text;

namespace LunarLabs.Parser.XML
{

    public class XMLWriter
    {
        public static string WriteToString(DataNode node)
        {
            StringBuilder builder = new StringBuilder();

            WriteNode(builder, node, 0);

            return builder.ToString();
        }

        private static void WriteNode(StringBuilder buffer, DataNode node, int tabs)
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
                if (child.Children.Any())
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

            if (cc == 0)
            {
                buffer.Append('/');
            }
            buffer.Append('>');
            buffer.AppendLine();

            if (cc == 0)
            {
                return;
            }

            foreach (DataNode child in node.Children)
            {
                if (!child.Children.Any())
                {
                    continue;
                }

                WriteNode(buffer, child, tabs + 1);
            }


            for (int i = 0; i < tabs; i++)
            {
                buffer.Append('\t');
            }

            buffer.Append('<');
            buffer.Append('/');
            buffer.Append(node.Name);
            buffer.Append('>');
            buffer.AppendLine();
        }

    }

}