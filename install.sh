#!/bin/bash

# MHRS-OtomatikRandevu için Akıllı Kurulum ve Güncelleme Betiği (v7.0 - "Sihirsiz" Stabil Sürüm)
# Platformu ve mimariyi algılar, bağımlılıkları kurar, en son sürümü indirir,
# ayarları korur ve evrensel bir başlatıcı betik ile PATH ayarını otomatik yapar.
# Bu sürüm, kullanıcı isteği üzerine RC dosyalarında temizlik yapmaz, sadece ekleme yapar.

set -e
set -o pipefail

# --- Değişkenler ---
SCRIPT_VERSION="v7.0"
REPO="MematiBas42/MHRS-OtomatikRandevu"
INSTALL_DIR="$HOME/mhrs_randevu"
LAUNCHER_DIR="$HOME/.local/bin"
LAUNCHER_PATH="$LAUNCHER_DIR/mhrs"
LATEST_RELEASE_URL="https://api.github.com/repos/$REPO/releases/latest"
APP_DLL="MHRS-OtomatikRandevu.dll"
VERSION_FILE="$INSTALL_DIR/version.txt"
PATH_COMMENT="# MHRS Otomatik Randevu için PATH ayarı"
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
        echo ">> API Yanıtı (ilk 5 satır):" >&2
        echo "$api_output" | head -n 5 >&2
        echo "" && return
    fi
    
    echo "$tag_name"
}

create_launcher() {
    local platform_identifier="$1"
    local launcher_content

    echo_info "\n--- Aşama 4: 'mhrs' Komutu Yapılandırılıyor ---"
    mkdir -p "$LAUNCHER_DIR"

    case "$platform_identifier" in
        termux-*) launcher_content="#!/bin/bash\ncd \"$INSTALL_DIR\"
dotnet $APP_DLL \"\$@\"" ;; 
        win-*) launcher_content="#!/bin/bash\ncd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu.exe \"\$@\"" ;; 
        alpine-*) launcher_content="#!/bin/sh\nexport COMPlus_GCServer=0\nexport COMPlus_GCHeapHardLimit=0x10000000\ncd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu \"\$@\"" ;; 
        *) launcher_content="#!/bin/bash\ncd \"$INSTALL_DIR\"
./MHRS-OtomatikRandevu \"\$@\"" ;; 
    esac

    echo -e "$launcher_content" > "$LAUNCHER_PATH"
    chmod +x "$LAUNCHER_PATH"
    echo_success "✓ Başlatıcı betik '$LAUNCHER_PATH' adresinde oluşturuldu."

    # --- PATH Configuration ---
    if [[ ":$PATH:" == ".*:"$LAUNCHER_DIR:"*" ]]; then
        echo_success "✓ '$LAUNCHER_DIR' dizini PATH içinde zaten mevcut."
        return
    fi
    
    echo_warn "UYARI: '$LAUNCHER_DIR' dizini PATH değişkeninizde bulunmuyor."
    
    local shell_name=""
    if [ -n "$SHELL" ]; then
        shell_name=$(basename "$SHELL")
    elif [ -f "/bin/ash" ]; then
        echo_info "--> \$SHELL değişkeni boş, ancak /bin/ash bulundu. Kabuk 'ash' olarak varsayılıyor."
        shell_name="ash"
    fi

        local files_to_try=()
    if [ "$shell_name" = "bash" ]; then
        files_to_try=("$HOME/.bashrc")
    elif [ "$shell_name" = "zsh" ]; then
        files_to_try=("$HOME/.zshrc")
    elif [ "$shell_name" = "ash" ] || [ "$shell_name" = "sh" ]; then
        files_to_try=("$HOME/.profile" "$HOME/.ashrc" "$HOME/.shrc")
    fi

    if [ ${#files_to_try[@]} -gt 0 ]; then
        # cleanup_rc_files "${files_to_try[@]}" # This function was removed in the new version

        echo_info "--> PATH ayarları şu dosyalara yazılıyor: ${files_to_try[*]}"
        local export_line="export PATH=\
$HOME/.local/bin:$PATH"
        
        for rc_file in "${files_to_try[@]}"; do
            echo "" >> "$rc_file"
            echo "$PATH_COMMENT" >> "$rc_file"
            echo "$export_line" >> "$rc_file"
        done
        
        # Special case for ash: ensure .profile sets ENV to source .shrc for interactive shells
        if [ "$shell_name" = "ash" ] || [ "$shell_name" = "sh" ]; then
            if ! grep -q "export ENV" "$HOME/.profile" 2>/dev/null ; then
                 echo_info "--> 'ash' için $HOME/.profile dosyasına ENV ayarı ekleniyor."
                 echo "" >> "$HOME/.profile"
                 echo "$PATH_COMMENT (Etkileşimli Kabuk için)" >> "$HOME/.profile"
                 echo "export ENV=$HOME/.shrc" >> "$HOME/.profile"
            fi
        fi

        echo_success "✓ PATH ayarları ilgili yapılandırma dosyalarına yazıldı."
        echo_warn "Değişikliğin etkinleşmesi için terminalinizi yeniden başlatmanız gerekmektedir."
    else
        echo_error "Desteklenmeyen veya tespit edilemeyen kabuk. Lütfen '$LAUNCHER_DIR' dizinini manuel olarak PATH'inize ekleyin."
    fi
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

    if [ ! -d "$INSTALL_DIR" ] || [ ! -f "$VERSION_FILE" ]; then
        echo_info "İlk kurulum: Sürüm $remote_version indiriliyor..."
    else
        local local_version
        local_version=$(cat "$VERSION_FILE")
        if [ "$local_version" = "$remote_version" ]; then
            echo_success "✓ Uygulama zaten güncel (Sürüm: ${local_version})."
            create_launcher "$platform_identifier"
            echo_info "\n--- Kurulum Tamamlandı ---"
            echo_success "✓ Sonraki sefer 'mhrs' yazarak uygulamayı direkt çalıştırabilirsiniz."
            echo_info "i Güncelleme kontrolü için bu kurulum betiğini yeniden çalıştırmanız yeterlidir."
            echo_info "\nUygulamayı şimdi başlatmak için ENTER tuşuna basın veya çıkmak için CTRL+C yapın."
            read -r
            $LAUNCHER_PATH
            exit 0
        fi
        echo_info "Yeni sürüm bulundu: $remote_version. Güncelleniyor..."
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
    echo_success "✓ Sonraki sefer 'mhrs' yazarak uygulamayı direkt çalıştırabilirsiniz."
    echo_info "i Güncelleme kontrolü için bu kurulum betiğini yeniden çalıştırmanız yeterlidir."
    echo_info "\nUygulamayı şimdi başlatmak için ENTER tuşuna basın veya çıkmak için CTRL+C yapın."
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
        echo_warn "Termux üzerindeki bağımlılık kontrolü (dotnet-sdk-8.0) şu anda devre dışıdır ve manuel kurulum gerektirebilir."
        perform_install_or_update "termux-arm64" "MHRS-OtomatikRandevu-termux-arm64.zip"
    elif [ -f "/etc/alpine-release" ]; then
        echo_info "Alpine Linux ortamı algılandı."
        perform_install_or_update "alpine-arm64" "MHRS-OtomatikRandevu-alpine-arm64.zip"
    elif [ "$OS_TYPE" = "Linux" ]; then
        echo_info "Genel Linux ortamı algılandı."
        perform_install_or_update "linux-x64" "MHRS-OtomatikRandevu-linux-x64.zip"
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
