using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Tsonic.Runtime;

public static class JSON
{
    public static T parse<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T>(string text) where T : new()
    {
        using var doc = JsonDocument.Parse(text);
        var result = ConvertJsonElement(doc.RootElement);

        if (typeof(T) == typeof(object))
        {
            return (T)(object?)result!;
        }

        if (result is DynamicObject dynObj)
        {
            var tempDict = new Dictionary<string, object?>();
            foreach (var key in dynObj.GetKeys())
            {
                tempDict[key] = dynObj[key];
            }

            return Structural.CloneFromDictionary<T>(tempDict)!;
        }

        return (T)(object?)result!;
    }

    private static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Array => ConvertJsonArray(element),
            JsonValueKind.Object => ConvertJsonObject(element),
            _ => null,
        };
    }

    private static object ConvertJsonArray(JsonElement element)
    {
        var items = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            items.Add(ConvertJsonElement(item));
        }

        return items.ToArray();
    }

    private static object ConvertJsonObject(JsonElement element)
    {
        var obj = new DynamicObject();
        foreach (var prop in element.EnumerateObject())
        {
            obj[prop.Name] = ConvertJsonElement(prop.Value);
        }

        return obj;
    }

    public static string stringify(object? value)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        WriteValue(writer, value);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case long l:
                writer.WriteNumberValue(l);
                break;
            case uint ui:
                writer.WriteNumberValue(ui);
                break;
            case byte bt:
                writer.WriteNumberValue(bt);
                break;
            case short sh:
                writer.WriteNumberValue(sh);
                break;
            case DynamicObject dynObj:
                WriteDynamicObject(writer, dynObj);
                break;
            case IDictionary<string, object?> dict:
                WriteObject(writer, dict);
                break;
            case IEnumerable<object?> enumerable:
                WriteArray(writer, enumerable);
                break;
            default:
                var objDict = Structural.ToDictionary(value);
                WriteObject(writer, objDict);
                break;
        }
    }

    private static void WriteDynamicObject(Utf8JsonWriter writer, DynamicObject obj)
    {
        writer.WriteStartObject();
        foreach (var key in obj.GetKeys())
        {
            writer.WritePropertyName(key);
            WriteValue(writer, obj[key]);
        }
        writer.WriteEndObject();
    }

    private static void WriteObject(Utf8JsonWriter writer, IDictionary<string, object?> dict)
    {
        writer.WriteStartObject();
        foreach (var kvp in dict)
        {
            writer.WritePropertyName(kvp.Key);
            WriteValue(writer, kvp.Value);
        }
        writer.WriteEndObject();
    }

    private static void WriteArray(Utf8JsonWriter writer, IEnumerable enumerable)
    {
        writer.WriteStartArray();
        foreach (var item in enumerable)
        {
            WriteValue(writer, item);
        }
        writer.WriteEndArray();
    }
}
