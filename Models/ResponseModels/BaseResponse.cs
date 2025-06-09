using System.Net;
using System.Collections.Generic;

namespace MHRS_OtomatikRandevu.Models.ResponseModels
{
    public class BaseResponse
    {
        public bool Success { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string>? Messages { get; set; }
    }
}
