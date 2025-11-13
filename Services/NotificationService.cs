using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using MHRS_OtomatikRandevu.Services.Abstracts;
using MHRS_OtomatikRandevu.Utils;

namespace MHRS_OtomatikRandevu.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string? TELEGRAM_API_TOKEN;
        private readonly string? TELEGRAM_CHAT_ID;

        public NotificationService(IConfiguration configuration)
        {
            try
            {
                TELEGRAM_API_TOKEN = configuration["TELEGRAM_API_TOKEN"];
                TELEGRAM_CHAT_ID = configuration["TELEGRAM_CHAT_ID"];

                if (IsConfigEmpty())
                {
                    Logger.WriteLineAndLog("UYARI: Telegram API Token veya Chat ID bilgisi appsettings.json dosyasında eksik. Bildirimler gönderilemeyecek.");
                }
            }
            catch (Exception ex)
            {
                 Logger.Error("appsettings.json dosyası okunurken hata oluştu. Bildirim servisi devre dışı.", ex);
            }
        }

        private bool IsConfigEmpty()
        {
            return string.IsNullOrEmpty(TELEGRAM_API_TOKEN) || TELEGRAM_API_TOKEN == "BURAYA_BOTFATHERDAN_TELEGRAM_API_TOKEN" ||
                   string.IsNullOrEmpty(TELEGRAM_CHAT_ID) || TELEGRAM_CHAT_ID == "BURAYA_TELEGRAM_CHAT_ID";
        }

        public async Task SendNotification(string message)
        {
            if (IsConfigEmpty())
            {
                Logger.Warn("Telegram bilgileri eksik, bildirim gönderilemiyor.");
                return;
            }

            var url = $"https://api.telegram.org/bot{TELEGRAM_API_TOKEN}/sendMessage";
            var payload = new
            {
                chat_id = TELEGRAM_CHAT_ID,
                text = message,
                parse_mode = "Markdown"
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, payload);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.Error($"Telegram API'ye bildirim gönderilemedi. Status: {response.StatusCode}, Response: {errorContent}");
                    return;
                }
                Logger.Info("Telegram bildirimi başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                Logger.Error("Telegram'a bildirim gönderilirken bir istisna oluştu.", ex);
            }
        }
    }
}
