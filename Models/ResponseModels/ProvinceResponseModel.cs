using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class ProvinceResponseModel
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; } = default!; // DÜZELTME

        [JsonPropertyName("children")]
        public List<ProvinceResponseModel> Children { get; set; } = new(); // DÜZELTME
    }
}
