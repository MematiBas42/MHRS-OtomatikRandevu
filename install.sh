#!/bin/bash

# MHRS-OtomatikRandevu için Akıllı Kurulum ve Güncelleme Betiği (v7.1 - Standart Dizin Kurulumu)
# Bu sürüm, kısayolu /usr/local/bin altına kurarak PATH sorunlarını tamamen ortadan kaldırır.

set -e
set -o pipefail

# --- Değişkenler ---
SCRIPT_VERSION="v7.1"
REPO="MematiBas42/MHRS-OtomatikRandevu"
INSTALL_DIR="$HOME/mhrs_randevu"
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
    # sudo is now a mandatory dependency for installing the launcher
    for dep in curl grep cut sed unzip sudo; do
        if ! command -v "$dep" >/dev/null 2>&1; then
            echo_error "HATA: Gerekli araç '$dep' bulunamadı. Lütfen kurup tekrar deneyin."
            exit 1
        fi
    done
    echo_success "✓ Temel araçlar mevcut."
}

get_latest_remote_version() {
    local api_output
    api_output=$(curl -s -L "$LATEST_RELEASE_URL")
    
    if [ -z "$api_output" ]; then
        echo_error "HATA: curl komutundan boş yanıt alındı. Github API hız limitine takılmış olabilirsiniz."
        echo "" && return
    fi

    local tag_name
    tag_name=$(echo "$api_output" | grep '"tag_name":' | head -n 1 | cut -d '"' -f 4 || true)

    if [ -z "$tag_name" ]; then
        echo_error "HATA: Alınan yanıttan sürüm (tag_name) bilgisi ayıklanamadı."
        echo "--> API Yanıtı (ilk 5 satır):" >&2
        echo "$api_output" | head -n 5 >&2
        echo "" && return
    fi
    
    echo "$tag_name"
}

create_launcher() {
    local platform_identifier="$1"
    local launcher_content
    local LAUNCHER_DIR="/usr/local/bin"
    local LAUNCHER_PATH="$LAUNCHER_DIR/mhrs"

    echo_info "\n--- Aşama 4: 'mhrs' Komutu Standart Dizine Kuruluyor ---"
    
    case "$platform_identifier" in
        termux-*) launcher_content="#!/bin/bash\ncd \"$INSTALL_DIR\"\ndotnet $APP_DLL \"\$@\"" ;; 
        win-*) launcher_content="#!/bin/bash\ncd \"$INSTALL_DIR\"\n./MHRS-OtomatikRandevu.exe \"\$@\"" ;; 
        alpine-*) launcher_content="#!/bin/sh\nexport COMPlus_GCServer=0\nexport COMPlus_GCHeapHardLimit=0x10000000\ncd \"$INSTALL_DIR\"\n./MHRS-OtomatikRandevu \"\$@\"" ;; 
        *) launcher_content="#!/bin/bash\ncd \"$INSTALL_DIR\"\n./MHRS-OtomatikRandevu \"\$@\"" ;; 
    esac

    local temp_launcher
    temp_launcher=$(mktemp)
    echo -e "$launcher_content" > "$temp_launcher"
    
    echo_info "--> Başlatıcı betik '$LAUNCHER_PATH' adresine kopyalanıyor (root yetkisi gerekebilir)..."
    
    # Use sudo to move the file and set permissions.
    # This works for both root users (sudo does nothing) and non-root users with sudo rights.
    sudo mv "$temp_launcher" "$LAUNCHER_PATH"
    sudo chmod +x "$LAUNCHER_PATH"
    
    echo_success "✓ Başlatıcı betik '$LAUNCHER_PATH' adresine başarıyla kuruldu."
    echo_success "✓ 'mhrs' komutu artık sistem genelinde kullanılabilir olmalı."
}

perform_install_or_update() {
    local platform_identifier="$1"
    local asset_zip_name="$2"

    echo_info "\n--- Aşama 3: Kurulum ve Güncelleme ---"

    local remote_version
    remote_version=$(get_latest_remote_version)
    
    if [ -z "$remote_version" ]; then
        echo_error "HATA: Uzak sürüm bilgisi alınamadı. Betik sonlandırılıyor."
        exit 1
    fi

    local current_version=""
    if [ -f "$VERSION_FILE" ]; then
        current_version=$(cat "$VERSION_FILE")
    fi

    if [ "$current_version" = "$remote_version" ]; then
        echo_success "✓ Uygulama zaten güncel (Sürüm: ${current_version})."
        create_launcher "$platform_identifier"
        echo_info "\n--- Kurulum Tamamlandı ---"
        echo_success "✓ 'mhrs' komutunu direkt kullanarak uygulamayı çalıştırabilirsiniz."
        echo_info "\nUygulamayı şimdi başlatmak için ENTER tuşuna basın veya çıkmak için CTRL+C yapın."
        read -r
        mhrs
        exit 0
elif [ -z "$current_version" ]; then
        echo_info "İlk kurulum: Sürüm $remote_version indiriliyor..."
    else
        echo_info "Yeni sürüm bulundu: $remote_version (Mevcut: $current_version). Güncelleniyor..."
    fi

    local temp_backup_dir
    temp_backup_dir=$(mktemp -d)

    if [ -d "$INSTALL_DIR" ]; then
        echo_info "Eski ayarlar yedekleniyor..."
        find "$INSTALL_DIR" -name "$CONFIG_FILE" -exec cp {} "$temp_backup_dir/" \;
        find "$INSTALL_DIR" -name "$TOKEN_PATTERN" -exec cp {} "$temp_backup_dir/" \;
        rm -rf "$INSTALL_DIR"
    fi
    mkdir -p "$INSTALL_DIR"

    DOWNLOAD_URL=$(curl -sL "$LATEST_RELEASE_URL" | grep "browser_download_url.*$asset_zip_name" | cut -d '"' -f 4 || true)
    if [ -z "$DOWNLOAD_URL" ]; then
        echo_error "HATA: '$asset_zip_name' için indirme URL'si bulunamadı."
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

    echo_info "\n--- Kurulum Tamamlandı ---"
    echo_success "✓ 'mhrs' komutunu direkt kullanarak uygulamayı çalıştırabilirsiniz."
    echo_info "\nUygulamayı şimdi başlatmak için ENTER tuşuna basın veya çıkmak için CTRL+C yapın."
    read -r
    
    echo_info "Uygulama başlatılıyor..."
    mhrs
}

main() {
    echo_info "MHRS Otomatik Randevu Kurulum Betiği ${SCRIPT_VERSION}"
    check_common_deps
    echo_info "\n--- Aşama 2: Platform ve Bağımlılıklar Algılanıyor ---"
    
    OS_TYPE=$(uname -s)
    ARCH_TYPE=$(uname -m)
    
    # Platform detection logic can be simplified now as we don't need shell-specific PATH logic
    if [ -f "/etc/alpine-release" ]; then
        echo_info "Alpine Linux ortamı algılandı."
         if [ "$ARCH_TYPE" = "aarch64" ]; then
             perform_install_or_update "alpine-arm64" "MHRS-OtomatikRandevu-alpine-arm64.zip"
        else
            echo_error "Desteklenmeyen Alpine Mimarisi: $ARCH_TYPE"
            exit 1
        fi
    elif [ -d "/data/data/com.termux" ]; then
        echo_info "Termux ortamı algılandı."
        echo_warn "Termux üzerindeki bağımlılık kontrolü (dotnet-sdk-8.0) şu anda devre dışıdır ve manuel kurulum gerektirebilir."
        perform_install_or_update "termux-arm64" "MHRS-OtomatikRandevu-termux-arm64.zip"
    elif [ "$OS_TYPE" = "Linux" ]; then
        echo_info "Genel Linux ortamı algılandı."
         if [ "$ARCH_TYPE" = "x86_64" ]; then
            perform_install_or_update "linux-x64" "MHRS-OtomatikRandevu-linux-x64.zip"
        else
            echo_error "Desteklenmeyen Linux Mimarisi: $ARCH_TYPE"
            exit 1
        fi
    elif [[ "$OS_TYPE" == "CYGWIN"* || "$OS_TYPE" == "MINGW"* || "$OS_TYPE" == "MSYS"* ]]; then
        echo_info "Windows (WSL/Git Bash) ortamı algılandı."
        if [ "$ARCH_TYPE" = "x86_64" ]; then
            perform_install_or_update "win-x64" "MHRS-OtomatikRandevu-win-x64.zip"
        else
            perform_install_or_update "win-x86" "MHRS-OtomatikRandevu-win-x86.zip"
        fi
    else
        echo_error "Desteklenmeyen İşletim Sistemi: $OS_TYPE"
        exit 1
    fi
}

main