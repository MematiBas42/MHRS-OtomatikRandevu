using System.Collections.Generic;

namespace MHRS_OtomatikRandevu.Models
{
    public enum FilterMode
    {
        Include, // Sadece belirtilen saatleri al
        Exclude  // Belirtilen saatleri alma
    }

    public class DateFilterRule
    {
        public FilterMode Mode { get; set; }
        public HashSet<int> Hours { get; set; } = new HashSet<int>();
    }
}
