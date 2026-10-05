using System.Text.Json;
using System.Text.Json.Serialization;

namespace full.API.Infrastructure;

/// <summary>
/// Reads a JSON string or number into a string. Lets older clients that send phone
/// numbers as numeric values keep working after the field became a string.
/// </summary>
public sealed class StringOrNumberJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => System.Text.Encoding.UTF8.GetString(reader.ValueSpan),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Expected a string or number but found {reader.TokenType}.")
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
