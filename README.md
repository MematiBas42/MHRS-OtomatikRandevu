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

### 1️⃣ Programı İndirme
1. Bu GitHub deposunun sağ tarafındaki **Releases (Sürümler)** sekmesine tıklayın.
2. En güncel sürüm altındaki **Assets** bölümünden işletim sisteminize uygun dosyayı indirin:
   - **Windows için:** `MHRS-OtomatikRandevu-win-x64.zip`
   - **Linux için:** `MHRS-OtomatikRandevu-linux-x64.zip`
   - **Termux (Android) için:** `MHRS-OtomatikRandevu-linux-arm64.zip`
3. `.zip` dosyasını bir klasöre çıkartın.

### 2️⃣ Ayarları Yapılandırma
1. Çıkarttığınız klasörde `appsettings.json` dosyasını bir metin editörü ile açın.
2. Aşağıdaki bölümü kendi bilgilerinize göre düzenleyin:

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

### 3️⃣ Programı Çalıştırma
- **Windows'ta:** `MHRS-OtomatikRandevu.exe` dosyasına çift tıklayın.
- **Linux veya Termux'ta:** Terminali açın, programın olduğu klasöre gidin (`cd <klasör_yolu>`) ve aşağıdaki komutları çalıştırın:
  ```bash
  chmod +x MHRS-OtomatikRandevu
  ./MHRS-OtomatikRandevu
  ```
- İlk çalıştırmada T.C. kimlik numaranız ve MHRS şifreniz istenecek.
- Menüleri kullanarak randevu kriterlerinizi belirleyin. Program arka planda sürekli arama yapacaktır.

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