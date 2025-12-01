using System.Text.Json;
using System.Text.Json.Serialization;
using Sharp.Shared.Types;

namespace Ptr.Shared.Json.Serialization;

public class JsonVectorConverter : JsonConverter<Vector>
{
    public override Vector Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Vector must be represented as a JSON array.");
        }

        var vector = new Vector();
        var index = 0;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (index >= 3)
            {
                // Skip any additional elements beyond the first 3
                reader.Skip();
                continue;
            }

            if (reader.TokenType != JsonTokenType.Number)
            {
                var axisName = index switch
                {
                    0 => "X",
                    1 => "Y",
                    2 => "Z",
                    _ => $"Index {index}"
                };
                throw new JsonException($"Vector.{axisName} must be a number.");
            }

            var value = reader.GetSingle();
            vector[index] = value;
            index++;
        }

        return vector;
    }

    public override void Write(Utf8JsonWriter writer, Vector value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.X);
        writer.WriteNumberValue(value.Y);
        writer.WriteNumberValue(value.Z);
        writer.WriteEndArray();
    }
}