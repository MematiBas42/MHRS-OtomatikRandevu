#!/bin/bash

# MHRS-OtomatikRandevu için Akıllı Kurulum ve Güncelleme Betiği (v5.7 - PATH Düzeltmesi)
# Platformu ve mimariyi algılar, bağımlılıkları kurar, en son sürümü indirir,
# ayarları korur ve evrensel bir başlatıcı betik ile PATH ayarını otomatik yapar.

set -e
set -o pipefail

# --- Değişkenler ---
SCRIPT_VERSION="v5.7"
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
 echo_info() { echo -e "\033[1;34m$1\033[0m"; }
 echo_success() { echo -e "\033[1;32m$1\033[0m"; }
 echo_error() { echo -e "\033[1;31m$1\033[0m" >&2; }
 echo_warn() { echo -e "\033[1;33m$1\033[0m"; }

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
    local platform_identifier="$1" # e.g., 'win-x64', 'termux-arm'
    local launcher_content

    echo_info "\n--- Aşama 4: 'mhrs' Komutu Yapılandırılıyor ---"
    mkdir -p "$LAUNCHER_DIR"

    case "$platform_identifier" in
        termux-*) 
            launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
dotnet $APP_DLL \"$@\""
            ;; 
        win-*) 
            launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu.exe \"$@\""
            ;; 
        alpine-*) 
            launcher_content="#!/bin/sh
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
export COMPlus_GCServer=0
export COMPlus_GCHeapHardLimit=0x10000000 # 256MB Heap Limiti
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu \"$@\""
            ;; 
        *) 
            launcher_content="#!/bin/bash
# Bu betik 'install.sh' tarafından otomatik olarak oluşturulmuştur.
cd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu \"$@\""
            ;; 
    esac

    echo "$launcher_content" > "$LAUNCHER_PATH"
    chmod +x "$LAUNCHER_PATH"
    echo_success "✓ Başlatıcı betik '$LAUNCHER_PATH' adresinde oluşturuldu."

    case ":$PATH:" in
        *:":$LAUNCHER_DIR:"*) 
            echo_success "✓ '$LAUNCHER_DIR' dizini PATH içinde zaten mevcut."
            ;; 
        *)
            echo_warn "UYARI: '$LAUNCHER_DIR' dizini PATH değişkeninizde bulunmuyor."
            
            shell_name=$(basename "$SHELL")
            local export_line="export PATH=\"\$HOME/.local/bin:\$PATH\""
            
            if [ "$shell_name" = "bash" ]; then
                files_to_try=("$HOME/.bashrc")
            elif [ "$shell_name" = "zsh" ]; then
                files_to_try=("$HOME/.zshrc")
            elif [ "$shell_name" = "ash" ] || [ "$shell_name" = "sh" ]; then
                # Brute-force for ash/sh by writing to all common files
                files_to_try=("$HOME/.profile" "$HOME/.ashrc" "$HOME/.shrc")
            else
                files_to_try=()
            fi

            if [ ${#files_to_try[@]} -gt 0 ]; then
                echo_info "PATH değişkeni şu dosyalara yazılıyor: ${files_to_try[*]}"
                for rc_file in "${files_to_try[@]}"; do
                    if ! grep -q 'export PATH="$HOME/.local/bin:$PATH"' "$rc_file" 2>/dev/null; then
                        echo "" >> "$rc_file"
                        echo "# MHRS Otomatik Randevu için PATH ayarı" >> "$rc_file"
                        echo "$export_line" >> "$rc_file"
                    fi
                done
                echo_success "✓ PATH ayarları ilgili yapılandırma dosyalarına yazıldı."
                echo_warn "Değişikliğin etkinleşmesi için terminalinizi yeniden başlatmanız gerekmektedir."
            else
                echo_error "Desteklenmeyen kabuk ($shell_name). Lütfen '$LAUNCHER_DIR' dizinini manuel olarak PATH'inize ekleyin."
            fi
            ;;
    esac
}

perform_install_or_update() {
    local platform_identifier="$1"
    local asset_zip_name="$2"

    echo_info "\n--- Aşama 3: Kurulum ve Güncelleme ---"

    remote_version=$(get_latest_remote_version)
    if [ -z "$remote_version" ]; then
        echo_error "HATA: Uzak sürüm bilgisi alınamadı."
        exit 1
    fi

    if [ ! -d "$INSTALL_DIR" ] || [ ! -f "$VERSION_FILE" ]; then
        echo_info "İlk kurulum algılandı."
    else
        local_version=$(cat "$VERSION_FILE")
        if [ "$local_version" = "$remote_version" ]; then
            echo_success "✓ Uygulama zaten güncel (Sürüm: ${local_version})."
            create_launcher "$platform_identifier"
        else
            echo_info "Yeni sürüm bulundu: $remote_version. İndiriliyor..."

            local temp_backup_dir=$(mktemp -d)

            if [ -d "$INSTALL_DIR" ]; then
                echo_info "Eski ayarlar yedekleniyor..."
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

            echo_info "Dosyalar arşivden çıkarılıyor..."
            unzip -oq "$INSTALL_DIR/$asset_zip_name" -d "$INSTALL_DIR"
            rm "$INSTALL_DIR/$asset_zip_name"

            if [ -d "$temp_backup_dir" ] && [ "$(ls -A "$temp_backup_dir")" ]; then
                echo_info "Eski ayarlar geri yükleniyor..."
                find "$temp_backup_dir" -name "$CONFIG_FILE" -exec mv {} "$INSTALL_DIR/" \;
                find "$temp_backup_dir" -name "$TOKEN_PATTERN" -exec mv {} "$INSTALL_DIR/" \;
            fi
            rm -rf "$temp_backup_dir"

            echo "$remote_version" > "$VERSION_FILE"
            echo_success "✓ Kurulum/Güncelleme başarıyla tamamlandı! (Sürüm: $remote_version)"
            create_launcher "$platform_identifier"
        fi
    fi

    echo_info "\n--- Kurulum Tamamlandı ---"
    echo_success "✓ Sonraki sefer 'mhrs' yazarak uygulamayı direkt çalıştırabilirsiniz."
    echo_info "i Güncelleme kontrolü için bu kurulum betiğini yeniden çalıştırmanız yeterlidir."
    echo_info "\nUygulamayı şimdi başlatmak için ENTER tuşuna basın..."
    read -r
    
    echo_info "Uygulama başlatılıyor..."
    if [[ "$platform_identifier" == "alpine-"* ]]; then
        echo_info "(Alpine için GC bellek düzeltmeleri uygulanıyor...)"
        COMPlus_GCServer=0 COMPlus_GCHeapHardLimit=0x10000000 $LAUNCHER_PATH
    else
        $LAUNCHER_PATH
    fi
}

main() {
    echo_info "MHRS Otomatik Randevu Kurulum Betiği ${SCRIPT_VERSION}"
    check_common_deps
    echo_info "\n--- Aşama 2: Platform ve Bağımlılıklar Algılanıyor ---"
    
    OS_TYPE=$(uname -s)
    ARCH_TYPE=$(uname -m)
    
    if [ -d "/data/data/com.termux" ]; then
        echo_info "Termux ortamı algılandı."
        echo_info "Gerekli Termux paketleri kontrol ediliyor/güncelleniyor..."
        echo_warn "Bu işlem cihazınızın hızına ve internet bağlantınıza göre uzun sürebilir."
        
        DEBIAN_FRONTEND=noninteractive pkg update -y -o Dpkg::Options::=\"--force-confold\"
        DEBIAN_FRONTEND=noninteractive pkg upgrade -y -o Dpkg::Options::=\"--force-confold\"
        pkg install -y dotnet-sdk-8.0

        echo_success "✓ .NET 8 SDK ve Termux paketleri kontrol edildi."

        if [ "$ARCH_TYPE" = "aarch64" ]; then
            echo_info "Mimari: arm64"
            perform_install_or_update "termux-arm64" "MHRS-OtomatikRandevu-termux-arm64.zip"
        elif [[ "$ARCH_TYPE" == "arm"* ]]; then
            echo_info "Mimari: arm (32-bit)"
            perform_install_or_update "termux-arm" "MHRS-OtomatikRandevu-termux-arm.zip"
        else
            echo_error "Desteklenmeyen Termux Mimarisi: $ARCH_TYPE"
            exit 1
        fi
        return
    fi
    
    if [ -f "/etc/alpine-release" ]; then
        echo_info "Alpine Linux ortamı algılandı."
        if [ "$ARCH_TYPE" = "aarch64" ]; then
            echo_info "Mimari: arm64"
            echo_info "Gerekli Alpine paketleri kontrol ediliyor/kuruluyor..."
            if ! command -v sudo >/dev/null 2>&1; then
                apk add --no-cache sudo
            fi
            sudo apk add --no-cache icu-libs openssl lttng-ust
            echo_success "✓ Gerekli Alpine bağımlılıkları kontrol edildi."
            perform_install_or_update "alpine-arm64" "MHRS-OtomatikRandevu-alpine-arm64.zip"
        else
            echo_error "Desteklenmeyen Alpine Linux Mimarisi: $ARCH_TYPE. Sadece arm64 desteklenmektedir."
            exit 1
        fi
        return
    fi

    case "$OS_TYPE" in
        Linux)
            echo_info "Genel Linux ortamı algılandı."
            if [ "$ARCH_TYPE" = "x86_64" ]; then
                echo_info "Mimari: x86_64"
                echo_info "Gerekli sistem bağımlılıkları kontrol ediliyor..."
                if ! command -v sudo >/dev/null 2>&1; then
                    echo_error "'sudo' komutu bulunamadı. Lütfen bağımlılıkları manuel kurun: libicu, libssl"
                else
                    if command -v apt-get >/dev/null 2>&1;
                        then
                        sudo apt-get install -y libicu-dev libssl-dev > /dev/null
                    elif command -v dnf >/dev/null 2>&1;
                        then
                        sudo dnf install -y libicu libssl > /dev/null
                    elif command -v pacman >/dev/null 2>&1;
                        then
                        (sudo pacman -S --needed --noconfirm icu || echo_warn "UYARI: 'icu' paketi kurulamadı. Sisteminizi 'sudo pacman -Syu' ile güncellemeniz gerekebilir.")
                        (sudo pacman -S --needed --noconfirm openssl || echo_warn "UYARI: 'openssl' paketi kurulamadı.")
                    fi
                fi
                echo_success "✓ Gerekli Linux bağımlılıkları kontrol edildi."
                perform_install_or_update "linux-x64" "MHRS-OtomatikRandevu-linux-x64.zip"
            else
                echo_error "Desteklenmeyen Linux Mimarisi: $ARCH_TYPE. Sadece x86_64 desteklenmektedir."
                exit 1
            fi
            ;; 
        CYGWIN*|MINGW*|MSYS*) 
            echo_info "Windows (WSL/Git Bash) ortamı algılandı."
            if [ "$ARCH_TYPE" = "x86_64" ]; then
                echo_info "Mimari: x86_64"
                perform_install_or_update "win-x64" "MHRS-OtomatikRandevu-win-x64.zip"
            elif [ "$ARCH_TYPE" = "i686" ] || [ "$ARCH_TYPE" = "i386" ]; then
                echo_info "Mimari: x86 (32-bit)"
                perform_install_or_update "win-x86" "MHRS-OtomatikRandevu-win-x86.zip"
            else
                echo_error "Desteklenmeyen Windows Mimarisi: $ARCH_TYPE."
                exit 1
            fi
            ;; 
        *)
            echo_error "Desteklenmeyen İşletim Sistemi: $OS_TYPE"
            exit 1
            ;; 
    esac
}

main
