using System;
using System.Collections.Generic;
using System.Globalization;

namespace LunarParser
{
    public enum NodeKind
    {
        Unknown,
        Object,
        Array,
        Field
    }

    public class DataNode
    {
        protected List<DataNode> _children = new List<DataNode>();
        public IEnumerable<DataNode> Children { get { return _children; } }

        public DataNode Parent { get; private set; }

        public int ChildCount { get { return _children.Count; } }

        public string Name { get; private set; }
        public string Value { get; set; }
        public NodeKind Kind { get; private set; }

        private DataNode(NodeKind kind, string name = null, string value = null)
        {
            this.Kind = kind;
            this.Parent = null;
            this.Name = name;
            this.Value = value;
        }

        public static DataNode CreateObject(string name)
        {
            return new DataNode(NodeKind.Object, name);
        }

        public static DataNode CreateArray(string name)
        {
            return new DataNode(NodeKind.Array, name);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }
            return "[Null]";
        }

        public DataNode AddNode(DataNode node)
        {
            if (node == null)
            {
                return null;
            }

            this._children.Add(node);
            node.Parent = this;
            return node;
        }

        public void AddField(string name, object value)
        {
            if (this.Kind != NodeKind.Object)
            {
                throw new Exception("The kind of this node is not 'object'!");
            }

            var child = new DataNode(NodeKind.Field, name, value.ToString());
            this.AddNode(child);
        }

        public bool HasNode(string name, int index = 0)
        {
            return GetNode(name, index) != null;
        }

        public DataNode GetNode(string name, int index = 0)
        {
            int n = 0;

            foreach (DataNode child in _children)
            {
                if (String.Compare(child.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (n >= index)
                    {
                        return child;
                    }
                    else {
                        n++;
                    }

                }
            }

            return null;
        }

        public long ReadLong(string name, long defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                long result = defaultValue; 
                if (long.TryParse(node.Value, out result))
                    return result;
            }

            return defaultValue;
        }


        public int ReadInt32(string name, int defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                int result = defaultValue;
                if (int.TryParse(node.Value, out result))
                    return result;
            }

            return defaultValue;
        }

        public uint ReadUInt32(string name, uint defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                uint result = defaultValue;
                if (uint.TryParse(node.Value, out result))
                    return result;
            }

            return defaultValue;
        }


        public byte ReadByte(string name, byte defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                byte result = defaultValue;
                if (byte.TryParse(node.Value, out result))
                    return result;
            }

            return defaultValue;
        }

        public sbyte ReadSByte(string name, sbyte defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                sbyte result = defaultValue;
                if (sbyte.TryParse(node.Value, out result))
                    return result;
            }

            return defaultValue;
        }

        public T ReadEnum<T>(string name, T defaultValue = default(T)) where T : IConvertible
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                int result = 0;
                if (int.TryParse(node.Value, out result))
                    return (T)(object)result;
            }

            return defaultValue;
        }

        public bool ReadBoolean(string name, bool defaultValue = false)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return (node.Value.Equals("1") || string.Equals(node.Value, "true", StringComparison.CurrentCultureIgnoreCase));
            }

            return defaultValue;
        }

        public Decimal ReadDecimal(string name, decimal defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                decimal result = defaultValue;
                if (decimal.TryParse(node.Value.Replace(",","."), NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }

        public float ReadFloat(string name, float defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                float result = defaultValue;
                if (float.TryParse(node.Value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }

        public string ReadString(string name, string defaultValue = "", bool tryParent = false)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.Value;
            }

            if (tryParent && Parent != null)
            {
                return Parent.ReadString(name, defaultValue, true);
            }

            return defaultValue;
        }
    }
}
