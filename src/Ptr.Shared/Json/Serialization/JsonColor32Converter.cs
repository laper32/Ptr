using System.Text.Json;
using System.Text.Json.Serialization;
using Sharp.Shared.Types;

namespace Ptr.Shared.Json.Serialization;

public class JsonColor32Converter : JsonConverter<Color32>
{
    public override Color32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Color32 must be represented as a JSON array.");
        }

        var color = new Color32();
        var index = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (index >= 4)
            {
                // Skip any additional elements beyond the first 4
                reader.Skip();
                continue;
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                var componentName = index switch
                {
                    0 => "R",
                    1 => "G",
                    2 => "B",
                    3 => "A",
                    _ => $"Index {index}"
                };
                throw new JsonException($"Color32.{componentName} must be a number.");
            }

            var value = reader.GetInt32();
            if (value < 0 || value > 255)
            {
                var componentName = index switch
                {
                    0 => "R",
                    1 => "G",
                    2 => "B",
                    3 => "A",
                    _ => $"Index {index}"
                };
                throw new JsonException($"Color32.{componentName} must be between 0 and 255, got {value}.");
            }

            color[index] = (byte)value;
            index++;
        }

        return color;
    }

    public override void Write(Utf8JsonWriter writer, Color32 value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.R);
        writer.WriteNumberValue(value.G);
        writer.WriteNumberValue(value.B);
        writer.WriteNumberValue(value.A);
        writer.WriteEndArray();
    }
}