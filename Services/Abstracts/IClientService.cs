// Dosya Yolu: Services/Abstracts/IClientService.cs
#nullable enable
using MHRS_OtomatikRandevu.Models.RequestModels; // RandevuIptalEtYeniAlRequestModel için
using MHRS_OtomatikRandevu.Models.ResponseModels; // ApiResponse, BaseResponse için
// using System.Collections.Generic; // Eğer doğrudan List<T> gibi bir şey döndürmüyorsa veya almıyorsa gerekmeyebilir
using System.Threading.Tasks;

namespace MHRS_OtomatikRandevu.Services.Abstracts
{
    public interface IClientService
    {
        void AddOrUpdateAuthorizationHeader(string jwtToken);

        Task<ApiResponse<T>?> Get<T>(string baseUrl, string endpoint) where T : class;

        Task<T?> GetSimple<T>(string baseUrl, string endpoint) where T : class;

        Task<ApiResponse<T>?> Post<T>(string baseUrl, string endpoint, object requestModel) where T : class;

        Task<BaseResponse> PostSimple<T>(string baseUrl, string endpoint, object requestModel) where T : class;

        // >>> BU SATIRIN DOĞRU VE EKSİKSİZ OLDUĞUNDAN EMİN OLUN <<<
        Task<BaseResponse> PostForCancelAndRebook(string baseUrl, string endpoint, RandevuIptalEtYeniAlRequestModel payload);
    }
}
