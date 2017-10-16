using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunarParser.Binary
{
    public class BINWriter
    {
        private static void GenDic(Dictionary<string, ushort> dic, DataNode node)
        {
            if (!dic.ContainsKey(node.Name))
            {
                dic[node.Name] = (ushort)(dic.Count + 1);
            }

            foreach (var child in node.Children)
            {
                GenDic(dic, child);
            }
        }

        private static void WriteNode(BinaryWriter writer, Dictionary<string, ushort> dic, DataNode node)
        {
            ushort id = dic[node.Name];
            writer.Write(id);

            byte[] bytes = string.IsNullOrEmpty(node.Value) ? null : Encoding.UTF8.GetBytes(node.Value);
            ushort len = (byte)(bytes == null ? 0 : bytes.Length);
            writer.Write(len);
            if (bytes != null)
            {
                writer.Write(bytes);
            }

            int childCount = node.ChildCount;
            writer.Write(childCount);

            foreach (var child in node.Children)
            {
                WriteNode(writer, dic, child);
            }
        }

        public static byte[] SaveToBytes(DataNode root)
        { 
            var dic = new Dictionary<string, ushort>();
            GenDic(dic, root);

            using (var stream = new MemoryStream(1024))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    int entryCount = dic.Count;
                    writer.Write(entryCount);
                    foreach (var entry in dic)
                    {
                        writer.Write(entry.Value);
                        var bytes = Encoding.UTF8.GetBytes(entry.Key);
                        byte len = (byte)bytes.Length;
                        writer.Write(len);
                        writer.Write(bytes);
                    }

                    WriteNode(writer, dic, root);

                }

                return stream.ToArray();
            }


        }
    }
}
