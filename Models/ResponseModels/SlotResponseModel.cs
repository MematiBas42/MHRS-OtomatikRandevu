// Models/ResponseModels/SlotResponseModel.cs

using System.Collections.Generic; // SlotResponseModel, DoctorSlot vb. List kullandığı için bu da gerekebilir.
using System.Text.Json.Serialization;

namespace MHRS_OtomatikRandevu.Models.ResponseModels // Namespace tanımı
{ // <<< EKSİK OLAN AÇMA KÜME PARANTEZİ

    public class SlotResponseModel
    {
        [JsonPropertyName("hekimSlotList")]
        public List<DoctorSlot>? HekimSlotList { get; set; }

        [JsonPropertyName("gun")]
        public string? Gun { get; set; }

        [JsonPropertyName("bos")]
        public bool Bos { get; set; }
    }

    public class DoctorSlot
    {
        [JsonPropertyName("muayeneYeriSlotList")]
        public List<PlaceSlot>? MuayeneYeriSlotList { get; set; }

        [JsonPropertyName("hekim")]
        public HekimDetay? Hekim { get; set; }
        
        [JsonPropertyName("kurum")]
        public KurumDetay? Kurum { get; set; }

        [JsonPropertyName("klinik")]
        public KlinikDetay? Klinik { get; set; }

        [JsonPropertyName("aksiyon")]
        public AksiyonDetay? Aksiyon { get; set; }

        [JsonPropertyName("bos")]
        public bool Bos { get; set; }
        
        [JsonIgnore]
        public int HekimId => Hekim?.MhrsHekimId ?? 0;

        [JsonIgnore]
        public string HekimAdi => Hekim != null ? $"{Hekim.Ad} {Hekim.Soyad}".Trim() : "Bilinmeyen Hekim";
    }

    public class HekimDetay
    {
        [JsonPropertyName("mhrsHekimId")]
        public int MhrsHekimId { get; set; }

        [JsonPropertyName("ad")]
        public string? Ad { get; set; }

        [JsonPropertyName("soyad")]
        public string? Soyad { get; set; }
    }
    
    public class KurumDetay
    {
        [JsonPropertyName("mhrsKurumId")]
        public int MhrsKurumId { get; set; }
        [JsonPropertyName("kurumAdi")]
        public string? KurumAdi { get; set; }
    }

    public class KlinikDetay
    {
        [JsonPropertyName("mhrsKlinikId")]
        public int MhrsKlinikId { get; set; }
        [JsonPropertyName("mhrsKlinikAdi")]
        public string? MhrsKlinikAdi { get; set; }
    }
    public class AksiyonDetay
    {
        [JsonPropertyName("id")]
        public int Id {get; set;}
        [JsonPropertyName("aksiyonAdi")]
        public string? AksiyonAdi {get; set;}
    }

    public class PlaceSlot
    {
        [JsonPropertyName("saatSlotList")]
        public List<DateSlot>? SaatSlotList { get; set; }

        [JsonPropertyName("muayeneYeri")]
        public MuayeneYeriDetay? MuayeneYeri { get; set; }

        [JsonPropertyName("bos")]
        public bool Bos { get; set; }

        [JsonIgnore]
        public string MuayeneYeriAdi => MuayeneYeri?.Adi ?? "Bilinmeyen Muayene Yeri";
        
        [JsonIgnore]
        public int MuayeneYeriId => MuayeneYeri?.Id ?? 0; 
        
        // PlaceSlot içinde doğrudan kurumId yoksa ve DoctorSlot seviyesinden alınacaksa bu özellik burada olmamalı.
        // Eğer API yanıtında PlaceSlot içinde kurumId varsa bu kalabilir.
        // Son API loguna göre PlaceSlot içinde yoktu, DoctorSlot->Kurum içindeydi.
        // Şimdilik yoruma alıyorum, GetAllPotentialSlots'ta SubSlot.MhrsKurumId kullanılacak.
        // [JsonPropertyName("kurumId")]
        // public int KurumId { get; set; } 
    }
    
    public class MuayeneYeriDetay
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("adi")]
        public string? Adi { get; set; }
    }

    public class DateSlot
    {
        [JsonPropertyName("slotList")]
        public List<Slot>? SlotList { get; set; }

        [JsonPropertyName("bos")]
        public bool Bos { get; set; }

        [JsonPropertyName("saat")]
        public string? Saat { get; set; }
    }

    public class Slot
    {
        [JsonPropertyName("slot")]
        public SubSlot? SubSlot { get; set; }

        [JsonPropertyName("bos")]
        public bool Bos { get; set; }
    }

    // Kullanıcının en son gönderdiği SubSlot tanımı:
    public class SubSlot
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("fkCetvelId")]
        public long FkCetvelId { get; set; }

        [JsonPropertyName("muayeneYeriId")] 
        public long MuayeneYeriId { get; set; } // Program.cs'de (int)'e cast edilecek.

        [JsonPropertyName("baslangicZamani")]
        public string? BaslangicZamani { get; set; } 

        [JsonPropertyName("bitisZamani")]
        public string? BitisZamani { get; set; } 

        [JsonPropertyName("kalanKullanim")]
        public int KalanKullanim { get; set; }

        [JsonPropertyName("mhrsHekimId")]
        public int MhrsHekimId { get; set; }

        [JsonPropertyName("mhrsKurumId")]
        public int MhrsKurumId { get; set; }
        
        [JsonIgnore]
        public string? HekimAdi { get; set; } 

        [JsonIgnore]
        public string? MuayeneYeriAdi { get; set; }
        
        [JsonIgnore]
        public string? KurumAdi { get; set; } // Hastane adını tutmak için
    }

} // <<< EKSİK OLAN KAPAMA KÜME PARANTEZİ
