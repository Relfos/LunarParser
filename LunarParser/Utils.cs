using System.Collections.Generic;

namespace LunarParser
{
    public static class DataNodeUtils
    {
        /// <summary>
        /// Converts a dictionary to a DataSource
        /// </summary>
        public static DataNode ToDataSource(this Dictionary<string, string> dic, string name)
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

        /// <summary>
        /// Converts a DataSource to a Dictionary
        /// </summary>
        public static Dictionary<string, string> ToDictionary(this DataNode node, string name)
        {
            if (node == null)
            {
                return null;
            }

            var result = new Dictionary<string, string>();
            foreach (var child in node.Children)
            {
                if (!string.IsNullOrEmpty(child.Value))
                {
                    result[child.Name] = child.Value;
                }
            }
            return result;
        }

    }

}
