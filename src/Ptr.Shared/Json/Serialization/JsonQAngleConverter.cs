using System.Text.Json;
using System.Text.Json.Serialization;
using Ptr.Shared.Types;

namespace Ptr.Shared.Json.Serialization;

public class JsonQAngleConverter : JsonConverter<QAngle>
{
    public override QAngle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("QAngle must be represented as a JSON array.");
        }

        var angle = new QAngle();
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
                    0 => "Pitch",
                    1 => "Yaw",
                    2 => "Roll",
                    _ => $"Index {index}"
                };
                throw new JsonException($"QAngle.{axisName} must be a number.");
            }

            var value = reader.GetSingle();
            angle[index] = value;
            index++;
        }

        return angle;
    }

    public override void Write(Utf8JsonWriter writer, QAngle value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Pitch);
        writer.WriteNumberValue(value.Yaw);
        writer.WriteNumberValue(value.Roll);
        writer.WriteEndArray();
    }
}