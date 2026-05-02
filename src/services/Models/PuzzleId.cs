using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossWords.Services.Models;

/// <summary>
/// Strongly-typed identifier for a crossword puzzle. Wraps an unsigned integer value.
/// Implicitly convertible to and from <see cref="uint"/>.
/// </summary>
/// <param name="Value">The underlying unsigned integer identifier.</param>
[JsonConverter(typeof(PuzzleIdJsonConverter))]
[TypeConverter(typeof(PuzzleIdTypeConverter))]
public readonly record struct PuzzleId(uint Value) : IComparable<PuzzleId>
{
    /// <summary>Implicitly converts a <see cref="PuzzleId"/> to its underlying <see cref="uint"/> value.</summary>
    public static implicit operator uint(PuzzleId id) => id.Value;

    /// <summary>Implicitly converts a <see cref="uint"/> to a <see cref="PuzzleId"/>.</summary>
    public static implicit operator PuzzleId(uint value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString();

    /// <inheritdoc />
    public int CompareTo(PuzzleId other) => Value.CompareTo(other.Value);
}

/// <summary>
/// JSON converter for <see cref="PuzzleId"/>. Serializes as a number and deserializes from both numbers and numeric strings.
/// Also supports use as a JSON dictionary key.
/// </summary>
public class PuzzleIdJsonConverter : JsonConverter<PuzzleId>
{
    /// <inheritdoc />
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

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, PuzzleId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }

    /// <inheritdoc />
    public override PuzzleId ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (str != null && uint.TryParse(str, out var val))
            return new PuzzleId(val);
        throw new JsonException($"Cannot convert property name '{str}' to PuzzleId.");
    }

    /// <inheritdoc />
    public override void WriteAsPropertyName(Utf8JsonWriter writer, PuzzleId value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Value.ToString());
    }
}

/// <summary>
/// Type converter for <see cref="PuzzleId"/>. Enables ASP.NET Core model binding from route and query string parameters.
/// </summary>
public class PuzzleIdTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || sourceType == typeof(uint) || base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string str when uint.TryParse(str, out var uintVal) => new PuzzleId(uintVal),
            uint uintVal => new PuzzleId(uintVal),
            _ => base.ConvertFrom(context, culture, value)
        };
    }

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || destinationType == typeof(uint) || base.CanConvertTo(context, destinationType);
    }

    /// <inheritdoc />
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
