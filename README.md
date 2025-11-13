# 🏥 MHRS Otomatik Randevu Botu

Bu proje, **Türkiye'deki Merkezi Hekim Randevu Sistemi (MHRS)** üzerinden belirlediğiniz kriterlere göre **otomatik olarak randevu arayan** ve uygun bir randevu bulunduğunda sizi anında bilgilendiren bir **.NET 8 konsol uygulamasıdır**.

Uygulama, modern .NET özellikleri kullanılarak **taşınabilir, performanslı ve trim-uyumlu** olacak şekilde tamamen yeniden düzenlenmiştir.

---

## 🚀 Temel Özellikler

### ✅ Çoklu ve Detaylı Kriterlerle Arama
- **Coğrafi Seçim**: Birden fazla il, ilçe ve alt bölge (örneğin: *İstanbul Avrupa/Anadolu*) seçebilirsiniz.
- **Tıbbi Seçim**: Birden fazla klinik, hastane, poliklinik ve muayene yeri seçilebilir.
- **Hekim Seçimi**: Belirli doktorlar veya “Farketmez” seçeneği ile tüm doktorlar arasında arama yapılabilir.

### 🧠 Gelişmiş Filtreleme
- **Tarih Aralığı**: Belirli başlangıç ve bitiş tarihleri arasında arama yapılabilir.
- **Saat Filtresi**:
  - Sadece belirli saatlerde (örneğin: *14:00, 15:00*) arama
  - Belirli saatler hariç tüm saatlerde arama (örneğin: *08:00 ve 09:00 hariç*)

### 🔒 Akıllı Oturum Yönetimi
- **Otomatik Giriş**: Geçerli bir oturum (token) dosyası bulunduğunda, şifre sormadan otomatik olarak giriş yapar.
- **Otomatik Token Yenileme**: Oturum süresi dolduğunda veya API hatası alındığında, program yeni token alarak çalışmaya devam eder.
- **Güvenli Bilgi Saklama**: Token, bir sonraki çalıştırmada tekrar kullanılmak üzere **T.C. kimlik numarasına özel** olarak saklanır.

### 🤖 Otomatik Randevu Alma ve Bildirim
- **Anında Randevu Alma**: Uygun randevu bulunduğunda otomatik olarak alınır.
- **Akıllı Yeniden Rezervasyon**: Daha erken bir randevu bulunduğunda mevcut randevuyu iptal eder ve yeni randevuyu alır.
- **Telegram Bildirimi**: Randevu detayları anında Telegram üzerinden gönderilir.
- **Sesli Alarm (isteğe bağlı)**: Randevu bulunduğunda sesli uyarı verir (Sadece Windows).

### 💡 Kullanıcı Dostu Arayüz ve Hata Yönetimi
- Adım adım ilerleyen, anlaşılır konsol menüleri.
- Hatalı girişlerde veya API hatalarında (örneğin: *"Servis kullanım sınırına takıldınız"*) kullanıcıyı bilgilendirme.
- Tüm işlemler **kimlik numarasına özel** log dosyalarında tutulur.

---

## 🛠️ Nasıl Kullanılır? (Son Kullanıcılar İçin)

### 🚀 Tek Komutla Kurulum ve Güncelleme!

Uygulamayı kurmak veya mevcut kurulumunuzu en son sürüme güncellemek için tek yapmanız gereken, terminalinizde aşağıdaki komutu çalıştırmak:

```bash
curl -sL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh | bash
```

Bu betik, platformunuzu (Windows, Linux, Termux) otomatik olarak algılar, gerekli bağımlılıkları kurar (sudo şifreniz istenebilir), en son sürümü indirir ve uygulamayı başlatır.

#### ✨ Betiğin Özellikleri:
-   **Akıllı Güncelleme:** Mevcut kurulumunuzu kontrol eder. Yeni bir sürüm varsa, `appsettings.json` ve `token_*.txt` dosyalarınızı koruyarak sadece uygulama dosyalarını günceller.
-   **Otomatik Alias:** İlk kurulumda, uygulamayı kolayca başlatmak için `mhrs` adında bir kısayol (alias) oluşturur. Terminali yeniden başlattıktan sonra sadece `mhrs` yazarak uygulamayı çalıştırabilirsiniz.
-   **Platforma Özel Bağımlılıklar:** Linux dağıtımları için (Ubuntu/Debian, Fedora, Arch) gerekli sistem kütüphanelerini otomatik olarak kurar.
-   **Termux Desteği:** Termux ortamında .NET SDK'sını otomatik olarak kurar ve uygulamayı çalıştırır.
-   **Otomatik Başlatma:** Kurulum/güncelleme tamamlandıktan sonra uygulamayı otomatik olarak başlatır.

### 2️⃣ Ayarları Yapılandırma

Uygulama ilk kez çalıştırıldığında veya güncellendiğinde, `appsettings.json` dosyanız `$HOME/mhrs_randevu/` klasöründe bulunacaktır. Bu dosyayı bir metin editörü ile açarak Telegram bildirimleri ve diğer ayarları yapılandırabilirsiniz:

```json
{
  "TELEGRAM_API_TOKEN": "BURAYA_BOTFATHERDAN_TELEGRAM_API_TOKEN",
  "TELEGRAM_CHAT_ID": "BURAYA_TELEGRAM_CHAT_ID",
  "isLogging": "true",
  "PlayAlarmOnFound": "true",
  "MinimumMinutesToAppointment": "60"
}
```

#### 🔹 `TELEGRAM_API_TOKEN` Ayarı
- Telegram’da `@BotFather` ile `/newbot` komutunu kullanarak bot oluşturun.
- Size verilen token’ı yukarıdaki `"..."` kısmına yapıştırın.

#### 🔹 `TELEGRAM_CHAT_ID` Ayarı
- Telegram’da `@raw_info_bot`'u açın ve `/start` yazın.
- Bot size kullanıcı bilgilerinizi gönderecek. `chat -> id` kısmındaki sayıyı `"..."` kısmına yazın.

> ⚠️ **Uyarı:** Telegram botunuzun size mesaj gönderebilmesi için, bota en az bir kere mesaj atmalısınız (örneğin: *Merhaba*).

### 3️⃣ Uygulamayı Çalıştırma

Kurulum betiği uygulamayı otomatik olarak başlatacaktır. Sonraki çalıştırmalar için:

-   Terminali yeniden başlattıktan sonra sadece `mhrs` yazarak uygulamayı başlatabilirsiniz.
-   Veya `$HOME/mhrs_randevu` klasörüne gidip manuel olarak çalıştırabilirsiniz:
    -   **Windows'ta:** `.\MHRS-OtomatikRandevu-win-x64.exe`
    -   **Linux'ta:** `./MHRS-OtomatikRandevu-linux-x64`
    -   **Termux'ta:** `dotnet MHRS-OtomatikRandevu.dll`

İlk çalıştırmada T.C. kimlik numaranız ve MHRS şifreniz istenecek. Menüleri kullanarak randevu kriterlerinizi belirleyin. Program arka planda sürekli arama yapacaktır.

---

## 💻 Geliştiriciler İçin

Bu proje, `.NET 8` ile yazılmıştır. Geliştirme yapmak isteyenler için:

### 🔧 Gerekli Araçlar
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### ▶️ Çalıştırma
```bash
git clone https://github.com/mematibas42/MHRS-OtomatikRandevu.git
cd MHRS-OtomatikRandevu
# appsettings.json dosyasını oluşturup yapılandırın
dotnet run
```

---
📬 Geri bildirim, öneri veya katkı sağlamak isterseniz [issue](https://github.com/mematibas42/MHRS-OtomatikRandevu/issues) veya [pull request](https://github.com/mematibas42/MHRS-OtomatikRandevu/pulls)  oluşturabilirsiniz.

Bu proje geliştirilirken [enescaakir/MHRS-OtomatikRandevu](https://github.com/enescaakir/MHRS-OtomatikRandevu) deposundan faydalanılmıştır.
@enescaakir 'a teşekkürler.
---

Umarım sizin için faydalı olur! 🎉