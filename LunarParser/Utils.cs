using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

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

        /// <summary>
        /// Converts an object to a DataSource
        /// </summary>
        public static DataNode ToDataSource(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = obj.GetType();

            var info = type.GetTypeInfo();
            var fields = info.DeclaredFields.Where(f => f.IsPublic);

            var name = type.Name.ToLower();
            var result = DataNode.CreateObject(name);

            foreach (var field in fields)
            {
                var val = field.GetValue(obj);
                if (val != null)
                {
                    var node = result.AddField(field.Name.ToLower(), val);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a DataSource to an Object
        /// </summary>
        public static T ToObject<T>(this DataNode node)
        {
            if (node == null)
            {
                return default(T);
            }

            Type type = typeof(T);

            var info = type.GetTypeInfo();
            var fields = info.DeclaredFields.Where(f => f.IsPublic);

            var result = Activator.CreateInstance<T>();
            // box result otherwise structs values wont update
            object obj = result;

            foreach (var field in fields)
            {
                if (!node.HasNode(field.Name))
                {
                    continue;
                }

                var fieldType = field.FieldType;
                var str = node.GetString(field.Name);

                #region TYPES LIST
                if (fieldType == typeof(string))
                {
                    field.SetValue(obj, str);
                }
                else
                if (fieldType == typeof(byte))
                {
                    byte val;
                    byte.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(sbyte))
                {
                    sbyte val;
                    sbyte.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(short))
                {
                    short val;
                    short.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(ushort))
                {
                    ushort val;
                    ushort.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(int))
                {
                    int val;
                    int.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(uint))
                {
                    uint val;
                    uint.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(long))
                {
                    long val;
                    long.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(ulong))
                {
                    ulong val;
                    ulong.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(float))
                {
                    float val;
                    float.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(double))
                {
                    double val;
                    double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(decimal))
                {
                    decimal val;
                    decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat, out val);
                    field.SetValue(obj, val);
                }
                else
                if (fieldType == typeof(bool))
                {
                    bool val;
                    bool.TryParse(str, out val);
                    field.SetValue(obj, val);
                }
                else
                {
                    throw new Exception("Cannot unserialize field of type " + type.Name);
                }
                #endregion
            }

            return (T)obj;
        }

    }

}
