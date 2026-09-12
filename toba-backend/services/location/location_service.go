package location

import (
	"math/rand"
	"strconv"

	"toba-backend/config"
	"toba-backend/models"

	"github.com/gofiber/fiber/v2"
)

func GetAllLocations() ([]models.Location, error) {
	var locations []models.Location

	if err := config.DB.
		Where("is_active = ?", true).
		Order("level_order ASC").
		Find(&locations).Error; err != nil {
		return nil, err
	}

	return locations, nil
}

func GetLocationDetail(locationID string) (*models.Location, error) {
	id, err := strconv.ParseUint(locationID, 10, 64)
	if err != nil {
		return nil, err
	}

	var location models.Location

	if err := config.DB.
		Where("id = ? AND is_active = ?", uint(id), true).
		First(&location).Error; err != nil {
		return nil, err
	}

	return &location, nil
}

func GetLocationFact(locationID string) (*models.LocationFact, error) {
	id, err := strconv.ParseUint(locationID, 10, 64)
	if err != nil {
		return nil, err
	}

	var facts []models.LocationFact

	if err := config.DB.
		Where("location_id = ?", uint(id)).
		Find(&facts).Error; err != nil {
		return nil, err
	}

	if len(facts) == 0 {
		return nil, nil
	}

	fact := facts[rand.Intn(len(facts))]

	return &fact, nil
}

func GetLocationID(locationID string) (uint, error) {
	id, err := strconv.ParseUint(locationID, 10, 64)
	if err != nil {
		return 0, fiber.ErrBadRequest
	}

	return uint(id), nil
}
