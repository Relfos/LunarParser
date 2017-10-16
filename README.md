# Lunar Parser
XML/JSON parsers for C#.

Lunar Parser was written with the intent of making parsing of data as easy as possible.  

Get a string and pass it to the proper parser and get data back in a tree structure.
It's also possible to manually create your own data nodes and then serialize them into a string.
And there's also some utility methods to convert back and forth between data nodes and string dictionaries.

## Installation

    PM> Install-Package LunarParser

# Getting Started

LunarParser supports:

- .NET Core
- .NET Framework 3.5 and above
- Mono & Xamarin
- UWP

## Supported Formats

- XML
- JSON
- BIN (custom format)

## TODO

- Rewrite XML parser (more clean code + support for comment tags)
- Add Unit tests and sample projects
- Support for C# dynamic?

# Usage

Import the package:

```c#
using LunarParser;
```

Here's some examples.

```c#
// reading XML
var root = XMLReader.ReadFromString("<message><content>Hello world!</content></message>");
var msg = root.GetNode("message");
var content = msg.GetString("content");
Console.WriteLine("Message: " + content);
```

```c#
// reading JSON
var root = JSONReader.ReadFromString("{\"message\": { \"content\": \"Hello world!\" } }");
var msg = root.GetNode("message");
var content = msg.GetString("content");
Console.WriteLine("Message: " + content);
```

```c#
// writing XML (same could be done for json, using JSONWriter)
var msg = DataSource.CreateObject("message");
msg.AddField("content", "Hello world!");
var xml = XMLWriter.WriteToString(msg);
Console.WriteLine("XML: " + xml);
System.IO.File.WriteAllText("hello.xml", xml);
```

```c#
// converting a dictionary to XML (same could be done for json, using JSONWriter)
var dic = new Dictionary<string, string>();
dic["dog"] = "barf";
dic["cat"] = "meow";
dic["fish"] = "blublu";
var data = dic.ToDataSource(dic);

// its also easy to iterate on child nodes
foreach (var child in data.Children) {
	Console.WriteLine(child.Name + " = " + child.Value);
}

var xml = XMLWriter.WriteToString(msg);
Console.WriteLine("XML: " + xml);

// you can also do the opposite...
dic = data.ToDictionary();
```

# Binary format

Lunar Parser supports reading and writing data nodes in a custom binary format.

This format will preserve the tree structure and node values, same as in XML and JSON formats, however it is much faster to parse and write and the file size is smaller.

Very useful to send data in a tree format over a network connection.

# Contact

Let me know if you find bugs or if you have suggestions to improve the code.

And maybe follow me [@onihunters](https://twitter.com/onihunters) :)