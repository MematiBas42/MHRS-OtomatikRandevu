// Dosya Yolu: Models/RequestModels/RandevuIptalEtYeniAlRequestModel.cs
#nullable enable
using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.RequestModels
{
    public class RandevuIptalEtYeniAlRequestModel
    {
        [JsonPropertyName("fkSlotId")]
        public long FkSlotId { get; set; }

        [JsonPropertyName("fkCetvelId")]
        public long FkCetvelId { get; set; }

        [JsonPropertyName("yenidogan")]
        public bool Yenidogan { get; set; } = false; // Genellikle false olacak

        [JsonPropertyName("muayeneYeriId")]
        public long MuayeneYeriId { get; set; } // SubSlot.MuayeneYeriId long olduğu için long yaptık

        [JsonPropertyName("baslangicZamani")]
        public string? BaslangicZamani { get; set; }

        [JsonPropertyName("bitisZamani")]
        public string? BitisZamani { get; set; }

        [JsonPropertyName("randevuNotu")]
        public string? RandevuNotu { get; set; } = ""; // JSON'da boş string olarak geldi
    }
}
