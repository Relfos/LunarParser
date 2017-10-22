using LunarParser;
using LunarParser.JSON;
using LunarParser.XML;
using LunarParser.YAML;
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

        [Test]
        public void TestYAMLReader()
        {
            var root = YAMLReader.ReadFromString("---\nmessage:\n  content: Hello world!");
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue(content.Equals("Hello world!"));
        }

        [Test]
        public void TestDateTime()
        {
            var date = DateTime.Now;

            var root = DataNode.CreateObject("test");
            Assert.NotNull(root);

            root.AddField("date", date);

            Assert.IsTrue(root.ChildCount == 1);

            var xml = XMLWriter.WriteToString(root);
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            Assert.NotNull(root);
            Assert.IsTrue("test".Equals(root.Name));

            var otherDate = root.GetDateTime("date");
            Assert.IsTrue(otherDate.Equals(date));           
        }

        private struct Color
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public Color(byte R, byte G, byte B, byte A = 255)
            {
                this.R = R;
                this.G = G;
                this.B = B;
                this.A = A;
            }
        }

        [Test]
        public void TestStructs()
        {
            var color = new Color(128, 200, 64, 255);

            var root = DataNode.CreateObject("test");
            Assert.NotNull(root);

            var obj = color.ToDataSource();
            Assert.IsTrue(obj.ChildCount == 4);

            root.AddNode(obj);

            Assert.IsTrue(root.ChildCount == 1);

            var xml = XMLWriter.WriteToString(root);
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            Assert.NotNull(root);
            Assert.IsTrue("test".Equals(root.Name));

            var content = root.GetNode("color");
            Assert.NotNull(content);
            Assert.IsTrue(content.ChildCount == 4);

            var otherColor = content.ToObject<Color>();

            Assert.IsTrue(otherColor.Equals(color));
        }

        private struct ColorGroup
        {
            public Color foreground;
            public Color background;

            public ColorGroup(Color foreground, Color background)
            {
                this.foreground = foreground;
                this.background = background;
            }
        }


        [Test]
        public void TestNestedStructs()
        {
            var color1 = new Color(128, 200, 64, 255);
            var color2 = new Color(230, 130, 60, 100);
            var cgroup = new ColorGroup(color1, color2);

            var root = DataNode.CreateObject("test");
            Assert.NotNull(root);

            var obj = cgroup.ToDataSource();
            Assert.IsTrue(obj.ChildCount == 2);

            root.AddNode(obj);

            Assert.IsTrue(root.ChildCount == 1);

            var xml = XMLWriter.WriteToString(root);
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            Assert.NotNull(root);
            Assert.IsTrue("test".Equals(root.Name));

            var content = root.GetNode("colorgroup");
            Assert.NotNull(content);
            Assert.IsTrue(content.ChildCount == 2);

            var otherGroup = content.ToObject<ColorGroup>();

            Assert.IsTrue(otherGroup.foreground.Equals(cgroup.foreground));
            Assert.IsTrue(otherGroup.background.Equals(cgroup.background));
        }

    }
}
