using System.Collections.Generic;
using System.Text.Json.Serialization;
using MHRS_OtomatikRandevu.Models;
using MHRS_OtomatikRandevu.Models.RequestModels;
using MHRS_OtomatikRandevu.Models.ResponseModels;

namespace MHRS_OtomatikRandevu.Utils
{
    [JsonSerializable(typeof(JwtTokenModel))]
    [JsonSerializable(typeof(LoggableLoginRequest))]
    [JsonSerializable(typeof(LoginRequestModel))]
    [JsonSerializable(typeof(AppointmentRequestModel))]
    [JsonSerializable(typeof(RandevuIptalEtYeniAlRequestModel))]
    [JsonSerializable(typeof(SlotRequestModel))]
    [JsonSerializable(typeof(ApiResponse<LoginResponseModel>))]
    [JsonSerializable(typeof(ApiResponse<List<GenericResponseModel>>))]
    [JsonSerializable(typeof(ApiResponse<List<ClinicResponseModel>>))]
    [JsonSerializable(typeof(ApiResponse<List<SlotResponseModel>>))]
    [JsonSerializable(typeof(ApiResponse<object>))]
    [JsonSerializable(typeof(List<GenericResponseModel>))]
    [JsonSerializable(typeof(BaseResponse))]
    [JsonSerializable(typeof(DetailedAppointmentResponse))]
    public partial class JsonContext : JsonSerializerContext
    {
    }
}
