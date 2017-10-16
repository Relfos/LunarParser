using System.Collections.Generic;

namespace LunarParser
{
    public static class DataNodeUtils
    {
        public static DataNode FromDictionary(this Dictionary<string, string> dic, string name)
        {
            if (dic == null)
            {
                return null;
            }

            var node = DataNode.CreateObject(name);
            foreach (var entry in dic)
            {
                node.AddField(entry.Key.ToLower(), entry.Value);
            }
            return node;
        }

        public static Dictionary<string, string> ToDictionary(this DataNode node, string name)
        {
            if (node == null)
            {
                return null;
            }

            var result = new Dictionary<string, string>();
            foreach (var child in node.Children)
            {
                result[child.Name] = child.Value;
            }
            return result;
        }

    }

}
