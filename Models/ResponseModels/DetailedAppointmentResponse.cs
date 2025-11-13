// Dosya Adı: Models/ResponseModels/DetailedAppointmentResponse.cs
#nullable enable
using System.Collections.Generic;

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class DetailedAppointmentResponse
    {
        public bool success { get; set; }
        public List<MhrsMessage> infos { get; set; } = new();
        public List<MhrsMessage> warnings { get; set; } = new();
        public List<MhrsMessage> errors { get; set; } = new();
    }
}
