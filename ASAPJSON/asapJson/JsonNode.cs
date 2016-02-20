﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asapJson
{
    public class JsonNode
    {
        public enum Type
        {
            Array,
            String,
            Number,
            Object,
            Boolean
        }
        private Type type;
        private object value;
        public JsonNode()
        {
            type = Type.Object;
            value = null;
        }
        public JsonNode(Dictionary<string, JsonNode> input)
        {
            type = Type.Object;
            value = (object)input;
        }
        public JsonNode(List<JsonNode> input)
        {
            type = Type.Array;
            value = (object)input;
        }
        public JsonNode(string input, bool isJson = false)
        {
            if (isJson)
            {
                type = Type.Object;
                value = null;
                System.IO.MemoryStream memStream = new System.IO.MemoryStream();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(memStream);
                sw.Write(input);
                sw.Flush();
                memStream.Seek(0, System.IO.SeekOrigin.Begin);
                read(new System.IO.StreamReader(memStream));
                memStream.Close();
            }
            else
            {
                type = Type.String;
                value = (object)input;
            }
        }
        public JsonNode(double input)
        {
            type = Type.Number;
            value = (object)input;
        }
        public JsonNode(bool input)
        {
            type = Type.Boolean;
            value = (object)input;
        }
        public JsonNode(System.IO.StreamReader input)
        {
            type = Type.Object;
            value = null;
            read(input);
        }

        public JsonNode getByPath(string path, char delimeter = '/', bool createIfNonExisting = false)
        {
            var pathArgs = path.Split(delimeter);
            JsonNode curNode = this;
            foreach (var next in pathArgs)
            {
                if (string.IsNullOrEmpty(next))
                    continue;
                if (curNode.getType() == Type.Object)
                {
                    Dictionary<string, JsonNode> val;
                    curNode.getValue(out val);
                    if(val == null)
                    {
                        val = new Dictionary<string, JsonNode>();
                        curNode.setValue(val);
                    }
                    if (!val.TryGetValue(next, out curNode))
                    {
                        if (createIfNonExisting)
                        {
                            curNode = new JsonNode(new Dictionary<string, JsonNode>());
                            val.Add(next, curNode);
                        }
                        else
                        {
                            throw new Exception("Cannot resolve path to end. No next node at '" + next + '\'');
                        }
                    }
                }
                else
                {
                    throw new Exception("Cannot resolve path to end. No next node at '" + next + '\'');
                }
            }
            return curNode;
        }
        public void setAtPath(JsonNode node, string path, char delimeter = '/')
        {
            int index = path.LastIndexOf(delimeter);
            JsonNode lastNode;
            if(index > 0)
            {
                lastNode = getByPath(path.Substring(0, index), delimeter, true); ;
            }
            else
            {
                lastNode = this;
            }

            if (lastNode.getType() == Type.Object)
            {
                Dictionary<string, JsonNode> val;
                lastNode.getValue(out val);
                if (val == null)
                {
                    val = new Dictionary<string, JsonNode>();
                    lastNode.setValue(val);
                }
                if (index > 0)
                {
                    val.Add(path.Substring(index + 1), node);
                }
                else
                {
                    val.Add(path, node);
                }
            }
            else
            {
                throw new Exception("Cannot set path to end. Parent node is not an object");
            }
        }

        public Type getType()
        {
            return this.type;
        }

        public void getValue(out List<JsonNode> val)
        {
            if (this.type == Type.Array)
                val = (List<JsonNode>)this.value;
            else
                throw new TypeAccessException("JsonNode type != Type.Array");
        }
        public void getValue(out string val)
        {
            if (this.type == Type.String)
                val = (string)this.value;
            else
                throw new TypeAccessException("JsonNode type != Type.String");
        }
        public void getValue(out double val)
        {
            if (this.type == Type.Number)
                val = (double)this.value;
            else
                throw new TypeAccessException("JsonNode type != Type.Number");
        }
        public void getValue(out Dictionary<string, JsonNode> val)
        {
            if (this.type == Type.Object)
                val = this.value == null ? null : (Dictionary<string, JsonNode>)this.value;
            else
                throw new TypeAccessException("JsonNode type != Type.Object");
        }
        public void getValue(out bool val)
        {
            if (this.type == Type.Boolean)
                val = (bool)this.value;
            else
                throw new TypeAccessException("JsonNode type != Type.Boolean");
        }

        public void setValue(List<JsonNode> val)
        {
            this.value = (object)val;
            this.type = Type.Array;
        }
        public void setValue(string val)
        {
            this.value = (object)val;
            this.type = Type.String;
        }
        public void setValue(double val)
        {
            this.value = (object)val;
            this.type = Type.Number;
        }
        public void setValue(Dictionary<string, JsonNode> val)
        {
            this.value = (object)val;
            this.type = Type.Object;
        }
        public void setValue(bool val)
        {
            this.value = (object)val;
            this.type = Type.Boolean;
        }
        public void setValue()
        {
            this.value = null;
            this.type = Type.Object;
        }

        #region Reading
        /*
         * Note to the curious reader:
         * I tried something new in here in regards of control flow ...
         * it kind of messed up everything due to the fact that i got
         * like 1k of ifElse shit + specific onePurpose flags all over
         * the place in theese functions ...
         * It did not made things more simple ... but well ...
         * was an experiment :)
         */
        public void read(System.IO.StreamReader sr)
        {
            switch (sr.Peek())
            {
                default:
                    throw new Exception("Unexpected character '" + sr.Peek() + '\'');
                case 'n': case 'N':
                    sr.Read();
                    int next = sr.Read();
                    if (next != 'u' && next != 'U')
                        throw new Exception("Parsing Object failed: Expected 'u' or 'U', got '" + (char)next + '\'');
                    next = sr.Read();
                    if (next != 'l' && next != 'L')
                        throw new Exception("Parsing Object failed: Expected 'l' or 'L', got '" + (char)next + '\'');
                    next = sr.Read();
                    if (next != 'l' && next != 'L')
                        throw new Exception("Parsing Object failed: Expected 'l' or 'L', got '" + (char)next + '\'');
                    this.setValue();
                    break;
                case '{':
                    readObject(sr);
                    break;
                case '[':
                    readArray(sr);
                    break;
                case '"':
                    readString(sr);
                    break;
                case '-':  case '0': case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': case '9':
                    readNumber(sr);
                    break;
                case 't': case 'f': case 'T': case 'F':
                    readBoolean(sr);
                    break;
            }
        }
        private void readObject(System.IO.StreamReader sr)
        {
            int i;
            bool isOpen = false;
            bool hasLabel = false;
            bool getNextValue = false;
            bool getNextKeySet = false;
            string label = "";
            char lastChar = '\0';
            Dictionary<string, JsonNode> dict = new Dictionary<string, JsonNode>();
            this.setValue(dict);
            while ((i = sr.Peek()) >= 0)
            {
                char c = (char)i;
                if (!(c == ' ' || c == '\t' || c == '\n' || c == '\r'))
                {
                    if (isOpen)
                    {//node is opened ('{' already got read) and ready to be parsed
                        if (hasLabel)
                        {//Label for current object item was already started and thus can be
                            if (getNextKeySet)
                            {//Next key set needs to be received (value already parsed) or the current object has to be closed
                                if (c == ',')
                                {
                                    hasLabel = false;
                                    getNextKeySet = false;
                                    getNextValue = false;
                                    hasLabel = false;
                                }
                                else if (c == '}')
                                {
                                    c = (char)sr.Read();
                                    lastChar = c;
                                    break;
                                }
                                else
                                {
                                    throw new Exception("Parsing Object failed: Expected ',' or '}', got '" + c + '\'');
                                }
                            }
                            else if (getNextValue)
                            {//Next value of current label needs to get parsed
                                JsonNode child = new JsonNode();
                                child.read(sr);
                                dict.Add(label, child);
                                getNextKeySet = true;
                                lastChar = '\0';
                                continue;
                            }
                            else
                            {//Separator for label and string expected
                                if (c != ':')
                                    throw new Exception("Parsing Object failed: Expected '\"', got '" + c + '\'');
                                getNextValue = true;
                            }
                        }
                        else
                        {//No label yet found --> get label of current item
                            if (c != '"')
                                throw new Exception("Parsing Object failed: Expected '\"', got '" + c + '\'');
                            label = parseStringFromEncoded(sr);
                            hasLabel = true;
                            lastChar = '"';
                            continue;
                        }
                    }
                    else
                    {//node yet has to be opened
                        if (c != '{')
                            throw new Exception("Parsing Object failed: Expected '{', got '" + c + '\'');
                        isOpen = true;
                    }
                }
                c = (i = sr.Read()) > 0 ? (char)c : '\0';
                lastChar = c;
            }
            if(lastChar != '}')
                throw new Exception("Parsing Object failed: Expected '}', got '" + lastChar + '\'');
        }
        private void readArray(System.IO.StreamReader sr)
        {
            int i;
            List<JsonNode> array = new List<JsonNode>();
            this.setValue(array);
            bool isOpen = false;
            bool allowNext = true;
            char lastChar = '\0';
            while ((i = sr.Peek()) >= 0)
            {
                char c = (char)i;
                if (!(c == ' ' || c == '\t' || c == '\n' || c == '\r'))
                {
                    if (isOpen)
                    {
                        if (c == ']')
                        {
                            c = (i = sr.Read()) > 0 ? (char)c : '\0';
                            lastChar = c;
                            break;
                        }
                        else if (allowNext)
                        {
                            JsonNode node = new JsonNode();
                            node.read(sr);
                            array.Add(node);
                            allowNext = false;
                            lastChar = '\0';
                            continue;
                        }
                        else if (c == ',')
                        {
                            allowNext = true;
                        }
                        else
                        {
                            throw new Exception("Parsing Object failed: Expected ',' or ']', got '" + c + '\'');
                        }
                    }
                    else
                    {
                        if (c != '[')
                            throw new Exception("Parsing Object failed: Expected '{', got '" + c + '\'');
                        isOpen = true;
                        allowNext = true;
                    }
                }
                c = (i = sr.Read()) > 0 ? (char)c : '\0';
                lastChar = c;
            }
        }
        private void readString(System.IO.StreamReader sr)
        {
            this.setValue(parseStringFromEncoded(sr));
        }
        private static string parseStringFromEncoded(System.IO.StreamReader sr)
        {
            StringBuilder sb = new StringBuilder();
            int i;
            if (sr.Peek() != '"')
                throw new Exception("Parsing Object failed: Expected '\"', got '" + (char)sr.Peek() + '\'');
            sr.Read();
            bool escape = false;
            char lastChar = '\0';
            while ((i = sr.Peek()) >= 0)
            {
                char c = (char)i;
                if(escape)
                {
                    switch (c)
                    {
                        default:
                            throw new Exception("Parsing Object failed: Invalid escape: '" + c + '\'');
                        case '"':
                            sb.Append('"');
                            break;
                        case '/':
                            sb.Append('/');
                            break;
                        case 'b':
                            sb.Append("\b");
                            break;
                        case 'f':
                            sb.Append("\f");
                            break;
                        case 'n':
                            sb.Append("\n");
                            break;
                        case 'r':
                            sb.Append("\r");
                            break;
                        case 't':
                            sb.Append("\t");
                            break;
                    }
                    escape = false;
                }
                else
                {
                    if(c == '\\')
                    {
                        escape = true;
                    }
                    else if(c == '"')
                    {
                        c = (char)sr.Read();
                        lastChar = c;
                        break;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                c = (char)sr.Read();
                lastChar = c;
            }
            if(lastChar != '"')
                throw new Exception("Parsing Object failed: Expected '\"', got '" + lastChar + '\'');
            return sb.ToString();
        }
        private void readNumber(System.IO.StreamReader sr)
        {
            StringBuilder sb = new StringBuilder();
            int i;
            if (sr.Peek() == '-')
                sb.Append('-');
            bool isFrontNumber = true;
            while((i = sr.Peek()) >= 0)
            {
                char c = (char)i;
                if(isFrontNumber)
                {
                    if(c == '0' && sb.Length == 0)
                    {
                        sb.Append(c);
                        sb.Append('.');
                        isFrontNumber = false;
                    }
                    else if(c >= '0' && c <= '9')
                    {
                        sb.Append(c);
                    }
                    else if(c == '.')
                    {
                        sb.Append(c);
                        isFrontNumber = false;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (c >= '0' && c <= '9')
                    {
                        sb.Append(c);
                    }
                    else if(c == 'e' || c == 'E' || c == '+' || c == '-')
                    {
                        throw new Exception("Parsing Object failed: Expected DIGIT, got '" + c + "', Note: ASAP-JSON is not able to parse eg. 0.1234e10");
                    }
                    else
                    {
                        break;
                    }
                }
                sr.Read();
            }
            this.setValue(double.Parse(sb.ToString(), System.Globalization.CultureInfo.InvariantCulture));
        }
        private void readBoolean(System.IO.StreamReader sr)
        {
            int i = sr.Peek();
            if (i == 't' || i == 'T')
            {
                sr.Read();
                i = sr.Read();
                if (i != 'r' && i != 'R')
                    throw new Exception("Parsing Object failed: Expected 'r' or 'R', got '" + (char)i + '\'');
                i = sr.Read();
                if (i != 'u' && i != 'U')
                    throw new Exception("Parsing Object failed: Expected 'u' or 'U', got '" + (char)i + '\'');
                i = sr.Read();
                if (i != 'e' && i != 'E')
                    throw new Exception("Parsing Object failed: Expected 'e' or 'E', got '" + (char)i + '\'');
                this.setValue(true);
            }
            else if(i == 'f' | i == 'F')
            {
                sr.Read();
                i = sr.Read();
                if (i != 'a' && i != 'A')
                    throw new Exception("Parsing Object failed: Expected 'a' or 'A', got '" + (char)i + '\'');
                i = sr.Read();
                if (i != 'l' && i != 'L')
                    throw new Exception("Parsing Object failed: Expected 'l' or 'L', got '" + (char)i + '\'');
                i = sr.Read();
                if (i != 's' && i != 'S')
                    throw new Exception("Parsing Object failed: Expected 's' or 'S', got '" + (char)i + '\'');
                i = sr.Read();
                if (i != 'e' && i != 'E')
                    throw new Exception("Parsing Object failed: Expected 'e' or 'E', got '" + (char)i + '\'');
                this.setValue(false);
            }
        }
        #endregion

        public void print(System.IO.StreamWriter sw)
        {
            switch(this.type)
            {
                case Type.Array:
                    {
                        List<JsonNode> obj;
                        bool flag = false;
                        this.getValue(out obj);
                        sw.Write('[');
                        foreach(var it in obj)
                        {
                            if (flag)
                                sw.Write(',');
                            else
                                flag = true;
                            it.print(sw);
                        }
                        sw.Write(']');
                    }
                    break;
                case Type.Boolean:
                    {
                        bool obj;
                        this.getValue(out obj);
                        sw.Write(obj ? "true" : "false");
                    }
                    break;
                case Type.Number:
                    {
                        double obj;
                        this.getValue(out obj);
                        sw.Write(obj.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                    break;
                case Type.Object:
                    {
                        Dictionary<string, JsonNode> obj;
                        bool flag = false;
                        this.getValue(out obj);
                        if (obj == null)
                        {
                            sw.Write("null");
                        }
                        else
                        {
                            sw.Write('{');
                            foreach (var it in obj)
                            {
                                if (flag)
                                    sw.Write(',');
                                else
                                    flag = true;
                                sw.Write('"');
                                sw.Write(it.Key);
                                sw.Write('"');
                                sw.Write(':');
                                it.Value.print(sw);
                            }
                            sw.Write('}');
                        }
                    }
                    break;
                case Type.String:
                    {
                        string obj;
                        this.getValue(out obj);
                        sw.Write('"');
                        var sb = new StringBuilder();
                        foreach(var it in obj)
                        {
                            switch(it)
                            {
                                default:
                                    sb.Append(it);
                                    break;
                                case '"':
                                    sb.Append("\\\"");
                                    break;
                                //case '/':
                                //    sb.Append("\\/");
                                //    break;
                                case '\b':
                                    sb.Append("\\b");
                                    break;
                                case '\f':
                                    sb.Append("\\f");
                                    break;
                                case '\n':
                                    sb.Append("\\n");
                                    break;
                                case '\r':
                                    sb.Append("\\r");
                                    break;
                                case '\t':
                                    sb.Append("\\t");
                                    break;
                            }
                        }
                        sw.Write(sb.ToString());
                        sw.Write('"');
                    }
                    break;
                default:
                    throw new Exception("Not Implemented type");
            }
        }

        public override string ToString()
        {
            System.IO.MemoryStream memStream = new System.IO.MemoryStream();
            System.IO.StreamWriter sw = new System.IO.StreamWriter(memStream);
            this.print(sw);
            sw.Flush();
            memStream.Seek(0, System.IO.SeekOrigin.Begin);
            string s = new System.IO.StreamReader(memStream).ReadToEnd();
            memStream.Close();

            return s;
        }
    }
}
