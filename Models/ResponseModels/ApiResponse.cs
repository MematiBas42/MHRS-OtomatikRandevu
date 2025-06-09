// Dosya Yolu: Models/ResponseModels/ApiResponse.cs
#nullable enable
using System.Collections.Generic;
using System.Text.Json.Serialization; // JsonPropertyName için

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class ApiResponse<T> where T : class
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        // DÜZELTME: Bu listeler artık MhrsMessage tipinde olacak
        [JsonPropertyName("infos")]
        public List<MhrsMessage> Infos { get; set; } = new();

        [JsonPropertyName("warnings")]
        public List<MhrsMessage> Warnings { get; set; } = new();

        [JsonPropertyName("errors")]
        public List<MhrsMessage> Errors { get; set; } = new();
        
        [JsonPropertyName("lang")] // Loglarda bu alan da görünüyordu
        public string? Lang { get; set; }

        // Eğer API bazen bu alanlar için basit string listesi döndürüyorsa
        // (List<string>), o zaman daha karmaşık bir çözüm (örn: özel JsonConverter)
        // gerekebilir. Ancak loglarınız genellikle MhrsMessage yapısına uygun görünüyor.
    }
}
