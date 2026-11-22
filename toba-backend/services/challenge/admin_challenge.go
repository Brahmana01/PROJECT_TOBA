package challenge

import (
	"errors"
	"strings"
	"time"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
	"gorm.io/gorm"
	"gorm.io/gorm/clause"
)

func AdminGetChallenges() ([]models.WeeklyChallenge, error) {
	var challenges []models.WeeklyChallenge
	if err := config.DB.Order("created_at DESC").Find(&challenges).Error; err != nil {
		return nil, err
	}
	return challenges, nil
}

func AdminCreateChallenge(title, description, gameType string, rewardXP, rewardStars int, dataJSON string, durationDays int) (*models.WeeklyChallenge, error) {
	title = strings.TrimSpace(title)
	description = strings.TrimSpace(description)
	gameType = NormalizeGameType(gameType)
	if title == "" || description == "" {
		return nil, errors.New("judul dan deskripsi wajib diisi")
	}
	if rewardXP < 0 || rewardStars < 0 {
		return nil, errors.New("reward tidak boleh negatif")
	}
	if err := ValidateChallengeContent(gameType, dataJSON); err != nil {
		return nil, err
	}

	duration := durationDays
	if duration <= 0 {
		duration = 7
	}
	if duration > 90 {
		return nil, errors.New("durasi tantangan maksimal 90 hari")
	}

	var created models.WeeklyChallenge
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.Model(&models.WeeklyChallenge{}).Where("is_active = ?", true).Update("is_active", false).Error; err != nil {
			return err
		}
		now := time.Now()
		created = models.WeeklyChallenge{
			Title: title, Description: description, GameType: gameType,
			RewardXP: rewardXP, RewardStars: rewardStars,
			DataJSON: dataJSON, IsActive: true,
			StartDate: now, EndDate: now.AddDate(0, 0, duration),
		}
		return tx.Create(&created).Error
	})
	if err != nil {
		return nil, err
	}
	return &created, nil
}

func AdminUpdateChallenge(id, title, description, gameType string, rewardXP, rewardStars int, dataJSON string, isActive *bool) (*models.WeeklyChallenge, string, int) {
	gameType = NormalizeGameType(gameType)
	title = strings.TrimSpace(title)
	description = strings.TrimSpace(description)
	if title == "" || description == "" || rewardXP < 0 || rewardStars < 0 {
		return nil, "Judul, deskripsi, dan reward harus valid", fiber.StatusBadRequest
	}
	if err := ValidateChallengeContent(gameType, dataJSON); err != nil {
		return nil, err.Error(), fiber.StatusBadRequest
	}

	var challenge models.WeeklyChallenge
	var businessMessage string
	var businessStatus int
	err := config.DB.Transaction(func(tx *gorm.DB) error {
		if err := tx.Clauses(clause.Locking{Strength: "UPDATE"}).First(&challenge, id).Error; err != nil {
			return err
		}

		if isActive != nil {
			if *isActive && !challenge.IsActive && time.Now().After(challenge.EndDate) {
				businessMessage = "Tantangan sudah berakhir. Buat tantangan baru untuk periode berikutnya."
				businessStatus = fiber.StatusConflict
				return errors.New(businessMessage)
			}
			challenge.IsActive = *isActive
		}

		challenge.Title = title
		challenge.Description = description
		challenge.GameType = gameType
		challenge.RewardXP = rewardXP
		challenge.RewardStars = rewardStars
		challenge.DataJSON = dataJSON

		// Keep a single active challenge when an existing challenge is edited.
		if challenge.IsActive {
			if err := tx.Model(&models.WeeklyChallenge{}).
				Where("id <> ? AND is_active = ?", challenge.ID, true).
				Update("is_active", false).Error; err != nil {
				return err
			}
		}

		return tx.Save(&challenge).Error
	})
	if err != nil {
		if businessStatus != 0 {
			return nil, businessMessage, businessStatus
		}
		if errors.Is(err, gorm.ErrRecordNotFound) {
			return nil, "Tantangan tidak ditemukan", fiber.StatusNotFound
		}
		return nil, "Gagal memperbarui tantangan", fiber.StatusInternalServerError
	}
	return &challenge, "", fiber.StatusOK
}

// Challenges are deactivated instead of physically deleted so historical
// progress and session references remain intact.
func AdminDeleteChallenge(id string) (string, int) {
	result := config.DB.Model(&models.WeeklyChallenge{}).Where("id = ?", id).Update("is_active", false)
	if result.Error != nil {
		return "Gagal menonaktifkan tantangan", fiber.StatusInternalServerError
	}
	if result.RowsAffected == 0 {
		return "Tantangan tidak ditemukan", fiber.StatusNotFound
	}
	return "Tantangan berhasil dinonaktifkan", fiber.StatusOK
}
