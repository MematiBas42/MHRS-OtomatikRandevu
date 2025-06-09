using System.Threading.Tasks;

namespace MHRS_OtomatikRandevu.Services.Abstracts
{
    /// <summary>
    /// Bildirim gönderme işlemlerini yöneten servis için arayüz.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Belirtilen mesajı asenkron olarak bir bildirim kanalına gönderir.
        /// </summary>
        /// <param name="message">Gönderilecek metin mesajı.</param>
        /// <returns>Operasyonun tamamlandığını belirten bir Task.</returns>
        Task SendNotification(string message);
    }
}
