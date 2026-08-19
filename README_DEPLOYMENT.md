---

## A. BACKEND (Golang)

### Prasyarat
- Go 1.21+
- PostgreSQL 14+
- pgAdmin (opsional, untuk manajemen DB)

### Langkah Setup

1. **Buat database PostgreSQL**
```sql
   CREATE DATABASE toba_db;
```

2. **Konfigurasi .env**

DB_HOST=localhost
DB_USER=postgres
DB_PASSWORD=rahasia
DB_NAME=toba_db
DB_PORT=5432
SERVER_PORT=8080

3. **Jalankan server**
```bash
   cd toba-backend
   go mod tidy
   go run main.go
```
   Server akan otomatis:
   - Migrate semua tabel
   - Seed data awal (karakter, lokasi, soal, dll)
   - Berjalan di port 8080

4. **Verifikasi**
   Buka browser: `http://localhost:8080/api/`
   Response: `{"status":"success","message":"API TOBA v1 ready!"}`

---

## B. UNITY (Game Android)

### Prasyarat
- Unity 6 LTS (6000.x)
- Android Build Support module
- Android SDK & NDK (via Unity Hub)
- JDK 17+

### Langkah Setup

1. **Buka project Unity**
   - Buka Unity Hub
   - Add project → pilih folder `toba-app/`

2. **Install NativeWebSocket**
   - Window → Package Manager
   - + → Add package from git URL:
   https://github.com/endel/NativeWebSocket.git#upm

3. **Setup project otomatis**
   - Di Unity menu bar: `TOBA → Setup → Setup Project Lengkap`
   - Ini akan membuat scene, konfigurasi Android settings, dll

4. **Update IP Backend untuk device Android**
   - `TOBA → Setup → Update IP Backend untuk Android`
   - Masukkan IP komputer yang menjalankan backend
   - Contoh: `192.168.1.100`
   - **PENTING**: Pastikan HP dan komputer terhubung ke WiFi yang sama

5. **Test di Unity Editor**
   - `TOBA → Debug → Play dari MainScene`
   - Atau tekan Play button biasa

6. **Build APK**
   - Debug: `TOBA → Build → Android APK (Debug)`
   - Release: `TOBA → Build → Android APK (Release)`
   - APK tersimpan di `toba-app/Builds/Android/`

---

## C. ADMIN PANEL (React)

### Prasyarat
- Node.js 18+
- npm atau yarn

### Langkah Setup

```bash
cd toba-admin
npm install
npm run dev
```

Akses di: `http://localhost:5173`

Login default:
- Username: `admin`
- Password: `admin123`

---

## D. TESTING FLOW LENGKAP

### Urutan yang benar:
1. ✅ Jalankan PostgreSQL
2. ✅ Jalankan backend: `go run main.go`
3. ✅ Buka Unity → Play
4. ✅ Register akun baru
5. ✅ Login
6. ✅ Test semua fitur

### Checklist fitur:
- [ ] Register & Login
- [ ] Pilih karakter
- [ ] Menu Utama tampil
- [ ] Jelajah Budaya → peta muncul
- [ ] Klik lokasi → detail muncul
- [ ] Quiz berjalan
- [ ] Hasil quiz & reward
- [ ] Trivia Budaya → kategori muncul
- [ ] Battle Quiz → buat & join room
- [ ] WebSocket battle berjalan
- [ ] Galeri Budaya
- [ ] Leaderboard
- [ ] Profil & statistik
- [ ] Achievement popup

---

## E. KONFIGURASI UNTUK ANDROID DEVICE

### Di APIManager.cs:
```csharp
// Ganti localhost dengan IP komputer kamu
private string BASE_URL = "http://192.168.1.100:8080/api";
private string WS_URL   = "ws://192.168.1.100:8080/ws";
```

### Firewall Windows:
Izinkan port 8080 di Windows Firewall:

Windows Defender Firewall → Advanced Settings
→ Inbound Rules → New Rule → Port → TCP 8080

### Atau via PowerShell (Admin):
```powershell
netsh advfirewall firewall add rule name="TOBA Backend" dir=in action=allow protocol=TCP localport=8080
```

---

## F. TROUBLESHOOTING

| Masalah | Solusi |
|---|---|
| `dial tcp: connection refused` | Pastikan backend berjalan & port 8080 terbuka |
| `WebSocket upgrade required` | Normal untuk request non-WS ke endpoint WS |
| `relation does not exist` | Jalankan `go run main.go` untuk auto-migrate |
| APK tidak bisa connect | Ganti `localhost` dengan IP komputer di APIManager.cs |
| `crypto/bcrypt` error | Jalankan `go get golang.org/x/crypto` |
| Scene kosong di Unity | Jalankan `TOBA → Setup → Setup Project Lengkap` |
| NativeWebSocket error | Install via Package Manager (lihat langkah B.2) |

---

## G. DATA SEED AWAL

Setelah server pertama kali dijalankan, database otomatis terisi:

| Data | Jumlah |
|---|---|
| Karakter | 2 (Ucok, Butet) |
| Gelar | 5 |
| Lokasi Jelajah | 10 |
| Fakta Budaya | 40+ |
| Soal Trivia | 50 (5 kategori × 10) |
| Soal Battle | 15 |
| Achievement | 14 |
| Game Settings | 10 |
| Admin | 1 (admin/admin123) |

---

## H. STRUKTUR FILE UNITY FINAL
Assets/Scripts/
├── Core/
│ ├── APIManager.cs
│ ├── BaseScreen.cs
│ ├── DataModels.cs
│ ├── ScreenManager.cs
│ ├── SoundManager.cs
│ ├── TOBATheme.cs
│ └── UIBuilder.cs
├── Components/
│ ├── HeaderBar.cs
│ ├── FooterNavbar.cs
│ └── StarRating.cs
├── Screens/
│ ├── SplashScreen.cs
│ ├── WelcomeScreen.cs
│ ├── LoginScreen.cs
│ ├── CharacterSelectScreen.cs
│ ├── RegisterNameScreen.cs
│ ├── RegisterPinScreen.cs
│ ├── RegisterSummaryScreen.cs
│ ├── MainMenuScreen.cs
│ ├── ProfileScreen.cs
│ ├── MapScreen.cs
│ ├── LocationDetailScreen.cs
│ ├── QuizScreen.cs
│ ├── QuizResultScreen.cs
│ ├── TriviaMenuScreen.cs
│ ├── TriviaQuizScreen.cs
│ ├── TriviaResultScreen.cs
│ ├── BattleMenuScreen.cs
│ ├── BattleWaitingScreen.cs
│ ├── BattleQuizScreen.cs
│ ├── GalleryScreen.cs
│ ├── LeaderboardScreen.cs
│ ├── SettingScreen.cs
│ ├── AchievementPopup.cs
│ └── LevelUpPopup.cs
├── Editor/
│ ├── AndroidBuildScript.cs
│ └── TOBAEditorSetup.cs
└── MainApp.cs

