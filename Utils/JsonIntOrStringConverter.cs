using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Utils
{
    public class JsonIntOrStringConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (int.TryParse(reader.GetString(), out int intValue))
                {
                    return intValue;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }

            return -1;
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}