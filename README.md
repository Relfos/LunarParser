# Lunar Parser
XML/JSON parsers for C#.

# Why Lunar Parser?
There already excellent solutions for parsing JSON and XML in C#.
However they usually require one of the following:
- Creating classes by hand
- C# Dynamic feature
- Cumbersome casting to things like Dictionary<Dictionary<string, object>>

Besides that, each file format requires a different library that works in completly different way.

Lunar Parser was written with the intent of making parsing of data as easy as possible and in with a way that makes possible to read or write a JSON or XML using exactly the same code for each. 

Get a string and pass it to the proper parser and get data back in a tree structure.
It's also possible to manually create your own data nodes and then serialize them into a string.
And there's also some utility methods to convert back and forth between data nodes and dictionaries, or even serialize an object to a DataNode in a generic way.

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

# Usage

Import the package:

```c#
using LunarParser;
```

Here's some examples.

```c#
// reading XML
var root = XMLReader.ReadFromString("<message><content>Hello world!</content></message>");
var msg = root["message"];
var content = msg.GetString("content");
Console.WriteLine("Message: " + content);
```

```c#
// reading JSON
var root = JSONReader.ReadFromString("{\"message\": { \"content\": \"Hello world!\" } }");
var msg = root["message"];
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
// converting a dictionary to a data node
var dic = new Dictionary<string, string>();
dic["dog"] = "barf";
dic["cat"] = "meow";
dic["fish"] = "blublu";
var data = dic.ToDataSource(dic);

// its also easy to iterate on child nodes
foreach (var child in data.Children) {
	Console.WriteLine(child.Name + " = " + child.Value);
}

//same could be done for json, using JSONWriter
var xml = XMLWriter.WriteToString(msg);
Console.WriteLine("XML: " + xml);

// you can also do the opposite...
dic = data.ToDictionary();
```

# Notes

## DateTime type

Lunar Parser automatically DateTime to UNIX timestamps.

So if you serialize it to JSON or XML you will find just a very long number instead of multiple fields.

This saves space without losing any data down to seconds (miliseconds will be lost, any dates before UNIX epoch will be lost). 

If this behavior is not suitable for your program, please compile Lunar Parser from source, with the DATETIME_AS_TIMESTAMPS conditional symbol removed.

## Binary format

Lunar Parser supports reading and writing data nodes in a custom binary format.

This format will preserve the tree structure and node values, same as in XML and JSON formats, however it is much faster to parse and write and the file size is smaller.

Very useful to send data in a tree format over a network connection.

## Generic serialization of objects

In the latest version there is ToObject() / FromDataSource extension methods that allow converting any C# object to a DataSource and back to the same object.

This works by inspecting public fields with reflection. Properties are currently ignored.

However note that those methods are still experimental, and currently they won't supported nested objects, but will work fine with simple structs / classes.

# Contact

Let me know if you find bugs or if you have suggestions to improve the code.

And maybe follow me [@onihunters](https://twitter.com/onihunters) :)