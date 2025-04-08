using LunarLabs.Parser;
using LunarLabs.Parser.CSV;
using LunarLabs.Parser.JSON;
using LunarLabs.Parser.XML;
using LunarLabs.Parser.YAML;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LunarParserTests
{
    [TestFixture]
    public class ParserTests
    {
        [Test]
        public void TestAddAndRemove()
        {
            var root = DataNode.CreateArray(null);
            root.AddField("hello", "dog");
            ClassicAssert.NotNull(root.ChildCount == 1);

            root.SetField("hello", "cat");
            ClassicAssert.NotNull(root.ChildCount == 1);

            root.RemoveNodeByName("hello");
            ClassicAssert.NotNull(root.ChildCount == 0);
        }

        [Test]
        public void TestLongPath()
        {
            var root = DataNode.CreateObject("test");

            root["test"]["user"]["name"].SetValue("mr.doggo");
            root["test"]["user"]["age"].SetValue(69);

            ClassicAssert.NotNull(root.ChildCount == 1);

            var node = root["test"]["user"];
            ClassicAssert.NotNull(node.ChildCount == 2);

            var name = node.GetString("name");
            ClassicAssert.IsTrue(name == "mr.doggo");

            var age = node.GetInt32("age");
            ClassicAssert.IsTrue(age == 69);
        }


        #region XML
        [Test]
        public void TestXMLReader()
        {
            var xml = "<message><content>Hello world!</content></message>";

            var root = XMLReader.ReadFromString(xml);
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLEscaping()
        {
            var xml = "<message>Hello&amp;world!</message>";

            var root = XMLReader.ReadFromString(xml);
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.Value;
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            var expected = "Hello&world!";
            ClassicAssert.IsTrue(content.Equals(expected));

            root = DataNode.CreateObject("message");
            ClassicAssert.NotNull(root);
            root.Value = expected;

            var xml2 = XMLWriter.WriteToString(root, escape: true).Trim();
            ClassicAssert.IsTrue(xml2.Equals(xml));
        }

        [Test]
        public void TestXMLWriter()
        {
            var root = DataNode.CreateObject("data");
            ClassicAssert.NotNull(root);

            var temp = DataNode.CreateObject("entry");
            temp.AddField("name", "xx");
            root.AddNode(temp);

            var xml = XMLWriter.WriteToString(root);
            var expected = "<data>\n\t<entry name=\"xx\" />\n</data>\n";
            expected = expected.Replace("\n", Environment.NewLine);
            ClassicAssert.IsTrue(xml == expected);

            xml = XMLWriter.WriteToString(root, true);
            expected = "<data>\n\t<entry>\n\t\t<name>xx</name>\n\t</entry>\n</data>\n";
            expected = expected.Replace("\n", Environment.NewLine);
            ClassicAssert.IsTrue(xml == expected);
        }

        [Test]
        public void TestXMLReaderFull()
        {
            var root = XMLReader.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?><!-- In this collection, we will keep each title \"as is\" --><videos><video><title>The Distinguished Gentleman</title><director>Jonathan Lynn</director><length>112 Minutes</length><format>DVD</format><rating>R</rating></video><video><title>Her Alibi</title><director>Bruce Beresford</director><length>94 Mins</length><format>DVD</format><rating>PG-13</rating> </video></videos>");
            ClassicAssert.NotNull(root);

            var videos = root["videos"];
            ClassicAssert.NotNull(videos);

            ClassicAssert.IsTrue("videos".Equals(videos.Name));
            ClassicAssert.IsTrue(videos.ChildCount.Equals(2));

            var content = videos.GetNodeByName("video");
            ClassicAssert.NotNull(content);
            ClassicAssert.IsTrue(content.ChildCount.Equals(5));
        }

        [Test]
        public void TestXMLComments()
        {
            var root = XMLReader.ReadFromString("<message><!--This is a comment, will be ignored--><content>Hello world!</content></message>");
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);
            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));
            ClassicAssert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLEmpty()
        {
            var root = XMLReader.ReadFromString("");
            ClassicAssert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("    ");
            ClassicAssert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("<!---->");
            ClassicAssert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("<!-- nsbdghfds <msg>hello</msg> fdgf -->");
            ClassicAssert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("<!-- <aa /> -->");
            ClassicAssert.True(root.ChildCount.Equals(0));
        }

        [Test]
        public void TestXMLRoot()
        {
            var root = XMLReader.ReadFromString("<message></message>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);
            ClassicAssert.IsEmpty(msg.Value);

            root = XMLReader.ReadFromString("<message>aaa</message>");
            ClassicAssert.NotNull(root);
            msg = root["message"];
            ClassicAssert.NotNull(msg);
            ClassicAssert.AreEqual("aaa", msg.Value);

            root = XMLReader.ReadFromString("<message><!--aa--></message>");
            ClassicAssert.NotNull(root);
            msg = root["message"];
            ClassicAssert.NotNull(msg);
            ClassicAssert.IsEmpty(msg.Value);
        }

        // Valid in XML
        [Test]
        public void TestXMLCommentsTags()
        {
            var root = XMLReader.ReadFromString("<message><!-- will - - <- be ignored-->" +
                                                "<!--df <! - - </ m\"es\"sage > dd=\"aa\" -->" +
                                                "<content>Hello world!</content>" +
                                                "<!-- df <!- - </message> --> </message>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));
            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));
            ClassicAssert.AreEqual("Hello world!", content);
        }

        // Not strictly valid in XML, but accepted in HTML and others
        [Test]
        public void TestXMLCommentsUnbalanced()
        {
            var root = XMLReader.ReadFromString("<message><!-- will <-- be ignored-->" +
                                                "<content> <!--df \" \" <!-- </ message > --> " +
                                                "Hello world!</content></message>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);
            ClassicAssert.AreEqual("message", msg.Name);
            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));
            ClassicAssert.AreEqual("Hello world!", content.Trim());
        }

        [Test]
        public void TestXMLProlog()
        {
            var root = XMLReader.ReadFromString("<!--This is a comment, will be ignored--><message>" +
                                                "<content>Hello world!</content></message>");
            ClassicAssert.NotNull(root);
            ClassicAssert.IsTrue(root.ChildCount.Equals(1));

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLAttributes()
        {
            var root = XMLReader.ReadFromString("<message content=\"Hello world!\"/>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLAttributesIgnored()
        {
            var root = XMLReader.ReadFromString("<message content=\"Hello /> world!\"/>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue("Hello /> world!".Equals(content));
        }

        [Test]
        public void TestXMLText()
        {
            var root = XMLReader.ReadFromString("<message attribute=\"something\">other</message>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var attr = msg.GetString("attribute");
            ClassicAssert.IsTrue("something".Equals(attr));

            ClassicAssert.IsTrue("other".Equals(msg.Value));
        }

        [Test]
        public void TestXMLShortTag()
        {
            var root = XMLReader.ReadFromString("<message attribute=\"something\"><go /></message>");
            ClassicAssert.NotNull(root);
            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var attr = msg.GetString("attribute");
            ClassicAssert.IsTrue("something".Equals(attr));

            ClassicAssert.IsTrue(msg.ChildCount == 2);

            var child = msg.GetNodeByIndex(1);
            ClassicAssert.IsNotNull(child);
            ClassicAssert.IsTrue("go".Equals(child.Name));
            ClassicAssert.IsTrue(string.IsNullOrEmpty(child.Value));
        }

        [Test]
        public void TestXMLCData()
        {
            string test = String.Format(@"<message> <content><![CDATA[test<>me]]></content></message>");
            var root = XMLReader.ReadFromString(test);
            var msg = root["message"];
            var content = msg.GetString("content");
            ClassicAssert.IsTrue(content.Equals("test<>me"));

            test = String.Format(@"<message> <content><![CDATA[test<>me]<[]]></content></message>");
            root = XMLReader.ReadFromString(test);
            msg = root["message"];
            content = msg.GetString("content");
            ClassicAssert.IsTrue(content.Equals("test<>me]<["));

            test = String.Format(@"<message><content>![CDATA[testme]]</content></message>");
            root = XMLReader.ReadFromString(test);
            msg = root["message"];
            content = msg.GetString("content");
            ClassicAssert.IsTrue(content.Equals("![CDATA[testme]]"));

            test = String.Format("<message><content><![CDATA[line1.test<>me\nline2.hello\nthirdline]]></content></message>");

            root = XMLReader.ReadFromString(test);
            msg = root["message"];
            content = msg.GetString("content");
            ClassicAssert.IsTrue(content.Equals("line1.test<>me\nline2.hello\nthirdline"));
        }
        #endregion

        #region JSON
        [Test]
        public void TestJSONReader()
        {
            var hello = "The {{Strange}} [[Message]]!";
            var json = "{\"message\": { \"content\": \""+hello+"\"} }";

            var root = JSONReader.ReadFromString(json);
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            // alternate way
            msg = root.GetNodeByName("message");
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue(hello.Equals(content));
        }

        [Test]
        public void TestJSONReaderEscapedSymbols()
        {
            var escapedString = "Symbols: \\b \\f \\n \\r \\t \\\\ \\\"";
            var unescapedString = "Symbols: \b \f \n \r \t \\ \"";
            var json = "{\"message\": { \"content\": \"" + escapedString + "\"} }";

            var root = JSONReader.ReadFromString(json);
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue(unescapedString.Equals(content));
        }


        [Test]
        public void TestJSONSimpleValue()
        {
            var root = DataNode.CreateString("test");
            root.Value = "hello";

            var json = JSONWriter.WriteToString(root);

            var result = JSONReader.ReadFromString(json);

            ClassicAssert.IsTrue(result.ChildCount == 1);
            result = result.GetNodeByIndex(0);

            ClassicAssert.IsTrue(result.Name == root.Name);
            ClassicAssert.IsTrue(result.Value == root.Value);
        }

        [Test]
        public void TestJSONTypes()
        {
            var root = JSONReader.ReadFromString("{\"message\": { \"number\": 3.14159, \"negative\": -52, \"check\":true, \"item\": null, \"science\":-1.0e-5, \"science_alt\":-2.0e+5} }");
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            // alternate way
            msg = root.GetNodeByName("message");
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var number = msg.GetFloat("number");
            ClassicAssert.IsTrue(Math.Abs(number - 3.14159) < 0.001f);
            ClassicAssert.IsTrue(msg.GetNodeByName("number").Kind == NodeKind.Numeric);

            var negative = msg.GetInt32("negative");
            ClassicAssert.IsTrue(negative == -52);
            ClassicAssert.IsTrue(msg.GetNodeByName("negative").Kind == NodeKind.Numeric);

            var check = msg.GetBool("check");
            ClassicAssert.IsTrue(check);
            ClassicAssert.IsTrue(msg.GetNodeByName("check").Kind == NodeKind.Boolean);

            var item = msg.GetNodeByName("item");
            ClassicAssert.IsNotNull(item);
            ClassicAssert.IsTrue(msg.GetNodeByName("item").Kind == NodeKind.Null);
            ClassicAssert.IsTrue(string.IsNullOrEmpty(item.Value));

            var number2 = msg.GetFloat("science");
            ClassicAssert.IsTrue(Math.Abs(number2 - (-1.0e-5)) < 0.001f);
            ClassicAssert.IsTrue(msg.GetNodeByName("science").Kind == NodeKind.Numeric);
        }

        [Test]
        public void TestJSONArray()
        {
            var root = JSONReader.ReadFromString("{\"message\": { \"content\": [0, 1, 2, 3]} }");
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            // alternate way
            msg = root.GetNodeByName("message");
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg["content"];
            ClassicAssert.IsTrue(content.ChildCount == 4);

            for (int i = 0; i < 4; i++)
            {
                var number = content.GetNodeByIndex(i);
                ClassicAssert.IsTrue(i.ToString().Equals(number.Value));
            }
        }

        [Test]
        public void TestJSONArrayWriter()
        {
            var root = DataNode.CreateArray(null);
            root.AddField(null, "hello");
            root.AddField(null, "1");
            root.AddField(null, "2");

            var json = JSONWriter.WriteToString(root);
            ClassicAssert.NotNull(json);

            var other = JSONReader.ReadFromString(json);
            ClassicAssert.NotNull(other);

            ClassicAssert.IsTrue(other.ChildCount == root.ChildCount);

            for (int i = 0; i < root.ChildCount; i++)
            {
                var child = root.GetNodeByIndex(i);
                var otherChild = other.GetNodeByIndex(i);

                ClassicAssert.NotNull(child);
                ClassicAssert.NotNull(otherChild);

                ClassicAssert.IsTrue(child.Name == otherChild.Name);
                ClassicAssert.IsTrue(child.Value == otherChild.Value);
            }
        }

        [Test]
        public void TestJSONObjectWriter()
        {
            var color = new Color(200, 100, 220, 128);
            var root = DataNode.CreateObject(null);
            root.AddNode(color.ToDataNode());

            var json = JSONWriter.WriteToString(root);
            ClassicAssert.NotNull(json);

            var other = JSONReader.ReadFromString(json);
            ClassicAssert.NotNull(other);

            ClassicAssert.IsTrue(other.ChildCount == root.ChildCount);

            for (int i = 0; i < root.ChildCount; i++)
            {
                var child = root.GetNodeByIndex(i);
                var otherChild = other.GetNodeByIndex(i);

                ClassicAssert.NotNull(child);
                ClassicAssert.NotNull(otherChild);

                ClassicAssert.IsTrue(child.Name == otherChild.Name);
                ClassicAssert.IsTrue(child.Value == otherChild.Value);
            }
        }

        [Test]
        public void TestJSONTyping()
        {
            var root = DataNode.CreateObject(null);
            var val = "0000";
            root.AddField("msg", val);

            var json = JSONWriter.WriteToString(root);
            ClassicAssert.NotNull(json);

            var other = JSONReader.ReadFromString(json);
            ClassicAssert.NotNull(other);

            ClassicAssert.IsTrue(other.ChildCount == root.ChildCount);

            var otherVal = other.GetString("msg");
            ClassicAssert.IsTrue(otherVal.Equals(val));
        }
        #endregion

        #region YAML
        [Test]
        public void TestYAMLReader()
        {
            var root = YAMLReader.ReadFromString("---\nmessage:\n  content: Hello world!");
            ClassicAssert.NotNull(root);

            var msg = root["message"];
            ClassicAssert.NotNull(msg);

            ClassicAssert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            ClassicAssert.IsFalse(string.IsNullOrEmpty(content));

            ClassicAssert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestYAMLIdentationBlock()
        {
            var root = YAMLReader.ReadFromString("layout: list\r\ntitle: iDEX Activities\r\nslug: activities\r\ndescription: >\r\n  This page is for blogging activities of iDEX.");
            ClassicAssert.NotNull(root);
        }
        #endregion

        #region CSV
        private struct Animal
        {
            public int id;
            public string name;
        }

        [Test]
        public void TestCSVReader()
        {
            var csv = "id,name\n1,Dog\n2,\"The \"\"Mr\"\"Cat\"\n399,\"Fish,Blue\"\n412,\"Heavy Bird\"";
            var root = CSVReader.ReadFromString(csv);
            ClassicAssert.NotNull(root);

            ClassicAssert.IsTrue(root.ChildCount == 4);

            var animals = root.ToArray<Animal>();
            ClassicAssert.IsTrue(animals.Length == 4);

            Animal animal;

            animal = animals[0];
            ClassicAssert.IsTrue(1.Equals(animal.id));
            ClassicAssert.IsTrue("Dog".Equals(animal.name));

            animal = animals[1];
            ClassicAssert.IsTrue(2.Equals(animal.id));
            ClassicAssert.IsTrue("The \"Mr\"Cat".Equals(animal.name));

            animal = animals[2];
            ClassicAssert.IsTrue(399.Equals(animal.id));
            ClassicAssert.IsTrue("Fish,Blue".Equals(animal.name));

            animal = animals[3];
            ClassicAssert.IsTrue(412.Equals(animal.id));
            ClassicAssert.IsTrue("Heavy Bird".Equals(animal.name));
        }
        #endregion

        #region DataNode
        [Test]
        public void TestDateTime()
        {
            var date = new DateTime(2017, 11, 29, 10, 30, 0);

            var root = DataNode.CreateObject("test");
            ClassicAssert.NotNull(root);

            root.AddField("first", date);
            root.AddField("second", date.ToString());
            root.AddField("third", "2017-11-29T10:30:00.000Z");

            ClassicAssert.IsTrue(root.ChildCount == 3);

            var xml = XMLWriter.WriteToString(root);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            ClassicAssert.NotNull(root);

            var test = root.GetNodeByName("test");
            ClassicAssert.IsTrue("test".Equals(test.Name));

            var first = test.GetDateTime("first");
            ClassicAssert.IsTrue(first.Equals(date));

            var second = test.GetDateTime("second");
            ClassicAssert.IsTrue(second.Equals(date));

            var third = test.GetDateTime("third");
            ClassicAssert.IsTrue(third.Equals(date));
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
            ClassicAssert.NotNull(root);

            var obj = color.ToDataNode();
            ClassicAssert.IsTrue(obj.ChildCount == 4);

            root.AddNode(obj);

            ClassicAssert.IsTrue(root.ChildCount == 1);

            var xml = XMLWriter.WriteToString(root);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            ClassicAssert.NotNull(root);

            var test = root.GetNodeByName("test");
            ClassicAssert.IsTrue("test".Equals(test.Name));

            var content = test.GetNodeByName("color");
            ClassicAssert.NotNull(content);
            ClassicAssert.IsTrue(content.ChildCount == 4);

            var otherColor = content.ToObject<Color>();

            ClassicAssert.IsTrue(otherColor.Equals(color));
        }

        [Test]
        public void TestStructArrays()
        {
            var red = new Color(255, 0, 0, 255);
            var green = new Color(0, 255, 0, 255);
            var blue = new Color(0, 0, 255, 255);
            var white = new Color(255, 255, 255, 255);
            var grey = new Color(128, 128, 128, 255);

            var root = DataNode.CreateObject("test");
            ClassicAssert.NotNull(root);

            var colors = new Color[] { red, green, blue, white, grey };
            var temp = colors.ToDataNode("colors");
            ClassicAssert.NotNull(temp);
            ClassicAssert.IsTrue(temp.ChildCount == 5);

            root.AddNode(temp);
            var xml = XMLWriter.WriteToString(root);

            root = XMLReader.ReadFromString(xml);

            var test = root["test"];
            temp = test["colors"];

            colors = temp.ToArray<Color>();
            ClassicAssert.IsTrue(colors.Length == 5);

            ClassicAssert.IsTrue(colors[0].Equals(red));
            ClassicAssert.IsTrue(colors[1].Equals(green));
            ClassicAssert.IsTrue(colors[2].Equals(blue));
            ClassicAssert.IsTrue(colors[3].Equals(white));
            ClassicAssert.IsTrue(colors[4].Equals(grey));
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
            ClassicAssert.NotNull(root);

            var obj = cgroup.ToDataNode();
            ClassicAssert.IsTrue(obj.ChildCount == 2);

            root.AddNode(obj);

            ClassicAssert.IsTrue(root.ChildCount == 1);

            var xml = XMLWriter.WriteToString(root);
            ClassicAssert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            ClassicAssert.NotNull(root);

            var test = root.GetNodeByName("test");
            ClassicAssert.IsTrue("test".Equals(test.Name));

            var content = test.GetNodeByName("colorgroup");
            ClassicAssert.NotNull(content);
            ClassicAssert.IsTrue(content.ChildCount == 2);

            var otherGroup = content.ToObject<ColorGroup>();

            ClassicAssert.IsTrue(otherGroup.foreground.Equals(cgroup.foreground));
            ClassicAssert.IsTrue(otherGroup.background.Equals(cgroup.background));
        }

        [Test]
        public void TestFindNodes()
        {
            var root = JSONReader.ReadFromString("{\"root\": { \"number\": 3.14159, \"check\":true, \"item\": {\"base\": \"found me\"} } }");
            ClassicAssert.NotNull(root);
            var msg = root["root"];
            ClassicAssert.NotNull(msg);

            // alternate way
            var child = root.FindNode("base");
            ClassicAssert.NotNull(child);
            ClassicAssert.AreEqual("base", child.Name);
            ClassicAssert.AreEqual("found me", child.Value);
        }

        [Test]
        public void TestAutoDetection()
        {
            var xml = "<message><content>Hello world!</content></message>";
            var json = "{\"message\": { \"content\": \"Hello world!\"} }";
            var yaml = "---\nmessage:\n  content: Hello world!";

            DataFormat format;

            format = DataFormats.DetectFormat(xml);
            ClassicAssert.IsTrue(format.Equals(DataFormat.XML));

            format = DataFormats.DetectFormat(json);
            ClassicAssert.IsTrue(format.Equals(DataFormat.JSON));

            format = DataFormats.DetectFormat(yaml);
            ClassicAssert.IsTrue(format.Equals(DataFormat.YAML));
        }

        public enum AnswerKind
        {
            Yes,
            No,
            Maybe
        }

        [Test]
        public void TestEnumParsing()
        {
            var root = DataNode.CreateObject();
            root.AddField("Answer", "Maybe");
            root.AddField("Other", "1");

            var answer = root.GetEnum<AnswerKind>("Answer");
            ClassicAssert.IsTrue(answer == AnswerKind.Maybe);

            var other = root.GetEnum<AnswerKind>("Other");
            ClassicAssert.IsTrue(other == AnswerKind.No);
        }

        [Test]
        public void TestAcessors()
        {
            var dogName = "barry";
            var catName = "bopi";

            var root = DataNode.CreateObject();
            root.AddField("dog", dogName);
            root.AddField("cat", catName);

            string s;

            s = root.GetString("dog");
            ClassicAssert.IsTrue(s == dogName);

            s = root["dog"].Value;
            ClassicAssert.IsTrue(s == dogName);

            s = root[0].Value;
            ClassicAssert.IsTrue(s == dogName);

            s = root[0].AsString();
            ClassicAssert.IsTrue(s == dogName);

            s = root.GetString("cat");
            ClassicAssert.IsTrue(s == catName);

            s = root["cat"].Value;
            ClassicAssert.IsTrue(s == catName);

            s = root[1].Value;
            ClassicAssert.IsTrue(s == catName);

            s = root[1].AsString();
            ClassicAssert.IsTrue(s == catName);
        }

        [Test]
        public void TestTypes()
        {
            var temp = DataNode.CreateObject("root");
            temp.AddField("a", 123);
            temp.AddField("f", 123.456);
            temp.AddField("d", "7.65e-6");
            temp.AddField("b", true);

            var json = JSONWriter.WriteToString(temp);
            var root = JSONReader.ReadFromString(json);
            root = root["root"];
            ClassicAssert.IsNotNull(root);

            ClassicAssert.IsTrue(root["a"].AsInt64() == 123);
            ClassicAssert.IsTrue(root.GetInt64("a") == 123);
            
            ClassicAssert.IsTrue(root["a"].AsUInt32() == 123);
            ClassicAssert.IsTrue(root.GetUInt32("a") == 123);

            ClassicAssert.IsTrue(root["a"].AsInt32() == 123);
            ClassicAssert.IsTrue(root.GetInt32("a") == 123);

            ClassicAssert.IsTrue(root["a"].AsByte() == 123);
            ClassicAssert.IsTrue(root.GetByte("a") == 123);

            ClassicAssert.IsTrue(root["a"].AsSByte() == 123);
            ClassicAssert.IsTrue(root.GetSByte("a") == 123);

            ClassicAssert.IsTrue(root["f"].AsFloat() == 123.456f);
            ClassicAssert.IsTrue(root.GetFloat("f") == 123.456f);

            ClassicAssert.IsTrue(root["d"].AsDecimal() == 0.00000765m);
            ClassicAssert.IsTrue(root.GetDecimal("d") == 0.00000765m);

            ClassicAssert.IsTrue(root["f"].AsDouble() == 123.456);
            ClassicAssert.IsTrue(root.GetDouble("f") == 123.456);

            ClassicAssert.IsTrue(root["b"].AsBool() );
            ClassicAssert.IsTrue(root.GetBool("b"));

            ClassicAssert.IsTrue(root["f"].AsString() == "123.456");
            ClassicAssert.IsTrue(root.GetString("f") == "123.456");
        }

        [Test]
        public void TestDefaults()
        {
            var root = DataNode.CreateObject();
            root.AddField("something", "5");
            root.AddField("other", "1");
            root.AddField("maybe", "yes");

            string s;

            s = root.GetString("maybe", "no");
            ClassicAssert.IsTrue(s == "yes");

            s = root.GetString("never", "no");
            ClassicAssert.IsTrue(s == "no");

            bool b;

            b = root.GetBool("other");
            ClassicAssert.IsTrue(b);

            b = root.GetBool("missing");
            ClassicAssert.IsFalse(b);

            b = root.GetBool("missing", true);
            ClassicAssert.IsTrue(b);

            b = root.GetBool("something");
            ClassicAssert.IsFalse(b);
        }

        [Test]
        public void TestArray()
        {
            var root = DataNode.CreateArray();
            root.AddNode(DataNode.CreateValue("first"));
            root.AddValue("second");
            root.AddField(null, "third");

            ClassicAssert.IsTrue(root.ChildCount == 3);

            string s;

            s = root.GetNodeByIndex(0).AsString();
            ClassicAssert.IsTrue(s.Equals("first"));

            s = root.GetNodeByIndex(1).AsString();
            ClassicAssert.IsTrue(s.Equals("second"));

            s = root.GetNodeByIndex(2).AsString();
            ClassicAssert.IsTrue(s.Equals("third"));
        }

        [Test]
        public void TestAsObject()
        {
            var root = DataNode.CreateObject("temp");
            root.AddField("hello", "world");
            root.AddField("number", "4");
            root.AddField("bool", true);

            ClassicAssert.IsTrue(root.ChildCount == 3);

            var s = root.GetObject<string>("hello", "");
            ClassicAssert.IsTrue(s.Equals("world"));

            var n = root.GetObject<int>("number", 0);
            ClassicAssert.IsTrue(n == 4);

            var b = root.GetObject<bool>("bool", false);
            ClassicAssert.IsTrue(b == true);
        }

        [Test]
        public void TestNodeToDictionary()
        {
            var dic = new Dictionary<string, int>();

            dic.Add("one", 1);
            dic.Add("two", 2);
            dic.Add("three", 3);

            var root = dic.FromDictionary("temp");

            ClassicAssert.IsTrue(root.ChildCount == dic.Count);

            foreach (var entry in dic)
            {
                var val = root.GetInt32(entry.Key);

                ClassicAssert.IsTrue(val == entry.Value);
            }

            var other = root.ToDictionary<int>("temp");
            ClassicAssert.IsTrue(other.Count == dic.Count);

            foreach (var entry in dic)
            {
                var val = other[entry.Key];

                ClassicAssert.IsTrue(val == entry.Value);
            }
        }

        [Test]
        public void TestNodeToHashSet()
        {
            var set = new HashSet<string>();

            set.Add("one");
            set.Add("two");
            set.Add("three");

            var root = set.FromHashSet<string>("temp");

            ClassicAssert.IsTrue(root.ChildCount == set.Count);

            foreach (var entry in set)
            {
                ClassicAssert.IsTrue(root.Children.Any(x => x.Value == entry));
            }

            var other = root.ToHashSet<string>();
            ClassicAssert.IsTrue(other.Count == set.Count);

            foreach (var entry in set)
            {
                ClassicAssert.IsTrue(other.Contains(entry));
            }

            var node = root.GetNodeByIndex(1);
            ClassicAssert.IsTrue(root.RemoveNode(node));
            ClassicAssert.IsTrue(root.ChildCount == set.Count - 1);
        }

        #endregion
    }
}
