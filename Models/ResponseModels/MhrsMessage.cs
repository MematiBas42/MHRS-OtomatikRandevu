// Dosya Yolu: Models/ResponseModels/MhrsMessage.cs
#nullable enable

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class MhrsMessage
    {
        // API yanıtındaki "kodu" ve "mesaj" alanlarına karşılık gelir
        public string? kodu { get; set; }
        public string? mesaj { get; set; }
    }
}
