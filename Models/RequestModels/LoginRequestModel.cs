using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.RequestModels
{
    public class LoginRequestModel
    {
        [JsonPropertyName("kullaniciAdi")]
        public string KullaniciAdi { get; set; }

        [JsonPropertyName("parola")]
        public string Parola { get; set; }

        [JsonPropertyName("islemKanali")]
        public string IslemKanali { get; set; } = "VATANDAS_WEB"; // Artık { get; set; }

        [JsonPropertyName("girisTipi")]
        public string GirisTipi { get; set; } = "PAROLA"; // Artık { get; set; }

        [JsonPropertyName("gizlilikSozlesmeOnay")]
        public bool GizlilikSozlesmeOnay { get; set; } = true; // EKLENDİ
    }
}
