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
        String,
        Numeric,
        Boolean,
        Null
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

        public DataNode this[int index]
        {
            get { return GetNodeByIndex(index); }
        }

        public static DataNode CreateObject(string name = null)
        {
            return new DataNode(NodeKind.Object, name);
        }

        public static DataNode CreateArray(string name = null)
        {
            return new DataNode(NodeKind.Array, name);
        }

        public static DataNode CreateValue(object value)
        {

            NodeKind kind;
            var val = ConvertValue(value, out kind);
            return new DataNode(kind, null, val);
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

            if (this.Value != null) return this.Value;

            if (this.Parent == null) return "[Root]";

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

        private static readonly long epochTicks = new DateTime(1970, 1, 1).Ticks;

        public DataNode AddValue(object value)
        {
            return AddField(null, value);
        }

        public DataNode AddField(string name, object value)
        {
            if (this.Kind != NodeKind.Array && this.Kind != NodeKind.Object)
            {
                throw new Exception("The kind of this node is not 'object'!");
            }

            if (value is DataNode)
            {
                throw new Exception("Cannot add a node as a field!");
            }

            NodeKind kind;
            string val = ConvertValue(value, out kind);

            var child = new DataNode(kind, name, val);
            this.AddNode(child);
            return child;
        }

#if DATETIME_AS_TIMESTAMPS
        internal static DateTime FromTimestamp(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        internal static long ToTimestamp(DateTime value)
        {
            long epoch = (value.Ticks - 621355968000000000) / 10000000;
            return epoch;
        }

#endif

        private static string ConvertValue(object value, out NodeKind kind)
        {
            if (value == null)
            {
                kind = NodeKind.Null;
                return "";                
            }

            string val;

#if DATETIME_AS_TIMESTAMPS
            // convert dates to unix timestamps
            if (value.GetType() == typeof(DateTime))
            {
                val = ToTimestamp(((DateTime)value)).ToString();
                kind = NodeKind.Numeric;
            }
            else
#endif
            if (value is float)
            {
                val = ((float)value).ToString(CultureInfo.InvariantCulture);
                kind = NodeKind.Numeric;
            }
            else
            if (value is double)
            {
                val = ((double)value).ToString(CultureInfo.InvariantCulture);
                kind = NodeKind.Numeric;
            }
            else
            if (value is decimal)
            {
                val = ((decimal)value).ToString(CultureInfo.InvariantCulture);
                kind = NodeKind.Numeric;
            }
            else
            if (value is bool)
            {
                val = ((bool)value)?"true":"false";
                kind = NodeKind.Boolean;
            }
            else
            {
                val = value.ToString();
                kind = NodeKind.String;
            }

            return val;
        }

        public bool HasNode(string name, int index = 0)
        {
            return GetNode(name, index) != null;
        }

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
            return FindNode(name, 0, maxdepth > 0 ? maxdepth : int.MaxValue);
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

        #region LONG
        public long AsLong(long defaultValue = 0)
        {
            long result = defaultValue;
            if (long.TryParse(this.Value, out result))
                return result;

            return defaultValue;
        }

        public long GetLong(string name, long defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsLong(defaultValue);
            }

            return defaultValue;
        }

        public long GetLong(int index, long defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsLong(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region INT32
        public int AsInt32(int defaultValue = 0)
        {
            int result = defaultValue;
            if (int.TryParse(this.Value, out result))
                return result;

            return defaultValue;
        }

        public int GetInt32(string name, int defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsInt32(defaultValue);
            }

            return defaultValue;
        }

        public int GetInt32(int index, int defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsInt32(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region UINT32
        public uint AsUInt32(uint defaultValue = 0)
        {
            uint result = defaultValue;
            if (uint.TryParse(this.Value, out result))
                return result;

            return defaultValue;
        }

        public uint GetUInt32(string name, uint defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsUInt32(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region BYTE
        public byte AsByte(byte defaultValue = 0)
        {
            byte result = defaultValue;
            if (byte.TryParse(this.Value, out result))
                return result;

            return defaultValue;
        }

        public byte GetByte(string name, byte defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsByte(defaultValue);
            }

            return defaultValue;
        }

        public byte GetByte(int index, byte defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsByte(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region SBYTE
        public sbyte AsSByte(sbyte defaultValue = 0)
        {
            sbyte result = defaultValue;
            if (sbyte.TryParse(this.Value, out result))
                return result;

            return defaultValue;
        }

        public sbyte GetSByte(string name, sbyte defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsSByte(defaultValue);
            }

            return defaultValue;
        }

        public sbyte GetSByte(int index, sbyte defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsSByte(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region ENUM
        public T AsEnum<T>(T defaultValue = default(T)) where T : IConvertible
        {
            try
            {
                return (T)Enum.Parse(typeof(T), this.Value, /* ignorecase */ true);
            }
            catch (Exception)
            {
                int result = 0;
                if (int.TryParse(this.Value, out result))
                {
                    return (T)(object)result;
                }
            }

            return defaultValue;
        }

        public T GetEnum<T>(string name, T defaultValue = default(T)) where T : IConvertible
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsEnum<T>(defaultValue);
            }

            return defaultValue;
        }

        public T GetEnum<T>(int index, T defaultValue = default(T)) where T : IConvertible
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsEnum<T>(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region BOOL
        public bool AsBool(bool defaultValue = false)
        {
            if (this.Value.Equals("1") || string.Equals(this.Value, "true", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }

            if (this.Value.Equals("0") || string.Equals(this.Value, "false", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            return defaultValue;
        }

        public bool GetBool(string name, bool defaultValue = false)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsBool(defaultValue);
            }

            return defaultValue;
        }

        public bool GetBool(int index, bool defaultValue = false)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsBool(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region FLOAT
        public float AsFloat(float defaultValue = 0)
        {
            float result = defaultValue;
            if (float.TryParse(this.Value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public float GetFloat(string name, float defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsFloat(defaultValue);
            }

            return defaultValue;
        }

        public float GetFloat(int index, float defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsFloat(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region DOUBLE
        public double AsDouble(double defaultValue = 0)
        {
            double result = defaultValue;
            if (double.TryParse(this.Value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public double GetDouble(string name, double defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsDouble(defaultValue);
            }

            return defaultValue;
        }

        public double GetDouble(int index, double defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsDouble(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region DECIMAL
        public Decimal AsDecimal(decimal defaultValue = 0)
        {
            decimal result = defaultValue;
            if (decimal.TryParse(this.Value, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public Decimal GetDecimal(string name, decimal defaultValue = 0)
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsDecimal(defaultValue);
            }

            return defaultValue;
        }

        public Decimal GetDecimal(int index, decimal defaultValue = 0)
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.AsDecimal(defaultValue);
            }

            return defaultValue;
        }
        #endregion

        #region STRING
        public string AsString(string defaultValue = "")
        {
            if (this.Value != null)
                return this.Value;

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

        public string GetString(int index, string defaultValue = "")
        {
            DataNode node = this.GetNodeByIndex(index);
            if (node != null)
            {
                return node.Value;
            }

            return defaultValue;
        }
        #endregion

        #region DATETIME
        public DateTime AsDateTime(DateTime defaultValue = default(DateTime))
        {
#if DATETIME_AS_TIMESTAMPS
            long ticks;
            if (long.TryParse(this.Value, out ticks))
            {
                return FromTimestamp(ticks);
            }
#endif
            DateTime result;
            if (DateTime.TryParse(this.Value, out result))
            {
                return result;
            }

            return defaultValue;
        }

        public DateTime GetDateTime(string name, DateTime defaultValue = default(DateTime))
        {
            DataNode node = this.GetNode(name);
            if (node != null)
            {
                return node.AsDateTime(defaultValue);
            }

            return defaultValue;
        }
        #endregion
    }
}
