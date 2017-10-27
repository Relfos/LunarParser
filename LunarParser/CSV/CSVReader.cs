using System;
using System.Collections.Generic;
using System.Text;

namespace LunarParser.CSV
{
    public class CSVReader
    {
        private enum State
        {
            Header,
            Content
        }

        public static DataNode ReadFromString(string contents)
        {
            var root = DataNode.CreateArray(null);

            var header = new List<string>();

            int index = 0;
            var state = State.Header;
            char c;
            int fieldIndex = 0;

            bool isEscaped = false;

            var content = new StringBuilder();

            DataNode currentNode = null;

            while (index < contents.Length)
            {
                c = contents[index];
                
                index++;
                
                switch (state)
                {
                    case State.Header:
                        {
                            if (c == ',' || c == '\n')
                            {
                                header.Add(content.ToString().Trim());
                                content.Length = 0;
                            }

                            switch (c)
                            {
                                case ',': { break; }

                                case '\n':
                                    {
                                        state = State.Content;
                                        break;
                                    }

                                default:
                                    {
                                        content.Append(c);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Content:
                        {
                            if (!isEscaped && (c == ',' || c == '\n'))
                            {
                                if (fieldIndex < header.Count)
                                {
                                    currentNode.AddField(header[fieldIndex], content.ToString());
                                }

                                content.Length = 0;

                                fieldIndex++;

                                if (c =='\n') 
                                {
                                    fieldIndex = 0;
                                    currentNode = null;
                                }

                                break;
                            }

                            if (c == '"')
                            {
                                if (isEscaped && index<contents.Length && contents[index] == '"')
                                {
                                    index++;
                                }
                                else
                                {
                                    isEscaped = !isEscaped;
                                    break;
                                }
                            }

                            if (currentNode == null)
                            {
                                currentNode = DataNode.CreateObject(null);
                                root.AddNode(currentNode);
                            }

                            content.Append(c);

                            break;
                        }

                }
            }

            if (currentNode != null && fieldIndex < header.Count)
            {
                currentNode.AddField(header[fieldIndex], content.ToString());
            }

            return root;
        }
    }
}
