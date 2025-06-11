using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.RequestModels
{
    public class LoginRequestModel
    {
        [JsonPropertyName("kullaniciAdi")]
        public string KullaniciAdi { get; set; } = default!; // DÜZELTME

        [JsonPropertyName("parola")]
        public string Parola { get; set; } = default!; // DÜZELTME

        [JsonPropertyName("islemKanali")]
        public string IslemKanali { get; set; } = "VATANDAS_WEB";

        [JsonPropertyName("girisTipi")]
        public string GirisTipi { get; set; } = "PAROLA";

        [JsonPropertyName("gizlilikSozlesmeOnay")]
        public bool GizlilikSozlesmeOnay { get; set; } = true;
    }
}
