//#define DATETIME_AS_TIMESTAMPS

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

        public bool HasChildren { get { return _children.Count > 0; } }

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

        public DataNode this[string name]
        {
            get { return GetNode(name); }
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
                if (ChildCount == 0)
                {
                    return Name + " = " + Value;
                }
                return Name;
            }

            return this.Parent != null ? "[Node]" : "[Root]";
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

        private static readonly long epochTicks = new DateTime(1970, 1, 1).Ticks;

        public DataNode AddField(string name, object value)
        {
            if (this.Kind == NodeKind.Field)
            {
                throw new Exception("The kind of this node is not 'object'!");
            }

            if (value == null)
            {
                throw new Exception("Value for field is null!");
            }

            if (value is DataNode)
            {
                throw new Exception("Cannot add a node as a field!");
            }

#if DATETIME_AS_TIMESTAMPS
            // convert dates to unix timestamps
            if (value.GetType() == typeof(DateTime))
            {
                value = (((DateTime)value).Ticks - epochTicks).ToString();
            }
#endif

            var child = new DataNode(NodeKind.Field, name, value.ToString());
            this.AddNode(child);
            return child;
        }

        public bool HasNode(string name, int index = 0)
        {
            return GetNode(name, index) != null;
        }

        #region GET_XXX methods
        // internal auxiliary
        private DataNode FindNode(string name, int ndepth, int maxdepth)
        {
            if (String.Compare(this.Name, name, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return this;
            }

            if (ndepth >= maxdepth)
            {
                return null;
            }

            foreach (DataNode child in _children)
            {
                DataNode n = child.FindNode(name, ndepth + 1, maxdepth);
                if (n != null)
                {
                    return n;
                }
            }

            return null;
        }

        public DataNode FindNode(string name, int maxdepth = 0)
        {
            return FindNode(name, 0, maxdepth > 0? maxdepth : int.MaxValue );
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
                    else
                    {
                        n++;
                    }

                }
            }

            return null;
        }

        public DataNode GetNodeByIndex(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                return null;
            }

            return _children[index];
        }

        public T GetEnumValue<T>(string value, T defaultValue = default(T)) where T : IConvertible
        {
            try {
                return (T)Enum.Parse(typeof(T), GetString(value), /* ignorecase */ true);
            }
            catch (Exception) {
                return defaultValue; // 0
            }
        }

        public long GetLong(string name, long defaultValue = 0)
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

        public int GetInt32(string name, int defaultValue = 0)
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

        public uint GetUInt32(string name, uint defaultValue = 0)
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

        public byte GetByte(string name, byte defaultValue = 0)
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

        public sbyte GetSByte(string name, sbyte defaultValue = 0)
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

        public T GetEnum<T>(string name, T defaultValue = default(T)) where T : IConvertible
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

        public bool GetBool(string name, bool defaultValue = false)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return (node.Value.Equals("1") || string.Equals(node.Value, "true", StringComparison.CurrentCultureIgnoreCase));
            }

            return defaultValue;
        }

        public Decimal GetDecimal(string name, decimal defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                decimal result = defaultValue;
                if (decimal.TryParse(node.Value.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result))
                {
                    return result;
                }
            }

            return defaultValue;
        }

        public float GetFloat(string name, float defaultValue = 0)
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

        public string GetString(string name, string defaultValue = "")
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.Value;
            }

            return defaultValue;
        }

#if DATETIME_AS_TIMESTAMPS
        public DateTime GetDateTime(string name, DateTime defaultValue = default(DateTime))
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                long ticks;
                if (long.TryParse(node.Value, out ticks))
                {
                    ticks += epochTicks;
                    return new DateTime(ticks);
                }
            }

            return defaultValue;
        }
#endif

#endregion
    }
}
