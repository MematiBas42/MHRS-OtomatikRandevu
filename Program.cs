// GÜNCELLEME: Log ve Token dosyaları TCKN'ye özel olarak çalışma dizininde oluşturulur.
// YENİ GÜNCELLEME: Randevu aramasına saat bazında filtreleme eklendi.
// YENİ GÜNCELLEME (KULLANICI İSTEKLERİ): Token hatasında yeniden başlatma yerine token yenileme ve Randevu Bulundu ekranı/mantığı iyileştirildi.
// DÜZELTME: Derleme hatalarına yol açan ASCII Art string'i onarıldı ve Telegram bildirim çıktısı eklendi.
// DÜZELTME 2: NotificationService çağrısı, sağlanan NotificationService.cs dosyasıyla uyumlu hale getirildi.
// DÜZELTME 3: async void sorunu giderildi, programın bildirim göndermeden kapanması engellendi.
// DÜZELTME 4: CS0029 'void' to 'bool' hatası giderildi.
// DÜZELTME 5: LGN2001 ve GNL2029 hata kodları için özel kullanıcı uyarıları eklendi.
// DÜZELTME 6: CS1501 'WriteText' metodu için tekrar yükleme hatası giderildi.
// YENİ DÜZELTME (10.06.2024): Randevu bulundu ekranındaki çift çıktı sorunu giderildi.
// YENİ ÖZELLİK (10.06.2024): Güne özel saat filtresi ve "çok yakın" randevuları engelleme eklendi.
// YENİ ÖZELLİK (11.06.2025): Gelişmiş, tarihe göre saat filtreleme mekanizması eklendi.
// GÜNCELLEME (11.06.2025): Gelişmiş filtreye genel (varsayılan) kural desteği eklendi.
// DERLEME DÜZELTMELERİ (11.06.2025): Logger.Warn hatası ve tüm nullable referans uyarıları giderildi.
using MHRS_OtomatikRandevu.Models;
using MHRS_OtomatikRandevu.Models.RequestModels;
using MHRS_OtomatikRandevu.Models.ResponseModels;
using MHRS_OtomatikRandevu.Services;
using MHRS_OtomatikRandevu.Services.Abstracts;
using MHRS_OtomatikRandevu.Urls;
using MHRS_OtomatikRandevu.Utils;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Configuration; // app.config okumak için eklendi
using System.Threading;
using System.Globalization;

namespace MHRS_OtomatikRandevu
{
    public class SearchCombination
    {
        public int ProvinceId { get; set; } = -1;
        public string ProvinceText { get; set; } = "FARKETMEZ";
        public int DistrictId { get; set; } = -1;
        public string DistrictText { get; set; } = "FARKETMEZ";
        public int ClinicId { get; set; } = -1;
        public string ClinicText { get; set; } = "FARKETMEZ";

        public int AnaKurumId { get; set; } = -1;
        public string AnaKurumText { get; set; } = "FARKETMEZ";
        public int HospitalId { get; set; } = -1;
        public string HospitalText { get; set; } = "FARKETMEZ";

        public int PlaceId { get; set; } = -1;
        public string PlaceText { get; set; } = "FARKETMEZ";
        public int DoctorId { get; set; } = -1;
        public string DoctorText { get; set; } = "FARKETMEZ";

        public SearchCombination ShallowCopy() => (SearchCombination)this.MemberwiseClone();

        public override string ToString()
        {
            return $"İl: {ProvinceText}, İlçe: {DistrictText}, Klinik: {ClinicText}, " +
                   $"Hastane: {HospitalText}, Muayene Yeri: {PlaceText}, Doktor: {DoctorText}";
        }
    }

    public class SlotResult
    {
        public List<SubSlot> Slots { get; set; } = new List<SubSlot>();
        public bool SessionEnded { get; set; } = false;
    }

    public enum AppointmentAttemptResult
    {
        Success,
        Failed,
        StopSearching
    }


    public class Program
    {
        static string? TC_NO;
        static string? SIFRE;

        static string? JWT_TOKEN;
        static DateTime TOKEN_END_DATE;

        static IClientService _client = null!;
        static INotificationService _notificationService = null!;
        static int minimumMinutesToAppointment = 0; // Çok yakın randevuları engellemek için

        static List<int> GetMultipleSelections(List<GenericResponseModel>? options, string prompt, string entityName, bool allowFarketmez = true, string parentInfo = "")
        {
            List<int> selectedIds = new List<int>();
            string? inputString;
            bool validInput = false;

            if (options == null || !options.Any())
            {
                string message = $"{parentInfo}{entityName} için gösterilecek seçenek bulunamadı.";
                Logger.WriteLineAndLog(message);
                Thread.Sleep(1500);
                if (allowFarketmez)
                {
                    Logger.WriteLineAndLog($"Otomatik olarak '{entityName} - FARKETMEZ' seçiliyor.");
                    Thread.Sleep(1500);
                    return new List<int> { -1 };
                }
                else
                {
                    Logger.WriteLineAndLog($"Bu adımda bir seçim yapılması zorunludur ve seçenek bulunamadı. Bu arama dalı için '{entityName}' adımı atlanacak.");
                    Thread.Sleep(2000);
                    return new List<int>();
                }
            }

            do
            {
                Console.Clear();
                if (!string.IsNullOrEmpty(parentInfo)) Console.WriteLine(parentInfo);
                Console.WriteLine($"--- {entityName} Seçimi ---");
                Console.WriteLine("-------------------------------------------");
                if (allowFarketmez) Console.WriteLine("0-FARKETMEZ");
                for (int i = 0; i < options.Count; i++) Console.WriteLine($"{i + 1}-{options[i].Text}");
                Console.WriteLine("-------------------------------------------");

                inputString = Logger.ReadLineAndLog($"{prompt} (örn: 1 veya 1,2,3){(allowFarketmez ? $".\n'FARKETMEZ' için sadece 0 giriniz" : "")}: ");
                if (inputString == null) { HandleExit(false); return new List<int>(); }

                selectedIds.Clear();

                if (string.IsNullOrWhiteSpace(inputString))
                {
                    ConsoleUtil.WriteText("Giriş boş olamaz. Lütfen geçerli bir numara girin.", 1500);
                    Thread.Sleep(1500);
                    continue;
                }

                string trimmedInput = inputString.Trim();

                if (allowFarketmez && trimmedInput == "0")
                {
                    selectedIds.Add(-1);
                    validInput = true;
                }
                else
                {
                    var parts = trimmedInput.Split(',');
                    bool allPartsValidThisAttempt = true;

                    if (allowFarketmez && parts.Contains("0") && parts.Length > 1)
                    {
                        ConsoleUtil.WriteText("'0' (FARKETMEZ) seçeneği, diğer numaralarla birlikte kullanılamaz. Lütfen sadece '0' girin veya diğer numaraları seçin.", 2500);
                        allPartsValidThisAttempt = false;
                        Thread.Sleep(2500);
                    }
                    else
                    {
                        foreach (var part in parts)
                        {
                            string currentPartTrimmed = part.Trim();
                            if (int.TryParse(currentPartTrimmed, out int choice))
                            {
                                if (allowFarketmez && choice == 0)
                                {
                                    ConsoleUtil.WriteText($"'{currentPartTrimmed}' girişi bu bağlamda geçersiz. '0' sadece FARKETMEZ için tek başına girilebilir.", 2500);
                                    allPartsValidThisAttempt = false;
                                    Thread.Sleep(2500);
                                    break;
                                }

                                if (choice > 0 && choice <= options.Count)
                                {
                                    int selectedValue = options[choice - 1].Value;
                                    if (!selectedIds.Contains(selectedValue))
                                    {
                                        selectedIds.Add(selectedValue);
                                    }
                                }
                                else
                                {
                                    ConsoleUtil.WriteText($"'{currentPartTrimmed}' {entityName} için geçersiz bir sıra numarası. Lütfen listedeki (1-{options.Count}) aralığından seçim yapın.", 2500);
                                    allPartsValidThisAttempt = false;
                                    Thread.Sleep(2500);
                                    break;
                                }
                            }
                            else
                            {
                                ConsoleUtil.WriteText($"'{currentPartTrimmed}' geçerli bir sayı değil. Lütfen sayısal değerler girin.", 2500);
                                allPartsValidThisAttempt = false;
                                Thread.Sleep(2500);
                                break;
                            }
                        }
                    }

                    if (allPartsValidThisAttempt && selectedIds.Any())
                    {
                        validInput = true;
                    }
                    else if (allPartsValidThisAttempt && !selectedIds.Any() && !(allowFarketmez && trimmedInput == "0"))
                    {
                        ConsoleUtil.WriteText($"Geçerli bir {entityName} seçimi yapılmadı veya tüm girişler zaten seçilmişti. Lütfen tekrar deneyin.", 2000);
                    }
                }
            } while (!validInput);
            return selectedIds;
        }

        static List<int> GetMultipleSelectionsForPlaces(List<ClinicResponseModel>? options, string prompt, string entityName, bool allowFarketmez = true, string parentInfo = "")
        {
            var genericOptions = options?.Select(c => new GenericResponseModel { Value = c.Value, Text = c.Text }).ToList();
            return GetMultipleSelections(genericOptions, prompt, entityName, allowFarketmez, parentInfo);
        }
        
        static bool PassesDateBasedFilter(DateTime slotDateTime, Dictionary<DateTime, DateFilterRule> dateSpecificRules, DateFilterRule? globalRule)
        {
            var slotDate = slotDateTime.Date;
            
            if (dateSpecificRules.TryGetValue(slotDate, out var specificRule))
            {
                int slotHour = slotDateTime.Hour;
                if (specificRule.Mode == FilterMode.Include)
                {
                    return specificRule.Hours.Contains(slotHour);
                }
                else 
                {
                    return !specificRule.Hours.Contains(slotHour);
                }
            }
            else if (globalRule != null)
            {
                int slotHour = slotDateTime.Hour;
                if (globalRule.Mode == FilterMode.Include)
                {
                    return globalRule.Hours.Contains(slotHour);
                }
                else 
                {
                    return !globalRule.Hours.Contains(slotHour);
                }
            }
            
            return true;
        }


        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (sender, e) => {
                e.Cancel = true;
                HandleExit(false);
            };

            Logger.Info("================ UYGULAMA BAŞLATILDI ================");
            _client = new ClientService();
            _notificationService = new NotificationService();

            try
            {
                minimumMinutesToAppointment = int.Parse(ConfigurationManager.AppSettings["MinimumMinutesToAppointment"] ?? "0");
            }
            catch { minimumMinutesToAppointment = 0; }


            Logger.WriteLineAndLog("MHRS Otomatik Randevu uygulaması başlatıldı.");
            
            LoginFlow:

            #region Giriş Yap Bölümü
            while (string.IsNullOrEmpty(JWT_TOKEN))
            {
                Console.Clear();
                if (string.IsNullOrEmpty(TC_NO) || string.IsNullOrEmpty(SIFRE))
                {
                    Console.WriteLine("MHRS Otomatik Randevu Sistemine Hoşgeldiniz.\nLütfen giriş yapmak için bilgilerinizi giriniz.");
                    TC_NO = Logger.ReadLineAndLog("TC: ");
                    if (TC_NO == null) { HandleExit(false); return; }

                    if (!string.IsNullOrEmpty(TC_NO))
                    {
                        Logger.Initialize(TC_NO);
                    }

                    SIFRE = Logger.ReadLineAndLog("Şifre: ", isPassword: true);
                    if (SIFRE == null) { HandleExit(false); return; }
                }

                if (string.IsNullOrWhiteSpace(TC_NO) || string.IsNullOrWhiteSpace(SIFRE))
                {
                    ConsoleUtil.WriteText("TC Kimlik Numarası ve Şifre boş olamaz.", 1500);
                    TC_NO = null; SIFRE = null; 
                    Thread.Sleep(1500); continue;
                }

                Logger.WriteLineAndLog("Giriş Yapılıyor...");
                var tokenData = await GetToken(_client);
                if (tokenData == null || string.IsNullOrEmpty(tokenData.Token))
                {
                    Logger.Error("Giriş başarısız oldu. Bilgiler yeniden isteniyor.");
                    TC_NO = null; SIFRE = null; 
                    Thread.Sleep(3000);
                    continue;
                }
                JWT_TOKEN = tokenData.Token; TOKEN_END_DATE = tokenData.Expiration;
                Logger.Info($"Token başarıyla alındı. Geçerlilik sonu: {TOKEN_END_DATE}");
                _client.AddOrUpdateAuthorizationHeader(JWT_TOKEN);
            }
            #endregion

            List<SearchCombination> searchCombinations = new List<SearchCombination> { new SearchCombination() };
            List<SearchCombination> nextTierCombinations;

            #region İl Seçim Bölümü
            Logger.Info("İl Seçim Bölümü Başladı.");
            Logger.WriteLineAndLog("İl Seçimi Yapılıyor...");
            List<GenericResponseModel>? provinceListResponse = null;
            int provinceRetryCount = 0;

            while (provinceListResponse == null && provinceRetryCount < 2)
            {
                Logger.Info($"API Request (GET) to {MHRSUrls.GetProvinces}");
                provinceListResponse = await _client.GetSimple<List<GenericResponseModel>>(MHRSUrls.BaseUrl, MHRSUrls.GetProvinces);
                Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, provinceListResponse, $"Response from {MHRSUrls.GetProvinces}");
                
                if (provinceListResponse == null || !provinceListResponse.Any())
                {
                    provinceRetryCount++;
                    Logger.Error($"İl listesi alınamadı (Deneme: {provinceRetryCount}). Token geçersiz olabilir. Token yenilenip tekrar denenecek.");
                    ConsoleUtil.WriteText("Token geçersiz, yenileniyor...", 2000);

                    _client = new ClientService();
                    Logger.Info("ClientService (HttpClient ve Çerezler) sıfırlandı.");

                    var tokenData = await GetToken(_client, forceRefresh: true);
                    if (tokenData == null || string.IsNullOrEmpty(tokenData.Token))
                    {
                        Logger.Error("Otomatik token yenileme başarısız. Giriş ekranına yönlendiriliyor.");
                        ConsoleUtil.WriteText("Token otomatik olarak yenilenemedi, lütfen bilgilerinizi yeniden girin.", 3000);
                        JWT_TOKEN = null;
                        TC_NO = null;
                        SIFRE = null;
                        Thread.Sleep(2000);
                        Console.Clear();
                        goto LoginFlow;
                    }
                    JWT_TOKEN = tokenData.Token;
                    TOKEN_END_DATE = tokenData.Expiration;
                    _client.AddOrUpdateAuthorizationHeader(JWT_TOKEN);
                    Logger.Info("Token başarıyla yenilendi. İl listesi tekrar isteniyor.");
                    provinceListResponse = null; 
                }
            }

            if (provinceListResponse == null || !provinceListResponse.Any()) 
            {
                Logger.Error("Tüm denemelere rağmen il listesi alınamadı. Program sonlandırılıyor.");
                ConsoleUtil.WriteText("Kritik bir hata oluştu, il listesi alınamıyor. Program kapatılıyor.", 3000);
                HandleExit(true); 
                return;
            }

            var allProvinces = provinceListResponse.DistinctBy(p => p.Value).OrderBy(p => p.Value).ToList();
            List<int> selectedProvinceValues = GetMultipleSelections(allProvinces, "İl Numaralarını Seçiniz", "İl", allowFarketmez: false);

            if (!selectedProvinceValues.Any()) { ConsoleUtil.WriteText("Geçerli bir il seçimi yapılmadı!", 2000); HandleExit(false); return; }

            nextTierCombinations = new List<SearchCombination>();
            foreach (int provValue in selectedProvinceValues)
            {
                var selectedProv = allProvinces.FirstOrDefault(p => p.Value == provValue);
                if (selectedProv == null) continue;

                if (provValue == 34)
                {
                    var istanbulOptions = new List<GenericResponseModel> {
                        new GenericResponseModel { Value = 341, Text = "İSTANBUL (AVRUPA)" },
                        new GenericResponseModel { Value = 342, Text = "İSTANBUL (ANADOLU)" }
                    };
                    List<int> istanbulSubSelections = GetMultipleSelections(istanbulOptions, "İstanbul için alt bölge seçiniz", "İstanbul Alt Bölgesi", false, selectedProv.Text + " için ");

                    if (!istanbulSubSelections.Any()) continue;

                    foreach (int istanbulSubValue in istanbulSubSelections)
                    {
                        var subOpt = istanbulOptions.FirstOrDefault(s => s.Value == istanbulSubValue);
                        if (subOpt == null) continue;
                        nextTierCombinations.Add(new SearchCombination { ProvinceId = istanbulSubValue, ProvinceText = subOpt.Text });
                    }
                }
                else
                {
                    nextTierCombinations.Add(new SearchCombination { ProvinceId = provValue, ProvinceText = selectedProv.Text });
                }
            }
            searchCombinations = nextTierCombinations;
            if (!searchCombinations.Any()) { ConsoleUtil.WriteText("İl seçimi sonucu geçerli kombinasyon oluşturulamadı.", 2000); HandleExit(false); return; }
            #endregion

            #region İlçe Seçim Bölümü
            nextTierCombinations = new List<SearchCombination>();
            foreach (var combo in searchCombinations)
            {
                string districtUrl = string.Format(MHRSUrls.GetDistricts, combo.ProvinceId);
                Logger.Info($"API Request (GET) to {districtUrl}");
                var districtList = await _client.GetSimple<List<GenericResponseModel>>(MHRSUrls.BaseUrl, districtUrl);
                Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, districtList, $"Response from {districtUrl}");

                List<int> selectedDistrictValues = GetMultipleSelections(districtList, "İlçe Numaralarını Seçiniz", "İlçe", true, combo.ProvinceText + " için ");
                if (!selectedDistrictValues.Any()) { continue; }
                foreach (int distValue in selectedDistrictValues)
                {
                    var newCombo = combo.ShallowCopy();
                    newCombo.DistrictId = distValue;
                    newCombo.DistrictText = distValue == -1 ? "FARKETMEZ" : districtList?.FirstOrDefault(d => d.Value == distValue)?.Text ?? "Bilinmeyen İlçe";
                    nextTierCombinations.Add(newCombo);
                }
            }
            searchCombinations = nextTierCombinations;
            if (!searchCombinations.Any()) { ConsoleUtil.WriteText("İlçe seçimi kombinasyonları oluşturulamadı.", 2000); HandleExit(false); return; }
            #endregion

            #region Klinik Seçim Bölümü
            nextTierCombinations = new List<SearchCombination>();
            foreach (var combo in searchCombinations)
            {
                string clinicUrl = string.Format(MHRSUrls.GetClinics, combo.ProvinceId, combo.DistrictId);
                Logger.Info($"API Request (GET) to {clinicUrl}");
                var clinicListResponse = await _client.Get<List<GenericResponseModel>>(MHRSUrls.BaseUrl, clinicUrl);

                if (clinicListResponse?.Success == true)
                {
                    Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, clinicListResponse, $"Response from {clinicUrl}");
                }

                if (clinicListResponse == null || !clinicListResponse.Success || clinicListResponse.Data == null || !clinicListResponse.Data.Any())
                {
                    Logger.Error($"Klinik listesi alınamadı: İl={combo.ProvinceText}, İlçe={combo.DistrictText}.");
                    Logger.LogObject(LogLevel.API_RESPONSE_FAIL, clinicListResponse, "Klinik Listesi Yanıtı (Başarısız)");
                    continue;
                }
                List<int> selectedClinicValues = GetMultipleSelections(clinicListResponse.Data, "Klinik Numaralarını Seçiniz", "Klinik", false, $"{combo.ProvinceText} - {combo.DistrictText} için ");
                if (!selectedClinicValues.Any()) { continue; }
                foreach (int clinicValue in selectedClinicValues)
                {
                    var selectedClinic = clinicListResponse.Data.FirstOrDefault(c => c.Value == clinicValue);
                    if (selectedClinic == null) continue;
                    var newCombo = combo.ShallowCopy();
                    newCombo.ClinicId = clinicValue;
                    newCombo.ClinicText = selectedClinic.Text;
                    nextTierCombinations.Add(newCombo);
                }
            }
            searchCombinations = nextTierCombinations;
            if (!searchCombinations.Any()) { ConsoleUtil.WriteText("Klinik seçimi kombinasyonları oluşturulamadı.", 2000); HandleExit(false); return; }
            #endregion

            #region Hastane Seçim Bölümü
            nextTierCombinations = new List<SearchCombination>();
            foreach (var combo in searchCombinations)
            {
                string hospitalUrl = string.Format(MHRSUrls.GetHospitals, combo.ProvinceId, combo.DistrictId, combo.ClinicId);
                Logger.Info($"API Request (GET) to {hospitalUrl}");
                var hospitalListResponse = await _client.Get<List<GenericResponseModel>>(MHRSUrls.BaseUrl, hospitalUrl);

                if (hospitalListResponse?.Success == true)
                {
                    Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, hospitalListResponse, $"Response from {hospitalUrl}");
                }

                if (hospitalListResponse == null || !hospitalListResponse.Success || hospitalListResponse.Data == null)
                {
                    Logger.Error($"Hastane listesi alınamadı: ... Klinik={combo.ClinicText}. FARKETMEZ varsayılıyor.");
                    Logger.LogObject(LogLevel.API_RESPONSE_FAIL, hospitalListResponse, "Hastane Listesi Yanıtı (Başarısız)");
                    var newComboNoHospital = combo.ShallowCopy();
                    newComboNoHospital.HospitalId = -1;
                    newComboNoHospital.HospitalText = "FARKETMEZ (Hastane Listesi Alınamadı)";
                    nextTierCombinations.Add(newComboNoHospital);
                    continue;
                }

                List<int> selectedHospitalMainValues = GetMultipleSelections(hospitalListResponse.Data, "Hastane Numaralarını Seçiniz", "Hastane", true, $"{combo.ClinicText} için ");
                if (!selectedHospitalMainValues.Any()) { continue; }

                foreach (int hValue in selectedHospitalMainValues)
                {
                    if (hValue == -1)
                    {
                        var newComboFarketmez = combo.ShallowCopy();
                        newComboFarketmez.HospitalId = -1;
                        newComboFarketmez.HospitalText = "FARKETMEZ";
                        nextTierCombinations.Add(newComboFarketmez);
                        continue;
                    }

                    var hospital = hospitalListResponse.Data.FirstOrDefault(h => h.Value == hValue);
                    if (hospital == null) continue;

                    var baseCombo = combo.ShallowCopy();
                    baseCombo.AnaKurumId = hospital.Value;
                    baseCombo.AnaKurumText = hospital.Text;

                    if (hospital.Children != null && hospital.Children.Any())
                    {
                        var allUnits = new List<GenericResponseModel> { new GenericResponseModel { Value = hospital.Value, Text = hospital.Text + " (Ana Kurum)" } };
                        allUnits.AddRange(hospital.Children);

                        List<int> selectedSubUnitValues = GetMultipleSelections(allUnits, "Hastane/Poliklinik seçiniz", "Hastane/Poliklinik", true, $"{hospital.Text} için ");

                        if (!selectedSubUnitValues.Any()) { continue; }

                        if (selectedSubUnitValues.Count == 1 && selectedSubUnitValues[0] == -1)
                        {
                            foreach (var unit in allUnits)
                            {
                                var newComboChild = baseCombo.ShallowCopy();
                                newComboChild.HospitalId = unit.Value;
                                newComboChild.HospitalText = unit.Text;
                                nextTierCombinations.Add(newComboChild);
                            }
                        }
                        else
                        {
                            foreach (int subValue in selectedSubUnitValues)
                            {
                                var selUnit = allUnits.FirstOrDefault(u => u.Value == subValue);
                                if (selUnit == null) continue;

                                var newComboChild = baseCombo.ShallowCopy();
                                newComboChild.HospitalId = subValue;
                                newComboChild.HospitalText = selUnit.Text;
                                nextTierCombinations.Add(newComboChild);
                            }
                        }
                    }
                    else
                    {
                        var newComboNoChildren = baseCombo.ShallowCopy();
                        newComboNoChildren.HospitalId = hValue;
                        newComboNoChildren.HospitalText = hospital.Text;
                        nextTierCombinations.Add(newComboNoChildren);
                    }
                }
            }
            searchCombinations = nextTierCombinations;
            if (!searchCombinations.Any()) { ConsoleUtil.WriteText("Hastane seçimi sonucu geçerli kombinasyon oluşturulamadı.", 2000); HandleExit(false); return; }
            #endregion

            #region Muayene Yeri Seçim Bölümü
            nextTierCombinations = new List<SearchCombination>();
            foreach (var combo in searchCombinations)
            {
                if (combo.HospitalId == -1)
                {
                    var newCombo = combo.ShallowCopy();
                    newCombo.PlaceId = -1; newCombo.PlaceText = "FARKETMEZ";
                    nextTierCombinations.Add(newCombo);
                    continue;
                }
                string placeUrl = string.Format(MHRSUrls.GetPlaces, combo.HospitalId, combo.ClinicId);
                Logger.Info($"API Request (GET) to {placeUrl}");
                var placeListResponse = await _client.Get<List<ClinicResponseModel>>(MHRSUrls.BaseUrl, placeUrl);

                if (placeListResponse?.Success == true)
                {
                    Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, placeListResponse, $"Response from {placeUrl}");
                }

                if (placeListResponse == null || !placeListResponse.Success || placeListResponse.Data == null)
                {
                    Logger.LogObject(LogLevel.API_RESPONSE_FAIL, placeListResponse, $"Response from {placeUrl}");
                    var newCombo = combo.ShallowCopy();
                    newCombo.PlaceId = -1;
                    newCombo.PlaceText = "FARKETMEZ (Muayene Yeri Listesi Alınamadı)";
                    nextTierCombinations.Add(newCombo);
                    continue;
                }

                var originalPlaceList = placeListResponse.Data;
                var filteredPlaceList = new List<ClinicResponseModel>(originalPlaceList);

                if (combo.HospitalId == combo.AnaKurumId)
                {
                    filteredPlaceList = originalPlaceList.Where(p => p.Text.Contains("Merkez")).ToList();
                    Logger.Info($"Ana Kurum seçildi. Muayene yeri listesi 'Merkez' içerenlerle ({filteredPlaceList.Count} adet) sınırlandırıldı.");
                }
                else if (combo.HospitalId != combo.AnaKurumId)
                {
                    filteredPlaceList = originalPlaceList.Where(p => !p.Text.Contains("Merkez")).ToList();
                    Logger.Info($"Alt birim seçildi. Muayene yeri listesi 'Merkez' içermeyenlerle ({filteredPlaceList.Count} adet) sınırlandırıldı.");
                }

                List<int> selectedPlaceValues = GetMultipleSelectionsForPlaces(filteredPlaceList, "Muayene Yeri Numaralarını Seçiniz", "Muayene Yeri", true, $"{combo.HospitalText} için ");
                if (!selectedPlaceValues.Any()) { continue; }
                foreach (int placeValue in selectedPlaceValues)
                {
                    var newCombo = combo.ShallowCopy();
                    newCombo.PlaceId = placeValue;
                    newCombo.PlaceText = placeValue == -1 ? "FARKETMEZ" : originalPlaceList.FirstOrDefault(p => p.Value == placeValue)?.Text ?? "Bilinmeyen Muayene Yeri";
                    nextTierCombinations.Add(newCombo);
                }
            }
            searchCombinations = nextTierCombinations;
            if (!searchCombinations.Any()) { ConsoleUtil.WriteText("Muayene Yeri seçimi kombinasyonları oluşturulamadı.", 2000); HandleExit(false); return; }
            #endregion

            #region Doktor Seçim Bölümü
            nextTierCombinations = new List<SearchCombination>();
            foreach (var combo in searchCombinations)
            {
                if (combo.HospitalId == -1)
                {
                    var newCombo = combo.ShallowCopy();
                    newCombo.DoctorId = -1; newCombo.DoctorText = "FARKETMEZ";
                    nextTierCombinations.Add(newCombo);
                    continue;
                }

                string doctorUrl = $"/api/kurum/hekim/hekim-klinik/hekim-select-input/anakurum/{combo.AnaKurumId}/kurum/{combo.HospitalId}/klinik/{combo.ClinicId}";
                Logger.Info($"API Request (GET) to {doctorUrl}");

                var doctorListResponse = await _client.Get<List<GenericResponseModel>>(MHRSUrls.BaseUrl, doctorUrl);

                if (doctorListResponse?.Success == true)
                {
                    Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, doctorListResponse, $"Response from {doctorUrl}");
                }

                if (doctorListResponse == null || !doctorListResponse.Success || doctorListResponse.Data == null)
                {
                    Logger.LogObject(LogLevel.API_RESPONSE_FAIL, doctorListResponse, $"Response from {doctorUrl}");
                    var newCombo = combo.ShallowCopy();
                    newCombo.DoctorId = -1;
                    newCombo.DoctorText = "FARKETMEZ (Doktor Listesi Alınamadı)";
                    nextTierCombinations.Add(newCombo);
                    continue;
                }
                List<int> selectedDoctorValues = GetMultipleSelections(doctorListResponse.Data, "Doktor Numaralarını Seçiniz", "Doktor", true, $"{combo.HospitalText} ({combo.PlaceText}) - {combo.ClinicText} için ");
                if (!selectedDoctorValues.Any()) { continue; }
                foreach (int doctorValue in selectedDoctorValues)
                {
                    var newCombo = combo.ShallowCopy();
                    newCombo.DoctorId = doctorValue;
                    newCombo.DoctorText = doctorValue == -1 ? "FARKETMEZ" : doctorListResponse.Data.FirstOrDefault(d => d.Value == doctorValue)?.Text ?? "Bilinmeyen Doktor";
                    nextTierCombinations.Add(newCombo);
                }
            }
            searchCombinations = nextTierCombinations;
            if (!searchCombinations.Any()) { ConsoleUtil.WriteText("Doktor seçimi kombinasyonları oluşturulamadı.", 2000); HandleExit(false); return; }
            #endregion

            #region Tarih Seçim Bölümü
            string? startDate = null; string? endDate = null;
            ConsoleUtil.WriteText("Tarih girmek istemiyorsanız boş bırakınız (En yakın tarihten başlar).", 0);
            ConsoleUtil.WriteText($"UYARI: Bitiş tarihi en fazla {DateTime.Now.AddDays(MHRSUrls.RandevuAralikGunSayisi).ToString("dd-MM-yyyy")} olabilir.\n", 0);
            do
            {
                var sDateInput = Logger.ReadLineAndLog("Başlangıç tarihi (GG-AA-YYYY): ");
                if (sDateInput == null) { HandleExit(false); return; }
                if (string.IsNullOrWhiteSpace(sDateInput)) { startDate = null; break; }
                try
                {
                    var dA = sDateInput.Split(new char[] { '-', '/', '.' }).Select(int.Parse).ToArray();
                    startDate = new DateTime(dA[2], dA[1], dA[0]).ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                }
                catch { ConsoleUtil.WriteText("Geçersiz tarih formatı.", 0); }
            } while (true);

            do
            {
                var eDateInput = Logger.ReadLineAndLog("Bitiş tarihi (GG-AA-YYYY): ");
                if (eDateInput == null) { HandleExit(false); return; }
                if (string.IsNullOrWhiteSpace(eDateInput)) { endDate = null; break; }
                try
                {
                    var dA = eDateInput.Split(new char[] { '-', '/', '.' }).Select(int.Parse).ToArray();
                    var dt = new DateTime(dA[2], dA[1], dA[0], 23, 59, 59);
                    if (startDate != null && dt < DateTime.Parse(startDate))
                    {
                        ConsoleUtil.WriteText("Bitiş tarihi başlangıç tarihinden önce olamaz.", 0); continue;
                    }
                    endDate = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                }
                catch { ConsoleUtil.WriteText("Geçersiz tarih formatı.", 0); }
            } while (true);
            #endregion

            #region Gelişmiş Tarih ve Saat Filtresi Bölümü
            var dateBasedFilters = new Dictionary<DateTime, DateFilterRule>();
            DateFilterRule? globalFilter = null; 
            Console.Clear();
            Console.WriteLine("--- Gelişmiş Tarih ve Saat Filtresi ---");
            Console.WriteLine("Filtre girmek istemiyorsanız bu adımı boş geçebilirsiniz (ENTER).");
            Console.WriteLine("Format: GG-AA-YYYY, saat1, saat2... ; -saat3, -saat4... ; ...");
            Console.WriteLine("  - Tarihe Özel Kural: 24-06-2025, 9, 10 (Sadece saat 9 ve 10'u al)");
            Console.WriteLine("  - Tarihe Özel Kural: 25-06-2025, -8, -15 (Saat 8 ve 15'i alma)");
            Console.WriteLine("  - Genel Kural (Tarih belirtilmezse): -8, -9 (Diğer tüm günler için 8 ve 9'u alma)");

            string? filterInput = Logger.ReadLineAndLog("\nLütfen filtre kurallarını giriniz: ");

            if (!string.IsNullOrWhiteSpace(filterInput))
            {
                var allRuleStrings = filterInput.Split(';');
                foreach (var ruleText in allRuleStrings)
                {
                    if (string.IsNullOrWhiteSpace(ruleText)) continue;

                    var parts = ruleText.Split(',').Select(p => p.Trim()).ToList();
                    if (!parts.Any()) continue;
                    
                    if (DateTime.TryParseExact(parts[0], "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ruleDate))
                    {
                        var rule = new DateFilterRule();
                        bool modeSet = false;

                        for (int i = 1; i < parts.Count; i++)
                        {
                            if (int.TryParse(parts[i], out int hour))
                            {
                                var currentMode = hour > 0 ? FilterMode.Include : FilterMode.Exclude;
                                if (!modeSet) { rule.Mode = currentMode; modeSet = true; }
                                else if (rule.Mode != currentMode) { Logger.Warn($"Kural '{ruleText}' içinde karışık mod kullanılamaz. Atlanıyor."); rule.Hours.Clear(); break; }
                                rule.Hours.Add(Math.Abs(hour));
                            }
                        }
                        if (rule.Hours.Any()) dateBasedFilters[ruleDate.Date] = rule;
                    }
                    else
                    {
                        if (globalFilter != null) { Logger.Warn("Birden fazla genel kural tanımlandı. Sadece ilki kullanılacak."); continue; }
                        
                        var rule = new DateFilterRule();
                        bool modeSet = false;

                        foreach(var part in parts)
                        {
                             if (int.TryParse(part, out int hour))
                            {
                                var currentMode = hour > 0 ? FilterMode.Include : FilterMode.Exclude;
                                if (!modeSet) { rule.Mode = currentMode; modeSet = true; }
                                else if (rule.Mode != currentMode) { Logger.Warn($"Genel kural '{ruleText}' içinde karışık mod kullanılamaz. Atlanıyor."); rule.Hours.Clear(); break; }
                                rule.Hours.Add(Math.Abs(hour));
                            }
                        }
                        if (rule.Hours.Any()) globalFilter = rule;
                    }
                }
                Console.WriteLine("Filtreler başarıyla işlendi. Arama başlıyor...");
                Thread.Sleep(2000);
            }
            #endregion

            #region Randevu Alım Bölümü
            ConsoleUtil.WriteText("Seçimleriniz doğrultusunda randevu aranacak...", 2000);
            Console.Clear();
            Logger.Info($"Arama için toplam {searchCombinations.Count} farklı kombinasyon oluşturuldu.");
            Console.WriteLine($"Toplam {searchCombinations.Count} farklı spesifik tercih kombinasyonu mevcut.");
            foreach (var combo in searchCombinations) Console.WriteLine($"- {combo}");
            Console.WriteLine("\nAPI çağrılarını azaltmak için benzer kriterlere sahip sorgular gruplanarak yapılacaktır.");

            bool appointmentState = false;
            do
            {
                if (TOKEN_END_DATE == default || TOKEN_END_DATE < DateTime.Now.AddMinutes(-1))
                {
                    Logger.WriteLineAndLog("Token süresi dolmuş veya geçersiz. Yeniden giriş yapılıyor...");

                    _client = new ClientService();
                    Logger.Info("ClientService (HttpClient ve Çerezler) sıfırlandı.");

                    var tknData = await GetToken(_client, true);
                    if (tknData == null || string.IsNullOrEmpty(tknData.Token))
                    {
                        Logger.Error("Token yenilenemedi! 10 saniye sonra ana döngü tekrar deneyecek.");
                        Thread.Sleep(10000);
                        continue;
                    }

                    JWT_TOKEN = tknData.Token;
                    TOKEN_END_DATE = tknData.Expiration;
                    _client.AddOrUpdateAuthorizationHeader(JWT_TOKEN);
                    Logger.WriteLineAndLog("Token başarıyla yenilendi.");
                }

                bool sessionInvalidated = false;

                var groupedApiQueries = searchCombinations
                    .GroupBy(c => new { IlId = c.ProvinceId, IlceId = c.DistrictId, KlinikId = c.ClinicId, KurumId = c.HospitalId })
                    .ToList();

                Logger.WriteLineAndLog($"\n{groupedApiQueries.Count()} farklı ana grup için genişletilmiş API sorgusu yapılacak.");

                foreach (var apiGroup in groupedApiQueries)
                {
                    var key = apiGroup.Key;
                    List<SearchCombination> combosInThisApiGroup = apiGroup.ToList();
                    var repCombo = combosInThisApiGroup.First();

                    List<int> targetPlaceIdsInGroup = combosInThisApiGroup.Select(c => c.PlaceId).Distinct().ToList();
                    List<int> targetDoctorIdsInGroup = combosInThisApiGroup.Select(c => c.DoctorId).Distinct().ToList();

                    Logger.WriteLineAndLog($"\n'{repCombo.HospitalText}' için slotlar alınıyor...");

                    SlotRequestModel broadRequest = new SlotRequestModel
                    {
                        MhrsIlId = key.IlId,
                        MhrsIlceId = key.IlceId,
                        MhrsKlinikId = key.KlinikId,
                        MhrsKurumId = key.KurumId,
                        MuayeneYeriId = -1,
                        MhrsHekimId = -1,
                        BaslangicZamani = startDate,
                        BitisZamani = endDate
                    };

                    var slotResult = await GetAllPotentialSlots(_client, broadRequest);

                    if (slotResult.SessionEnded)
                    {
                        Logger.WriteLineAndLog("!!! Sunucu oturumu sonlandırdı (LGN2001). Anında yeniden giriş denenecek. !!!");
                        sessionInvalidated = true;
                        break;
                    }

                    List<SubSlot> allSlotsFromApi = slotResult.Slots;
                    if (allSlotsFromApi == null || !allSlotsFromApi.Any()) { Logger.Info("Bu geniş sorgu için API'den hiç slot dönmedi."); continue; }
                    Logger.WriteLineAndLog($"   API'den {allSlotsFromApi.Count} potansiyel slot alındı. Şimdi yerel filtreleme uygulanacak...");

                    List<SubSlot> locallyFilteredSlots = new List<SubSlot>();
                    foreach (SubSlot slot in allSlotsFromApi)
                    {
                        bool placeMatch = targetPlaceIdsInGroup.Contains(-1) || targetPlaceIdsInGroup.Contains((int)slot.MuayeneYeriId);
                        bool doctorMatch = targetDoctorIdsInGroup.Contains(-1) || targetDoctorIdsInGroup.Contains(slot.MhrsHekimId);

                        if (string.IsNullOrEmpty(slot.BaslangicZamani)) continue; // Null kontrolü
                        DateTime slotDateTime = DateTime.Parse(slot.BaslangicZamani);
                        
                        bool isHourMatch = PassesDateBasedFilter(slotDateTime, dateBasedFilters, globalFilter);
                        
                        bool notTooSoon = true;
                        if (minimumMinutesToAppointment > 0 && slotDateTime.Date == DateTime.Today)
                        {
                            if ((slotDateTime - DateTime.Now).TotalMinutes < minimumMinutesToAppointment)
                            {
                                notTooSoon = false;
                            }
                        }
                        
                        if (placeMatch && doctorMatch && isHourMatch && notTooSoon)
                        {
                            locallyFilteredSlots.Add(slot);
                        }
                    }

                    Logger.WriteLineAndLog($"   Yerel filtreleme sonrası {locallyFilteredSlots.Count} uygun slot bulundu.");
                    locallyFilteredSlots = locallyFilteredSlots.OrderBy(s => DateTime.Parse(s.BaslangicZamani!)).ToList();

                    foreach (SubSlot finalSlotToBook in locallyFilteredSlots)
                    {
                        string doktorAdi = finalSlotToBook.HekimAdi ?? finalSlotToBook.MhrsHekimId.ToString();
                        string yerAdi = finalSlotToBook.MuayeneYeriAdi ?? finalSlotToBook.MuayeneYeriId.ToString();
                        string kurumAdi = finalSlotToBook.KurumAdi ?? repCombo.HospitalText;

                        Logger.WriteLineAndLog($"   Randevu deneniyor: {finalSlotToBook.BaslangicZamani} Dr:{doktorAdi} Yer:{yerAdi} Kurum:{kurumAdi}");
                        
                        if(finalSlotToBook.BaslangicZamani == null || finalSlotToBook.BitisZamani == null)
                        {
                            Logger.Error($"Slot ID {finalSlotToBook.Id} için başlangıç veya bitiş zamanı null. Atlanıyor.");
                            continue;
                        }

                        var appReq = new AppointmentRequestModel
                        {
                            FkSlotId = finalSlotToBook.Id,
                            FkCetvelId = finalSlotToBook.FkCetvelId,
                            MuayeneYeriId = finalSlotToBook.MuayeneYeriId,
                            BaslangicZamani = finalSlotToBook.BaslangicZamani,
                            BitisZamani = finalSlotToBook.BitisZamani
                        };

                        var attemptResult = await MakeAppointment(_client, appReq, true, repCombo, finalSlotToBook);

                        if (attemptResult == AppointmentAttemptResult.Success)
                        {
                            appointmentState = true;
                            break;
                        }
                        if (attemptResult == AppointmentAttemptResult.StopSearching)
                        {
                            Logger.WriteLineAndLog("   Mevcut randevu daha erken olduğu için bu gruptaki diğer saatler denenmeyecek.");
                            break;
                        }
                    }
                    if (appointmentState) break;
                }

                if (sessionInvalidated)
                {
                    TOKEN_END_DATE = DateTime.MinValue;
                    continue;
                }

                if (appointmentState) break;

                string beklemeMesaji = $"\nTüm gruplar denendi, müsait randevu bulunamadı | Kontrol Saati: {DateTime.Now:HH:mm:ss}";
                Logger.WriteLineAndLog(beklemeMesaji);
                Logger.WriteLineAndLog($"{MHRSUrls.BeklemeSuresiDakika} dk sonra tekrar denenecek...");
                Thread.Sleep(TimeSpan.FromMinutes(MHRSUrls.BeklemeSuresiDakika));
                Console.Clear();
            } while (!appointmentState);
            #endregion
            
            if (!appointmentState)
            {
                HandleExit(false);
            }
        }

        static async Task<JwtTokenModel?> GetToken(IClientService client, bool forceRefresh = false)
        {
            if (string.IsNullOrEmpty(TC_NO)) return null;
            
            string tokenFileName = $"token_{TC_NO}.txt";
            string tokenFilePath = Path.Combine(AppContext.BaseDirectory, tokenFileName);

            try
            {
                if (forceRefresh && File.Exists(tokenFilePath))
                {
                    try { File.Delete(tokenFilePath); Logger.Info("Eski token dosyası (forceRefresh nedeniyle) silindi."); }
                    catch (Exception ex) { Logger.Error("Token dosyası silinirken hata (forceRefresh).", ex); }
                }

                if (!forceRefresh && File.Exists(tokenFilePath))
                {
                    var tokenJson = await File.ReadAllTextAsync(tokenFilePath);
                    var storedToken = JsonSerializer.Deserialize<JwtTokenModel>(tokenJson);
                    if (storedToken != null && !string.IsNullOrEmpty(storedToken.Token) && storedToken.Expiration > DateTime.Now.AddMinutes(5))
                    {
                        Logger.WriteLineAndLog("Kaydedilmiş ve geçerli token bulundu.");
                        return storedToken;
                    }
                    else if (File.Exists(tokenFilePath))
                    {
                        try { File.Delete(tokenFilePath); Logger.Info("Süresi dolmuş/geçersiz token dosyası silindi."); }
                        catch (Exception ex) { Logger.Error("Token dosyası silinirken hata (süre dolumu/geçersiz).", ex); }
                    }
                }
            }
            catch (Exception ex) { Logger.Error("Token dosyası okunurken/işlenirken hata.", ex); }

            if (string.IsNullOrEmpty(SIFRE))
            {
                Logger.Error("Giriş bilgileri (Şifre) bulunamadı. Token alınamıyor.");
                return null;
            }

            Logger.WriteLineAndLog(forceRefresh ? "Token yenileniyor (forceRefresh)..." : "Yeni token alınıyor (dosya yok veya geçersizdi)...");

            var actualLoginRequest = new LoginRequestModel { KullaniciAdi = TC_NO, Parola = SIFRE, IslemKanali = "VATANDAS_WEB", GizlilikSozlesmeOnay = true };

            var loggableLoginRequest = new { actualLoginRequest.KullaniciAdi, Parola = "**********", actualLoginRequest.IslemKanali, actualLoginRequest.GizlilikSozlesmeOnay };
            Logger.LogObject(LogLevel.API_REQUEST, loggableLoginRequest, $"Login Request to {MHRSUrls.Login}");

            var loginResp = await client.Post<LoginResponseModel>(MHRSUrls.BaseUrl, MHRSUrls.Login, actualLoginRequest);

            if (loginResp == null)
            {
                Logger.Error("Giriş isteği başarısız (loginResp null). Sunucuya ulaşılamıyor olabilir.");
                Logger.LogObject(LogLevel.API_RESPONSE_FAIL, loginResp, "Login Yanıtı (Detay)");
                return null;
            }

            if (loginResp.Data != null && !string.IsNullOrEmpty(loginResp.Data.Jwt))
            {
                var expiration = JwtTokenUtil.GetTokenExpireTime(loginResp.Data.Jwt);
                var tokenToSave = new JwtTokenModel { Token = loginResp.Data.Jwt, Expiration = expiration };
                Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, loginResp, "Login Yanıtı (Başarılı)");
                try
                {
                    await File.WriteAllTextAsync(tokenFilePath, JsonSerializer.Serialize(tokenToSave));
                }
                catch (Exception ex) { Logger.Error("Yeni token dosyaya yazılamadı.", ex); }

                return tokenToSave;
            }
            else
            {
                MhrsMessage? firstError = loginResp.Errors?.FirstOrDefault();
                if (firstError != null)
                {
                    string logMessage = $"Login API hatası: {firstError.mesaj} (Kod: {firstError.kodu})";
                    Logger.Error(logMessage);
                    Logger.LogObject(LogLevel.API_RESPONSE_FAIL, loginResp, "Login Yanıtı (Detay)");

                    if (firstError.kodu == "GNL2029")
                    {
                        ConsoleUtil.WriteText("\nUYARI: MHRS servis kullanım sınırına takıldınız.", 2000);
                        ConsoleUtil.WriteText("Bu durum genellikle kısa sürede çok fazla deneme yapıldığında olur.", 2000);
                        ConsoleUtil.WriteText("Çözüm için modeminizi kapatıp 1-2 dakika bekledikten sonra tekrar açın.", 3000);
                        ConsoleUtil.WriteText("Program 10 saniye içinde kapanacak...", 10000);
                        Environment.Exit(1);
                    }
                    else if(firstError.kodu == "LGN2001")
                    {
                        ConsoleUtil.WriteText("\nUYARI: Başka bir yerden giriş yapıldığı için oturum sonlandırıldı.", 2000);
                        ConsoleUtil.WriteText("Temiz bir oturumla yeniden denemek için lütfen bilgilerinizi tekrar girin.", 3000);
                    }
                    else
                    {
                        ConsoleUtil.WriteText($"\nHATA: {firstError.mesaj}", 3000);
                    }
                }
                else
                {
                    Logger.Error("Bilinmeyen giriş hatası.");
                    ConsoleUtil.WriteText("\nHATA: Giriş yapılamadı, bilinmeyen bir sorun oluştu.", 3000);
                }

                if (File.Exists(tokenFilePath))
                {
                    try { File.Delete(tokenFilePath); Logger.Info("Başarısız giriş denemesi sonrası (varsa) eski token dosyası silindi."); }
                    catch (Exception ex) { Logger.Error("Token dosyası silinirken hata (başarısız giriş).", ex); }
                }

                return null;
            }
        }

        static async Task<SlotResult> GetAllPotentialSlots(IClientService client, SlotRequestModel slotRequestModel)
        {
            var result = new SlotResult();
            if (string.IsNullOrWhiteSpace(slotRequestModel.BaslangicZamani))
                slotRequestModel.BaslangicZamani = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (string.IsNullOrWhiteSpace(slotRequestModel.BitisZamani))
                slotRequestModel.BitisZamani = DateTime.Now.AddDays(MHRSUrls.RandevuAralikGunSayisi).ToString("yyyy-MM-dd HH:mm:ss");

            Logger.LogObject(LogLevel.API_REQUEST, slotRequestModel, $"Slot Request to {MHRSUrls.GetSlots}");
            var slotListResp = await client.Post<List<SlotResponseModel>>(MHRSUrls.BaseUrl, MHRSUrls.GetSlots, slotRequestModel);

            if (slotListResp == null || !slotListResp.Success || slotListResp.Data == null)
            {
                string err = "Slot API'sinden yanıt alınamadı veya başarısız.";
                if (slotListResp != null)
                {
                    if (slotListResp.Errors.Any())
                    {
                        err = $"Slot API Hatası: {slotListResp.Errors.First().mesaj} (Kod: {slotListResp.Errors.First().kodu})";
                        if (slotListResp.Errors.Any(e => e.kodu == "LGN2001"))
                        {
                            result.SessionEnded = true;
                        }
                    }
                    else if (slotListResp.Warnings.Any()) err = $"Slot API Uyarısı: {slotListResp.Warnings.First().mesaj} (Kod: {slotListResp.Warnings.First().kodu})";
                    else if (slotListResp.Infos.Any()) err = $"Slot API Bilgisi: {slotListResp.Infos.First().mesaj} (Kod: {slotListResp.Infos.First().kodu})";
                }
                Logger.Error(err);
                Logger.LogObject(LogLevel.API_RESPONSE_FAIL, slotListResp, "Slot Arama Yanıtı (Detay)");
                return result;
            }
            Logger.LogObject(LogLevel.API_RESPONSE_SUCCESS, slotListResp, "Slot Arama Yanıtı");

            foreach (var gunSlot in slotListResp.Data)
            {
                if (gunSlot.HekimSlotList == null) continue;
                foreach (var hekimSlot in gunSlot.HekimSlotList)
                {
                    if (hekimSlot.MuayeneYeriSlotList == null) continue;
                    foreach (var muayeneYeriSlot in hekimSlot.MuayeneYeriSlotList)
                    {
                        if (muayeneYeriSlot.SaatSlotList == null) continue;
                        foreach (var saatSlot in muayeneYeriSlot.SaatSlotList.Where(ss => ss.Bos))
                        {
                            if (saatSlot.SlotList == null) continue;
                            foreach (var slotItem in saatSlot.SlotList.Where(s => s.Bos))
                            {
                                if (slotItem.SubSlot != null)
                                {
                                    var subSlot = slotItem.SubSlot;

                                    subSlot.HekimAdi = hekimSlot.Hekim?.Ad + " " + hekimSlot.Hekim?.Soyad;
                                    subSlot.MuayeneYeriAdi = muayeneYeriSlot.MuayeneYeri?.Adi;
                                    subSlot.KurumAdi = hekimSlot.Kurum?.KurumAdi;

                                    result.Slots.Add(subSlot);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        static async Task<AppointmentAttemptResult> MakeAppointment(
            IClientService client,
            AppointmentRequestModel standartRandevuIstekModeli,
            bool sendNotification,
            SearchCombination aramaKriterleri,
            SubSlot alinanSlotDetaylari)
        {
            Logger.LogObject(LogLevel.API_REQUEST, standartRandevuIstekModeli, $"Appointment Request to {MHRSUrls.MakeAppointment}");
            var randevuResp = await client.PostSimple<BaseResponse>(MHRSUrls.BaseUrl, MHRSUrls.MakeAppointment, standartRandevuIstekModeli);

            string? rawJson = randevuResp.Messages?.FirstOrDefault();
            DetailedAppointmentResponse? detailedResp = null;

            if (!string.IsNullOrEmpty(rawJson))
            {
                try { detailedResp = JsonSerializer.Deserialize<DetailedAppointmentResponse>(rawJson); }
                catch (JsonException jex) { Logger.Error($"Randevu yanıtı JSON olarak ayrıştırılamadı. Ham Yanıt: {rawJson}", jex); }
            }

            Logger.LogObject(randevuResp.Success ? LogLevel.API_RESPONSE_SUCCESS : LogLevel.API_RESPONSE_FAIL, randevuResp, "Randevu Alma Yanıtı");

            if (detailedResp != null && detailedResp.warnings.Any(w => w.kodu == "RND5015"))
            {
                var rnd5015Warning = detailedResp.warnings.First(w => w.kodu == "RND5015");
                Logger.WriteLineAndLog($"\nUYARI (RND5015): {rnd5015Warning.mesaj}");

                DateTime yeniRandevuTarihi = DateTime.Parse(alinanSlotDetaylari.BaslangicZamani!);
                DateTime eskiRandevuTarihi = DateTime.MinValue;
                bool shouldAttemptCancelAndRebook = false;

                Match dateMatch = Regex.Match(rnd5015Warning.mesaj ?? "", @"(\d{2}\.\d{2}\.\d{4})");
                if (dateMatch.Success && DateTime.TryParseExact(dateMatch.Groups[1].Value, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out eskiRandevuTarihi))
                {
                    if (yeniRandevuTarihi.Date < eskiRandevuTarihi.Date)
                    {
                        shouldAttemptCancelAndRebook = true;
                        Logger.WriteLineAndLog("Yeni bulunan randevu, mevcut randevunuzdan daha erken bir tarihte! Otomatik iptal ve yeniden alma denenecek.");
                    }
                    else
                    {
                        Logger.WriteLineAndLog("Yeni bulunan randevu, mevcut randevunuzdan daha ERKEN DEĞİL. Mevcut randevu korunacak.");
                        return AppointmentAttemptResult.StopSearching;
                    }
                }
                else { Logger.Error("RND5015 uyarısındaki mevcut randevu tarihi ayrıştırılamadı."); }

                if (shouldAttemptCancelAndRebook)
                {
                    var cancelAndRebookRequest = new RandevuIptalEtYeniAlRequestModel
                    {
                        FkSlotId = alinanSlotDetaylari.Id,
                        FkCetvelId = alinanSlotDetaylari.FkCetvelId,
                        MuayeneYeriId = alinanSlotDetaylari.MuayeneYeriId,
                        BaslangicZamani = alinanSlotDetaylari.BaslangicZamani,
                        BitisZamani = alinanSlotDetaylari.BitisZamani,
                    };

                    Logger.LogObject(LogLevel.API_REQUEST, cancelAndRebookRequest, $"Cancel and Rebook Request to {MHRSUrls.CancelAndRebookAppointment}");
                    randevuResp = await client.PostForCancelAndRebook(MHRSUrls.BaseUrl, MHRSUrls.CancelAndRebookAppointment, cancelAndRebookRequest);
                    Logger.LogObject(randevuResp.Success ? LogLevel.API_RESPONSE_SUCCESS : LogLevel.API_RESPONSE_FAIL, randevuResp, "Cancel and Rebook Yanıtı");

                    rawJson = randevuResp.Messages?.FirstOrDefault();
                    detailedResp = null;
                    if (!string.IsNullOrEmpty(rawJson))
                    {
                        try { detailedResp = JsonSerializer.Deserialize<DetailedAppointmentResponse>(rawJson); }
                        catch (JsonException) { }
                    }

                    if (randevuResp.Success && (detailedResp == null || (!detailedResp.errors.Any() && !detailedResp.warnings.Any())))
                    {
                        await HandleAppointmentFound(aramaKriterleri, alinanSlotDetaylari, "YENİ RANDEVU ALINDI (ESKİSİ İPTAL EDİLDİ)!");
                        return AppointmentAttemptResult.Success;
                    }
                }
            }

            if (randevuResp.Success && (detailedResp == null || (!detailedResp.errors.Any() && !detailedResp.warnings.Any())))
            {
                await HandleAppointmentFound(aramaKriterleri, alinanSlotDetaylari, "YENİ RANDEVU ALINDI!");
                return AppointmentAttemptResult.Success;
            }

            if (detailedResp != null)
            {
                if (detailedResp.errors.Any())
                {
                    var error = detailedResp.errors.First();
                    Logger.WriteLineAndLog($"Randevu ALINAMADI! Sebep: {error.mesaj} (Kod: {error.kodu})");
                }
                if (!detailedResp.success && detailedResp.warnings.Any(w => w.kodu != "RND5015"))
                {
                    var warning = detailedResp.warnings.First(w => w.kodu != "RND5015");
                    Logger.WriteLineAndLog($"Randevu ALINAMADI! Uyarı: {warning.mesaj} (Kod: {warning.kodu})");
                }
            }
            else if (!randevuResp.Success)
            {
                Logger.WriteLineAndLog($"Randevu ALINAMADI! Genel Hata: {randevuResp.StatusCode}");
            }
            else
            {
                Logger.WriteLineAndLog($"Randevu ALINAMADI! Beklenmedik durum. Yanıt: {rawJson}");
            }

            return AppointmentAttemptResult.Failed;
        }
        
        static async Task HandleAppointmentFound(SearchCombination aramaKriterleri, SubSlot alinanSlotDetaylari, string successMessage)
        {
            Console.Clear();

            string asciiArt = @"
========================================================================================================================
//  ooooooooo.         .o.       ooooo      ooo oooooooooo.   oooooooooooo oooooo     oooo ooooo     ooo              ||
//  `888   `Y88.      .888.      `888b.     `8  `888    `Y8b  `888      `8  `888.     .8   `888      `8               ||
//   888   .d88      .8 888.      8 `88b.    8   888      888  888           `888.   .8     888       8               ||
//   888ooo88P      .8  `888.     8   `88b.  8   888      888  888oooo8       `888. .8      888       8               ||
//   888`88b.      .88ooo8888.    8     `88b.8   888      888  888             `888.8       888       8               ||
//   888  `88b.   .8      `888.   8       `888   888     d88   888       o      `888        `88.    .8                ||
//  o888o  o888o o88o     o8888o o8o        `8  o888bood8P    o888ooooood8       `8           `YbodP                  ||
//                 oooooooooo.  ooooo     ooo ooooo        ooooo     ooo ooooo      ooo oooooooooo.   ooooo     ooo   ||
//                 `888    `Y8b `888      `8  `888         `888      `8  `888b.     `8  `888    `Y8b  `888      `8    ||
//                  888     888  888       8   888          888       8   8 `88b.    8   888      888  888       8    ||
//                  888oooo888   888       8   888          888       8   8   `88b.  8   888      888  888       8    ||
//                  888    `88b  888       8   888          888       8   8     `88b.8   888      888  888       8    ||
//                  888    .88P  `88.    .8    888       o  `88.    .8    8       `888   888     d88   `88.    .8     ||
//                 o888bood8P      `YbodP     o888ooooood8    `YbodP     o8o        `8  o888bood8P       `YbodP       ||
//                                                                                                                    ||
//                                MematiBas42 acil şifalar diler...                                                   ||
//                                                                                                                    ||
========================================================================================================================";
            Console.WriteLine(asciiArt);
            
            string headerMessage = $"\n=============== {successMessage} ===============\n";
            Console.WriteLine(headerMessage); 
            Logger.Info(headerMessage);       

            string detay = $"Tarih      : {DateTime.Parse(alinanSlotDetaylari.BaslangicZamani!):dd MMMM yyyy, dddd HH:mm}\n" +
                           $"Hastane    : {alinanSlotDetaylari.KurumAdi ?? aramaKriterleri.HospitalText}\n" +
                           $"Klinik     : {aramaKriterleri.ClinicText}\n" +
                           $"Muayene Yeri: {alinanSlotDetaylari.MuayeneYeriAdi ?? "Belirtilmedi"}\n" +
                           $"Doktor     : {alinanSlotDetaylari.HekimAdi ?? "Belirtilmedi"}";

            string detailsHeader = "\n--- RANDEVU DETAYLARI ---";
            Console.WriteLine(detailsHeader);
            Console.WriteLine(detay);
            Console.WriteLine("\n========================================================\n");
            
            Logger.Info(detailsHeader + "\n" + detay);
            
            try
            {
                Logger.Info("Telegram bildirimi gönderiliyor...");
                string notificationMessage = $"✅ RANDEVU BULUNDU! ✅\n\n" +
                                             $"Tarih: {DateTime.Parse(alinanSlotDetaylari.BaslangicZamani!):dd MMMM yyyy, dddd HH:mm}\n" +
                                             $"Hastane: {alinanSlotDetaylari.KurumAdi ?? aramaKriterleri.HospitalText}\n" +
                                             $"Klinik: {aramaKriterleri.ClinicText}\n" +
                                             $"Muayene Yeri: {alinanSlotDetaylari.MuayeneYeriAdi ?? "Belirtilmedi"}\n" +
                                             $"Doktor: {alinanSlotDetaylari.HekimAdi ?? "Belirtilmedi"}";
                
                // DÜZELTME: await eklendi.
                await _notificationService.SendNotification(notificationMessage);
                //Console.WriteLine("Telegram bildirimi gönderildi.");
            }
            catch (Exception ex)
            {
                Logger.Error("Telegram bildirimi gönderilirken beklenmedik bir hata oluştu.", ex);
                Console.WriteLine("Telegram bildirimi gönderilemedi (Kritik Hata).");
            }

            try
            {
                if (bool.TryParse(ConfigurationManager.AppSettings["PlayAlarmOnFound"], out bool playAlarm) && playAlarm)
                {
                    // DÜZELTME: Platform kontrolü eklendi ve Task.Run beklenecek.
                    if (OperatingSystem.IsWindows())
                    {
                        Logger.Info("PlayAlarmOnFound=true. Alarm çalınıyor...");
                        await Task.Run(() => {
                            try
                            {
                                for (int i = 0; i < 30; i++)
                                {
                                    Console.Beep(800, 500);
                                    Thread.Sleep(500);
                                }
                            } catch (Exception) {}
                        });
                    }
                    else
                    {
                        Logger.Info("PlayAlarmOnFound=true, ancak işletim sistemi Windows olmadığı için alarm çalınamıyor.");
                    }
                }
            } catch (Exception ex) {
                Logger.Error("app.config okunurken veya alarm çalınırken hata.", ex);
            }
            
            DeleteTokenFile();
            
            Console.WriteLine("\nÇıkmak için ENTER tuşuna basın...");
            while (Console.ReadKey(true).Key != ConsoleKey.Enter) 
            {
            }
            Environment.Exit(0);
        }

        static void DeleteTokenFile()
        {
            if (string.IsNullOrEmpty(TC_NO)) return;

            string tokenFileName = $"token_{TC_NO}.txt";
            string tokenFilePath = Path.Combine(AppContext.BaseDirectory, tokenFileName);
            try
            {
                if (File.Exists(tokenFilePath))
                {
                    File.Delete(tokenFilePath);
                    Logger.WriteLineAndLog("Oturum sonlandırıldı ve token dosyası güvenlik amacıyla silindi.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Çıkış sırasında token dosyası silinirken bir hata oluştu.", ex);
            }
        }


        static void HandleExit(bool forceDeleteToken)
        {
            Console.Clear();
            Console.WriteLine("Çıkış yapılıyor...");

            bool delete = forceDeleteToken;

            if (!forceDeleteToken)
            {
                Console.Write("Güvenlik için token dosyası silinsin mi? (E/H): ");
                string? response = Console.ReadLine()?.Trim().ToUpper();
                delete = (response == "E");
            }
            
            if (delete)
            {
                DeleteTokenFile();
            }
            else
            {
                Logger.WriteLineAndLog("Token dosyası silinmedi. Program sonlandırılıyor.");
            }

            Logger.Info("================ UYGULAMA KULLANICI TARAFINDAN SONLANDIRILDI ================");
            Console.WriteLine("Program sonlandırılıyor...");
            Thread.Sleep(1000);
            Environment.Exit(0);
        }
    }
}
