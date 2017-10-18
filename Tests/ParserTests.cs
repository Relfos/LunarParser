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
        public void TestXMLReaderFull()
        {
            var root = XMLReader.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?><!-- In this collection, we will keep each title \"as is\" --><videos><video><title>The Distinguished Gentleman</title><director>Jonathan Lynn</director><length>112 Minutes</length><format>DVD</format><rating>R</rating></video><video><title>Her Alibi</title><director>Bruce Beresford</director><length>94 Mins</length><format>DVD</format><rating>PG-13</rating> </video></videos>");
            Assert.NotNull(root);
            Assert.IsTrue("videos".Equals(root.Name));
            Assert.IsTrue(root.ChildCount.Equals(2));

            var content = root.GetNode("video");
            Assert.NotNull(content);
            Assert.IsTrue(content.ChildCount.Equals(5));
        }

        [Test]
        public void TestXMLComments()
        {
            var root = XMLReader.ReadFromString("<message><!--This is a comment, will be ignored--><content>Hello world!</content></message>");
            Assert.NotNull(root);
            Assert.IsTrue("message".Equals(root.Name));

            var content = root.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue(content.Equals("Hello world!"));
        }

        [Test]
        public void TestXMLProlog()
        {
            var root = XMLReader.ReadFromString("<!--This is a comment, will be ignored--><message><content>Hello world!</content></message>");
            Assert.NotNull(root);
            Assert.IsTrue("message".Equals(root.Name));
            Assert.IsTrue(root.ChildCount.Equals(1));

            var content = root.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue(content.Equals("Hello world!"));
        }

        [Test]
        public void TestXMLAttributes()
        {
            var root = XMLReader.ReadFromString("<message content=\"Hello world!\"/>");
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
