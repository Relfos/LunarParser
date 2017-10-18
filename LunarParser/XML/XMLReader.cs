using System;
using System.Text;

namespace LunarParser.XML
{
    public class XMLReader
    {
        private enum State
        {
            Next,
            Name,
            Prolog,
            Comment,
            AttributeName,
            AttributeQuote,
            AttributeValue,
            NextAttribute,
            Content,
        }


        public static DataNode ReadFromString(string contents)
        {
            int index = 0;
            var root = ReadNode(contents, ref index);
            return root;
        }

        private static string GetPos(string contents, int index)
        {
            return "offset " + index;
        }

        private static DataNode ReadNode(string contents, ref int index)
        {
            DataNode result = null;

            var state = State.Next;
            char c;

            StringBuilder name_content = new StringBuilder();
            StringBuilder value_content = new StringBuilder();

            int rewind_index = index;

            do
            {
                bool isWhiteSpace;
                bool next = false;
                bool inside = state == State.Content || state == State.Name || state == State.AttributeName || state == State.AttributeValue;

                do
                {
                    if (index >= contents.Length)
                    {
                        throw new Exception($"XML parsing exception, unexpected end of data");
                    }

                    c = contents[index];
                    isWhiteSpace = Char.IsWhiteSpace(c);

                    if (!isWhiteSpace)
                    {
                        rewind_index = index;
                    }

                    index++;


                    next = isWhiteSpace && !inside;
                } while (next);

                switch (state)
                {
                    case State.Next:
                        {
                            switch (c)
                            {
                                case '<':
                                    {                                        
                                        state = State.Name;
                                        name_content.Length = 0;
                                        break;
                                    }

                                default:
                                    {
                                        throw new Exception($"XML parsingexception at {GetPos(contents, index)}, unexpected character");
                                    }
                            }
                            break;
                        }

                    case State.Name:
                        {
                            switch (c)
                            {
                                case '?':
                                    {
                                        if (contents[index - 2] == '<')
                                        {
                                            state = State.Prolog;
                                        }
                                        else
                                        {
                                            name_content.Append(c);
                                        }
                                        break;
                                    }

                                case '!':
                                    {
                                        if (index< contents.Length-3 && contents[index - 2] == '<' && contents[index] == '-' && contents[index+1] == '-')
                                        {
                                            state = State.Comment;
                                        }
                                        else
                                        {
                                            name_content.Append(c);
                                        }
                                        break;
                                    }

                                case '/':
                                    {
                                        break;
                                    }

                                case '>':
                                    {
                                        result = DataNode.CreateObject(name_content.ToString());

                                        if (contents[index-2] == '/')
                                        {
                                            return result;
                                        }

                                        state = State.Content;
                                        break;
                                    }

                                case ' ':
                                    {
                                        result = DataNode.CreateObject(name_content.ToString());
                                        name_content.Length = 0;
                                        state = State.AttributeName;
                                        break;
                                    }

                                default:
                                    {
                                        name_content.Append(c);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.AttributeName:
                        {
                            switch (c)
                            {
                                case '=':
                                    {
                                        state = State.AttributeQuote;
                                        break;
                                    }

                                default:
                                    {
                                        name_content.Append(c);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.AttributeQuote:
                        {
                            if (c == '"')
                            {
                                state = State.AttributeValue;
                                value_content.Length = 0;
                            }
                            else
                            {
                                throw new Exception($"XML parsingexception at {GetPos(contents, index)}, unexpected character");
                            }
                            break;
                        }

                    case State.AttributeValue:
                        {
                            switch (c)
                            {
                                case '"':
                                    {
                                        result.AddField(name_content.ToString(), value_content.ToString());
                                        state = State.NextAttribute;
                                        break;
                                    }

                                default:
                                    {
                                        value_content.Append(c);
                                        break;
                                    }
                            }

                            break;
                        }

                    case State.NextAttribute:
                        {
                            switch (c)
                            {
                                case '/':
                                    {
                                        break;
                                    }

                                case '>':
                                    {
                                        if (contents[index-2] == '/')
                                        {
                                            return result;
                                        }

                                        state = State.Content;

                                        break;
                                    }
                            }

                            break;
                        }

                    case State.Prolog:
                        {
                            if (c == '>')
                            {
                                state = State.Next;
                            }
                            break;
                        }

                    case State.Comment:
                        {
                            switch (c)
                            {
                                case '>':
                                    {
                                        if (contents[index - 2] == '-' && contents[index - 3] == '-')
                                        {
                                            state = State.Next;
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.Content:
                        {
                            switch (c)
                            {
                                case '<':
                                    {
                                        if (index<contents.Length && contents[index] == '/')
                                        {
                                            result.Value = value_content.ToString();
                                            return result;
                                        }
                                        else
                                        {
                                            index--;
                                            var child = ReadNode(contents, ref index);
                                            result.AddNode(child);
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        value_content.Append(c);
                                        break;
                                    }
                            }
                            break;
                        }



                }

            } while (true);
        }
    }
}
