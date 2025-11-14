#!/bin/bash

# MHRS-OtomatikRandevu için Akıllı Kurulum ve Güncelleme Betiği (v4 - Gelişmiş Kullanıcı Deneyimi)
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
 echo_info() { printf "\033[1;34m%s\033[0m\n" "$1"; }
 echo_success() { printf "\033[1;32m%s\033[0m\n" "$1"; }
 echo_error() { printf "\033[1;31m%s\033[0m\n" "$1" >&2; }
 echo_warn() { printf "\033[1;33m%s\033[0m\n" "$1"; }

# --- Betik Mantığı ---

check_common_deps() {
    echo_info "\n--- Aşama 1: Temel Araçlar Kontrol Ediliyor ---"
    for dep in curl grep cut sed unzip; do
        if ! command -v "$dep" >/dev/null 2>&1; then
            echo_error "HATA: Gerekli araç '$dep' bulunamadı. Lütfen kurup tekrar deneyin."
            exit 1
        fi
    done
    echo_success "✓ Temel araçlar mevcut."
}

get_latest_remote_version() {
    curl -s "$LATEST_RELEASE_URL" | grep '"tag_name":' | head -n 1 | cut -d '"' -f 4
}

create_launcher() {
    local platform_type="$1"
    local launcher_content
    local shell_rc_file

    echo_info "\n--- Aşama 4: 'mhrs' Komutu Oluşturuluyor ---"
    mkdir -p "$LAUNCHER_DIR"

    # Platforma özel başlatıcı içeriğini belirle
    if [ "$platform_type" = "termux" ]; then
        launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
dotnet $APP_DLL \"$@\""
    elif [ "$platform_type" = "windows" ]; then
        launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu.exe \"$@\""
    else # linux
        launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu \"$@\""
    fi

    # Betiği oluştur ve çalıştırılabilir yap
    echo "$launcher_content" > "$LAUNCHER_PATH"
    chmod +x "$LAUNCHER_PATH"
    echo_success "✓ Başlatıcı betik '$LAUNCHER_PATH' adresinde oluşturuldu."

    # PATH kontrolü yap ve gerekirse otomatik ekle
    case ":$PATH:" in
        *":$LAUNCHER_DIR:"*) 
            echo_success "✓ '$LAUNCHER_DIR' dizini PATH içinde zaten mevcut."
            ;;
        *)
            echo_warn "UYARI: '$LAUNCHER_DIR' dizini PATH değişkeninizde bulunmuyor."
            
            shell_name=$(basename "$SHELL")
            if [ "$shell_name" = "bash" ]; then
                shell_rc_file="$HOME/.bashrc"
            elif [ "$shell_name" = "zsh" ]; then
                shell_rc_file="$HOME/.zshrc"
            else
                shell_rc_file=""
            fi

            if [ -n "$shell_rc_file" ] && [ -f "$shell_rc_file" ]; then
                echo_info "PATH değişkeni '$shell_rc_file' dosyasına otomatik olarak ekleniyor..."
                if ! grep -q "export PATH=\"\$HOME/.local/bin:\\$PATH\"" "$shell_rc_file"; then
                    echo "" >> "$shell_rc_file"
                    echo "# MHRS Otomatik Randevu için PATH ayarı" >> "$shell_rc_file"
                    echo "export PATH=\"\$HOME/.local/bin:\\$PATH\"" >> "$shell_rc_file"
                    echo_success "✓ PATH ayarı eklendi."
                    echo_warn "Değişikliğin etkinleşmesi için terminalinizi yeniden başlatmanız gerekmektedir."
                else
                    echo_info "PATH ayarı '$shell_rc_file' içinde zaten mevcut."
                fi
            else
                echo_error "Desteklenmeyen kabuk. Lütfen '$LAUNCHER_DIR' dizinini manuel olarak PATH'inize ekleyin."
            fi
            ;;
    esac
}

perform_install_or_update() {
    local platform_type="$1"
    local asset_zip_name="$2"
    local is_first_install=false
    local local_version=""

    echo_info "\n--- Aşama 3: Kurulum ve Güncelleme ---"

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
        echo_success "✓ Uygulama zaten güncel (Sürüm: ${local_version})."
    else
        echo_info "Yeni sürüm bulundu: $remote_version. Kurulum/Güncelleme yapılıyor..."

        local temp_backup_dir
        temp_backup_dir=$(mktemp -d)

        if [ -d "$INSTALL_DIR" ]; then
            echo_info "Eski dosyalar yedekleniyor..."
            find "$INSTALL_DIR" -name "$CONFIG_FILE" -exec cp {} "$temp_backup_dir/" \;
            find "$INSTALL_DIR" -name "$TOKEN_PATTERN" -exec cp {} "$temp_backup_dir/" \;
            rm -rf "$INSTALL_DIR"
        fi
        mkdir -p "$INSTALL_DIR"

        DOWNLOAD_URL=$(curl -s "$LATEST_RELEASE_URL" | grep "browser_download_url.*$asset_zip_name" | cut -d '"' -f 4)
        if [ -z "$DOWNLOAD_URL" ]; then
            echo_error "HATA: $asset_zip_name için indirme URL'si bulunamadı."
            exit 1
        fi

        echo_info "Uygulama dosyaları indiriliyor..."
        curl -L -o "$INSTALL_DIR/$asset_zip_name" "$DOWNLOAD_URL"

        echo_info "Dosyalar arşivden (sessiz modda) çıkarılıyor..."
        unzip -oq "$INSTALL_DIR/$asset_zip_name" -d "$INSTALL_DIR"
        rm "$INSTALL_DIR/$asset_zip_name"

        if [ -d "$temp_backup_dir" ] && [ "$(ls -A "$temp_backup_dir")" ]; then
            echo_info "Eski ayarlar ve token dosyaları geri yükleniyor..."
            find "$temp_backup_dir" -name "$CONFIG_FILE" -exec mv {} "$INSTALL_DIR/" \;
            find "$temp_backup_dir" -name "$TOKEN_PATTERN" -exec mv {} "$INSTALL_DIR/" \;
        fi
        rm -rf "$temp_backup_dir"

        echo "$remote_version" > "$VERSION_FILE"
        echo_success "✓ Kurulum/Güncelleme başarıyla tamamlandı! (Sürüm: $remote_version)"
    fi

    create_launcher "$platform_type"

    echo_info "\n--- Kurulum Tamamlandı ---"
    echo_info "Uygulamayı başlatmak için ENTER tuşuna basın..."
    read -r

    echo_info "Uygulama ilk kez çalıştırılıyor..."
    cd "$INSTALL_DIR"
    if [ "$platform_type" = "termux" ]; then
        dotnet $APP_DLL
    elif [ "$platform_type" = "windows" ]; then
        ./MHRS-OtomatikRandevu.exe
    else # linux
        ./MHRS-OtomatikRandevu
    fi
}

main() {
    check_common_deps

    echo_info "\n--- Aşama 2: Platform ve Bağımlılıklar ---"

    if [ -d "/data/data/com.termux" ]; then
        echo_info "Termux ortamı algılandı."
        if ! command -v dotnet >/dev/null 2>&1; then
                echo_warn ".NET 8 SDK'sı bulunamadı. Kuruluyor..."
                DEBIAN_FRONTEND=noninteractive pkg update -y -o Dpkg::Options::=\"--force-confold\" >/dev/null
                DEBIAN_FRONTEND=noninteractive pkg upgrade -y -o Dpkg::Options::=\"--force-confold\" >/dev/null
                pkg install -y curl unzip git dotnet-sdk-8.0 >/dev/null
                echo_success "✓ Gerekli Termux bağımlılıkları kuruldu."
        else
            echo_success "✓ .NET 8 SDK zaten kurulu."
        fi
        perform_install_or_update "termux" "MHRS-OtomatikRandevu-termux-arm64.zip"
    else
        OS_TYPE=$(uname -s)
        ARCH_TYPE=$(uname -m)

        case "$OS_TYPE" in
            Linux)
                if [ "$ARCH_TYPE" = "x86_64" ]; then
                    echo_info "Linux (x64) ortamı algılandı."
                    echo_info "Gerekli sistem bağımlılıkları kontrol ediliyor..."
                    if command -v apt-get >/dev/null 2>&1; then
                        sudo apt-get install -y libicu-dev libssl-dev > /dev/null
                    elif command -v dnf >/dev/null 2>&1; then
                        sudo dnf install -y libicu libssl > /dev/null
                    elif command -v pacman >/dev/null 2>&1; then
                        if ! pacman -Q icu >/dev/null 2>&1; then sudo pacman -S --needed --noconfirm icu > /dev/null; fi
                        if ! pacman -Q openssl >/dev/null 2>&1; then sudo pacman -S --needed --noconfirm openssl > /dev/null; fi
                    fi
                    echo_success "✓ Gerekli Linux bağımlılıkları kontrol edildi."
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