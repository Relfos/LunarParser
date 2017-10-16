using System;
using System.Globalization;

namespace LunarParser.XML
{

    public class XMLReader
    {
        private const char TAG_START = '<';
        private const char TAG_END = '>';
        private const char QUOTE = '"';
        private const char SLASH = '/';
        private const char EQUALS = '=';
        private static string BEGIN_QUOTE = "" + EQUALS + QUOTE;

        public static DataNode ReadFromString(string xml)
        {
            int index = 0;
            int lastIndex = 0;
            int prevIndex = 0;
            DataNode rootNode = null;
            DataNode currentNode = null;

            bool openTag = false;

            xml = xml.Replace(" \n", "");
            xml = xml.Replace("\n", "");

            while (true)
            {
                index = xml.IndexOf(TAG_START, lastIndex);

                if (index < 0 || index >= xml.Length)
                    break;

                index++;

                lastIndex = xml.IndexOf(TAG_END, index);
                if (lastIndex < 0 || lastIndex >= xml.Length)
                    break;


                int tagLength = lastIndex - index;
                string xmlTag = xml.Substring(index, tagLength);

                if (xmlTag[0] == '?')
                {
                    continue;
                }

                if (xmlTag[0] == SLASH)
                {
                    currentNode.Value = xml.Substring(prevIndex + 1, (index - prevIndex) - 2);
                    currentNode = currentNode.Parent;
                    continue;
                }

                prevIndex = lastIndex;
                openTag = true;

                if (xmlTag[tagLength - 1] == SLASH)
                {
                    xmlTag = xmlTag.Substring(0, tagLength - 1);
                    openTag = false;
                }


                DataNode node = ParseTag(xmlTag);

                if (currentNode != null)
                {
                    currentNode.AddNode(node);
                }

                if (openTag || currentNode == null)
                {
                    currentNode = node;
                }


                if (rootNode == null)
                {
                    rootNode = node;
                }
            }

            return rootNode;
        }


        private static DataNode ParseTag(string xmlTag)
        {
            int nameEnd = xmlTag.IndexOf(' ', 0);
            if (nameEnd < 0)
            {
                return DataNode.CreateObject(xmlTag);
            }

            string tagName = xmlTag.Substring(0, nameEnd);
            var node = DataNode.CreateObject(tagName);

            string attrString = xmlTag.Substring(nameEnd, xmlTag.Length - nameEnd);
            ParseAttributes(attrString, node);

            return node;
        }

        private static void ParseAttributes(string xmlTag, DataNode node)
        {
            int index = 0;
            int attrNameIndex = 0;
            int lastIndex = 0;

            while (true)
            {
                index = xmlTag.IndexOf(BEGIN_QUOTE, lastIndex);
                if (index < 0 || index > xmlTag.Length)
                    break;

                attrNameIndex++;
                while (xmlTag[attrNameIndex] <= ' ')
                {
                    attrNameIndex++;
                }
                string attrName = xmlTag.Substring(attrNameIndex, index - attrNameIndex);

                index += 2;

                lastIndex = xmlTag.IndexOf(QUOTE, index);
                if (lastIndex < 0 || lastIndex > xmlTag.Length)
                {
                    break;
                }

                int tagLength = lastIndex - index;
                string attrValue = xmlTag.Substring(index, tagLength);

                node.AddField(attrName, attrValue);

                attrNameIndex = lastIndex;
                //Debug.Log("loaded node " + attrName);
            }

        }

    }

}