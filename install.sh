#!/bin/bash

# MHRS-OtomatikRandevu için Akıllı Kurulum ve Güncelleme Betiği (v3 - Kurşun Geçirmez)
# Platformu algılar, bağımlılıkları kurar, en son sürümü indirir,
# ayarları korur ve evrensel bir başlatıcı betik oluşturur.

set -e
set -o pipefail

# --- Değişkenler ---
REPO="MematiBas42/MHRS-OtomatikRandevu"
INSTALL_DIR="$HOME/mhrs_randevu"
LAUNCHER_DIR="$HOME/.local/bin"
LAUNCHER_PATH="$LAUNCHER_DIR/mhrs"
LATEST_RELEASE_URL="https://api.github.com/repos/$REPO/releases/latest"
APP_DLL="MHRS-OtomatikRandevu.dll"
VERSION_FILE="$INSTALL_DIR/version.txt"
CONFIG_FILE="appsettings.json"
TOKEN_PATTERN="token_*.txt"

# --- Renkler ve Yardımcı Fonksiyonlar ---
echo_info() { printf "\033[1;34m%s\033[0m\n" ""; }
echo_success() { printf "\033[1;32m%s\033[0m\n" ""; }
echo_error() { printf "\033[1;31m%s\033[0m\n" "" >&2; }
echo_warn() { printf "\033[1;33m%s\033[0m\n" ""; }

# Gerekli araçların varlığını kontrol et
check_common_deps() {
    for dep in curl grep cut sed unzip; do
        if ! command -v "$dep" >/dev/null 2>&1; then
            echo_error "HATA: Gerekli araç '$dep' bulunamadı. Lütfen kurup tekrar deneyin."
            exit 1
        fi
    done
}

# En son sürüm etiketini GitHub API'sinden al
get_latest_remote_version() {
    curl -s "$LATEST_RELEASE_URL" | grep '"tag_name":' | head -n 1 | cut -d '"' -f 4
}

# Evrensel başlatıcı betiği oluşturma ve PATH kontrolü
create_launcher() {
    local platform_type=""
    local launcher_content

    echo_info "'$LAUNCHER_PATH' adresinde başlatıcı betik oluşturuluyor..."
    mkdir -p "$LAUNCHER_DIR"

    # Platforma özel başlatıcı içeriğini belirle
    if [ "$platform_type" = "termux" ]; then
        launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
dotnet $APP_DLL \"\$@\""
    elif [ "$platform_type" = "windows" ]; then
        launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu.exe \"\$@\""
    else # linux
        launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu \"\$@\""
    fi

    # Betiği oluştur ve çalıştırılabilir yap
    echo "$launcher_content" > "$LAUNCHER_PATH"
    chmod +x "$LAUNCHER_PATH"

    echo_success "✓ 'mhrs' komutu başarıyla oluşturuldu."

    # PATH kontrolü yap ve kullanıcıyı bilgilendir
    case ":$PATH:" in
        *":$LAUNCHER_DIR:"*)
            # Zaten PATH içinde, bir şey yapma
            ;;
        *)
            echo_warn "----------------------------------------------------------------"
            echo_warn "UYARI: '$LAUNCHER_DIR' dizini PATH değişkeninizde bulunmuyor."
            echo_info "Uygulamayı her yerden 'mhrs' komutuyla çalıştırmak için, lütfen aşağıdaki satırı kabuk yapılandırma dosyanıza (~/.bashrc, ~/.zshrc vb.) ekleyin:"
            echo_info "export PATH=\"\$HOME/.local/bin:\$PATH\""
            echo_info "Bu değişikliğin ardından terminalinizi yeniden başlatmanız gerekmektedir."
            echo_warn "----------------------------------------------------------------"
            ;;
    esac
}

# --- Kurulum ve Güncelleme Mantığı ---
perform_install_or_update() {
    local platform_type=""
    local asset_zip_name="$2"
    local is_first_install=false
    local local_version=""
    
    remote_version=$(get_latest_remote_version)
    if [ -z "$remote_version" ]; then
        echo_error "HATA: Uzak sürüm bilgisi alınamadı."
        exit 1
    fi

    if [ ! -d "$INSTALL_DIR" ] || [ ! -f "$VERSION_FILE" ]; then
        is_first_install=true
        echo_info "İlk kurulum algılandı."
    else
        local_version=$(cat "$VERSION_FILE")
    fi

    if [ "$local_version" = "$remote_version" ]; then
        echo_info "Uygulama zaten güncel (Sürüm: ${local_version})."
    else
        echo_info "Yeni sürüm bulundu: $remote_version. Kurulum/Güncelleme yapılıyor..."

        local temp_backup_dir
        temp_backup_dir=$(mktemp -d)
        
        if [ -d "$INSTALL_DIR" ]; then
            # Token ve config dosyalarını yedekle
            find "$INSTALL_DIR" -name "$CONFIG_FILE" -exec cp {} "$temp_backup_dir/" \;
            find "$INSTALL_DIR" -name "$TOKEN_PATTERN" -exec cp {} "$temp_backup_dir/" \;
            # Eski dosyaları sil
            rm -rf "$INSTALL_DIR"
        fi
        mkdir -p "$INSTALL_DIR"

        # İndir
        DOWNLOAD_URL=$(curl -s "$LATEST_RELEASE_URL" | grep "browser_download_url.*$asset_zip_name" | cut -d '"' -f 4)
        if [ -z "$DOWNLOAD_URL" ]; then
            echo_error "HATA: $asset_zip_name için indirme URL'si bulunamadı."
            exit 1
        fi
        
        echo_info "$asset_zip_name indiriliyor..."
        curl -L -o "$INSTALL_DIR/$asset_zip_name" "$DOWNLOAD_URL"
        
        # Çıkar ve temizle
        unzip -o "$INSTALL_DIR/$asset_zip_name" -d "$INSTALL_DIR"
        rm "$INSTALL_DIR/$asset_zip_name"
        
        # Yedekleri geri yükle
        find "$temp_backup_dir" -name "$CONFIG_FILE" -exec mv {} "$INSTALL_DIR/" \;
        find "$temp_backup_dir" -name "$TOKEN_PATTERN" -exec mv {} "$INSTALL_DIR/" \;
        rm -rf "$temp_backup_dir"

        # Yeni sürüm bilgisini kaydet
        echo "$remote_version" > "$VERSION_FILE"
        echo_success "✓ Kurulum/Güncelleme başarıyla tamamlandı! (Sürüm: $remote_version)"
    fi

    # Her kurulum/güncellemede başlatıcıyı oluştur/güncelle
    create_launcher "$platform_type"

    # Uygulamayı doğrudan çalıştırma (sadece bu betik üzerinden çalıştırıldığında)
    echo_info "Kurulum sonrası ilk çalıştırma yapılıyor..."
    cd "$INSTALL_DIR"
    if [ "$platform_type" = "termux" ]; then
        dotnet $APP_DLL "$@"
    elif [ "$platform_type" = "windows" ]; then
        ./MHRS-OtomatikRandevu.exe "$@"
    else # linux
        ./MHRS-OtomatikRandevu "$@"
    fi
}

# --- Ana Betik Mantığı ---
main() {
    check_common_deps

    if [ -d "/data/data/com.termux" ]; then
        echo_info "Termux ortamı algılandı."
        if ! command -v dotnet >/dev/null 2>&1; then
                echo_warn ".NET 8 SDK'sı bulunamadı. Kuruluyor (Bu işlem biraz zaman alabilir)..."
                # dpkg'nin interaktif soru sormasını engelle, mevcut yapılandırmayı koru ve tam yükseltme yap
                DEBIAN_FRONTEND=noninteractive pkg update -y -o Dpkg::Options::="--force-confold"
                DEBIAN_FRONTEND=noninteractive pkg upgrade -y -o Dpkg::Options::="--force-confold"
                pkg install -y curl unzip git dotnet-sdk-8.0
        fi
        perform_install_or_update "termux" "MHRS-OtomatikRandevu-termux-arm64.zip"
    else
        OS_TYPE=$(uname -s)
        ARCH_TYPE=$(uname -m)

        case "$OS_TYPE" in
            Linux)
                if [ "$ARCH_TYPE" = "x86_64" ]; then
                    echo_info "Linux (x64) ortamı algılandı."
                    echo_info "Gerekli sistem bağımlılıkları kontrol ediliyor (sudo şifreniz istenebilir)..."
                    if command -v apt-get >/dev/null 2>&1; then
                        sudo apt-get install -y libicu-dev libssl-dev > /dev/null
                    elif command -v dnf >/dev/null 2>&1; then
                        sudo dnf install -y libicu libssl > /dev/null
                    elif command -v pacman >/dev/null 2>&1; then
                        # Sadece paketler kurulu değilse kurmayı dene
                        if ! pacman -Q icu >/dev/null 2>&1; then
                            sudo pacman -S --needed --noconfirm icu > /dev/null
                        fi
                        if ! pacman -Q openssl >/dev/null 2>&1; then
                            sudo pacman -S --needed --noconfirm openssl > /dev/null
                        fi
                    fi
                    perform_install_or_update "linux" "MHRS-OtomatikRandevu-linux-x64.zip"
                else
                    echo_error "Desteklenmeyen Linux mimarisi: $ARCH_TYPE. Sadece x86_64 desteklenmektedir."
                    exit 1
                fi
                ;; 
            CYGWIN*|MINGW*|MSYS*) 
                if [ "$ARCH_TYPE" = "x86_64" ]; then
                    echo_info "Windows (x64) ortamı algılandı."
                    perform_install_or_update "windows" "MHRS-OtomatikRandevu-win-x64.zip"
                else
                    echo_error "Desteklenmeyen Windows mimarisi: $ARCH_TYPE."
                    exit 1
                fi
                ;; 
            *)
                echo_error "Desteklenmeyen işletim sistemi: $OS_TYPE"
                exit 1
                ;; 
        esac
    fi
}

main