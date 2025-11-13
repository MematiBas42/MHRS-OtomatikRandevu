// Services/ClientService.cs
#nullable enable
using MHRS_OtomatikRandevu.Models.ResponseModels;
using MHRS_OtomatikRandevu.Services.Abstracts;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using MHRS_OtomatikRandevu.Models.RequestModels;
using MHRS_OtomatikRandevu.Utils;
using System.Text;

namespace MHRS_OtomatikRandevu.Services
{
    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;

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
                Logger.LogRawApiResponse(contentString, endpoint, "Get");

                if (response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(contentString)) return new ApiResponse<T> { Success = true, Data = null };
                    return JsonSerializer.Deserialize(contentString, typeof(ApiResponse<T>), JsonContext.Default) as ApiResponse<T>;
                }
                else
                {
                    try { return JsonSerializer.Deserialize(contentString, typeof(ApiResponse<T>), JsonContext.Default) as ApiResponse<T>; }
                    catch { return new ApiResponse<T> { Success = false }; }
                }
            }
            catch (JsonException jex) { Logger.Error($"JSON Dönüştürme Hatası ({endpoint}): {jex.Message}", jex); }
            catch (HttpRequestException e) { Logger.Error($"İstek hatası ({endpoint}): {e.Message}", e); }
            return null;
        }

        public async Task<T?> GetSimple<T>(string baseUrl, string endpoint) where T : class
        {
            try
            {
                var response = await _httpClient.GetAsync(baseUrl + endpoint);
                var contentString = await response.Content.ReadAsStringAsync();
                Logger.LogRawApiResponse(contentString, endpoint, "GetSimple");
                
                if(response.IsSuccessStatusCode)
                {
                    if (string.IsNullOrWhiteSpace(contentString)) return null;
                    if (contentString.Contains("\"LGN2000\"") || contentString.Contains("\"LGN2001\"")) {
                        Logger.Warn($"Oturum Sonlanmış/Geçersiz (LGN200x) - Endpoint: {endpoint}");
                        return null; 
                    }
                    return JsonSerializer.Deserialize(contentString, typeof(T), JsonContext.Default) as T;
                }
            }
            catch (JsonException jex) { Logger.Error($"JSON Dönüştürme Hatası ({endpoint}): {jex.Message}", jex); }
            catch (HttpRequestException e) { Logger.Error($"İstek Hatası ({endpoint}): {e.Message}", e); }
            return null;
        }

        public async Task<ApiResponse<T>?> Post<T>(string baseUrl, string endpoint, object payload) where T : class
        {
            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload, payload.GetType(), JsonContext.Default);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(baseUrl + endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();
                Logger.LogRawApiResponse(responseString, endpoint, "Post");
                
                if (string.IsNullOrWhiteSpace(responseString)) {
                    return new ApiResponse<T> { Success = response.IsSuccessStatusCode };
                }
                return JsonSerializer.Deserialize(responseString, typeof(ApiResponse<T>), JsonContext.Default) as ApiResponse<T>;
            }
            catch (JsonException jex) { Logger.Error($"JSON Dönüştürme Hatası ({endpoint}): {jex.Message}", jex); }
            catch (HttpRequestException e) { Logger.Error($"İstek hatası ({endpoint}): {e.Message}", e); }
            return null;
        }

        public async Task<BaseResponse> PostSimple<T>(string baseUrl, string endpoint, object payload) where T : class
        {
            var result = new BaseResponse { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest };
            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload, payload.GetType(), JsonContext.Default);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(baseUrl + endpoint, content);
                result.StatusCode = response.StatusCode;
                var responseContent = await response.Content.ReadAsStringAsync();
                Logger.LogRawApiResponse(responseContent, endpoint, "PostSimple");
                
                result.Messages = new List<string> { responseContent };
                if (response.IsSuccessStatusCode)
                {
                    result.Success = true; 
                }
            }
            catch (HttpRequestException e) { 
                result.Messages = new List<string> { e.Message };
                Logger.Error($"İstek hatası ({endpoint})", e);
            }
            return result;
        }
		public async Task<BaseResponse> PostForCancelAndRebook(string baseUrl, string endpoint, RandevuIptalEtYeniAlRequestModel payload)
        {
            var result = new BaseResponse { Success = false, StatusCode = System.Net.HttpStatusCode.BadRequest };
            try
            {
                var jsonPayload = JsonSerializer.Serialize(payload, JsonContext.Default.RandevuIptalEtYeniAlRequestModel);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(baseUrl + endpoint, content);
                result.StatusCode = response.StatusCode;
                var responseContent = await response.Content.ReadAsStringAsync();
                Logger.LogRawApiResponse(responseContent, endpoint, "PostForCancelAndRebook");
                
                result.Messages = new List<string> { responseContent }; 
                
                try
                {
                    var tempResp = JsonSerializer.Deserialize(responseContent, JsonContext.Default.DetailedAppointmentResponse);
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
                Logger.Error($"İstek hatası ({endpoint}): {e.Message}", e);
            }
            return result;
        }
    }
}
