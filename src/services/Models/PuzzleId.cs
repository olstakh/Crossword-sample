using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossWords.Services.Models;

[JsonConverter(typeof(PuzzleIdJsonConverter))]
[TypeConverter(typeof(PuzzleIdTypeConverter))]
public readonly record struct PuzzleId(uint Value) : IComparable<PuzzleId>
{
    public static implicit operator uint(PuzzleId id) => id.Value;
    public static implicit operator PuzzleId(uint value) => new(value);

    public override string ToString() => Value.ToString();

    public int CompareTo(PuzzleId other) => Value.CompareTo(other.Value);
}

public class PuzzleIdJsonConverter : JsonConverter<PuzzleId>
{
    public override PuzzleId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return new PuzzleId(reader.GetUInt32());
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (str != null && uint.TryParse(str, out var val))
                return new PuzzleId(val);
        }

        throw new JsonException("Cannot convert value to PuzzleId. Expected a non-negative number.");
    }

    public override void Write(Utf8JsonWriter writer, PuzzleId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }

    public override PuzzleId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (str != null && uint.TryParse(str, out var val))
            return new PuzzleId(val);
        throw new JsonException($"Cannot convert property name '{str}' to PuzzleId.");
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, PuzzleId value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value.ToString());
    }
}

public class PuzzleIdTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(uint) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string str when uint.TryParse(str, out var uintVal) => new PuzzleId(uintVal),
            uint uintVal => new PuzzleId(uintVal),
            _ => base.ConvertFrom(context, culture, value)
        };
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || destinationType == typeof(uint) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is PuzzleId puzzleId)
        {
            if (destinationType == typeof(string))
                return puzzleId.ToString();
            if (destinationType == typeof(uint))
                return puzzleId.Value;
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
