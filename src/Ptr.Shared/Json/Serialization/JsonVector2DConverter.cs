using System.Text.Json;
using System.Text.Json.Serialization;
using Sharp.Shared.Types;

namespace Ptr.Shared.Json.Serialization;

public class JsonVector2DConverter : JsonConverter<Vector2D>
{
    public override Vector2D Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Vector2D must be represented as a JSON array.");
        }

        var vector = new Vector2D();
        var index = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (index >= 2)
            {
                // Skip any additional elements beyond the first 2
                reader.Skip();
                continue;
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                var axisName = index switch
                {
                    0 => "X",
                    1 => "Y",
                    _ => $"Index {index}"
                };
                throw new JsonException($"Vector2D.{axisName} must be a number.");
            }

            var value = reader.GetSingle();
            switch (index)
            {
                case 0:
                    vector.X = value;
                    break;
                case 1:
                    vector.Y = value;
                    break;
            }

            index++;
        }

        return vector;
    }

    public override void Write(Utf8JsonWriter writer, Vector2D value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteEndArray();
    }
}