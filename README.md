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

### 🚀 Platformunuza Özel Tek Komutla Kurulum

Aşağıdan işletim sisteminize uygun komutu kopyalayıp terminalinize yapıştırın. Bu komut, `curl` gibi gerekli araçları kuracak, en son sürümü indirip yükleyecek ve uygulamayı sizin için başlatacaktır.

---

#### 🐧 **Linux (Debian / Ubuntu)**
```bash
sudo apt-get update -y && sudo apt-get install -y curl && curl -sL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh | bash
```

---

####  Fedora / CentOS / RHEL
```bash
sudo dnf install -y curl && curl -sL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh | bash
```

---

#### **Arch Linux**
```bash
sudo pacman -Syu --noconfirm curl && curl -sL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh | bash
```

---

#### 📱 **Termux (Android)**
```bash
curl -sL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh | bash
```

---

#### 🪟 **Windows**
Windows'ta kurulum için **Git Bash** veya **WSL (Windows Subsystem for Linux)** kullanmanız gerekmektedir. Bu ortamlardan birini açtıktan sonra, yukarıdaki **Debian/Ubuntu** komutunu kullanabilirsiniz.

---

### ✨ Kurulum Betiğinin Özellikleri
-   **Akıllı Güncelleme:** Mevcut kurulumunuzu kontrol eder. Yeni bir sürüm varsa, `appsettings.json` ve `token_*.txt` dosyalarınızı koruyarak sadece uygulama dosyalarını günceller.
-   **Otomatik Alias:** İlk kurulumda, uygulamayı kolayca başlatmak için `mhrs` adında bir kısayol (alias) oluşturur. Terminali yeniden başlattıktan sonra sadece `mhrs` yazarak uygulamayı çalıştırabilirsiniz.
-   **Platforma Özel Bağımlılıklar:** Gerekli sistem kütüphanelerini ve araçları sizin için otomatik olarak kurar.
-   **Otomatik Başlatma:** Kurulum/güncelleme tamamlandıktan sonra uygulamayı otomatik olarak başlatır.

### ⚙️ Ayarları Yapılandırma
Uygulama kurulduktan sonra, ayar dosyanız (`appsettings.json`) `$HOME/mhrs_randevu/` klasöründe bulunacaktır. Bu dosyayı bir metin editörü ile açarak Telegram bildirimleri ve diğer ayarları yapılandırabilirsiniz.

```json
{
  "TELEGRAM_API_TOKEN": "BURAYA_BOTFATHERDAN_TELEGRAM_API_TOKEN",
  "TELEGRAM_CHAT_ID": "BURAYA_TELEGRAM_CHAT_ID",
  "isLogging": "true",
  "PlayAlarmOnFound": "true",
  "MinimumMinutesToAppointment": "60"
}
```
> ⚠️ **Uyarı:** Telegram botunuzun size mesaj gönderebilmesi için, bota en az bir kere mesaj atmalısınız (örneğin: *Merhaba*).

### ▶️ Uygulamayı Çalıştırma
Kurulum betiği uygulamayı otomatik olarak başlatacaktır. Sonraki çalıştırmalar için:
-   Terminali yeniden başlattıktan sonra sadece `mhrs` yazarak uygulamayı başlatabilirsiniz.
-   Veya betiği tekrar çalıştırarak: `curl -sL https://raw.githubusercontent.com/MematiBas42/MHRS-OtomatikRandevu/master/install.sh | bash`

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