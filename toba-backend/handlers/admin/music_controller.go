package admin

import (
	"fmt"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/gofiber/fiber/v2"
)

func AdminGetMusicList(c *fiber.Ctx) error {
	uploadDir := "./uploads/music"

	os.MkdirAll(uploadDir, 0755)

	entries, err := os.ReadDir(uploadDir)
	if err != nil {
		return c.JSON([]fiber.Map{})
	}

	var files []fiber.Map

	for _, entry := range entries {
		if !entry.IsDir() {
			info, _ := entry.Info()

			files = append(files, fiber.Map{
				"filename": entry.Name(),
				"url":      "/uploads/music/" + entry.Name(),
				"size":     info.Size(),
				"type":     strings.ToLower(filepath.Ext(entry.Name())),
			})
		}
	}

	if files == nil {
		files = []fiber.Map{}
	}

	return c.JSON(files)
}

func AdminUploadMusic(c *fiber.Ctx) error {
	file, err := c.FormFile("music")
	if err != nil {
		return c.Status(400).JSON(fiber.Map{
			"message": "File tidak ditemukan",
		})
	}

	const maxMusicSize = 15 * 1024 * 1024

	if file.Size > maxMusicSize {
		return c.Status(400).JSON(fiber.Map{
			"message": "Ukuran audio maksimal 15MB",
		})
	}

	ext := strings.ToLower(filepath.Ext(file.Filename))

	allowed := map[string]bool{
		".mp3": true,
		".ogg": true,
		".wav": true,
	}

	if !allowed[ext] {
		return c.Status(400).JSON(fiber.Map{
			"message": "Format file tidak didukung. Gunakan mp3/ogg/wav",
		})
	}

	uploadDir := "./uploads/music"

	os.MkdirAll(uploadDir, 0755)

	filename := fmt.Sprintf(
		"music_%d%s",
		time.Now().UnixNano(),
		ext,
	)

	savePath := filepath.Join(uploadDir, filename)

	if err := c.SaveFile(file, savePath); err != nil {
		return c.Status(500).JSON(fiber.Map{
			"message": "Gagal menyimpan file",
		})
	}

	return c.JSON(fiber.Map{
		"filename": filename,
		"url":      "/uploads/music/" + filename,
	})
}

func AdminDeleteMusic(c *fiber.Ctx) error {
	filename := c.Params("filename")

	if strings.Contains(filename, "..") || strings.Contains(filename, "/") {
		return c.Status(400).JSON(fiber.Map{
			"message": "Nama file tidak valid",
		})
	}

	path := filepath.Join("./uploads/music", filename)

	if err := os.Remove(path); err != nil {
		return c.Status(404).JSON(fiber.Map{
			"message": "File tidak ditemukan",
		})
	}

	return c.JSON(fiber.Map{
		"message": "File berhasil dihapus",
	})
}
