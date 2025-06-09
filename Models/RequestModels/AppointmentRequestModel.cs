using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.RequestModels
{
    public class AppointmentRequestModel
    {
        [JsonPropertyName("fkSlotId")]
        public long FkSlotId { get; set; }

        [JsonPropertyName("fkCetvelId")]
        public long FkCetvelId { get; set; }

        [JsonPropertyName("yenidogan")]
        public bool Yenidogan { get; set; } = false;

        [JsonPropertyName("muayeneYeriId")]
        public long MuayeneYeriId { get; set; }

        [JsonPropertyName("baslangicZamani")]
        public string BaslangicZamani { get; set; }

        [JsonPropertyName("bitisZamani")]
        public string BitisZamani { get; set; }

        [JsonPropertyName("randevuNotu")]
        public string? RandevuNotu { get; set; } = "";

        // --- EKLENEN YENİ ÖZELLİKLER ---
        [JsonPropertyName("ekSureDurumu")]
        public int EkSureDurumu { get; set; } = 0;

        [JsonPropertyName("randevuKaynagi")]
        public int RandevuKaynagi { get; set; } = 0;

        [JsonPropertyName("fkDuyuruId")]
        public long? FkDuyuruId { get; set; }

        [JsonPropertyName("aciklama")]
        public string? Aciklama { get; set; }

        [JsonPropertyName("tumRandevular")]
        public bool TumRandevular { get; set; } = false;

        [JsonPropertyName("oncelikliGrupSeriNo")]
        public string? OncelikliGrupSeriNo { get; set; }

        [JsonPropertyName("mhrsCagriMerkeziIslem")]
        public bool MhrsCagriMerkeziIslem { get; set; } = false;

        [JsonPropertyName("randevuUcretOdemeUyariDurumu")]
        public int RandevuUcretOdemeUyariDurumu { get; set; } = 0;

        [JsonPropertyName("yesilListeGrupId")]
        public int YesilListeGrupId { get; set; } = -1;

        [JsonPropertyName("islemKanali")]
        public string IslemKanali { get; set; } = "VATANDAS_WEB";
    }
}
