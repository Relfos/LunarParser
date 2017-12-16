using LunarParser;
using LunarParser.CSV;
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
            var xml = "<message><content>Hello world!</content></message>";

            var root = XMLReader.ReadFromString(xml);
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLReaderFull()
        {
            var root = XMLReader.ReadFromString("<?xml version=\"1.0\" encoding=\"utf-8\"?><!-- In this collection, we will keep each title \"as is\" --><videos><video><title>The Distinguished Gentleman</title><director>Jonathan Lynn</director><length>112 Minutes</length><format>DVD</format><rating>R</rating></video><video><title>Her Alibi</title><director>Bruce Beresford</director><length>94 Mins</length><format>DVD</format><rating>PG-13</rating> </video></videos>");
            Assert.NotNull(root);

            var videos = root["videos"];
            Assert.NotNull(videos);

            Assert.IsTrue("videos".Equals(videos.Name));
            Assert.IsTrue(videos.ChildCount.Equals(2));

            var content = videos.GetNode("video");
            Assert.NotNull(content);
            Assert.IsTrue(content.ChildCount.Equals(5));
        }

        [Test]
        public void TestXMLComments()
        {
            var root = XMLReader.ReadFromString("<message><!--This is a comment, will be ignored--><content>Hello world!</content></message>");
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);
            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));
            Assert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLEmpty()
        {
            var root = XMLReader.ReadFromString("");
            Assert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("    ");
            Assert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("<!---->");
            Assert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("<!-- nsbdghfds <msg>hello</msg> fdgf -->");
            Assert.True(root.ChildCount.Equals(0));
            root = XMLReader.ReadFromString("<!-- <aa /> -->");
            Assert.True(root.ChildCount.Equals(0));
        }

        [Test]
        public void TestXMLRoot()
        {
            var root = XMLReader.ReadFromString("<message></message>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);
            Assert.IsEmpty(msg.Value);

            root = XMLReader.ReadFromString("<message>aaa</message>");
            Assert.NotNull(root);
            msg = root["message"];
            Assert.NotNull(msg);
            Assert.AreEqual("aaa", msg.Value);

            root = XMLReader.ReadFromString("<message><!--aa--></message>");
            Assert.NotNull(root);
            msg = root["message"];
            Assert.NotNull(msg);
            Assert.IsEmpty(msg.Value);
        }

        // Valid in XML
        [Test]
        public void TestXMLCommentsTags()
        {
            var root = XMLReader.ReadFromString("<message><!-- will - - <- be ignored-->" +
                                                "<!--df <! - - </ m\"es\"sage > dd=\"aa\" -->" +
                                                "<content>Hello world!</content>" +
                                                "<!-- df <!- - </message> --> </message>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));
            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));
            Assert.AreEqual("Hello world!", content);
        }

        // Not strictly valid in XML, but accepted in HTML and others
        [Test]
        public void TestXMLCommentsUnbalanced()
        {
            var root = XMLReader.ReadFromString("<message><!-- will <-- be ignored-->" +
                                                "<content> <!--df \" \" <!-- </ message > --> " +
                                                "Hello world!</content></message>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);
            Assert.AreEqual("message", msg.Name);
            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));
            Assert.AreEqual("Hello world!", content.Trim());
        }

        [Test]
        public void TestXMLProlog()
        {
            var root = XMLReader.ReadFromString("<!--This is a comment, will be ignored--><message>" +
                                                "<content>Hello world!</content></message>");
            Assert.NotNull(root);
            Assert.IsTrue(root.ChildCount.Equals(1));

            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLAttributes()
        {
            var root = XMLReader.ReadFromString("<message content=\"Hello world!\"/>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestXMLAttributesIgnored()
        {
            var root = XMLReader.ReadFromString("<message content=\"Hello /> world!\"/>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue("Hello /> world!".Equals(content));
        }

        [Test]
        public void TestXMLText()
        {
            var root = XMLReader.ReadFromString("<message attribute=\"something\">other</message>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var attr = msg.GetString("attribute");
            Assert.IsTrue("something".Equals(attr));

            Assert.IsTrue("other".Equals(msg.Value));
        }

        [Test]
        public void TestXMLShortTag()
        {
            var root = XMLReader.ReadFromString("<message attribute=\"something\"><go /></message>");
            Assert.NotNull(root);
            var msg = root["message"];
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var attr = msg.GetString("attribute");
            Assert.IsTrue("something".Equals(attr));

            Assert.IsTrue(msg.ChildCount == 2);

            var child = msg.GetNodeByIndex(1);
            Assert.IsNotNull(child);
            Assert.IsTrue("go".Equals(child.Name));
            Assert.IsTrue(string.IsNullOrEmpty(child.Value));
        }

        [Test]
        public void TestXMLCData()
        {
            string test = String.Format(@"<message> <content><![CDATA[test<>me]]></content></message>");
            var root = XMLReader.ReadFromString(test);
            var msg = root["message"];
            var content = msg.GetString("content");
            Assert.IsTrue(content.Equals("test<>me"));

            test = String.Format(@"<message> <content><![CDATA[test<>me]<[]]></content></message>");
            root = XMLReader.ReadFromString(test);
            msg = root["message"];
            content = msg.GetString("content");
            Assert.IsTrue(content.Equals("test<>me]<["));

            test = String.Format(@"<message><content>![CDATA[testme]]</content></message>");
            root = XMLReader.ReadFromString(test);
            msg = root["message"];
            content = msg.GetString("content");
            Assert.IsTrue(content.Equals("![CDATA[testme]]"));

            test = String.Format("<message><content><![CDATA[line1.test<>me\nline2.hello\nthirdline]]></content></message>");

            root = XMLReader.ReadFromString(test);
            msg = root["message"];
            content = msg.GetString("content");
            Assert.IsTrue(content.Equals("line1.test<>me\nline2.hello\nthirdline"));
        }

        [Test]
        public void TestJSONReader()
        {
            var json = "{\"message\": { \"content\": \"Hello world!\"} }";
            var root = JSONReader.ReadFromString(json);
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);

            // alternate way
            msg = root.GetNode("message");
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg.GetString("content");
            Assert.IsFalse(string.IsNullOrEmpty(content));

            Assert.IsTrue("Hello world!".Equals(content));
        }

        [Test]
        public void TestJSONTypes()
        {
            var root = JSONReader.ReadFromString("{\"message\": { \"number\": 3.14159, \"negative\": -52, \"check\":true, \"item\": null} }");
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);

            // alternate way
            msg = root.GetNode("message");
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var number = msg.GetFloat("number");
            Assert.IsTrue(Math.Abs(number - 3.14159) < 0.001f);
            Assert.IsTrue(msg.GetNode("number").Kind == NodeKind.Numeric);

            var negative = msg.GetInt32("negative");
            Assert.IsTrue(negative == -52);
            Assert.IsTrue(msg.GetNode("negative").Kind == NodeKind.Numeric);

            var check = msg.GetBool("check");
            Assert.IsTrue(check);
            Assert.IsTrue(msg.GetNode("check").Kind == NodeKind.Boolean);

            var item = msg.GetNode("item");
            Assert.IsNotNull(item);
            Assert.IsTrue(msg.GetNode("item").Kind == NodeKind.Null);
            Assert.IsTrue(string.IsNullOrEmpty(item.Value));
        }

        [Test]
        public void TestJSONArray()
        {
            var root = JSONReader.ReadFromString("{\"message\": { \"content\": [0, 1, 2, 3]} }");
            Assert.NotNull(root);

            var msg = root["message"];
            Assert.NotNull(msg);

            // alternate way
            msg = root.GetNode("message");
            Assert.NotNull(msg);

            Assert.IsTrue("message".Equals(msg.Name));

            var content = msg["content"];
            Assert.IsTrue(content.ChildCount == 4);

            for (int i = 0; i < 4; i++)
            {
                var number = content.GetNodeByIndex(i);
                Assert.IsTrue(i.ToString().Equals(number.Value));
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
            Assert.NotNull(json);

            var other = JSONReader.ReadFromString(json);
            Assert.NotNull(other);

            Assert.IsTrue(other.ChildCount == root.ChildCount);

            for (int i = 0; i < root.ChildCount; i++)
            {
                var child = root.GetNodeByIndex(i);
                var otherChild = other.GetNodeByIndex(i);

                Assert.NotNull(child);
                Assert.NotNull(otherChild);

                Assert.IsTrue(child.Name == otherChild.Name);
                Assert.IsTrue(child.Value == otherChild.Value);
            }
        }

        [Test]
        public void TestJSONObjectWriter()
        {
            var color = new Color(200, 100, 220, 128);
            var root = DataNode.CreateObject(null);
            root.AddNode(color.ToDataSource());

            var json = JSONWriter.WriteToString(root);
            Assert.NotNull(json);

            var other = JSONReader.ReadFromString(json);
            Assert.NotNull(other);

            Assert.IsTrue(other.ChildCount == root.ChildCount);

            for (int i = 0; i < root.ChildCount; i++)
            {
                var child = root.GetNodeByIndex(i);
                var otherChild = other.GetNodeByIndex(i);

                Assert.NotNull(child);
                Assert.NotNull(otherChild);

                Assert.IsTrue(child.Name == otherChild.Name);
                Assert.IsTrue(child.Value == otherChild.Value);
            }
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

            Assert.IsTrue("Hello world!".Equals(content));
        }

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
            Assert.NotNull(root);

            Assert.IsTrue(root.ChildCount == 4);

            var animals = root.ToArray<Animal>();
            Assert.IsTrue(animals.Length == 4);

            Animal animal;

            animal = animals[0];
            Assert.IsTrue(1.Equals(animal.id));
            Assert.IsTrue("Dog".Equals(animal.name));

            animal = animals[1];
            Assert.IsTrue(2.Equals(animal.id));
            Assert.IsTrue("The \"Mr\"Cat".Equals(animal.name));

            animal = animals[2];
            Assert.IsTrue(399.Equals(animal.id));
            Assert.IsTrue("Fish,Blue".Equals(animal.name));

            animal = animals[3];
            Assert.IsTrue(412.Equals(animal.id));
            Assert.IsTrue("Heavy Bird".Equals(animal.name));
        }

        [Test]
        public void TestDateTime()
        {
            var date = new DateTime(2017, 11, 29, 10, 30, 0);

            var root = DataNode.CreateObject("test");
            Assert.NotNull(root);

            root.AddField("first", date);
            root.AddField("second", date.ToString());
            root.AddField("third", "2017-11-29T10:30:00.000Z");

            Assert.IsTrue(root.ChildCount == 3);

            var xml = XMLWriter.WriteToString(root);
            Assert.IsFalse(string.IsNullOrEmpty(xml));

            root = XMLReader.ReadFromString(xml);
            Assert.NotNull(root);

            var test = root.GetNode("test");
            Assert.IsTrue("test".Equals(test.Name));

            var first = test.GetDateTime("first");
            Assert.IsTrue(first.Equals(date));

            var second = test.GetDateTime("second");
            Assert.IsTrue(second.Equals(date));

            var third = test.GetDateTime("third");
            Assert.IsTrue(third.Equals(date));
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

            var test = root.GetNode("test");
            Assert.IsTrue("test".Equals(test.Name));

            var content = test.GetNode("color");
            Assert.NotNull(content);
            Assert.IsTrue(content.ChildCount == 4);

            var otherColor = content.ToObject<Color>();

            Assert.IsTrue(otherColor.Equals(color));
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
            Assert.NotNull(root);

            var colors = new Color[] { red, green, blue, white, grey };
            var temp = colors.ToDataSource("colors");
            Assert.NotNull(temp);
            Assert.IsTrue(temp.ChildCount == 5);

            root.AddNode(temp);
            var xml = XMLWriter.WriteToString(root);

            root = XMLReader.ReadFromString(xml);

            var test = root["test"];
            temp = test["colors"];

            colors = temp.ToArray<Color>();
            Assert.IsTrue(colors.Length == 5);

            Assert.IsTrue(colors[0].Equals(red));
            Assert.IsTrue(colors[1].Equals(green));
            Assert.IsTrue(colors[2].Equals(blue));
            Assert.IsTrue(colors[3].Equals(white));
            Assert.IsTrue(colors[4].Equals(grey));
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

            var test = root.GetNode("test");
            Assert.IsTrue("test".Equals(test.Name));

            var content = test.GetNode("colorgroup");
            Assert.NotNull(content);
            Assert.IsTrue(content.ChildCount == 2);

            var otherGroup = content.ToObject<ColorGroup>();

            Assert.IsTrue(otherGroup.foreground.Equals(cgroup.foreground));
            Assert.IsTrue(otherGroup.background.Equals(cgroup.background));
        }

        [Test]
        public void TestFindNodes()
        {
            var root = JSONReader.ReadFromString("{\"root\": { \"number\": 3.14159, \"check\":true, \"item\": {\"base\": \"found me\"} } }");
            Assert.NotNull(root);
            var msg = root["root"];
            Assert.NotNull(msg);

            // alternate way
            var child = root.FindNode("base");
            Assert.NotNull(child);
            Assert.AreEqual("base", child.Name);
            Assert.AreEqual("found me", child.Value);
        }

        [Test]
        public void TestAutoDetection()
        {
            var xml = "<message><content>Hello world!</content></message>";
            var json = "{\"message\": { \"content\": \"Hello world!\"} }";
            var yaml = "---\nmessage:\n  content: Hello world!";

            DataFormat format;

            format = DataFormats.DetectFormat(xml);
            Assert.IsTrue(format.Equals(DataFormat.XML));

            format = DataFormats.DetectFormat(json);
            Assert.IsTrue(format.Equals(DataFormat.JSON));

            format = DataFormats.DetectFormat(yaml);
            Assert.IsTrue(format.Equals(DataFormat.YAML));
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
            Assert.IsTrue(answer == AnswerKind.Maybe);

            var other = root.GetEnum<AnswerKind>("Other");
            Assert.IsTrue(other == AnswerKind.No);
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
            Assert.IsTrue(s == dogName);

            s = root["dog"].Value;
            Assert.IsTrue(s == dogName);

            s = root[0].Value;
            Assert.IsTrue(s == dogName);

            s = root[0].AsString();
            Assert.IsTrue(s == dogName);

            s = root.GetString("cat");
            Assert.IsTrue(s == catName);

            s = root["cat"].Value;
            Assert.IsTrue(s == catName);

            s = root[1].Value;
            Assert.IsTrue(s == catName);

            s = root[1].AsString();
            Assert.IsTrue(s == catName);
        }

        [Test]
        public void TestTypes()
        {
            var root = DataNode.CreateObject();
            root.AddField("a", 123);
            root.AddField("f", 123.456);
            root.AddField("b", true);

            Assert.IsTrue(root["a"].AsLong() == 123);
            Assert.IsTrue(root.GetLong("a") == 123);
            
            Assert.IsTrue(root["a"].AsUInt32() == 123);
            Assert.IsTrue(root.GetUInt32("a") == 123);

            Assert.IsTrue(root["a"].AsInt32() == 123);
            Assert.IsTrue(root.GetInt32("a") == 123);

            Assert.IsTrue(root["a"].AsByte() == 123);
            Assert.IsTrue(root.GetByte("a") == 123);

            Assert.IsTrue(root["a"].AsSByte() == 123);
            Assert.IsTrue(root.GetSByte("a") == 123);

            Assert.IsTrue(root["f"].AsFloat() == 123.456f);
            Assert.IsTrue(root.GetFloat("f") == 123.456f);

            Assert.IsTrue(root["f"].AsDecimal() == 123.456m);
            Assert.IsTrue(root.GetDecimal("f") == 123.456m);

            Assert.IsTrue(root["f"].AsDouble() == 123.456);
            Assert.IsTrue(root.GetDouble("f") == 123.456);

            Assert.IsTrue(root["b"].AsBool() );
            Assert.IsTrue(root.GetBool("b"));

            Assert.IsTrue(root["f"].AsString() == "123.456");
            Assert.IsTrue(root.GetString("f") == "123.456");
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
            Assert.IsTrue(s == "yes");

            s = root.GetString("never", "no");
            Assert.IsTrue(s == "no");

            bool b;

            b = root.GetBool("other");
            Assert.IsTrue(b);

            b = root.GetBool("missing");
            Assert.IsFalse(b);

            b = root.GetBool("missing", true);
            Assert.IsTrue(b);

            b = root.GetBool("something");
            Assert.IsFalse(b);
        }


    }
}
