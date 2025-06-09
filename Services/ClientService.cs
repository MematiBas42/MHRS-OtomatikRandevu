// Services/ClientService.cs
#nullable enable
using MHRS_OtomatikRandevu.Models.ResponseModels;
using MHRS_OtomatikRandevu.Services.Abstracts;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MHRS_OtomatikRandevu.Models.RequestModels;

namespace MHRS_OtomatikRandevu.Services
{
    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions _jsonOptions = new() 
        { 
            PropertyNameCaseInsensitive = true, 
            NumberHandling = JsonNumberHandling.AllowReadingFromString 
        };

        public ClientService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        }

        public void AddOrUpdateAuthorizationHeader(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<ApiResponse<T>?> Get<T>(string baseUrl, string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(baseUrl + endpoint);
                var contentString = await response.Content.ReadAsStringAsync();
                
                // LOGLAMA KALDIRILDI (YORUM SATIRI)
                // Console.WriteLine($"--- API Yanıtı (GET): {endpoint} ---\n{contentString}\n-----------------------------------");

                if (response.IsSuccessStatusCode)
                {
                    // Başarı durumunda bile contentString boş olabilir veya JSON olmayabilir.
                    if (string.IsNullOrWhiteSpace(contentString)) return new ApiResponse<T> { Success = true, Data = null }; // Veya uygun bir hata yönetimi
                    return JsonSerializer.Deserialize<ApiResponse<T>>(contentString, _jsonOptions);
                }
                else
                {
                     // Hata durumunda da contentString'i ayrıştırmaya çalışabiliriz (eğer API hata detayını JSON ile veriyorsa)
                    try { return JsonSerializer.Deserialize<ApiResponse<T>>(contentString, _jsonOptions); }
                    catch { return new ApiResponse<T> { Success = false }; /* Ayrıştırma başarısız */ }
                }
            }
            catch (JsonException jex) { Console.WriteLine($"JSON Dönüştürme Hatası ({endpoint}): {jex.Message}"); }
            catch (HttpRequestException e) { Console.WriteLine($"İstek hatası ({endpoint}): {e.Message}"); }
            return null;
        }

        public async Task<T?> GetSimple<T>(string baseUrl, string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(baseUrl + endpoint);
                var contentString = await response.Content.ReadAsStringAsync();

                // LOGLAMA KALDIRILDI (YORUM SATIRI)
                // Console.WriteLine($"--- API Yanıtı (GET-SIMPLE): {endpoint} ---\n{contentString}\n-----------------------------------");
                
                if(response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(contentString)) return null;
                    // LGN2001 gibi durumlar success:false dönebilir ama HTTP status 200 olabilir, bu yüzden contentString'i kontrol et
                    if (contentString.Contains("\"LGN2000\"") || contentString.Contains("\"LGN2001\"")) {
                        Console.WriteLine($"!!! Oturum Sonlanmış/Geçersiz (LGN200x) - Endpoint: {endpoint} !!!");
                        return null; 
                    }
                    return JsonSerializer.Deserialize<T>(contentString, _jsonOptions);
                }
            }
            catch (JsonException jex) { Console.WriteLine($"JSON Dönüştürme Hatası ({endpoint}): {jex.Message}"); }
            catch (HttpRequestException e) { Console.WriteLine($"İstek Hatası ({endpoint}): {e.Message}"); }
            return null;
        }

        public async Task<ApiResponse<T>?> Post<T>(string baseUrl, string endpoint, object payload) where T : class
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(baseUrl + endpoint, payload, _jsonOptions);
                var responseString = await response.Content.ReadAsStringAsync();

                // LOGLAMA KALDIRILDI (YORUM SATIRI)
                // Console.WriteLine($"--- API Yanıtı (POST): {endpoint} ---\n{responseString}\n-----------------------------------");
                
                if (string.IsNullOrWhiteSpace(responseString)) {
                    // Yanıt boşsa, HTTP durum koduna göre bir ApiResponse döndür
                    return new ApiResponse<T> { Success = response.IsSuccessStatusCode };
                }
                return JsonSerializer.Deserialize<ApiResponse<T>>(responseString, _jsonOptions);
            }
            catch (JsonException jex) { System.Console.WriteLine($"JSON Dönüştürme Hatası ({endpoint}): {jex.Message}"); }
            catch (HttpRequestException e) { System.Console.WriteLine($"İstek hatası ({endpoint}): {e.Message}"); }
            return null;
        }

        public async Task<BaseResponse> PostSimple<T>(string baseUrl, string endpoint, object payload) where T : class
        {
            var result = new BaseResponse { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest };
            try
            {
                var response = await _httpClient.PostAsJsonAsync(baseUrl + endpoint, payload, _jsonOptions);
                result.StatusCode = response.StatusCode;
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // LOGLAMA KALDIRILDI (YORUM SATIRI)
                // Console.WriteLine($"--- API Yanıtı (POST-SIMPLE): {endpoint} ---\n{responseContent}\n-----------------------------------");
                
                result.Messages = new List<string> { responseContent }; // Ham yanıtı sakla (MakeAppointment'ta parse ediliyor)
                if (response.IsSuccessStatusCode)
                {
                    // API bazen success:false ama HTTP 200 dönebilir, bu yüzden JSON içindeki success'e de bakmak lazım.
                    // Şimdilik MakeAppointment içindeki detaylı parse'a güveniyoruz.
                    result.Success = true; 
                }
            }
            catch (HttpRequestException e) { result.Messages = new List<string> { e.Message }; }
            return result;
        }
		public async Task<BaseResponse> PostForCancelAndRebook(string baseUrl, string endpoint, RandevuIptalEtYeniAlRequestModel payload)
        {
            var result = new BaseResponse { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest };
            try
            {
                var response = await _httpClient.PostAsJsonAsync(baseUrl + endpoint, payload, _jsonOptions);
                result.StatusCode = response.StatusCode;
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // API Yanıt Loglaması (kapalıysa yorumda kalacak)
                // Console.WriteLine($"--- API Yanıtı (POST CancelAndRebook): {endpoint} ---\n{responseContent}\n-----------------------------------");
                
                result.Messages = new List<string> { responseContent }; 
                
                try
                {
                    var tempResp = JsonSerializer.Deserialize<DetailedAppointmentResponse>(responseContent, _jsonOptions);
                    if (tempResp != null)
                    {
                        result.Success = tempResp.success; 
                    } else {
                         result.Success = response.IsSuccessStatusCode; 
                    }
                }
                catch(JsonException) 
                {
                    result.Success = response.IsSuccessStatusCode; 
                }
            }
            catch (HttpRequestException e) 
            { 
                result.Messages = new List<string> { e.Message }; 
                result.Success = false;
            }
            return result;
        }
    }
}
