namespace MHRS_OtomatikRandevu.Urls
{
    public static class MHRSUrls
    {
        // Ana URL
        public const string BaseUrl = "https://prd.mhrs.gov.tr";

        // Giriş
        public const string Login = "/api/vatandas/login";

        // İl ve İlçe
        public const string GetProvinces = "/api/yonetim/genel/il/selectinput-tree";
        public const string GetDistricts = "/api/yonetim/genel/ilce/selectinput/{0}";

        // Klinik, Hastane, Muayene Yeri
        public const string GetClinics = "/api/kurum/kurum/kurum-klinik/il/{0}/ilce/{1}/kurum/-1/aksiyon/200/select-input";
        public const string GetHospitals = "/api/kurum/kurum/kurum-klinik/il/{0}/ilce/{1}/kurum/-1/klinik/{2}/ana-kurum/select-input";
        public const string GetPlaces = "/api/kurum/kurum/muayene-yeri/ana-kurum/{0}/kurum/-1/klinik/{1}/select-input";

        // Hekim
        public const string GetDoctors = "/api/kurum/hekim/hekim-klinik/hekim-select-input/anakurum/{0}/kurum/-1/klinik/{1}";

        // Randevu
        public const string GetSlots = "/api/kurum-rss/randevu/slot-sorgulama/slot";
        public const string MakeAppointment = "/api/kurum/randevu/randevu-ekle";
		public const string CancelAndRebookAppointment = "/api/kurum/randevu/randevu-iptal-et-yeni-al";
		
        // Ek Sabitler
        public const int RandevuAralikGunSayisi = 15;
        public const int BeklemeSuresiDakika = 1;
    }
}
