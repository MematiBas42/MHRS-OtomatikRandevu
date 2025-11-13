using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.RequestModels
{
    public class LoggableLoginRequest
    {
        [JsonPropertyName("KullaniciAdi")]
        public string? KullaniciAdi { get; set; }

        [JsonPropertyName("Parola")]
        public string Parola { get; set; } = "**********";

        [JsonPropertyName("IslemKanali")]
        public string? IslemKanali { get; set; }

        [JsonPropertyName("GizlilikSozlesmeOnay")]
        public bool GizlilikSozlesmeOnay { get; set; }
    }
}
