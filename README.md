
# 🏥 MHRS Otomatik Randevu Botu

Bu proje, **Türkiye'deki Merkezi Hekim Randevu Sistemi (MHRS)** üzerinden belirlediğiniz kriterlere göre **otomatik olarak randevu arayan** ve uygun bir randevu bulunduğunda sizi anında bilgilendiren bir **.NET konsol uygulamasıdır**.

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

- **Otomatik Token Yenileme**: Oturum süresi dolduğunda veya başka bir cihazdan giriş yapıldığında, program yeni token alarak çalışmaya devam eder.
- **Güvenli Bilgi Saklama**: Token, bir sonraki çalıştırmada tekrar kullanılmak üzere **T.C. kimlik numarasına özel** olarak saklanır.

### 🤖 Otomatik Randevu Alma ve Bildirim

- **Anında Randevu Alma**: Uygun randevu bulunduğunda otomatik olarak alınır.
- **Akıllı Yeniden Rezervasyon**: Daha erken bir randevu bulunduğunda mevcut randevuyu iptal eder ve yeni randevuyu alır.
- **Telegram Bildirimi**: Randevu detayları anında Telegram üzerinden gönderilir.
- **Sesli Alarm (isteğe bağlı)**: Randevu bulunduğunda sesli uyarı verir.

### 💡 Kullanıcı Dostu Arayüz ve Hata Yönetimi

- Adım adım ilerleyen, anlaşılır konsol menüleri
- Hatalı girişlerde veya API hatalarında (örneğin: *"Servis kullanım sınırına takıldınız"*) kullanıcıyı bilgilendirme
- Tüm işlemler **kimlik numarasına özel** log dosyalarında tutulur

---

## 🛠️ Nasıl Kullanılır? (Son Kullanıcılar İçin)

### 1️⃣ Programı İndirme

1. Bu GitHub deposunun sağ tarafındaki **Releases (Sürümler)** sekmesine tıklayın.
2. En güncel sürüm altında yer alan `MHRS-OtomatikRandevu.zip` dosyasını indirin.
3. `.zip` dosyasını çıkartın.

### 2️⃣ Telegram Bildirimlerini Ayarlama

1. Çıkarttığınız klasörde `MHRS-OtomatikRandevu.dll.config` dosyasını açın.
2. Aşağıdaki bölümü kendi bilgilerinize göre düzenleyin:

```xml
<appSettings>
    <!-- Telegram Bildirimleri için Ayarlar -->
    <add key="TELEGRAM_API_TOKEN" value="BURAYA_KENDI_BOT_TOKENINIZI_YAPISTIRIN" />
    <add key="TELEGRAM_CHAT_ID" value="BURAYA_KENDI_CHAT_ID_NIZI_YAPISTIRIN" />

    <!-- Program Ayarları -->
    <add key="isLogging" value="true" />
    <add key="PlayAlarmOnFound" value="true" />
</appSettings>
```

#### 🔹 `TELEGRAM_API_TOKEN` Ayarı

- Telegram’da `@BotFather` ile `/newbot` komutunu kullanarak bot oluşturun.
- Size verilen token’ı yukarıdaki `"value="..."` kısmına yapıştırın.

#### 🔹 `TELEGRAM_CHAT_ID` Ayarı

- Telegram’da `@raw_info_bot`'u açın ve `/start` yazın.
- Bot size kullanıcı bilgilerinizi gönderecek. `chat -> id` kısmındaki sayıyı `"value="..."` kısmına yazın.

> ⚠️ **Uyarı:** Telegram botunuzun size mesaj gönderebilmesi için, bota en az bir kere mesaj atmalısınız (örneğin: *Merhaba*).

#### 🔹 `PlayAlarmOnFound`

- `true` → Randevu bulunduğunda ses çalar.
- `false` → Sessiz çalışır.

### 3️⃣ Programı Çalıştırma

1. Ayarları yaptıktan sonra `MHRS-OtomatikRandevu.exe` dosyasına çift tıklayın.
2. T.C. kimlik numaranız ve MHRS şifreniz istenecek.
3. Menüleri kullanarak randevu kriterlerinizi belirleyin.
4. Program arka planda sürekli arama yapacaktır. Konsol penceresini açık bırakmanız yeterlidir.

---

## 💻 Geliştiriciler İçin

Bu proje, `.NET 8` ile yazılmıştır. Geliştirme yapmak isteyenler için:

### 🔧 Gerekli Araçlar

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### ▶️ Çalıştırma

```bash
git clone https://github.com/mematibas42/MHRS-OtomatikRandevu.git
cd MHRS-OtomatikRandevu
dotnet run
```

> Not: `App.config` dosyasını oluşturup yukarıdaki örneğe göre yapılandırmanız gerekir.

---
☕ Bir kahve ısmarlamak isterseniz:
[coff.ee/mematibas42](https://coff.ee/mematibas42) aracılığıyla destek olabilir veya;

Tron (trc20) USDT: TKkw5XRWXeZk4GHA1GibtcNpu8YpN9zzbJ
Adresine USDT transfer edebilirsiniz. 

📬 Geri bildirim, öneri veya katkı sağlamak isterseniz [issue](https://github.com/mematibas42/MHRS-OtomatikRandevu/issues) veya [pull request](https://github.com/mematibas42/MHRS-OtomatikRandevu/pulls)  oluşturabilirsiniz.

Bu proje geliştirilirken [enescaakir/MHRS-OtomatikRandevu](https://github.com/enescaakir/MHRS-OtomatikRandevu) deposundan faydalanılmıştır.
@enescaakir 'a teşekkürler.
---

Umarım sizin için faydalı olur! 🎉
