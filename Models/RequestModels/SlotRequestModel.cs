using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace MHRS_OtomatikRandevu.Models.RequestModels
{
    public class SlotRequestModel
    {
        [JsonPropertyName("aksiyonId")]
        public int AksiyonId { get; set; } = 200;

        [JsonPropertyName("baslangicZamani")]
        public string? BaslangicZamani { get; set; }

        [JsonPropertyName("bitisZamani")]
        public string? BitisZamani { get; set; }

        [JsonPropertyName("cinsiyet")]
        public string Cinsiyet { get; set; } = "F"; // Varsayılan değerler konulabilir

        [JsonPropertyName("ekRandevu")]
        public bool EkRandevu { get; set; } = true;

        [JsonPropertyName("mhrsHekimId")]
        public long MhrsHekimId { get; set; }

        [JsonPropertyName("mhrsIlId")]
        public long MhrsIlId { get; set; }

        [JsonPropertyName("mhrsIlceId")]
        public long MhrsIlceId { get; set; }

        [JsonPropertyName("mhrsKlinikId")]
        public long MhrsKlinikId { get; set; }

        [JsonPropertyName("mhrsKurumId")]
        public long MhrsKurumId { get; set; }

        [JsonPropertyName("muayeneYeriId")]
        public long MuayeneYeriId { get; set; }

        [JsonPropertyName("randevuZamaniList")]
        public List<string> RandevuZamaniList { get; set; } = new();

        [JsonPropertyName("tumRandevular")]
        public bool TumRandevular { get; set; } = false;

        // --- EKLENEN YENİ ÖZELLİKLER ---
        [JsonPropertyName("arananDanismanHekimId")]
        public int ArananDanismanHekimId { get; set; } = -1;

        [JsonPropertyName("arananKurumId")]
        public int ArananKurumId { get; set; } = -1;

        [JsonPropertyName("enabizHatirlatma")]
        public bool EnabizHatirlatma { get; set; } = true;

        [JsonPropertyName("randevuUcretOdemeUyariDurumu")]
        public int RandevuUcretOdemeUyariDurumu { get; set; } = 0;

        [JsonPropertyName("islemKanali")]
        public string IslemKanali { get; set; } = "VATANDAS_WEB";

        [JsonPropertyName("kcButonGoruntulemeDurumu")]
        public bool KcButonGoruntulemeDurumu { get; set; } = true;

        [JsonPropertyName("randevuUcretBilgisiVar")]
        public bool RandevuUcretBilgisiVar { get; set; } = false;

        [JsonPropertyName("randevuOncelikDurumu")]
        public int RandevuOncelikDurumu { get; set; } = 0;

        [JsonPropertyName("mhrsCagriMerkeziIslem")]
        public bool MhrsCagriMerkeziIslem { get; set; } = false;

        [JsonPropertyName("aramaBaglantiliRandevuIslemi")]
        public bool AramaBaglantiliRandevuIslemi { get; set; } = false;

        [JsonPropertyName("yesilListeGrupId")]
        public int YesilListeGrupId { get; set; } = -1;

        [JsonPropertyName("oncelikliGrupSeriNo")]
        public string? OncelikliGrupSeriNo { get; set; }

        [JsonPropertyName("randevuKaynagi")]
        public int RandevuKaynagi { get; set; } = 0;

        [JsonPropertyName("aramaId")]
        public string? AramaId { get; set; }

        [JsonPropertyName("istisnaTuru")]
        public int IstisnaTuru { get; set; } = -1;
    }
}
