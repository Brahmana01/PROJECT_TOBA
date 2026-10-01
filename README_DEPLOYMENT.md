PROJECT TOBA — Deployment & Setup Guide

Dokumen ini menjadi acuan setup, testing, dan deployment PROJECT TOBA pada kondisi project terbaru.

1. ARSITEKTUR PROJECT TERBARU

Repository utama:

PROJECT_TOBA/
├── TOBA_KEYSTORE/
├── toba-admin/
├── toba-app/toba/
├── toba-backend/
├── .gitattributes
├── .gitignore
├── README_DEPLOYMENT.md

Komponen utama:

toba-backend
    └── Golang + Fiber + GORM + PostgreSQL

toba-admin
    └── React + TypeScript + Vite

toba-app/toba
    └── Unity 6 LTS + C#

Arsitektur production:

                INTERNET
                    │
        ┌───────────┴───────────┐
        │                       │
        ▼                       ▼
   TOBA Android            TOBA Admin
        │                       │
        └───────────┬───────────┘
                    ▼
              HTTPS / WSS
                    │
                    ▼
          ┌───────────────────┐
          │ ServerLoka VPS    │
          │ Ubuntu 24.04 LTS  │
          │ 2 CPU / 4 GB RAM  │
          │ 60 GB Storage     │
          ├───────────────────┤
          │ Nginx             │
          │ Go/Fiber Backend  │
          │ PostgreSQL        │
          └───────────────────┘

VPS bukan AWS EC2. Server production yang digunakan adalah VPS ServerLoka.

2. SPESIFIKASI SERVER PRODUCTION

Server saat ini:

Item

Nilai

Provider

ServerLoka

OS

Ubuntu Server 24.04 LTS 64-bit

CPU

2 vCPU

RAM

4 GB

Storage

60 GB

Public IP

Dikelola melalui panel ServerLoka

Akses

SSH

Domain

Belum ditentukan

Public IP dan kredensial jangan ditulis ke repository.

Akses SSH diberikan oleh panel ServerLoka, contoh format:

ssh ubuntu@<PUBLIC_IP_VPS>

Password/credential VPS hanya dimasukkan langsung di terminal.

3. KOMPONEN YANG AKAN DIINSTALL DI VPS

Server production akan menjalankan:

Git
Go
PostgreSQL
Nginx

Backend Go dijalankan sebagai service Linux menggunakan systemd.

Production flow:

Internet
   ↓
Nginx
   ├── HTTPS → Go/Fiber
   └── WSS  → WebSocket
                ↓
             PostgreSQL

Backend tidak dijalankan menggunakan run_server.bat di VPS.

run_server.bat hanya untuk development Windows.

4. BACKEND — DEVELOPMENT LOCAL

Prasyarat

Go 1.21+

PostgreSQL 14+

pgAdmin opsional

Database local

CREATE DATABASE toba_db;

Contoh .env development:

DB_HOST=localhost
DB_USER=postgres
DB_PASSWORD=<PASSWORD_LOCAL>
DB_NAME=toba_db
DB_PORT=5432
SERVER_PORT=8080

Menjalankan backend

Development Windows:

run_server.bat

atau:

cd toba-backend
go mod tidy
go run main.go

Backend local menggunakan port:

8080

Endpoint:

http://localhost:8080/api/

Backend menjalankan:

AutoMigrate

Seed data

REST API

Authentication

WebSocket Battle

Static/upload handling sesuai konfigurasi project

5. BACKEND — PRODUCTION VPS

Tahap deployment

Urutan deployment production:

1. SSH ke VPS
2. Audit server
3. Install dependency
4. Clone repository
5. Setup PostgreSQL
6. Buat database production
7. Buat `.env` production
8. Build backend
9. Jalankan migration/seed
10. Test API dari VPS
11. Buat systemd service
12. Jalankan backend sebagai service
13. Install & konfigurasi Nginx
14. Setup domain
15. Setup HTTPS
16. Test WebSocket
17. Test Admin Panel
18. Test Unity

6. POSTGRESQL PRODUCTION

PostgreSQL dijalankan pada VPS yang sama dengan backend.

Konfigurasi awal yang direncanakan:

DB_HOST=localhost
DB_PORT=5432
DB_USER=<PRODUCTION_DB_USER>
DB_PASSWORD=<PRODUCTION_DB_PASSWORD>
DB_NAME=toba_db
SERVER_PORT=8080

Database production tidak menggunakan password development.

Jangan pernah commit .env production ke GitHub.

7. DATABASE MIGRATION & SEED

Backend saat start menjalankan AutoMigrate dan seed.

Data target project terbaru:

Data

Target

Character

2

Title

5

Location

10

Location Fact

40

Location Question

150

Trivia Category

5

Trivia Question

50

Battle Question

25

Achievement

14

Gallery Card

20

Game Settings

10

Admin

1

Weekly Challenge

sesuai seed aktif

Trivia

Target:

5 kategori × 10 soal = 50 soal

Setiap kategori harus memiliki konfigurasi:

Jumlah Soal per Sesi = 10
Aktif = true

Catatan penting:

question_count = 0 dapat menyebabkan Admin menampilkan:

0 soal/sesi

dan Unity dapat menerima sesi Trivia berjumlah 0 soal.

Karena itu konfigurasi kategori Trivia harus diverifikasi setelah seed.

Battle

Target terbaru:

25 bank soal Battle

Dokumentasi lama yang menyebut 15 soal tidak lagi menjadi target final.

8. ADMIN ACCOUNT

Untuk development, project dapat memiliki akun admin seed sesuai konfigurasi development.

Untuk production:

jangan menggunakan password development secara permanen

gunakan password production yang baru

jangan menyimpan password production di GitHub

jangan menaruh password production di README_DEPLOYMENT.md

Session admin disimpan di database dan dilindungi oleh Admin Auth Middleware.

9. ADMIN PANEL

Development

Prasyarat:

Node.js 18+

npm/yarn

cd toba-admin
npm install
npm run dev

Development URL:

http://localhost:5173

Admin Panel menggunakan backend API:

http://localhost:8080/api

Production

Admin Panel akan diarahkan ke:

https://<DOMAIN-API>/api

atau konfigurasi production lain yang disepakati.

Admin Panel dan backend adalah dua deployment terpisah:

Admin Panel
     ↓
HTTPS
     ↓
TOBA Backend

10. UNITY — DEVELOPMENT

Unity project berada di:

toba-app/toba/

Prasyarat:

Unity 6 LTS

Android Build Support

Android SDK & NDK

JDK 17+

NativeWebSocket bila diperlukan oleh project

Development Editor dapat menggunakan:

http://localhost:8080/api

WebSocket:

ws://localhost:8080/ws

11. UNITY — LOCAL ANDROID DEVICE

Saat backend masih berjalan di laptop dan APK diuji dari HP pada jaringan lokal:

HP
 ↓
Wi-Fi yang sama
 ↓
Laptop
 ↓
Go/Fiber :8080

Unity tidak boleh menggunakan:

localhost

pada HP.

Gunakan IP LAN komputer, contoh:

http://192.168.1.100:8080/api
ws://192.168.1.100:8080/ws

Port 8080 harus diizinkan oleh Windows Firewall.

12. UNITY — PRODUCTION

Setelah backend production selesai, Unity tidak lagi menggunakan IP LAN atau localhost.

Production:

REST:
https://<DOMAIN-API>/api

WebSocket:
wss://<DOMAIN-API>/ws

Flow:

TOBA APK
   ↓
Internet
   ↓
HTTPS / WSS
   ↓
ServerLoka VPS
   ↓
Nginx
   ↓
Go/Fiber
   ↓
PostgreSQL

13. API ENDPOINT PRODUCTION

Endpoint utama yang harus diuji:

GET  /api/
GET  /api/version

POST /api/register
POST /api/login
POST /api/logout

GET  /api/profile/:id

GET  /api/locations
GET  /api/locations/:id/detail
GET  /api/locations/:id/fact
GET  /api/locations/:id/quiz

GET  /api/trivia/categories
GET  /api/trivia/:id/quiz
POST /api/trivia/submit

GET  /api/leaderboard

POST /api/battle/create
POST /api/battle/join
GET  /api/battle/status/:code
POST /api/battle/answer
POST /api/battle/finish
POST /api/battle/disconnect
POST /api/battle/matchmake
GET  /api/battle/matchmake/check/:id

GET  /api/challenge/active
POST /api/challenge/submit

Admin:

/api/admin/...

14. NGINX

Nginx menjadi pintu masuk public.

Target:

Internet
   ↓
HTTPS :443
   ↓
Nginx
   ↓
localhost:8080
   ↓
Go/Fiber

Nginx juga harus meneruskan WebSocket Battle.

REST:

https://<DOMAIN-API>/api/...

WebSocket:

wss://<DOMAIN-API>/ws

15. SYSTEMD

Backend production tidak dijalankan hanya dengan:

go run main.go

untuk kondisi final.

Target:

systemd
   ↓
TOBA Backend

Dengan demikian:

VPS restart
   ↓
systemd
   ↓
TOBA Backend otomatis hidup

Perintah umum yang akan digunakan setelah service dibuat:

sudo systemctl status toba-backend
sudo systemctl restart toba-backend
sudo systemctl stop toba-backend
sudo systemctl start toba-backend

Log:

sudo journalctl -u toba-backend -f

16. DOMAIN & HTTPS

Domain production belum ditetapkan.

Setelah domain tersedia:

<DOMAIN-API>

akan diarahkan ke Public IP VPS.

Target final:

https://<DOMAIN-API>/api/
wss://<DOMAIN-API>/ws

HTTPS harus aktif sebelum production Android digunakan secara luas.

17. DEPLOYMENT ANDROID

APK/AAB bukan dijalankan di VPS.

Unity menghasilkan:

AAB

kemudian:

Unity
 ↓
Android App Bundle (.aab)
 ↓
Google Play Console
 ↓
Internal Testing
 ↓
Tester
 ↓
Android Device

Aplikasi Android mengakses backend production melalui HTTPS/WSS.

Production testing harus dilakukan dari HP menggunakan internet, bukan hanya Unity Editor.

18. URUTAN TESTING PRODUCTION

Backend

VPS dapat diakses via SSH

PostgreSQL aktif

Database toba_db tersedia

AutoMigrate berhasil

Seed berhasil

Backend build berhasil

systemd aktif

API /api/ merespons

Authentication bekerja

Trivia bekerja

Jelajah bekerja

Battle REST bekerja

Battle WebSocket bekerja

Admin

Admin Panel dapat mengakses API production

Login admin

Dashboard

Locations

Trivia

Battle

Achievement

Gallery

Settings

Players

Unity

Register

Login

Pilih karakter

Main Menu

Jelajah Budaya

Quiz

Quiz Result

Trivia

Trivia Result

Battle

WebSocket

Gallery

Leaderboard

Profile

Achievement

Challenge

19. HAL YANG TIDAK BOLEH MASUK GIT

Jangan commit:

.env
.env.production
SSH private key
VPS password
database password
API secret
Firebase secret
Android keystore password

Folder/key signing Android harus dijaga terpisah dari credential server.

20. CATATAN KONDISI PROJECT TERKINI

Per tahap deployment terbaru:

Backend local sudah berjalan.

Admin authentication sudah dapat berjalan setelah session lama dibersihkan.

Player 0 di Admin Panel bukan error karena database memang tidak melakukan seed player dummy.

Admin achievement memerlukan IsActive = true agar muncul di handler admin yang memfilter data aktif.

Trivia sudah memiliki 5 kategori.

Trivia sudah memiliki 50 soal.

Trivia Jumlah Soal per Sesi harus diatur menjadi 10 pada masing-masing kategori.

Backend Unity masih menggunakan endpoint development dan belum diarahkan ke VPS production.

VPS production menggunakan ServerLoka Ubuntu 24.04 LTS, bukan AWS EC2.

Deployment production belum dimulai.

Domain production belum ditentukan.

21. DEPLOYMENT ROADMAP

PHASE 1 — SERVER
SSH → Audit VPS → Install dependency

PHASE 2 — DATABASE
PostgreSQL → User → Database → Permission

PHASE 3 — BACKEND
Clone → .env → Build → Migration → Seed

PHASE 4 — SERVICE
systemd → Auto start → Logs

PHASE 5 — NETWORK
Nginx → Domain → HTTPS → WebSocket

PHASE 6 — ADMIN
Production API URL → Build → Deploy Admin

PHASE 7 — UNITY
Production API URL → Build AAB

PHASE 8 — GOOGLE PLAY
Internal Testing → Install → Test

PHASE 9 — FINAL
REST → Auth → Trivia → Jelajah → Battle → WebSocket → Admin

Deployment dilakukan bertahap. Setiap fase harus berhasil dan diverifikasi sebelum masuk fase berikutnya.