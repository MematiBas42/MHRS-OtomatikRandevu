using MHRS_OtomatikRandevu.Utils;
using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class ClinicResponseModel
    {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(JsonIntOrStringConverter))]
        public int Value { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;

        [JsonPropertyName("parent")]
        public int? Parent { get; set; }

        [JsonPropertyName("favori")]
        public bool Favori { get; set; }
    }
}
