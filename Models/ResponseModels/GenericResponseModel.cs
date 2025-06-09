using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class GenericResponseModel
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        // ProvinceResponseModel yerine GenericResponseModel olarak değiştirildi.
        [JsonPropertyName("children")]
        public List<GenericResponseModel>? Children { get; set; }
    }
}
