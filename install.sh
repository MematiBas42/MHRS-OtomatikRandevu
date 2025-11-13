#!/bin/sh

# MHRS-OtomatikRandevu için Akıllı Kurulum ve Güncelleme Betiği
# Platformu algılar, bağımlılıkları kurar, en son sürümü indirir,
# token ve appsettings.json'ı koruyarak günceller ve uygulamayı başlatır.

set -e

# --- Değişkenler ---
REPO="MematiBas42/MHRS-OtomatikRandevu"
INSTALL_DIR="$HOME/mhrs_randevu"
LATEST_RELEASE_URL="https://api.github.com/repos/$REPO/releases/latest"
APP_NAME="MHRS-OtomatikRandevu"
VERSION_FILE="$INSTALL_DIR/version.txt"
CONFIG_FILE="appsettings.json"
TOKEN_PATTERN="token_*.txt"

# --- Renkler ve Yardımcı Fonksiyonlar ---
COLOR_BLUE="\033[1;34m"
COLOR_GREEN="\033[1;32m"
COLOR_RED="\033[1;31m"
COLOR_YELLOW="\033[1;33m"
COLOR_RESET="\033[0m"

echo_info() { printf "${COLOR_BLUE}%s${COLOR_RESET}\n" "$1"; }
écho_success() { printf "${COLOR_GREEN}%s${COLOR_RESET}\n" "$1"; }
echo_error() { printf "${COLOR_RED}%s${COLOR_RESET}\n" "$1" >&2; }
echo_warn() { printf "${COLOR_YELLOW}%s${COLOR_RESET}\n" "$1"; }

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
    curl -s $LATEST_RELEASE_URL | grep "tag_name" | head -n 1 | cut -d '"' -f 4 | sed 's/v//'
}

# Alias ekleme fonksiyonu
add_alias() {
    local alias_cmd
    local shell_rc_file
    local shell_name=$(basename "$SHELL")

    if [ "$1" = "termux" ]; then
        alias_cmd="alias mhrs='cd $INSTALL_DIR && dotnet $APP_NAME.dll'"
    else
        alias_cmd="alias mhrs='cd $INSTALL_DIR && ./$APP_NAME'"
    fi

    case "$shell_name" in
        bash)
            shell_rc_file="$HOME/.bashrc"
            ;; 
        zsh)
            shell_rc_file="$HOME/.zshrc"
            ;; 
        *)
            echo_warn "UYARI: Desteklenmeyen kabuk ($shell_name). 'mhrs' kısayolu otomatik eklenemedi."
            echo_warn "Lütfen aşağıdaki komutu manuel olarak kabuk yapılandırma dosyanıza ekleyin:"
            echo_warn "  $alias_cmd"
            return
            ;; 
    esac

    if [ -f "$shell_rc_file" ]; then
        if ! grep -q "alias mhrs=" "$shell_rc_file"; then
            echo "$alias_cmd" >> "$shell_rc_file"
            echo_success "✓ 'mhrs' kısayolu '$shell_rc_file' dosyasına eklendi."
            echo_info "Kısayolun etkinleşmesi için terminali yeniden başlatın veya 'source $shell_rc_file' komutunu çalıştırın."
        else
            echo_info "'mhrs' kısayolu zaten '$shell_rc_file' dosyasında mevcut."
        fi
    else
        echo_warn "UYARI: Kabuk yapılandırma dosyası ($shell_rc_file) bulunamadı."
        echo_warn "Lütfen aşağıdaki komutu manuel olarak kabuk yapılandırma dosyanıza ekleyin:"
        echo_warn "  $alias_cmd"
    fi
}

# Uygulamayı başlatma fonksiyonu
run_app() {
    echo_info "Uygulama başlatılıyor..."
    cd "$INSTALL_DIR"
    if [ "$1" = "termux" ]; then
        dotnet "$APP_NAME.dll"
    else
        "./$APP_NAME"
    fi
}

# --- Kurulum ve Güncelleme Mantığı ---
perform_install_or_update() {
    local platform_type="$1" # "termux", "linux", "windows"
    local asset_name="$2"
    local is_first_install=false
    local local_version=""
    local remote_version=$(get_latest_remote_version)

    if [ ! -d "$INSTALL_DIR" ] || [ ! -f "$VERSION_FILE" ]; then
        is_first_install=true
        echo_info "İlk kurulum algılandı."
    else
        local_version=$(cat "$VERSION_FILE")
        echo_info "Yerel sürüm: $local_version"
    fi

    echo_info "Uzak sürüm: $remote_version"

    if [ "$is_first_install" = true ] || [ "$local_version" != "$remote_version" ]; then
        echo_info "Güncelleme veya ilk kurulum yapılıyor..."

        # Token ve config dosyalarını yedekle
        local temp_backup_dir=$(mktemp -d)
        if [ -f "$INSTALL_DIR/$CONFIG_FILE" ]; then
            cp "$INSTALL_DIR/$CONFIG_FILE" "$temp_backup_dir/"
            echo_info "Mevcut $CONFIG_FILE yedeklendi."
        fi
        for token_file in "$INSTALL_DIR/$TOKEN_PATTERN"; do
            if [ -f "$token_file" ]; then
                cp "$token_file" "$temp_backup_dir/"
                echo_info "Mevcut token dosyası yedeklendi."
            fi
        done

        # Eski kurulumu temizle (yedeklenenler hariç)
        if [ -d "$INSTALL_DIR" ]; then
            echo_info "Eski uygulama dosyaları temizleniyor..."
            find "$INSTALL_DIR" -mindepth 1 -maxdepth 1 ! -name "$CONFIG_FILE" ! -name "${TOKEN_PATTERN//\*/}" -exec rm -rf {} + || true
            # Log dosyalarını sil
            find "$INSTALL_DIR" -name "*.log" -delete || true
            find "$INSTALL_DIR" -name "*.txt" ! -name "${TOKEN_PATTERN//\*/}" -delete || true
        fi
        mkdir -p "$INSTALL_DIR" # Klasör silindiyse yeniden oluştur

        # İndir
        echo_info "$asset_name indiriliyor..."
        DOWNLOAD_URL=$(curl -s $LATEST_RELEASE_URL | grep "browser_download_url.*$asset_name" | cut -d '"' -f 4)
        if [ -z "$DOWNLOAD_URL" ]; then
            echo_error "HATA: $asset_name için indirme URL'si bulunamadı."
            exit 1
        fi
        curl -L -o "$INSTALL_DIR/$asset_name" "$DOWNLOAD_URL"
        echo_success "İndirme tamamlandı."

        # Çıkar/Yerleştir
        if echo "$asset_name" | grep -q ".zip$"; then
            echo_info "Uygulama dosyaları çıkarılıyor..."
            unzip -o "$INSTALL_DIR/$asset_name" -d "$INSTALL_DIR"
            rm "$INSTALL_DIR/$asset_name" # Arşivi sil
        else
            echo_info "Uygulama dosyaları yerleştiriliyor..."
            chmod +x "$INSTALL_DIR/$asset_name"
        fi

        # Yedeklenen dosyaları geri yükle
        if [ -f "$temp_backup_dir/$CONFIG_FILE" ]; then
            mv "$temp_backup_dir/$CONFIG_FILE" "$INSTALL_DIR/"
            echo_info "Yedeklenen $CONFIG_FILE geri yüklendi."
        fi
        for token_file in "$temp_backup_dir/$TOKEN_PATTERN"; do
            if [ -f "$token_file" ]; then
                mv "$token_file" "$INSTALL_DIR/"
                echo_info "Yedeklenen token dosyası geri yüklendi."
            fi
        done
        rm -rf "$temp_backup_dir" # Yedekleme dizinini sil

        # Yeni sürüm bilgisini kaydet
        echo "$remote_version" > "$VERSION_FILE"
        echo_success "✓ Kurulum/Güncelleme başarıyla tamamlandı!"

        if [ "$is_first_install" = true ]; then
            add_alias "$platform_type"
        fi
    else
        echo_info "Uygulama zaten güncel. Güncelleme gerekmiyor."
    fi

    run_app "$platform_type"
}

# --- Ana Betik Mantığı ---
check_common_deps

if [ -d "/data/data/com.termux" ]; then
    # Termux özel kurulumu
    echo_info "Termux ortamı algılandı."
    if ! command -v dotnet >/dev/null 2>&1; then
        echo_warn ".NET 8 SDK'sı bulunamadı. Kuruluyor (Bu işlem biraz zaman alabilir)..."
        pkg update -y && pkg upgrade -y
        pkg install -y curl unzip git
        curl -L https://raw.githubusercontent.com/Glow-Project/gl-dotnet/master/dotnet-install.sh | bash
        export DOTNET_ROOT=$HOME/.dotnet
        export PATH=$PATH:$HOME/.dotnet
        echo_success ".NET 8 SDK kuruldu."
        echo_warn "Lütfen terminali yeniden başlatın veya 'source ~/.bashrc' (veya kullandığınız kabuk dosyası) komutunu çalıştırın."
        echo_warn "Ardından betiği tekrar çalıştırın."
        exit 0
    else
        echo_info ".NET 8 SDK zaten kurulu."
    fi
    perform_install_or_update "termux" "mhrs-termux-arm64.zip"
else
    # Diğer Linux ve Windows platformları
    OS_TYPE=$(uname -s)
    ARCH_TYPE=$(uname -m)
    ASSET_NAME=""
    PLATFORM_TYPE=""

    case "$OS_TYPE" in
        Linux)
            PLATFORM_TYPE="linux"
            if [ "$ARCH_TYPE" = "x86_64" ]; then
                ASSET_NAME="MHRS-OtomatikRandevu-linux-x64"
                echo_info "Linux (x64) ortamı algılandı."
                echo_info "Gerekli sistem bağımlılıkları kontrol ediliyor ve kuruluyor (sudo şifreniz istenebilir)...";
                if command -v apt-get >/dev/null 2>&1; then
                    sudo apt-get update -y
                    sudo apt-get install -y libicu-dev libssl-dev
                elif command -v dnf >/dev/null 2>&1; then
                    sudo dnf install -y libicu libssl
                elif command -v pacman >/dev/null 2>&1; then
                    sudo pacman -S --needed --noconfirm icu openssl
                else
                    echo_warn "UYARI: Desteklenmeyen Linux dağıtımı. Gerekli bağımlılıkları manuel olarak kurmanız gerekebilir (libicu, libssl)."
                fi
            else
                echo_error "Desteklenmeyen Linux mimarisi: $ARCH_TYPE. Sadece x86_64 desteklenmektedir."
                exit 1
            fi
            ;; 
        CYGWIN*|MINGW*|MSYS*) 
            PLATFORM_TYPE="windows"
            if [ "$ARCH_TYPE" = "x86_64" ]; then
                ASSET_NAME="MHRS-OtomatikRandevu-win-x64.exe"
                echo_info "Windows (x64) ortamı algılandı."
            else
                echo_error "Desteklenmeyen Windows mimarisi: $ARCH_TYPE. Sadece x86_64 desteklenmektedir."
                exit 1
            fi
            ;; 
        *)
            echo_error "Desteklenmeyen işletim sistemi: $OS_TYPE"
            exit 1
            ;; 
    esac

    if [ -z "$ASSET_NAME" ]; then
        echo_error "HATA: Platformunuz için uygun sürüm bulunamadı."
        exit 1
    fi
    
    perform_install_or_update "$PLATFORM_TYPE" "$ASSET_NAME"
fi
