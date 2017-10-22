using System;
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
            throw new NotImplementedException();
        }
    }
}
