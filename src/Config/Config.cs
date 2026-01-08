using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LightweightJson
{
    public enum JsonType { Null, Bool, Number, String, Array, Object }

    public abstract class JsonValue
    {
        public abstract JsonType Type { get; }

        public virtual bool IsNull => Type == JsonType.Null;
        public virtual bool IsBool => Type == JsonType.Bool;
        public virtual bool IsNumber => Type == JsonType.Number;
        public virtual bool IsString => Type == JsonType.String;
        public virtual bool IsArray => Type == JsonType.Array;
        public virtual bool IsObject => Type == JsonType.Object;

        public virtual JsonObject AsObject() => throw new InvalidOperationException($"Not an object (was {Type}).");
        public virtual JsonArray AsArray() => throw new InvalidOperationException($"Not an array (was {Type}).");
        public virtual string AsString() => throw new InvalidOperationException($"Not a string (was {Type}).");
        public virtual bool AsBool() => throw new InvalidOperationException($"Not a bool (was {Type}).");
        public virtual JsonNumber AsNumber() => throw new InvalidOperationException($"Not a number (was {Type}).");

        public override string ToString() => Json.Stringify(this);
    }

    public sealed class JsonNull : JsonValue
    {
        public static readonly JsonNull Instance = new JsonNull();
        private JsonNull() { }
        public override JsonType Type => JsonType.Null;
    }

    public sealed class JsonBool : JsonValue
    {
        public bool Value { get; }
        public JsonBool(bool value) => Value = value;
        public override JsonType Type => JsonType.Bool;
        public override bool AsBool() => Value;
    }

    public sealed class JsonString : JsonValue
    {
        public string Value { get; }
        public JsonString(string value) => Value = value ?? "";
        public override JsonType Type => JsonType.String;
        public override string AsString() => Value;
    }

    public sealed class JsonNumber : JsonValue
    {
        // We store either a long (integral) or a double (floating), plus raw text for fidelity.
        public bool IsInteger { get; }
        public long Int64 { get; }
        public double Double { get; }
        public string Raw { get; }

        public JsonNumber(long value, string raw)
        {
            IsInteger = true;
            Int64 = value;
            Double = value;
            Raw = raw;
        }

        public JsonNumber(double value, string raw)
        {
            IsInteger = false;
            Double = value;
            Int64 = (long)value;
            Raw = raw;
        }

        public override JsonType Type => JsonType.Number;
        public override JsonNumber AsNumber() => this;

        public long AsInt64Checked()
        {
            if (IsInteger) return Int64;
            // best-effort check
            if (Double < long.MinValue || Double > long.MaxValue)
                throw new OverflowException("JSON number out of Int64 range.");
            if (Math.Abs(Double - Math.Round(Double)) > 0)
                throw new InvalidOperationException("JSON number is not an integer.");
            return (long)Math.Round(Double);
        }

        public double AsDouble() => Double;
    }

    public sealed class JsonArray : JsonValue, IEnumerable<JsonValue>
    {
        private readonly List<JsonValue> _items = new List<JsonValue>();
        public override JsonType Type => JsonType.Array;

        public int Count => _items.Count;
        public JsonValue this[int index] { get => _items[index]; set => _items[index] = value ?? JsonNull.Instance; }

        public void Add(JsonValue v) => _items.Add(v ?? JsonNull.Instance);
        public override JsonArray AsArray() => this;

        public IEnumerator<JsonValue> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }

    public sealed class JsonObject : JsonValue, IEnumerable<KeyValuePair<string, JsonValue>>
    {
        private readonly Dictionary<string, JsonValue> _props = new Dictionary<string, JsonValue>(StringComparer.Ordinal);

        public override JsonType Type => JsonType.Object;
        public override JsonObject AsObject() => this;

        public int Count => _props.Count;

        public bool TryGetValue(string key, out JsonValue value) => _props.TryGetValue(key, out value);

        public JsonValue this[string key]
        {
            get => _props.TryGetValue(key, out var v) ? v : JsonNull.Instance;
            set => _props[key] = value ?? JsonNull.Instance;
        }

        public void Add(string key, JsonValue value) => _props[key] = value ?? JsonNull.Instance;

        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator() => _props.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _props.GetEnumerator();

        // Convenience helpers (return defaultValue if missing/wrong type)
        public string GetString(string key, string defaultValue = "")
            => _props.TryGetValue(key, out var v) && v.IsString ? v.AsString() : defaultValue;

        public bool GetBool(string key, bool defaultValue = false)
            => _props.TryGetValue(key, out var v) && v.IsBool ? v.AsBool() : defaultValue;

        public double GetDouble(string key, double defaultValue = 0)
            => _props.TryGetValue(key, out var v) && v.IsNumber ? v.AsNumber().AsDouble() : defaultValue;
    }

    public sealed class JsonParseException : Exception
    {
        public int Index { get; }
        public JsonParseException(string message, int index) : base($"{message} (at index {index})")
        {
            Index = index;
        }
    }

    public static class Json
    {
        public static JsonValue Parse(string json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));
            var p = new Parser(json);
            var value = p.ParseValue();
            p.SkipWhitespace();
            if (!p.EOF)
                throw new JsonParseException("Unexpected trailing content", p.Index);
            return value;
        }

        public static bool TryParse(string json, out JsonValue value, out string error)
        {
            value = JsonNull.Instance;
            error = null;
            try
            {
                value = Parse(json);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public static string Stringify(JsonValue value, bool pretty = false, int indentSize = 2)
        {
            var sb = new StringBuilder();
            WriteValue(sb, value ?? JsonNull.Instance, pretty, indentSize, 0);
            return sb.ToString();
        }

        private static void WriteValue(StringBuilder sb, JsonValue value, bool pretty, int indentSize, int depth)
        {
            switch (value.Type)
            {
                case JsonType.Null:
                    sb.Append("null");
                    return;

                case JsonType.Bool:
                    sb.Append(((JsonBool)value).Value ? "true" : "false");
                    return;

                case JsonType.Number:
                    // Prefer Raw to preserve exponent/format as parsed.
                    sb.Append(((JsonNumber)value).Raw);
                    return;

                case JsonType.String:
                    WriteString(sb, ((JsonString)value).Value);
                    return;

                case JsonType.Array:
                    {
                        var arr = (JsonArray)value;
                        sb.Append('[');
                        if (arr.Count == 0) { sb.Append(']'); return; }

                        if (pretty) { sb.AppendLine(); }

                        for (int i = 0; i < arr.Count; i++)
                        {
                            if (pretty) Indent(sb, indentSize, depth + 1);
                            WriteValue(sb, arr[i], pretty, indentSize, depth + 1);
                            if (i != arr.Count - 1) sb.Append(',');
                            if (pretty) sb.AppendLine();
                        }

                        if (pretty) Indent(sb, indentSize, depth);
                        sb.Append(']');
                        return;
                    }

                case JsonType.Object:
                    {
                        var obj = (JsonObject)value;
                        sb.Append('{');
                        if (obj.Count == 0) { sb.Append('}'); return; }

                        if (pretty) { sb.AppendLine(); }

                        int i = 0;
                        foreach (var kv in obj)
                        {
                            if (pretty) Indent(sb, indentSize, depth + 1);
                            WriteString(sb, kv.Key);
                            sb.Append(pretty ? ": " : ":");
                            WriteValue(sb, kv.Value, pretty, indentSize, depth + 1);

                            if (i != obj.Count - 1) sb.Append(',');
                            if (pretty) sb.AppendLine();
                            i++;
                        }

                        if (pretty) Indent(sb, indentSize, depth);
                        sb.Append('}');
                        return;
                    }

                default:
                    throw new InvalidOperationException("Unknown JsonType.");
            }
        }

        private static void Indent(StringBuilder sb, int indentSize, int depth)
        {
            sb.Append(' ', indentSize * depth);
        }

        private static void WriteString(StringBuilder sb, string s)
        {
            sb.Append('"');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20)
                        {
                            sb.Append("\\u");
                            sb.Append(((int)c).ToString("x4"));
                        }
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
        }

        private sealed class Parser
        {
            private readonly string _s;
            public int Index { get; private set; }
            public bool EOF => Index >= _s.Length;

            public Parser(string s) { _s = s; Index = 0; }

            public void SkipWhitespace()
            {
                while (!EOF)
                {
                    char c = _s[Index];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r') Index++;
                    else break;
                }
            }

            private char Peek()
            {
                if (EOF) throw new JsonParseException("Unexpected end of input", Index);
                return _s[Index];
            }

            private char Read()
            {
                if (EOF) throw new JsonParseException("Unexpected end of input", Index);
                return _s[Index++];
            }

            private void Expect(char expected)
            {
                char c = Read();
                if (c != expected)
                    throw new JsonParseException($"Expected '{expected}' but found '{c}'", Index - 1);
            }

            public JsonValue ParseValue()
            {
                SkipWhitespace();
                if (EOF) throw new JsonParseException("Expected a value", Index);

                char c = Peek();
                switch (c)
                {
                    case '{': return ParseObject();
                    case '[': return ParseArray();
                    case '"': return new JsonString(ParseString());
                    case 't': ReadLiteral("true"); return new JsonBool(true);
                    case 'f': ReadLiteral("false"); return new JsonBool(false);
                    case 'n': ReadLiteral("null"); return JsonNull.Instance;
                    default:
                        if (c == '-' || (c >= '0' && c <= '9'))
                            return ParseNumber();
                        throw new JsonParseException($"Unexpected character '{c}'", Index);
                }
            }

            private void ReadLiteral(string lit)
            {
                for (int i = 0; i < lit.Length; i++)
                {
                    if (EOF || _s[Index] != lit[i])
                        throw new JsonParseException($"Expected '{lit}'", Index);
                    Index++;
                }
            }

            private JsonObject ParseObject()
            {
                Expect('{');
                SkipWhitespace();

                var obj = new JsonObject();

                if (!EOF && Peek() == '}')
                {
                    Index++;
                    return obj;
                }

                while (true)
                {
                    SkipWhitespace();
                    if (EOF) throw new JsonParseException("Unterminated object", Index);

                    if (Peek() != '"')
                        throw new JsonParseException("Object keys must be strings", Index);

                    string key = ParseString();
                    SkipWhitespace();
                    Expect(':');

                    JsonValue val = ParseValue();
                    obj.Add(key, val);

                    SkipWhitespace();
                    char c = Read();
                    if (c == '}') break;
                    if (c != ',') throw new JsonParseException("Expected ',' or '}' in object", Index - 1);
                }

                return obj;
            }

            private JsonArray ParseArray()
            {
                Expect('[');
                SkipWhitespace();

                var arr = new JsonArray();

                if (!EOF && Peek() == ']')
                {
                    Index++;
                    return arr;
                }

                while (true)
                {
                    JsonValue v = ParseValue();
                    arr.Add(v);

                    SkipWhitespace();
                    char c = Read();
                    if (c == ']') break;
                    if (c != ',') throw new JsonParseException("Expected ',' or ']' in array", Index - 1);

                    SkipWhitespace();
                }

                return arr;
            }

            private string ParseString()
            {
                Expect('"');
                var sb = new StringBuilder();

                while (true)
                {
                    if (EOF) throw new JsonParseException("Unterminated string", Index);

                    char c = Read();
                    if (c == '"') break;

                    if (c == '\\')
                    {
                        if (EOF) throw new JsonParseException("Unterminated escape sequence", Index);
                        char e = Read();
                        switch (e)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                sb.Append(ParseUnicodeEscape());
                                break;
                            default:
                                throw new JsonParseException($"Invalid escape '\\{e}'", Index - 1);
                        }
                    }
                    else
                    {
                        // JSON forbids raw control chars in strings
                        if (c < 0x20)
                            throw new JsonParseException("Unescaped control character in string", Index - 1);
                        sb.Append(c);
                    }
                }

                return sb.ToString();
            }

            private char ParseUnicodeEscape()
            {
                // Reads 4 hex digits after \u
                int start = Index;
                if (Index + 4 > _s.Length)
                    throw new JsonParseException("Invalid unicode escape (too short)", Index);

                int code = 0;
                for (int i = 0; i < 4; i++)
                {
                    char c = _s[Index++];
                    int v =
                        (c >= '0' && c <= '9') ? (c - '0') :
                        (c >= 'a' && c <= 'f') ? (10 + (c - 'a')) :
                        (c >= 'A' && c <= 'F') ? (10 + (c - 'A')) :
                        -1;

                    if (v < 0)
                        throw new JsonParseException("Invalid hex digit in unicode escape", start + i);

                    code = (code << 4) | v;
                }

                // Handle surrogate pairs to properly decode characters outside BMP.
                if (code >= 0xD800 && code <= 0xDBFF)
                {
                    // must be followed by \uXXXX low surrogate
                    int save = Index;

                    if (Index + 6 <= _s.Length && _s[Index] == '\\' && _s[Index + 1] == 'u')
                    {
                        Index += 2;
                        int low = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            char c = _s[Index++];
                            int v =
                                (c >= '0' && c <= '9') ? (c - '0') :
                                (c >= 'a' && c <= 'f') ? (10 + (c - 'a')) :
                                (c >= 'A' && c <= 'F') ? (10 + (c - 'A')) :
                                -1;
                            if (v < 0) { Index = save; return (char)code; }
                            low = (low << 4) | v;
                        }

                        if (low >= 0xDC00 && low <= 0xDFFF)
                        {
                            //int full = 0x10000 + (((code - 0xD800) << 10) | (low - 0xDC00));

                            Index = save; // fallback
                        }
                        else
                        {
                            Index = save;
                        }
                    }
                    else
                    {
                        Index = save;
                    }
                }

                return (char)code;
            }

            private JsonValue ParseNumber()
            {
                int start = Index;

                // Optional '-'
                if (Peek() == '-') Index++;

                if (EOF) throw new JsonParseException("Invalid number", Index);

                // Leading digits
                if (Peek() == '0')
                {
                    Index++;
                }
                else
                {
                    if (!IsDigit(Peek()))
                        throw new JsonParseException("Invalid number", Index);

                    while (!EOF && IsDigit(Peek())) Index++;
                }

                // Fraction
                if (!EOF && Peek() == '.')
                {
                    Index++;
                    if (EOF || !IsDigit(Peek()))
                        throw new JsonParseException("Invalid fraction in number", Index);

                    while (!EOF && IsDigit(Peek())) Index++;
                }

                // Exponent
                if (!EOF)
                {
                    char c = Peek();
                    if (c == 'e' || c == 'E')
                    {
                        Index++;
                        if (!EOF && (Peek() == '+' || Peek() == '-')) Index++;
                        if (EOF || !IsDigit(Peek()))
                            throw new JsonParseException("Invalid exponent in number", Index);

                        while (!EOF && IsDigit(Peek())) Index++;
                    }
                }

                string raw = _s.Substring(start, Index - start);

                // Try parse as long if it looks like an integer (no '.' or exponent)
                bool looksInt = raw.IndexOf('.') < 0 && raw.IndexOf('e') < 0 && raw.IndexOf('E') < 0;
                if (looksInt && long.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long l))
                    return new JsonNumber(l, raw);

                if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) &&
                    !double.IsInfinity(d) && !double.IsNaN(d))
                    return new JsonNumber(d, raw);

                throw new JsonParseException("Invalid number", start);
            }

            private static bool IsDigit(char c) => c >= '0' && c <= '9';
        }
    }
}
