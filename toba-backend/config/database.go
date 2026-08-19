package config

import (
	"fmt"
	"log"
	"os"
	"toba-backend/models"

	"github.com/joho/godotenv"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	"gorm.io/gorm/logger"
)

var DB *gorm.DB

func ConnectDatabase() {
	err := godotenv.Load()
	if err != nil {
		log.Println("File .env tidak ditemukan, menggunakan environment variable sistem")
	}

	dsn := fmt.Sprintf(
		"host=%s user=%s password=%s dbname=%s port=%s sslmode=disable TimeZone=Asia/Jakarta",
		os.Getenv("DB_HOST"),
		os.Getenv("DB_USER"),
		os.Getenv("DB_PASSWORD"),
		os.Getenv("DB_NAME"),
		os.Getenv("DB_PORT"),
	)

	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{
		Logger: logger.Default.LogMode(logger.Info),
	})
	if err != nil {
		log.Fatal("Gagal koneksi ke database: ", err)
	}

	DB = db
	log.Println("✅ Database terkoneksi!")

	// Auto migrate semua tabel
	autoMigrate()
	log.Println("✅ Auto migrate selesai!")
}

func autoMigrate() {
	DB.AutoMigrate(
		&models.Character{},
		&models.Title{},
		&models.Admin{},
		&models.Player{},
		&models.PlayerSession{},
		&models.Location{},
		&models.LocationFact{},
		&models.LocationQuestion{},
		&models.LocationQuestionOption{},
		&models.PlayerLocationProgress{},
		&models.TriviaCategory{},
		&models.TriviaQuestion{},
		&models.TriviaQuestionOption{},
		&models.PlayerTriviaHistory{},
		&models.PlayerTriviaAnswer{},
		&models.BattleQuestion{},
		&models.BattleQuestionOption{},
		&models.BattleRoom{},
		&models.BattleRoomQuestion{},
		&models.BattleAnswer{},
		&models.GalleryCard{},
		&models.PlayerGalleryCard{},
		&models.Achievement{},
		&models.PlayerAchievement{},
		&models.GameSetting{},
	)
}