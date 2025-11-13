#nullable enable
using MHRS_OtomatikRandevu.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using IdentityModel;
using System;
using IdentityModel.Client;

namespace MHRS_OtomatikRandevu.Utils
{
    public static class JwtTokenUtil
    {
        public static DateTime GetTokenExpireTime(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token) || !token.Contains("."))
                    return DateTime.MinValue;

                string[] parts = token.Split('.');
var payload = JsonSerializer.Deserialize(Base64Url.Decode(parts[1]), JsonContext.Default.JwtTokenModel);

                // DÜZELTME: payload null olabilir.
                if (payload == null)
                    return DateTime.MinValue;

                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(payload.ExpirationUnix);
                return dateTimeOffset.LocalDateTime;
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }
    }
}
