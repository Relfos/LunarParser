using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunarLabs.Parser.CSV
{
    public class CSVWriter
    {
        public static string WriteToString(DataNode node)
        {
            var content = new StringBuilder();

            var first = node.Children.FirstOrDefault();

            int index = 0;
            var header = new List<string>();
            foreach (var field in first.Children)
            {
                if (index > 0 )
                {
                    content.Append(',');
                }
                content.Append(field.Name);
                header.Add(field.Name);
                index++;
            }
            content.AppendLine();

            foreach (var item in node.Children)
            {
                index = 0;
                foreach (var fieldName in header)
                {
                    var field = item.GetNodeByName(fieldName);

                    if (index > 0)
                    {
                        content.Append(',');
                    }

                    if (field != null)
                    {
                        bool escape = field.Value.Contains(',') || field.Value.Contains('\n');

                        if (escape) { content.Append('"'); }
                        content.Append(field.Value);
                        if (escape) { content.Append('"'); }
                    }

                    index++;
                }

                content.AppendLine();
            }

            return content.ToString();
        }
    }
}
