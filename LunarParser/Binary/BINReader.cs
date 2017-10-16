using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunarParser.Binary
{
    public class BINReader
    {
        private static DataNode ReadNode(BinaryReader reader, Dictionary<ushort, string> dic)
        {
            ushort id = reader.ReadUInt16();
            string name = dic[id];
            string value = null;

            ushort len = reader.ReadUInt16();            
            if (len > 0)
            {
                byte[] bytes = reader.ReadBytes(len);
                value = System.Text.Encoding.UTF8.GetString(bytes);
            }

            int childCount = reader.ReadInt32();

            var node = DataNode.CreateObject(name);
            node.Value = value;

            while (childCount>0)
            {
                var child = ReadNode(reader, dic);
                node.AddNode(child);
                childCount--;
            }

            return node;
        }

        public static DataNode ReadFromBytes(byte[] bytes)
        {
            var dic = new Dictionary<ushort, string>();
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                int entryCount = reader.ReadInt32();
                while (entryCount>0)
                {
                    ushort id = reader.ReadUInt16();
                    byte len = reader.ReadByte();
                    var temp = reader.ReadBytes(len);

                    string val = System.Text.Encoding.UTF8.GetString(temp);

                    dic[id] = val;

                    entryCount--;
                }

                return ReadNode(reader, dic);
            }
        }
    }
}
