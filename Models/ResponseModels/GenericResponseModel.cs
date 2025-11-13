using MHRS_OtomatikRandevu.Utils;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class GenericResponseModel
    {
        [JsonPropertyName("value")]
        [JsonConverter(typeof(JsonIntOrStringConverter))]
        public int Value { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;

        [JsonPropertyName("children")]
        public List<GenericResponseModel>? Children { get; set; }
    }
}
