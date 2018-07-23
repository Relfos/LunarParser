using System.IO;

using LunarLabs.Parser.XML;
using LunarLabs.Parser.JSON;
using LunarLabs.Parser.YAML;
using LunarLabs.Parser.Binary;
using LunarLabs.Parser.CSV;
using System;

namespace LunarLabs.Parser
{
    public enum DataFormat
    {
        Unknown,
        BIN,
        XML,
        JSON,
        YAML,
        CSV,
    }

    public static class DataFormats
    {
        public static DataFormat GetFormatForExtension(string extension)
        {
            switch (extension)
            {
                case ".xml": return DataFormat.XML;
                case ".json": return DataFormat.JSON;
                case ".yaml": return DataFormat.YAML;
                case ".csv": return DataFormat.CSV;
                case ".bin": return DataFormat.BIN;

                default:
                    {
                        return DataFormat.Unknown;
                    }
            }
        }

        public static DataFormat DetectFormat(string content)
        {
            int i = 0;
            while (i<content.Length)
            {
                var c = content[i];
                i++;

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                switch (c)
                {
                    case '-': return DataFormat.YAML;
                    case '<': return DataFormat.XML;
                    case '{':
                    case '[': return DataFormat.JSON;
                    default: return DataFormat.Unknown;
                }
            }

            return DataFormat.Unknown;
        }

        public static DataNode LoadFromString(DataFormat format, string contents)
        {
            switch (format)
            {
                case DataFormat.XML: return XMLReader.ReadFromString(contents);
                case DataFormat.JSON: return JSONReader.ReadFromString(contents);
                case DataFormat.YAML: return YAMLReader.ReadFromString(contents);
                case DataFormat.CSV: return CSVReader.ReadFromString(contents);
                default:
                    {
                        throw new System.Exception("Format not supported");
                    }
            }
        }

        public static string SaveToString(DataFormat format, DataNode root)
        {
            switch (format)
            {
                case DataFormat.XML: return XMLWriter.WriteToString(root);
                case DataFormat.JSON: return JSONWriter.WriteToString(root);
                case DataFormat.YAML: return YAMLWriter.WriteToString(root);
                case DataFormat.CSV: return CSVWriter.WriteToString(root);
                default:
                    {
                        throw new System.Exception("Format not supported");
                    }
            }
        }

        public static DataNode LoadFromString(string content)
        {
            var format = DetectFormat(content);
            return LoadFromString(format, content);
        }

        /// <summary>
        /// Loads a node tree from a file, type is based on filename extension
        /// </summary>
        public static DataNode LoadFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException();
            }

            var extension = Path.GetExtension(fileName).ToLower();

            if (extension.Equals(".bin"))
            {
                var bytes = File.ReadAllBytes(fileName);
                return BINReader.ReadFromBytes(bytes);
            }

            var contents = File.ReadAllText(fileName);

            var format = GetFormatForExtension(extension);

            if (format == DataFormat.Unknown)
            {
                format = DetectFormat(contents);

                if (format == DataFormat.Unknown)
                {
                    throw new Exception("Could not detect format for " + fileName);
                }
            }

            return LoadFromString(format, contents);
        }

        public static void SaveToFile(string fileName, DataNode root)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            var format = GetFormatForExtension(extension);

            var content = SaveToString(format, root);
            File.WriteAllText(fileName, content);
        }

    }
}
