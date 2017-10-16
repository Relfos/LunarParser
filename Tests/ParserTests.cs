using LunarParser.JSON;
using LunarParser.XML;
using NUnit.Framework;
using System;

namespace LunarParserTests
{
    [TestFixture]
    public class ParserTests
    {

        [Test]
        public void TestXMLReader()
        {
            var root = XMLReader.ReadFromString("<message><content>Hello world!</content></message>");
            Assert.NotNull(root);
            Assert.IsTrue("message".Equals(root.Name));

            var content = root.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue(content.Equals("Hello world!"));
        }

        [Test]
        public void TestJSONReader()
        {
            var root = JSONReader.ReadFromString("{\"message\": { \"content\": \"Hello world!\"} }");
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);

            // alternate way
            msg = root.GetNode("message");
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue(content.Equals("Hello world!"));
        }
    }
}
