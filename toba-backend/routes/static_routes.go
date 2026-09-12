package routes

import "github.com/gofiber/fiber/v2"

func RegisterStaticRoutes(app *fiber.App) {
	app.Static("/uploads", "./uploads")

	app.Get("/", func(c *fiber.Ctx) error {
		return c.SendString("TOBA Backend is Running!")
	})
}
