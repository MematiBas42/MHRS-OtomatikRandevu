# 🏥 MHRS Otomatik Randevu Botu

Bu proje, **Türkiye'deki Merkezi Hekim Randevu Sistemi (MHRS)** üzerinden belirlediğiniz kriterlere göre **otomatik olarak randevu arayan** ve uygun bir randevu bulunduğunda sizi anında bilgilendiren bir **.NET 8 konsol uygulamasıdır**.

Uygulama, interaktif bir şekilde sizden İl, İlçe, Klinik, Hastane gibi bilgileri alarak arama yapar. Uygun bir randevu bulunduğunda, eğer Telegram bildirim ayarlarını yaptıysanız, size bildirim gönderir ve randevuyu sizin için alır.

---

## 🚀 Temel Özellikler

- **Detaylı Kriterlerle Arama**: Birden fazla il, ilçe, klinik, hastane ve doktor seçimi.
- **Gelişmiş Filtreleme**: Belirli tarih aralıkları ve saat dilimlerine göre arama yapabilme.
- **Akıllı Oturum Yönetimi**: Şifre sormadan otomatik giriş için oturum (token) saklama ve yenileme.
- **Otomatik Randevu Alma**: Uygun randevu bulunduğunda otomatik olarak alır.
- **Akıllı Yeniden Rezervasyon**: Daha erken bir randevu bulunduğunda mevcut randevuyu iptal edip yenisini alır.
- **Telegram Bildirimi**: Randevu detayları anında Telegram üzerinden gönderilir.
- **Sesli Alarm (Windows)**: Randevu bulunduğunda sesli uyarı verir.
- **Platform Desteği**: Windows, Linux ve Termux (Android) üzerinde çalışır.

---

## 🛠️ Kurulum ve Kullanım

### 🪟 Windows (Önerilen Yöntem)

1.  **GitHub Releases Sayfasına Gidin:**
    -   [**Buraya tıklayarak projenin en son sürüm sayfasına ulaşın.**](https://github.com/MematiBas42/MHRS-OtomatikRandevu/releases/latest)
2.  **Dosyayı İndirin:**
    -   `MHRS-OtomatikRandevu-win-x64.zip` isimli dosyayı indirin.
3.  **Arşivden Çıkarın:**
    -   İndirdiğiniz `.zip` dosyasına sağ tıklayın ve "Tümünü Ayıkla" veya "Extract All" seçeneği ile bir klasöre çıkarın.
4.  **Ayarları Yapılandırın (İsteğe Bağlı):**
    -   Çıkardığınız klasörün içinde `appsettings.json` dosyasını bir metin editörü (Not Defteri gibi) ile açın ve Telegram bilgilerinizi girin.
5.  **Uygulamayı Çalıştırın:**
    -   Klasördeki `MHRS-OtomatikRandevu.exe` dosyasına çift tıklayarak uygulamayı başlatın.

---

### 🐧 Linux & 📱 Termux (Tek Komutla Kurulum)

Aşağıdaki komutu terminalinize yapıştırıp çalıştırın. Bu betik, sizin için en son sürümü indirip kuracak ve uygulamayı başlatacaktır.

```bash
bash <(curl -sSL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh)
```

**Kurulum Betiğinin Özellikleri:**
-   **Akıllı Güncelleme:** Betiği her çalıştırdığınızda, yeni bir sürüm varsa `appsettings.json` ve oturum bilgilerinizi koruyarak uygulamayı günceller.
-   **Otomatik Kısayol:** İlk kurulumda, terminali yeniden başlattıktan sonra sadece `mhrs` yazarak uygulamayı çalıştırabilmeniz için bir kısayol (alias) ekler.
-   **Bağımlılık Yönetimi:** `dotnet` gibi gerekli bağımlılıkları sizin için kurar veya kurmanız için yönlendirme yapar.

---

### ⚙️ Telegram Ayarlarını Yapılandırma

Uygulamanın kurulduğu klasörde (`$HOME/mhrs_randevu` veya `.exe`'nin olduğu klasör) bulunan `appsettings.json` dosyasını düzenleyerek bildirimleri etkinleştirebilirsiniz.

```json
{
  "TELEGRAM_API_TOKEN": "BURAYA_BOTFATHERDAN_ALINAN_TOKEN",
  "TELEGRAM_CHAT_ID": "BURAYA_KENDI_CHAT_IDNIZ",
  "isLogging": "true",
  "PlayAlarmOnFound": "true",
  "MinimumMinutesToAppointment": "60"
}
```
> ⚠️ **Uyarı:** Telegram botunuzun size mesaj gönderebilmesi için, bota en az bir kere mesaj atmalısınız (örneğin: `/start`).

---

## 💻 Geliştiriciler İçin

Bu proje, `.NET 8` ile yazılmıştır. Geliştirme yapmak isteyenler için:

### Gerekli Araçlar
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Çalıştırma
```bash
git clone https://github.com/MematiBas42/MHRS-OtomatikRandevu.git
cd MHRS-OtomatikRandevu
# appsettings.json dosyasını manuel olarak yapılandırın
dotnet run
```

---
📬 Geri bildirim, öneri veya katkı sağlamak isterseniz [issue](https://github.com/MematiBas42/MHRS-OtomatikRandevu/issues) veya [pull request](https://github.com/MematiBas42/MHRS-OtomatikRandevu/pulls) oluşturabilirsiniz.

Bu proje geliştirilirken [enescaakir/MHRS-OtomatikRandevu](https://github.com/enescaakir/MHRS-OtomatikRandevu) deposundan faydalanılmıştır. @enescaakir'a teşekkürler.
