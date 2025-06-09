#nullable enable
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using MHRS_OtomatikRandevu.Services.Abstracts;
using MHRS_OtomatikRandevu.Utils;

namespace MHRS_OtomatikRandevu.Services
{
    /// <summary>
    /// Telegram aracılığıyla bildirim gönderen servis.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly string? TELEGRAM_API_TOKEN;
        private readonly string? TELEGRAM_CHAT_ID;

        public NotificationService()
        {
            try
            {
                TELEGRAM_API_TOKEN = ConfigurationManager.AppSettings["TELEGRAM_API_TOKEN"];
                TELEGRAM_CHAT_ID = ConfigurationManager.AppSettings["TELEGRAM_CHAT_ID"];

                if (IsConfigEmpty())
                {
                    Logger.WriteLineAndLog("UYARI: Telegram API Token veya Chat ID bilgisi app.config dosyasında eksik. Bildirimler gönderilemeyecek.");
                }
            }
            catch (Exception ex)
            {
                 Logger.Error("app.config dosyası okunurken hata oluştu. Bildirim servisi devre dışı.", ex);
            }
        }

        private bool IsConfigEmpty()
        {
            return string.IsNullOrEmpty(TELEGRAM_API_TOKEN) || string.IsNullOrEmpty(TELEGRAM_CHAT_ID);
        }

        /// <summary>
        /// Mesajı Telegram API'si üzerinden gönderir.
        /// </summary>
        /// <param name="message">Gönderilecek mesaj.</param>
        /// <returns>Operasyonun tamamlandığını belirten bir Task.</returns>
        public async Task SendNotification(string message)
        {
            if (IsConfigEmpty())
            {
                return; 
            }

            using (var client = new HttpClient())
            {
                try
                {
                    var requestUrl = $"https://api.telegram.org/bot{TELEGRAM_API_TOKEN}/sendMessage";
                    var parameters = new Dictionary<string, string>
                    {
                        { "chat_id", TELEGRAM_CHAT_ID! },
                        { "text", message }
                    };

                    var encodedContent = new FormUrlEncodedContent(parameters);
                    var response = await client.PostAsync(requestUrl, encodedContent);

                    if (response.IsSuccessStatusCode)
                    {
                        //Logger.WriteLineAndLog("Telegram bildirimi başarıyla gönderildi.");
                        Console.WriteLine("Telegram bildirimi gönderildi.");
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Logger.Error($"Telegram API hatası. Durum Kodu: {response.StatusCode}, Yanıt: {errorContent}");
                        Console.WriteLine("Telegram bildirimi gönderilemedi. Detaylar log dosyasında.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Telegram'a istek gönderilirken ağ/http hatası.", ex);
                    Console.WriteLine("Telegram bildirimi gönderilemedi (Ağ Hatası).");
                }
            }
        }
    }
}
