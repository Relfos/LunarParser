using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Collections;

namespace LunarLabs.Parser
{
    public static class DataNodeExtensions
    {
        /// <summary>
        /// Converts a dictionary to a DataSource
        /// </summary>
        public static DataNode FromDictionary(this IDictionary dic, string name)
        {
            if (dic == null)
            {
                return null;
            }

            var node = DataNode.CreateObject(name);
            foreach (var key in dic.Keys)
            {
                node.AddField(key.ToString().ToLower(), dic[key]);
            }
            return node;
        }

        /// <summary>
        /// Converts a DataSource to a Dictionary
        /// </summary>
        public static Dictionary<string, T> ToDictionary<T>(this DataNode node, string name)
        {
            if (node == null)
            {
                return null;
            }

            var result = new Dictionary<string, T>();

            foreach (var child in node.Children)
            {
                if (!string.IsNullOrEmpty(child.Value))
                {
                    result[child.Name] = child.AsObject<T>();
                }
            }
            return result;
        }

        public static DataNode ToDataNode<T>(this IEnumerable<T> obj, string name)
        {
            var result = DataNode.CreateArray(name);

            foreach (var item in obj)
            {
                var node = item.ToDataNode();
                result.AddNode(node);
            }

            return result;
        }

        /// <summary>
        /// Returns an array with all objects of type T in the children of the node with specified name
        /// </summary>
        public static T[] ToArray<T>(this DataNode node)
        {
            if (node == null)
            {
                return new T[]{ };
            }

            var name = typeof(T).Name.ToLower();

            int count = 0;
            foreach (var child in node.Children)
            {
                if (child.Name == null || child.Name.Equals(name))
                {
                    count++;
                }
            }

            var result = new T[count];
            int index = 0;

            foreach (var child in node.Children)
            {
                if (child.Name == null || child.Name.Equals(name))
                {
                    result[index] = child.ToObject<T>();
                    index++;
                }
            }

            return result;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort)
                || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong)
                || type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(bool)
                || type == typeof(string) || type == typeof(DateTime);
        }

        private static DataNode FromArray(object obj, string arrayName = null)
        {
            var result = DataNode.CreateArray(arrayName);

            var array = (Array)obj;
            var type = array.GetType();

            if (array != null && array.Length > 0)
            {
                var itemType = type.GetElementType();

                for (int i = 0; i < array.Length; i++)
                {
                    var item = array.GetValue(i);

                    if (itemType.IsPrimitive())
                    {
                        result.AddValue(item);
                    }
                    else
                    {
                        var itemNode = item.ToDataNode(null, true);
                        result.AddNode(itemNode);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts an object to a DataSource
        /// </summary>
        public static DataNode ToDataNode(this object obj, string name = null, bool isArrayElement = false)
        {
            if (obj == null)
            {
                return null;
            }

            Type type = obj.GetType();

            if (type.IsArray)
            {
                return FromArray(obj);
            }
            else
            if (IsPrimitive(type))
            {
                throw new Exception("Can't convert primitive type to DataNode");
            }

            TypeInfo info = null;
            var fields = Enumerable.Empty<FieldInfo>();

            Type currentClass = type;
            do
            {
                var currentInfo = currentClass.GetTypeInfo();
                if (currentClass == type)
                {
                    info = currentInfo;
                }

                var temp = currentInfo.DeclaredFields.Where(f => f.IsPublic);

                var fieldArray = temp.ToArray();

                fields = temp.Concat(fields);

                currentClass = currentInfo.BaseType;
                if (currentClass == typeof(object))
                {
                    break;
                }
            } while (true);
            

            if (name == null && !isArrayElement)
            {
                name = type.Name.ToLower();
            }

            var result = DataNode.CreateObject(name);

            foreach (var field in fields)
            {
                var val = field.GetValue(obj);

                var fieldName = field.Name.ToLower();
                var fieldTypeInfo = field.FieldType.GetTypeInfo();

                if (field.FieldType.IsPrimitive() || fieldTypeInfo.IsEnum)
                {
                    result.AddField(fieldName, val);
                }
                else
                if (fieldTypeInfo.IsArray)
                {
                    var arrayNode = FromArray(val, fieldName);
                    result.AddNode(arrayNode);
                }
                else
                if (val != null)
                {
                    var node = val.ToDataNode(fieldName);
                    result.AddNode(node);
                }               
                else
                {
                    result.AddField(fieldName, null);
                }
            }

            return result;
        }

        public static T ToObject<T>(this DataNode node)
        {
            if (node == null)
            {
                return default(T);
            }

            return (T)node.ToObject(typeof(T));
        }

        /// <summary>
        /// Converts a DataSource to an Object
        /// </summary>
        public static object ToObject(this DataNode node, Type objectType)
        {
            if (node == null)
            {
                return null;
            }

            var info = objectType.GetTypeInfo();
            var fields = info.DeclaredFields.Where(f => f.IsPublic);

            var result = Activator.CreateInstance(objectType);
            // box result otherwise structs values wont update
            object obj = result;

            foreach (var field in fields)
            {
                if (!node.HasNode(field.Name))
                {
                    continue;
                }

                var fieldType = field.FieldType;

                if (fieldType.IsPrimitive())
                {
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
                        throw new Exception("Cannot unserialize field of type " + objectType.Name);
                    }
                    #endregion
                }
                else
                {
                    var valNode = node.GetNodeByName(field.Name);
                    object val = valNode.ToObject(fieldType);
                    field.SetValue(obj, val);
                }
            }

            return Convert.ChangeType(obj, objectType);
        }

        public static DataNode FromHashSet<T>(this HashSet<T> set, string name)
        {
            var result = DataNode.CreateArray(name);

            foreach (var item in set)
            {
                result.AddValue(item);
            }

            return result;
        }

        public static HashSet<T> ToHashSet<T>(this DataNode node, string name = null)
        {
            var set = new HashSet<T>();

            foreach (var entry in node.Children)
            {
                bool valid;

                if (string.IsNullOrEmpty(name))
                {
                    valid = true;
                }
                else
                {
                    valid = entry.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
                }

                if (valid)
                {
                    var item = entry.AsObject<T>();
                    set.Add(item);
                }
            }

            return set;
        }

    }

}
