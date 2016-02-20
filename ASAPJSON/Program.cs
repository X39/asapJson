using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asapJson.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            {
                Dictionary<string, JsonNode> rootValue = new Dictionary<string, JsonNode>();
                JsonNode root = new JsonNode(rootValue);
                rootValue.Add("String", new JsonNode("testString\"WithEscapedQuote"));
                rootValue.Add("Number", new JsonNode(1.2345));
                rootValue.Add("boolTrue", new JsonNode(true));
                rootValue.Add("boolFalse", new JsonNode(false));
                rootValue.Add("NullObject", new JsonNode());

                List<JsonNode> arrayNodeValue = new List<JsonNode>();
                Dictionary<string, JsonNode> node2Value = new Dictionary<string, JsonNode>();
                node2Value.Add("String", new JsonNode("testString\"WithEscapedQuote"));
                node2Value.Add("Number", new JsonNode(1.2345));
                node2Value.Add("boolTrue", new JsonNode(true));
                node2Value.Add("boolFalse", new JsonNode(false));
                node2Value.Add("NullObject", new JsonNode());
                Dictionary<string, JsonNode> node3Value = new Dictionary<string, JsonNode>();
                node3Value.Add("String", new JsonNode("testString\"WithEscapedQuote"));
                node3Value.Add("Number", new JsonNode(1.2345));
                node3Value.Add("boolTrue", new JsonNode(true));
                node3Value.Add("boolFalse", new JsonNode(false));
                node3Value.Add("NullObject", new JsonNode());
                arrayNodeValue.Add(new JsonNode(node2Value));
                arrayNodeValue.Add(new JsonNode(node3Value));
                arrayNodeValue.Add(new JsonNode(1));
                arrayNodeValue.Add(new JsonNode(false));
                arrayNodeValue.Add(new JsonNode(2));
                arrayNodeValue.Add(new JsonNode("foo"));
                arrayNodeValue.Add(new JsonNode(3));
                arrayNodeValue.Add(new JsonNode("bar"));
                rootValue.Add("Array", new JsonNode(arrayNodeValue));
                Console.Write(root.ToString());
            }
            Console.Write("\n\n\n\n\n\n");
            {
                JsonNode readNode = new JsonNode();
                System.IO.MemoryStream memStream = new System.IO.MemoryStream();
                System.IO.StreamWriter sr = new System.IO.StreamWriter(memStream);
                sr.Write("{");
                sr.Write("	\"String\": \"testString\\\"WithEscapedQuote\",");
                sr.Write("	\"Number\": 1.2345,");
                sr.Write("	\"boolTrue\": true,");
                sr.Write("	\"boolFalse\": false,");
                sr.Write("	\"NullObject\": null,");
                sr.Write("	\"Array\": [{");
                sr.Write("		\"String\": \"testString\\\"WithEscapedQuote\",");
                sr.Write("		\"Number\": 1.2345,");
                sr.Write("		\"boolTrue\": true,");
                sr.Write("		\"boolFalse\": false,");
                sr.Write("		\"NullObject\": null");
                sr.Write("	}, {");
                sr.Write("		\"String\": \"testString\\\"WithEscapedQuote\",");
                sr.Write("		\"Number\": 1.2345,");
                sr.Write("		\"boolTrue\": true,");
                sr.Write("		\"boolFalse\": false,");
                sr.Write("		\"NullObject\": null");
                sr.Write("	}, 1, false, 2, \"foo\", 3, \"bar\"]");
                sr.Write("}");
                sr.Flush();
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                readNode.read(new System.IO.StreamReader(memStream));
                Console.Write(readNode.ToString());
            }
            Console.Write("\n\n\n\n\n\n");
            {
                JsonNode node = new JsonNode();
                node.setAtPath(new JsonNode("mostLikelyCrash"), "foobar");
                node.setAtPath(new JsonNode(123), "foo/bar/test/FuckSystem");
                node.setAtPath(new JsonNode(123), "foo/bar/test/this/shit");
                node.setAtPath(new JsonNode(true), "foo/bar/test/shitPass?");
                List<JsonNode> arrayNodeValue = new List<JsonNode>();
                arrayNodeValue.Add(new JsonNode(1));
                arrayNodeValue.Add(new JsonNode(false));
                arrayNodeValue.Add(new JsonNode(2));
                arrayNodeValue.Add(new JsonNode("foo"));
                arrayNodeValue.Add(new JsonNode(3));
                arrayNodeValue.Add(new JsonNode("bar"));
                node.setAtPath(new JsonNode(arrayNodeValue), "foo/array");

                JsonNode retNode = node.getByPath("foo/array");
                Console.Write(node.ToString());
            }
            Console.Write("\n\n\n\n\n\n");
            {
                JsonNode node = new JsonNode();
                Console.Write(node.ToString());
            }
            Console.ReadKey();
        }
    }
}
