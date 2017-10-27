using System;
using System.Collections.Generic;
using System.Text;

namespace LunarParser.YAML
{
    public class YAMLReader
    {
        private enum State
        {
            Header,
            Comment,
            Next,              
            Name,
            NewLine,
            Idents,
            Child,
            Content,
        }

        public static DataNode ReadFromString(string contents)
        {
            var lines = contents.Split('\n');
           
            if (!lines[0].StartsWith("---"))
            {
                throw new Exception($"YAML parsing exception, expected valid header");
            }

            int index = 1;
            var root = DataNode.CreateArray(null);
            ReadNodes(lines, ref index, 0, root);

            return root;
        }
        
        private static void ReadNodes(string[] lines, ref int index, int baseIndents, DataNode parent)
        {
            int expectedIdents = -1;

            DataNode currentNode = null;

            do
            {
                if (index >= lines.Length )
                {
                    return;
                }

                int identCount = 0;
                var content = lines[index];

                for (int i=0; i<content.Length; i++)
                {
                    char c = content[i];
                    if (c == ' ')
                    {
                        identCount++;
                    }
                    else
                    {
                        content = content.Substring(i);
                        break;
                    }
                }

                if (identCount < baseIndents)
                {
                    return;
                }

                index++;

                identCount -= baseIndents;

                if (expectedIdents == -1)
                {
                    expectedIdents = identCount;
                }
                else
                if (identCount != expectedIdents)
                {
                    throw new Exception($"YAML parsing exception, unexpected ammount of identation");
                }

                var temp = content.Split(':');

                if (temp.Length != 2)
                {
                    throw new Exception($"YAML parsing exception, bad formed line");
                }

                var name = temp[0].Trim();
                var val = temp[1].Trim();

                if (val.StartsWith("&"))
                {
                    val = null;
                }

                if (!string.IsNullOrEmpty(val))
                {
                    parent.AddField(name, val);
                }
                else
                {
                    currentNode = DataNode.CreateObject(name);
                    parent.AddNode(currentNode);

                    ReadNodes(lines, ref index, baseIndents + 1, currentNode);
                }


            } while (true);

        }
    }
}
