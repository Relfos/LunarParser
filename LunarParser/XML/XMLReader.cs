using System;
using System.Text;

namespace LunarParser.XML
{
    public class XMLReader
    {
        private enum State
        {
            Next,
            TagOpen,
            TagClose,
            Prolog,
            Comment,
            AttributeName,
            AttributeQuote,
            AttributeValue,
            NextAttribute,
            Content,
            CData,
            CDataClose,
        }

        private static bool CDataAt(string contents, int index)
        {
            var tag = "![CDATA[";
            for (int i=0; i<tag.Length; i++)
            {
                int ofs = i + index;
                if (ofs >= contents.Length) return false;

                var c = contents[ofs];

                if (c != tag[i]) return false;
            }

            return true;
        }

        public static DataNode ReadFromString(string contents)
        {
            int index = 0;
            var first = ReadNode(contents, ref index);
            var root = DataNode.CreateObject(null);
            root.AddNode(first);
            return root;
        }

        private static DataNode ReadNode(string contents, ref int index)
        {
            DataNode result = null;

            var state = State.Next;
            var prevState = State.Next;
            char c;

            StringBuilder name_content = new StringBuilder();
            StringBuilder value_content = new StringBuilder();

            int rewind_index = index;

            do
            {
                bool isWhiteSpace;
                bool next = false;
                bool inside = state == State.Content || state == State.TagOpen || state == State.TagClose ||
                              state == State.AttributeName || state == State.AttributeValue;

                do
                {
                    if (index >= contents.Length)
                    {
                        if (state == State.Next) // no useful data
                            return null;
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
                                        state = State.TagOpen;
                                        name_content.Length = 0;
                                        break;
                                    }

                                default:
                                    {
                                        throw new Exception($"XML parsingexception at {ParserUtils.GetOffsetError(contents, index)}, unexpected character");
                                    }
                            }
                            break;
                        }

                    case State.TagOpen:
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
                                            prevState = State.Next;
                                        }
                                        else
                                        {
                                            name_content.Append(c);
                                        }
                                        break;
                                    }

                                case '/':
                                    {
                                        result = DataNode.CreateObject(name_content.ToString());
                                        state = State.TagClose;
                                        break;
                                    }

                                case '>':
                                    {
                                        result = DataNode.CreateObject(name_content.ToString());
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

                    case State.TagClose:
                        {
                            switch (c)
                            {
                                case '>':
                                    {
                                        // previously created:
                                        // result = DataNode.CreateObject(name_content.ToString());
                                        return result;
                                    }
                                default:
                                    {
                                        // TODO: verify that the close tag matches the open tag
                                        // name_content.Append(c);
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.AttributeName:
                        {
                            switch (c)
                            {
                                case '/':
                                    {
                                        state = State.TagClose;
                                        break;
                                    }

                                case '=':
                                    {
                                        state = State.AttributeQuote;
                                        break;
                                    }

                                default:
                                    {
                                        if (name_content.Length > 0 || !char.IsWhiteSpace(c))
                                        {
                                            name_content.Append(c);
                                        }
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
                                throw new Exception($"XML parsingexception at {ParserUtils.GetOffsetError(contents, index)}, unexpected character");
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
                                        value_content.Length = 0;
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

                                default:
                                    {
                                        if (char.IsLetter(c))
                                        {
                                            name_content.Length = 0;
                                            name_content.Append(c);
                                            state = State.AttributeName;
                                        }
                                        else
                                        {
                                            throw new Exception($"XML parsingexception at {ParserUtils.GetOffsetError(contents, index)}, unexpected character");
                                        }

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
                                            state = prevState;
                                        }
                                        break;
                                    }
                            }
                            break;
                        }

                    case State.CData:
                        {
                            if (c == ']' && contents[index-2]==c && index<contents.Length && contents[index]=='>')
                            {
                                state = State.Content;
                                value_content.Length--;
                                index++;
                            }
                            else
                            {
                                value_content.Append(c);
                            }

                            break;
                        }

                    case State.Content:
                        {
                            switch (c)
                            {
                                case '<':
                                    {
                                        if (CDataAt(contents, index))
                                        {
                                            state = State.CData;
                                            index += 8;
                                        }
                                        else
                                        if (index<contents.Length && contents[index] == '/')
                                        {
                                            state = State.TagClose;
                                            result.Value += value_content.ToString();
                                        }
                                        else if (index< contents.Length-3 && contents[index] == '!' && contents[index+1] == '-' && contents[index+2] == '-')
                                        {
                                            state = State.Comment;
                                            prevState = State.Content;
                                            index += 2;
                                        }
                                        else
                                        {
                                            index--;
                                            var child = ReadNode(contents, ref index);
                                            if (child == null) // only valid at top-level. Here must be an input error
                                                 throw new Exception("XML parsing exception, unexpected end of data");
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
