@echo off
title TOBA Backend Server
color 0A

echo ========================================
echo    TOBA BACKEND SERVER
echo    Port: 8080
echo    Database: PostgreSQL (toba_db)
echo ========================================
echo.

:: Cek apakah Go terinstall
where go >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Go tidak ditemukan. Install Go terlebih dahulu.
    pause
    exit /b 1
)

:: Cek koneksi database
echo [INFO] Memulai server...
echo [INFO] Tekan Ctrl+C untuk menghentikan server
echo.

:: Download dependencies jika belum ada
if not exist "go.sum" (
    echo [INFO] Download dependencies...
    go mod tidy
    go mod download
)

:: Jalankan server
go run main.go

if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Server gagal dijalankan!
    echo [INFO] Pastikan PostgreSQL berjalan dan .env sudah dikonfigurasi
    pause
)