package database

import (
	"log"
	"toba-backend/config"
	"toba-backend/models"

	"golang.org/x/crypto/bcrypt"
)

func SeedAll() {
	log.Println("🌱 Memulai seeding database...")
	seedCharacters()
	seedTitles()
	seedLocations()
	seedLocationFacts()
	seedTriviaCategories()
	seedTriviaQuestions()
	seedBattleQuestions()
	seedAchievements()
	seedGameSettings()
	seedAdmins()
	seedLocationQuestions()
	log.Println("✅ Seeding selesai!")
}

// ============================================================
func seedCharacters() {
	characters := []models.Character{
		{Name: "Ucok", Image: "ucok", Description: "Karakter laki-laki Batak Toba"},
		{Name: "Butet", Image: "butet", Description: "Karakter perempuan Batak Toba"},
	}
	for _, c := range characters {
		config.DB.FirstOrCreate(&c, models.Character{Name: c.Name})
	}
	log.Println("  ✓ Characters")
}

// ============================================================
func seedTitles() {
	titles := []models.Title{
		{TitleName: "Wisatawan", MinimumLevel: 1},
		{TitleName: "Penjelajah Toba", MinimumLevel: 5},
		{TitleName: "Sahabat Budaya", MinimumLevel: 10},
		{TitleName: "Duta Budaya", MinimumLevel: 15},
		{TitleName: "Maestro Budaya Batak", MinimumLevel: 20},
	}
	for _, t := range titles {
		config.DB.FirstOrCreate(&t, models.Title{MinimumLevel: t.MinimumLevel})
	}
	log.Println("  ✓ Titles")
}

// ============================================================
func seedLocations() {
	locations := []models.Location{
		{
			Name:           "IT Del",
			Description:    "Institut Teknologi Del adalah perguruan tinggi yang terletak di Laguboti, Toba. Didirikan oleh Luhut Binsar Pandjaitan dan diresmikan oleh Susilo Bambang Yudhoyono pada 2012.",
			Image:          "itdel",
			RewardXP:       100,
			RewardStars:    3,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     1,
			IsActive:       true,
		},
		{
			Name:           "Pantai Bulbul",
			Description:    "Pantai Bulbul adalah salah satu pantai indah di Danau Toba yang terletak di Balige. Terkenal dengan pasir putihnya dan pemandangan Danau Toba yang menakjubkan.",
			Image:          "pantaibulbul",
			RewardXP:       120,
			RewardStars:    3,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     2,
			IsActive:       true,
		},
		{
			Name:           "Museum Batak",
			Description:    "Museum Batak TB Silalahi Center terletak di Balige, menyimpan ribuan koleksi benda budaya Batak Toba mulai dari pakaian adat, senjata tradisional, hingga peralatan rumah tangga.",
			Image:          "museumbatak",
			RewardXP:       140,
			RewardStars:    3,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     3,
			IsActive:       true,
		},
		{
			Name:           "Huta Siallagan",
			Description:    "Huta Siallagan adalah desa tradisional Batak Toba di Pulau Samosir. Terkenal dengan Batu Kursi Raja Siallagan yang digunakan untuk mengadili penjahat pada zaman dahulu.",
			Image:          "hutasiallagan",
			RewardXP:       160,
			RewardStars:    4,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     4,
			IsActive:       true,
		},
		{
			Name:           "Bukit Holbung",
			Description:    "Bukit Holbung atau Bukit Teletubbies adalah bukit hijau yang terletak di Kabupaten Samosir. Dari puncaknya, pengunjung dapat menikmati pemandangan Danau Toba yang spektakuler.",
			Image:          "bukitholbung",
			RewardXP:       180,
			RewardStars:    4,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     5,
			IsActive:       true,
		},
		{
			Name:           "Desa Tomok",
			Description:    "Desa Tomok adalah salah satu desa tertua di Pulau Samosir. Terkenal dengan Makam Raja Sidabutar yang merupakan situs bersejarah penting bagi masyarakat Batak Toba.",
			Image:          "desatomok",
			RewardXP:       200,
			RewardStars:    4,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     6,
			IsActive:       true,
		},
		{
			Name:           "Batu Gantung",
			Description:    "Batu Gantung adalah formasi batu unik yang menggantung di tepi Danau Toba, Parapat. Legenda masyarakat setempat menyebutkan batu ini berasal dari seorang putri yang bersedih.",
			Image:          "batugantung",
			RewardXP:       220,
			RewardStars:    4,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     7,
			IsActive:       true,
		},
		{
			Name:           "Menara Tele",
			Description:    "Menara Tele adalah menara pandang yang terletak di Desa Tele, Samosir. Dari menara ini pengunjung dapat melihat panorama Danau Toba dan hamparan sawah yang indah.",
			Image:          "menaratele",
			RewardXP:       240,
			RewardStars:    5,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     8,
			IsActive:       true,
		},
		{
			Name:           "Air Terjun Situmurun",
			Description:    "Air Terjun Situmurun atau Binangalom adalah air terjun yang langsung mengalir ke Danau Toba di Kabupaten Toba. Merupakan salah satu air terjun paling indah di kawasan Danau Toba.",
			Image:          "situmurun",
			RewardXP:       260,
			RewardStars:    5,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     9,
			IsActive:       true,
		},
		{
			Name:           "Sigale-gale",
			Description:    "Sigale-gale adalah patung boneka kayu yang dapat bergerak dan menari dalam upacara adat Batak Toba. Patung ini merupakan simbol penting dalam ritual pemakaman adat Batak.",
			Image:          "sigalegale",
			RewardXP:       300,
			RewardStars:    5,
			QuestionCount:  5,
			MinimumCorrect: 3,
			LevelOrder:     10,
			IsActive:       true,
		},
	}
	for _, loc := range locations {
		config.DB.FirstOrCreate(&loc, models.Location{LevelOrder: loc.LevelOrder})
	}
	log.Println("  ✓ Locations (10 lokasi)")
}

// ============================================================
func seedLocationFacts() {
	type factSeed struct {
		LocationOrder int
		Facts         []string
	}

	facts := []factSeed{
		{1, []string{
			"IT Del merupakan singkatan dari Inpres Toba Development.",
			"IT Del didirikan oleh Luhut Binsar Pandjaitan pada tahun 2001.",
			"IT Del terletak di tepi Danau Toba, Laguboti, Sumatera Utara.",
			"IT Del menjadi salah satu kampus teknologi terbaik di Sumatera.",
		}},
		{2, []string{
			"Pantai Bulbul memiliki pasir putih yang halus khas Danau Toba.",
			"Pantai Bulbul berlokasi di Balige, ibu kota Kabupaten Toba.",
			"Danau Toba adalah danau vulkanik terbesar di dunia.",
			"Danau Toba terbentuk akibat letusan supervulkan sekitar 74.000 tahun lalu.",
		}},
		{3, []string{
			"Museum Batak menyimpan lebih dari 1.000 koleksi benda budaya Batak.",
			"TB Silalahi adalah tokoh militer dan politisi asal Batak Toba.",
			"Ulos adalah kain tradisional Batak yang memiliki makna filosofis mendalam.",
			"Gorga adalah ukiran khas Batak yang biasa menghiasi rumah adat.",
		}},
		{4, []string{
			"Huta Siallagan berasal dari nama marga Siallagan yang mendiami desa ini.",
			"Batu kursi di Huta Siallagan digunakan sebagai tempat pengadilan adat.",
			"Pulau Samosir adalah pulau terbesar di dalam danau di dunia.",
			"Samosir memiliki luas sekitar 630 kilometer persegi.",
		}},
		{5, []string{
			"Bukit Holbung mendapat julukan Bukit Teletubbies karena bentuknya yang bergelombang hijau.",
			"Dari Bukit Holbung terlihat panorama Danau Toba 360 derajat.",
			"Ketinggian Bukit Holbung sekitar 1.000 meter di atas permukaan laut.",
			"Bukit Holbung ramai dikunjungi saat matahari terbit dan terbenam.",
		}},
		{6, []string{
			"Makam Raja Sidabutar di Desa Tomok berusia lebih dari 500 tahun.",
			"Sigale-gale pertama kali dibuat di Desa Tomok.",
			"Desa Tomok terkenal sebagai pusat kerajinan tangan Batak Toba.",
			"Kain ulos dari Tomok dikenal sebagai salah satu yang terbaik.",
		}},
		{7, []string{
			"Batu Gantung terletak di Parapat, Kabupaten Simalungun.",
			"Legenda Batu Gantung bercerita tentang seorang gadis bernama Seruni.",
			"Batu Gantung dapat dilihat dari kapal feri yang melintasi Danau Toba.",
			"Lokasi Batu Gantung menjadi salah satu ikon wisata Parapat.",
		}},
		{8, []string{
			"Menara Tele berdiri di ketinggian sekitar 1.300 meter di atas permukaan laut.",
			"Dari Menara Tele terlihat hamparan sawah dan Danau Toba sekaligus.",
			"Tele adalah nama desa yang menjadi gerbang masuk Pulau Samosir dari darat.",
			"Jalan Tele dikenal sebagai salah satu jalur pegunungan terindah di Sumatera Utara.",
		}},
		{9, []string{
			"Air Terjun Situmurun memiliki ketinggian sekitar 70 meter.",
			"Situmurun berarti turun bertahap dalam bahasa Batak Toba.",
			"Air terjun ini langsung jatuh ke perairan Danau Toba.",
			"Wisatawan bisa berenang di kolam alami di bawah air terjun ini.",
		}},
		{10, []string{
			"Sigale-gale dibuat sebagai pengganti anak bagi raja yang tidak memiliki keturunan.",
			"Sigale-gale dapat bergerak dengan mekanisme tali yang dikendalikan manusia.",
			"Upacara Sigale-gale biasanya disertai musik gondang.",
			"Gondang adalah alat musik perkusi tradisional Batak Toba.",
		}},
	}

	for _, fs := range facts {
		var loc models.Location
		config.DB.Where("level_order = ?", fs.LocationOrder).First(&loc)
		if loc.ID == 0 {
			continue
		}
		for _, factText := range fs.Facts {
			fact := models.LocationFact{
				LocationID: loc.ID,
				Fact:       factText,
			}
			config.DB.FirstOrCreate(&fact, models.LocationFact{
				LocationID: loc.ID,
				Fact:       factText,
			})
		}
	}
	log.Println("  ✓ Location Facts")
}

// ============================================================
func seedTriviaCategories() {
	categories := []models.TriviaCategory{
		{Name: "Makanan Tradisional", Description: "Kuliner khas Batak Toba", QuestionCount: 10, MinimumQuestion: 10, DisplayOrder: 1, IsActive: true},
		{Name: "Pakaian Adat", Description: "Busana tradisional Batak Toba", QuestionCount: 10, MinimumQuestion: 10, DisplayOrder: 2, IsActive: true},
		{Name: "Musik Tradisional", Description: "Alat musik dan lagu Batak Toba", QuestionCount: 10, MinimumQuestion: 10, DisplayOrder: 3, IsActive: true},
		{Name: "Tempat Wisata", Description: "Destinasi wisata di kawasan Toba", QuestionCount: 10, MinimumQuestion: 10, DisplayOrder: 4, IsActive: true},
		{Name: "Adat Istiadat", Description: "Tradisi dan budaya Batak Toba", QuestionCount: 10, MinimumQuestion: 10, DisplayOrder: 5, IsActive: true},
	}
	for _, c := range categories {
		config.DB.FirstOrCreate(&c, models.TriviaCategory{Name: c.Name})
	}
	log.Println("  ✓ Trivia Categories")
}

// ============================================================
func seedTriviaQuestions() {
	type qSeed struct {
		CategoryName string
		Question     string
		Explanation  string
		Options      []struct {
			Text      string
			IsCorrect bool
		}
	}

	questions := []qSeed{
		// MAKANAN
		{"Makanan Tradisional", "Apa nama masakan khas Batak Toba yang terbuat dari daging babi dicampur darah?",
			"Saksang adalah masakan ritual Batak Toba yang biasa disajikan dalam acara adat.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Saksang", true}, {"Rendang", false}, {"Gulai", false}, {"Arsik", false}}},
		{"Makanan Tradisional", "Masakan ikan yang dimasak dengan bumbu kuning khas Batak disebut?",
			"Arsik adalah masakan ikan mas dengan bumbu kunyit dan andaliman khas Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Arsik", true}, {"Pindang", false}, {"Pepes", false}, {"Saksang", false}}},
		{"Makanan Tradisional", "Bumbu rempah khas Batak yang menyerupai merica dengan rasa getir disebut?",
			"Andaliman adalah rempah khas Batak Toba yang tumbuh di kawasan Danau Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Andaliman", true}, {"Kemiri", false}, {"Ketumbar", false}, {"Jintan", false}}},
		{"Makanan Tradisional", "Minuman tradisional Batak yang terbuat dari fermentasi beras adalah?",
			"Tuak adalah minuman fermentasi tradisional yang sering hadir dalam upacara adat Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Tuak", true}, {"Brem", false}, {"Arak", false}, {"Tape", false}}},
		{"Makanan Tradisional", "Kue tradisional Batak yang dibungkus daun pisang dan dipanggang disebut?",
			"Itak Gurgur adalah kue tradisional Batak berbahan tepung beras yang khas.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Itak Gurgur", true}, {"Lemang", false}, {"Lepat", false}, {"Nagasari", false}}},
		{"Makanan Tradisional", "Sayur khas Batak yang terbuat dari daun singkong direbus dengan santan disebut?",
			"Sayur daun singkong dengan santan adalah lauk pendamping yang umum dalam masakan Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Daun Singkong Santan", true}, {"Gulai Pakis", false}, {"Lodeh", false}, {"Urap", false}}},
		{"Makanan Tradisional", "Makanan Batak yang dibuat dari nasi dipadatkan dan dibakar dalam bambu adalah?",
			"Naniura adalah hidangan ikan mentah yang diasinkan dengan bumbu khas Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Naniura", true}, {"Lemang", false}, {"Lontong", false}, {"Ketupat", false}}},
		{"Makanan Tradisional", "Ikan mas dalam masakan Batak Toba dikenal sebagai simbol?",
			"Ikan mas atau dekke mas dalam adat Batak melambangkan berkat dan kemakmuran.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Kemakmuran dan berkat", true}, {"Kesedihan", false}, {"Keberanian", false}, {"Kecantikan", false}}},
		{"Makanan Tradisional", "Cara memasak arsik yang khas adalah dengan?",
			"Arsik dimasak tanpa air tambahan, mengandalkan kandungan air dari bumbu dan bahan.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Tanpa air, hanya bumbu", true}, {"Digoreng dalam minyak", false}, {"Dikukus dalam bambu", false}, {"Dibakar di atas bara", false}}},
		{"Makanan Tradisional", "Daging yang paling sering digunakan dalam saksang adalah?",
			"Saksang paling umum menggunakan daging babi atau daging anjing dalam tradisi Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Babi", true}, {"Sapi", false}, {"Kambing", false}, {"Ayam", false}}},

		// PAKAIAN ADAT
		{"Pakaian Adat", "Kain tradisional Batak Toba yang diberikan sebagai simbol kasih sayang disebut?",
			"Ulos adalah kain tenun tradisional Batak yang memiliki makna filosofis dan spiritual mendalam.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Ulos", true}, {"Batik", false}, {"Songket", false}, {"Tenun", false}}},
		{"Pakaian Adat", "Ulos yang digunakan dalam upacara pernikahan Batak Toba disebut?",
			"Ulos Ragi Hotang adalah ulos yang khusus diberikan kepada pengantin dalam adat Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Ragi Hotang", true}, {"Sadum", false}, {"Bintang Maratur", false}, {"Ragi Idup", false}}},
		{"Pakaian Adat", "Hiasan kepala pria dalam adat Batak Toba disebut?",
			"Sabe-sabe atau ikat kepala adalah aksesori kepala tradisional pria Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Sabe-sabe", true}, {"Destar", false}, {"Blangkon", false}, {"Tanjak", false}}},
		{"Pakaian Adat", "Warna yang dominan dalam ulos Batak Toba adalah?",
			"Merah, hitam, dan putih adalah tiga warna utama dalam tenun ulos Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Merah, hitam, putih", true}, {"Biru, hijau, kuning", false}, {"Ungu, orange, merah", false}, {"Kuning, hijau, biru", false}}},
		{"Pakaian Adat", "Ulos yang diberikan kepada orang yang sedang berduka disebut?",
			"Ulos Sibolang diberikan kepada keluarga yang sedang berduka sebagai ungkapan belasungkawa.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Sibolang", true}, {"Ragi Hotang", false}, {"Sadum", false}, {"Ragi Idup", false}}},
		{"Pakaian Adat", "Proses menenun ulos secara tradisional menggunakan alat yang disebut?",
			"Alat tenun tradisional Batak disebut ATBM atau gedogan yang dioperasikan dengan tangan.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Gedogan", true}, {"Tombak", false}, {"Hasapi", false}, {"Gordang", false}}},
		{"Pakaian Adat", "Ulos yang memiliki makna kehidupan dan harapan disebut?",
			"Ulos Ragi Idup melambangkan kehidupan dan harapan yang terus berkembang.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Ragi Idup", true}, {"Sibolang", false}, {"Bintang Maratur", false}, {"Mangiring", false}}},
		{"Pakaian Adat", "Dalam adat Batak, pemberian ulos disebut?",
			"Mangulosi adalah ritual pemberian ulos yang memiliki makna penting dalam adat Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Mangulosi", true}, {"Marhata", false}, {"Martarombo", false}, {"Marpadan", false}}},
		{"Pakaian Adat", "Ulos yang digunakan oleh penari tortor biasanya berwarna?",
			"Penari tortor biasanya mengenakan ulos dengan warna merah sebagai simbol semangat.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Merah dan hitam", true}, {"Putih dan biru", false}, {"Hijau dan kuning", false}, {"Ungu dan emas", false}}},
		{"Pakaian Adat", "Perhiasan khas wanita Batak Toba yang terbuat dari perak disebut?",
			"Subang atau anting-anting perak adalah perhiasan tradisional wanita Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Subang", true}, {"Gelang Kana", false}, {"Kalung Emas", false}, {"Mahkota", false}}},

		// MUSIK TRADISIONAL
		{"Musik Tradisional", "Alat musik petik tradisional Batak Toba yang terbuat dari kayu disebut?",
			"Hasapi adalah alat musik petik tradisional Batak Toba yang menyerupai kecapi.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Hasapi", true}, {"Gitar", false}, {"Sitar", false}, {"Kecapi", false}}},
		{"Musik Tradisional", "Ensemble musik tradisional Batak Toba disebut?",
			"Gondang Sabangunan adalah ensemble musik tradisional Batak Toba yang lengkap.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Gondang Sabangunan", true}, {"Gamelan", false}, {"Kolintang", false}, {"Talempong", false}}},
		{"Musik Tradisional", "Alat musik tiup tradisional Batak Toba disebut?",
			"Sarune Bolon adalah alat musik tiup tradisional Batak Toba yang menghasilkan suara nyaring.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Sarune Bolon", true}, {"Seruling", false}, {"Suling", false}, {"Terompet", false}}},
		{"Musik Tradisional", "Tari tradisional Batak Toba yang diiringi musik gondang disebut?",
			"Tortor adalah tari tradisional Batak Toba yang diiringi gondang dan memiliki makna spiritual.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Tortor", true}, {"Saman", false}, {"Kecak", false}, {"Piring", false}}},
		{"Musik Tradisional", "Alat musik perkusi utama dalam gondang Batak Toba adalah?",
			"Taganing adalah seperangkat drum yang menjadi alat musik utama dalam gondang Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Taganing", true}, {"Kendang", false}, {"Bedug", false}, {"Rebana", false}}},
		{"Musik Tradisional", "Lagu daerah Batak Toba yang paling terkenal adalah?",
			"Sinanggar Tulo adalah salah satu lagu daerah Batak Toba yang paling populer.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Sinanggar Tulo", true}, {"Batak Na Uli", false}, {"Sialang Bungkuk", false}, {"Si Patokaan", false}}},
		{"Musik Tradisional", "Alat musik gesek tradisional Batak Toba disebut?",
			"Arbab adalah alat musik gesek tradisional Batak Toba yang terbuat dari tempurung kelapa.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Arbab", true}, {"Rebab", false}, {"Violin", false}, {"Erhu", false}}},
		{"Musik Tradisional", "Gondang dalam adat Batak Toba berfungsi sebagai?",
			"Gondang dalam adat Batak berfungsi sebagai media komunikasi dengan leluhur dan Sang Pencipta.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Media komunikasi dengan leluhur", true}, {"Hiburan semata", false}, {"Penanda waktu", false}, {"Penyambut tamu", false}}},
		{"Musik Tradisional", "Berapa jumlah gendang dalam ensemble Gondang Sabangunan?",
			"Gondang Sabangunan terdiri dari 5 gendang taganing dan 1 odap-odap.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"5", true}, {"3", false}, {"7", false}, {"9", false}}},
		{"Musik Tradisional", "Istilah untuk penyanyi dalam musik Batak Toba adalah?",
			"Parsibar-sibar atau parhata adalah sebutan untuk penyanyi atau pembawa lagu Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Parsibar-sibar", true}, {"Penabuh", false}, {"Dalang", false}, {"Griyo", false}}},

		// TEMPAT WISATA
		{"Tempat Wisata", "Danau Toba terletak di provinsi mana?",
			"Danau Toba terletak di Provinsi Sumatera Utara, Indonesia.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Sumatera Utara", true}, {"Sumatera Barat", false}, {"Riau", false}, {"Aceh", false}}},
		{"Tempat Wisata", "Pulau yang terletak di tengah Danau Toba disebut?",
			"Pulau Samosir adalah pulau vulkanik yang terletak di tengah Danau Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Samosir", true}, {"Nias", false}, {"Batam", false}, {"Bintan", false}}},
		{"Tempat Wisata", "Danau Toba terbentuk akibat letusan supervulkan pada berapa tahun lalu?",
			"Danau Toba terbentuk sekitar 74.000 tahun lalu akibat letusan supervolcano terbesar.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"74.000 tahun lalu", true}, {"10.000 tahun lalu", false}, {"1.000 tahun lalu", false}, {"100.000 tahun lalu", false}}},
		{"Tempat Wisata", "Kota terdekat dari Danau Toba yang menjadi pintu masuk wisatawan adalah?",
			"Parapat adalah kota di tepi Danau Toba yang menjadi pintu masuk utama wisatawan.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Parapat", true}, {"Medan", false}, {"Pematangsiantar", false}, {"Balige", false}}},
		{"Tempat Wisata", "Danau Toba diakui UNESCO sebagai?",
			"Danau Toba diakui UNESCO sebagai Global Geopark pada tahun 2020.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Global Geopark", true}, {"Warisan Budaya", false}, {"Taman Nasional", false}, {"Biosfer Dunia", false}}},
		{"Tempat Wisata", "Penyeberangan dari Parapat ke Samosir menggunakan?",
			"Kapal feri menjadi transportasi utama dari Parapat menuju Pulau Samosir.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Kapal feri", true}, {"Jembatan", false}, {"Helikopter", false}, {"Speedboat saja", false}}},
		{"Tempat Wisata", "Kabupaten yang melingkupi sebagian besar Danau Toba adalah?",
			"Kabupaten Toba (dahulu Toba Samosir) melingkupi wilayah terbesar Danau Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Kabupaten Toba", true}, {"Kabupaten Dairi", false}, {"Kabupaten Karo", false}, {"Kabupaten Humbang", false}}},
		{"Tempat Wisata", "Festival budaya tahunan yang digelar di kawasan Danau Toba disebut?",
			"Festival Danau Toba adalah event tahunan yang mempromosikan budaya Batak kepada dunia.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Festival Danau Toba", true}, {"Pesta Rakyat Toba", false}, {"Karnaval Samosir", false}, {"Pekan Batak", false}}},
		{"Tempat Wisata", "Kedalaman maksimum Danau Toba adalah sekitar?",
			"Danau Toba memiliki kedalaman maksimum sekitar 505 meter, menjadikannya danau terdalam di Asia Tenggara.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"505 meter", true}, {"200 meter", false}, {"100 meter", false}, {"1.000 meter", false}}},
		{"Tempat Wisata", "Luas Danau Toba adalah sekitar?",
			"Danau Toba memiliki luas sekitar 1.145 kilometer persegi.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"1.145 km²", true}, {"500 km²", false}, {"2.000 km²", false}, {"300 km²", false}}},

		// ADAT ISTIADAT
		{"Adat Istiadat", "Sistem kekerabatan masyarakat Batak Toba berdasarkan garis keturunan?",
			"Masyarakat Batak Toba menganut sistem patrilineal, yaitu garis keturunan dari pihak ayah.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Patrilineal", true}, {"Matrilineal", false}, {"Bilateral", false}, {"Ambilineal", false}}},
		{"Adat Istiadat", "Sistem adat Batak Toba yang mengatur hubungan kekerabatan disebut?",
			"Dalihan Na Tolu adalah sistem adat Batak yang mengatur tiga pilar hubungan kekerabatan.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Dalihan Na Tolu", true}, {"Suku Adat", false}, {"Marga Sistem", false}, {"Hula-hula", false}}},
		{"Adat Istiadat", "Tiga pilar dalam Dalihan Na Tolu adalah?",
			"Dalihan Na Tolu terdiri dari Hula-hula, Dongan Tubu, dan Boru.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Hula-hula, Dongan Tubu, Boru", true}, {"Raja, Rakyat, Pendeta", false}, {"Ayah, Ibu, Anak", false}, {"Kakek, Ayah, Cucu", false}}},
		{"Adat Istiadat", "Upacara kematian adat Batak Toba yang besar disebut?",
			"Saur Matua adalah upacara adat kematian tertinggi dalam budaya Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Saur Matua", true}, {"Mangongkal Holi", false}, {"Marhata", false}, {"Martumpol", false}}},
		{"Adat Istiadat", "Upacara pemindahan tulang leluhur dalam adat Batak disebut?",
			"Mangongkal Holi adalah ritual pemindahan tulang leluhur ke tempat yang lebih terhormat.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Mangongkal Holi", true}, {"Saur Matua", false}, {"Mardebata", false}, {"Tortor", false}}},
		{"Adat Istiadat", "Marga dalam suku Batak diturunkan dari?",
			"Marga dalam adat Batak diturunkan dari pihak ayah (patrilineal).",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Pihak ayah", true}, {"Pihak ibu", false}, {"Kedua orang tua", false}, {"Pilihan sendiri", false}}},
		{"Adat Istiadat", "Musyawarah adat dalam tradisi Batak Toba disebut?",
			"Rapat Adat atau Pardalanan adalah forum musyawarah dalam masyarakat Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Pardalanan", true}, {"Sidang Adat", false}, {"Musyawarah Marga", false}, {"Ronda Malam", false}}},
		{"Adat Istiadat", "Posisi Hula-hula dalam Dalihan Na Tolu adalah?",
			"Hula-hula adalah keluarga dari pihak istri yang memiliki posisi terhormat dalam adat Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Keluarga pihak istri", true}, {"Keluarga pihak suami", false}, {"Saudara kandung", false}, {"Tetangga dekat", false}}},
		{"Adat Istiadat", "Ucapan salam khas Batak Toba adalah?",
			"Horas adalah salam khas Batak Toba yang bermakna sehat, sejahtera, dan selamat.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Horas", true}, {"Mejuah-juah", false}, {"Yaahowu", false}, {"Njuah-juah", false}}},
		{"Adat Istiadat", "Rumah adat Batak Toba disebut?",
			"Rumah Bolon atau Jabu adalah rumah adat tradisional masyarakat Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Rumah Bolon", true}, {"Rumah Gadang", false}, {"Joglo", false}, {"Honai", false}}},
	}

	for _, q := range questions {
		var cat models.TriviaCategory
		config.DB.Where("name = ?", q.CategoryName).First(&cat)
		if cat.ID == 0 {
			continue
		}

		question := models.TriviaQuestion{
			CategoryID:  cat.ID,
			Question:    q.Question,
			Explanation: q.Explanation,
			IsActive:    true,
		}

		var existing models.TriviaQuestion
		if config.DB.Where("question = ? AND category_id = ?", q.Question, cat.ID).
			First(&existing).Error != nil {
			config.DB.Create(&question)
			for _, opt := range q.Options {
				config.DB.Create(&models.TriviaQuestionOption{
					QuestionID: question.ID,
					OptionText: opt.Text,
					IsCorrect:  opt.IsCorrect,
				})
			}
		}
	}
	log.Println("  ✓ Trivia Questions (50 soal)")
}

// ============================================================
func seedBattleQuestions() {
	type qSeed struct {
		Question    string
		Explanation string
		Options     []struct {
			Text      string
			IsCorrect bool
		}
	}

	questions := []qSeed{
		{"Apa nama danau terbesar di Indonesia?",
			"Danau Toba adalah danau terbesar di Indonesia dan danau vulkanik terbesar di dunia.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Danau Toba", true}, {"Danau Singkarak", false}, {"Danau Maninjau", false}, {"Danau Poso", false}}},
		{"Pulau apa yang berada di tengah Danau Toba?",
			"Pulau Samosir adalah pulau terbesar yang berada di tengah Danau Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Samosir", true}, {"Nias", false}, {"Batam", false}, {"Mentawai", false}}},
		{"Kain tradisional Batak Toba yang terkenal disebut?",
			"Ulos adalah kain tenun tradisional Batak Toba yang memiliki makna budaya mendalam.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Ulos", true}, {"Batik", false}, {"Songket", false}, {"Tapis", false}}},
		{"Sistem kekerabatan Batak yang mengatur tiga pilar hubungan disebut?",
			"Dalihan Na Tolu adalah sistem adat Batak yang mengatur hubungan hula-hula, dongan tubu, dan boru.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Dalihan Na Tolu", true}, {"Marga System", false}, {"Adat Minang", false}, {"Suku Adat", false}}},
		{"Tari tradisional Batak Toba yang diiringi gondang disebut?",
			"Tortor adalah tari tradisional Batak Toba yang memiliki makna ritual dan spiritual.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Tortor", true}, {"Saman", false}, {"Kecak", false}, {"Pendet", false}}},
		{"Ucapan salam khas masyarakat Batak Toba adalah?",
			"Horas adalah salam khas Batak Toba yang berarti sehat dan sejahtera.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Horas", true}, {"Marhaban", false}, {"Selamat", false}, {"Mejuah-juah", false}}},
		{"Masakan khas Batak dari ikan mas dengan bumbu kuning disebut?",
			"Arsik adalah masakan ikan mas khas Batak Toba yang dimasak dengan bumbu andaliman.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Arsik", true}, {"Pindang", false}, {"Saksang", false}, {"Gulai", false}}},
		{"Alat musik petik tradisional Batak Toba disebut?",
			"Hasapi adalah alat musik petik tradisional Batak Toba menyerupai kecapi.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Hasapi", true}, {"Sasando", false}, {"Kecapi", false}, {"Siter", false}}},
		{"Rumah adat Batak Toba disebut?",
			"Rumah Bolon adalah rumah adat tradisional masyarakat Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Rumah Bolon", true}, {"Rumah Gadang", false}, {"Joglo", false}, {"Tongkonan", false}}},
		{"Rempah khas Batak dengan rasa getir yang tumbuh di kawasan Toba?",
			"Andaliman adalah rempah khas Batak Toba yang sering digunakan dalam masakan arsik dan saksang.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Andaliman", true}, {"Kemiri", false}, {"Lada", false}, {"Ketumbar", false}}},
		{"Boneka kayu yang bisa bergerak dalam upacara adat Batak disebut?",
			"Sigale-gale adalah patung boneka kayu yang digerakkan dalam upacara adat Batak Toba.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Sigale-gale", true}, {"Wayang", false}, {"Ondel-ondel", false}, {"Barong", false}}},
		{"Festival budaya tahunan di kawasan Danau Toba disebut?",
			"Festival Danau Toba adalah event budaya tahunan yang mempromosikan kekayaan budaya Batak.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Festival Danau Toba", true}, {"Pesta Rakyat", false}, {"Karnaval Toba", false}, {"Pekan Budaya", false}}},
		{"UNESCO mengakui Danau Toba sebagai?",
			"Danau Toba diakui UNESCO sebagai Global Geopark pada tahun 2020.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Global Geopark", true}, {"Warisan Dunia", false}, {"Biosfer", false}, {"Taman Nasional", false}}},
		{"Makam Raja Sidabutar yang bersejarah terletak di?",
			"Makam Raja Sidabutar yang berusia 500 tahun terletak di Desa Tomok, Samosir.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Desa Tomok", true}, {"Parapat", false}, {"Balige", false}, {"Tarutung", false}}},
		{"Upacara pemindahan tulang leluhur Batak disebut?",
			"Mangongkal Holi adalah ritual sakral pemindahan tulang leluhur ke tempat yang lebih mulia.",
			[]struct {
				Text      string
				IsCorrect bool
			}{{"Mangongkal Holi", true}, {"Saur Matua", false}, {"Mardebata", false}, {"Tortor", false}}},
	}

	for _, q := range questions {
		var existing models.BattleQuestion
		if config.DB.Where("question = ?", q.Question).First(&existing).Error != nil {
			bq := models.BattleQuestion{
				Question:    q.Question,
				Explanation: q.Explanation,
				IsActive:    true,
			}
			config.DB.Create(&bq)
			for _, opt := range q.Options {
				config.DB.Create(&models.BattleQuestionOption{
					QuestionID: bq.ID,
					OptionText: opt.Text,
					IsCorrect:  opt.IsCorrect,
				})
			}
		}
	}
	log.Println("  ✓ Battle Questions (15 soal)")
}

// ============================================================
func seedAchievements() {
	achievements := []models.Achievement{
		{Name: "Penjelajah Pemula", Description: "Selesaikan 1 lokasi Jelajah Budaya", Icon: "🗺", ConditionType: "LOCATION", ConditionValue: 1, RewardXP: 100, RewardStars: 0, IsActive: true},
		{Name: "Penjelajah Sejati", Description: "Selesaikan 5 lokasi Jelajah Budaya", Icon: "🧭", ConditionType: "LOCATION", ConditionValue: 5, RewardXP: 300, RewardStars: 5, IsActive: true},
		{Name: "Master Penjelajah", Description: "Selesaikan semua 10 lokasi", Icon: "🏆", ConditionType: "LOCATION", ConditionValue: 10, RewardXP: 500, RewardStars: 10, IsActive: true},
		{Name: "Kuiz Perdana", Description: "Mainkan Trivia Budaya pertama kali", Icon: "❓", ConditionType: "TRIVIA", ConditionValue: 1, RewardXP: 50, RewardStars: 0, IsActive: true},
		{Name: "Pecinta Trivia", Description: "Mainkan Trivia sebanyak 10 kali", Icon: "📚", ConditionType: "TRIVIA", ConditionValue: 10, RewardXP: 200, RewardStars: 3, IsActive: true},
		{Name: "Pendekar Battle", Description: "Menangkan 1 Battle Quiz", Icon: "⚔", ConditionType: "BATTLE", ConditionValue: 1, RewardXP: 150, RewardStars: 2, IsActive: true},
		{Name: "Raja Battle", Description: "Menangkan 5 Battle Quiz", Icon: "👑", ConditionType: "BATTLE", ConditionValue: 5, RewardXP: 400, RewardStars: 8, IsActive: true},
		{Name: "Kolektor Bintang", Description: "Kumpulkan 20 bintang", Icon: "⭐", ConditionType: "STAR", ConditionValue: 20, RewardXP: 200, RewardStars: 0, IsActive: true},
		{Name: "Bintang Sejati", Description: "Kumpulkan 50 bintang", Icon: "🌟", ConditionType: "STAR", ConditionValue: 50, RewardXP: 500, RewardStars: 0, IsActive: true},
		{Name: "Naik Level", Description: "Capai Level 5", Icon: "📈", ConditionType: "LEVEL", ConditionValue: 5, RewardXP: 100, RewardStars: 5, IsActive: true},
		{Name: "Duta Budaya", Description: "Capai Level 15", Icon: "🎖", ConditionType: "LEVEL", ConditionValue: 15, RewardXP: 500, RewardStars: 10, IsActive: true},
		{Name: "Maestro Toba", Description: "Capai Level 20", Icon: "🎓", ConditionType: "LEVEL", ConditionValue: 20, RewardXP: 1000, RewardStars: 20, IsActive: true},
		{Name: "XP Pertama", Description: "Kumpulkan 500 XP", Icon: "✨", ConditionType: "XP", ConditionValue: 500, RewardXP: 0, RewardStars: 2, IsActive: true},
		{Name: "Pencari Ilmu", Description: "Kumpulkan 2000 XP", Icon: "💡", ConditionType: "XP", ConditionValue: 2000, RewardXP: 0, RewardStars: 5, IsActive: true},
	}

	for _, a := range achievements {
		config.DB.FirstOrCreate(&a, models.Achievement{Name: a.Name})
	}
	log.Println("  ✓ Achievements (14 achievement)")
}

// ============================================================
func seedGameSettings() {
	settings := []models.GameSetting{
		{SettingKey: "battle_question_count", SettingValue: "5", Description: "Jumlah soal per Battle"},
		{SettingKey: "battle_wait_timeout", SettingValue: "300", Description: "Waktu tunggu room battle (detik)"},
		{SettingKey: "trivia_question_count", SettingValue: "10", Description: "Jumlah soal per sesi Trivia"},
		{SettingKey: "location_question_count", SettingValue: "5", Description: "Jumlah soal per lokasi Jelajah"},
		{SettingKey: "xp_per_level", SettingValue: "500", Description: "XP yang dibutuhkan per level"},
		{SettingKey: "character_change_level", SettingValue: "5", Description: "Level minimum ganti karakter"},
		{SettingKey: "initial_xp", SettingValue: "0", Description: "XP awal pemain baru"},
		{SettingKey: "initial_stars", SettingValue: "0", Description: "Bintang awal pemain baru"},
		{SettingKey: "battle_reward_xp", SettingValue: "50", Description: "XP reward untuk pemenang battle"},
		{SettingKey: "battle_reward_stars", SettingValue: "2", Description: "Bintang reward pemenang battle"},
	}
	for _, s := range settings {
		config.DB.FirstOrCreate(&s, models.GameSetting{SettingKey: s.SettingKey})
	}
	log.Println("  ✓ Game Settings")
}

// ============================================================
func seedAdmins() {
	hash, _ := bcrypt.GenerateFromPassword([]byte("admin123"), bcrypt.DefaultCost)
	admin := models.Admin{
		Fullname:     "Super Admin TOBA",
		Username:     "admin",
		PasswordHash: string(hash),
		Role:         "SUPER_ADMIN",
	}
	config.DB.FirstOrCreate(&admin, models.Admin{Username: "admin"})
	log.Println("  ✓ Admin (username: admin, password: admin123)")
}

func seedLocationQuestions() {
	type optSeed struct {
		Text      string
		IsCorrect bool
	}
	type qSeed struct {
		LocationOrder int
		Question      string
		Explanation   string
		Options       []optSeed
	}

	questions := []qSeed{
		// ── IT DEL (level_order=1) ────────────────────────
		{1, "Apa kepanjangan dari IT Del?",
			"IT Del merupakan singkatan dari Institut Teknologi Del.",
			[]optSeed{{"Institut Teknologi Del", true}, {"Inpres Toba Development", false}, {"Institut Toba Deliserdang", false}, {"Inisiatif Teknologi Delta", false}}},
		{1, "Di kota mana IT Del berada?",
			"IT Del berlokasi di Laguboti, Kabupaten Toba, Sumatera Utara.",
			[]optSeed{{"Laguboti", true}, {"Balige", false}, {"Parapat", false}, {"Tarutung", false}}},
		{1, "Siapa pendiri IT Del?",
			"IT Del didirikan oleh Luhut Binsar Pandjaitan pada tahun 2001.",
			[]optSeed{{"Luhut Binsar Pandjaitan", true}, {"B.J. Habibie", false}, {"Susilo Bambang Yudhoyono", false}, {"Joko Widodo", false}}},
		{1, "Tahun berapa IT Del diresmikan?",
			"IT Del diresmikan oleh Presiden Susilo Bambang Yudhoyono pada tahun 2012.",
			[]optSeed{{"2012", true}, {"2001", false}, {"2005", false}, {"2010", false}}},
		{1, "IT Del terletak di tepi danau apa?",
			"IT Del terletak di tepi Danau Toba yang merupakan danau vulkanik terbesar di dunia.",
			[]optSeed{{"Danau Toba", true}, {"Danau Singkarak", false}, {"Danau Maninjau", false}, {"Danau Poso", false}}},
		{1, "Apa yang membuat IT Del unik dibanding kampus lain?",
			"IT Del terletak di kawasan wisata Danau Toba dan fokus pada teknologi untuk daerah.",
			[]optSeed{{"Terletak di tepi Danau Toba", true}, {"Kampus terbesar di Indonesia", false}, {"Didirikan oleh presiden", false}, {"Gratis untuk semua mahasiswa", false}}},
		{1, "Program studi apa yang pertama ada di IT Del?",
			"IT Del awalnya membuka program Diploma III Teknik Informatika.",
			[]optSeed{{"Teknik Informatika", true}, {"Kedokteran", false}, {"Hukum", false}, {"Manajemen Bisnis", false}}},
		{1, "IT Del berada di provinsi mana?",
			"IT Del berada di Provinsi Sumatera Utara.",
			[]optSeed{{"Sumatera Utara", true}, {"Sumatera Barat", false}, {"Riau", false}, {"Aceh", false}}},
		{1, "Apa visi utama IT Del?",
			"IT Del bertujuan menjadi perguruan tinggi teknologi terkemuka yang berkontribusi pada kemajuan bangsa.",
			[]optSeed{{"Menjadi kampus teknologi terkemuka", true}, {"Mencetak atlet nasional", false}, {"Mengembangkan pariwisata", false}, {"Menjadi kampus kedokteran terbaik", false}}},
		{1, "Danau Toba terbentuk akibat apa?",
			"Danau Toba terbentuk akibat letusan supervulkan sekitar 74.000 tahun yang lalu.",
			[]optSeed{{"Letusan supervulkan", true}, {"Gempa bumi besar", false}, {"Erosi tanah", false}, {"Hujan deras ribuan tahun", false}}},
		{1, "Apa nama kabupaten tempat IT Del berada?",
			"IT Del berada di Kabupaten Toba, Sumatera Utara.",
			[]optSeed{{"Kabupaten Toba", true}, {"Kabupaten Samosir", false}, {"Kabupaten Dairi", false}, {"Kabupaten Humbang", false}}},
		{1, "Siapa presiden yang meresmikan IT Del?",
			"IT Del diresmikan oleh Presiden Susilo Bambang Yudhoyono pada tahun 2012.",
			[]optSeed{{"Susilo Bambang Yudhoyono", true}, {"Megawati", false}, {"Joko Widodo", false}, {"Habibie", false}}},
		{1, "IT Del merupakan kampus berbasis apa?",
			"IT Del merupakan kampus berbasis teknologi yang fokus pada inovasi dan pengembangan SDM.",
			[]optSeed{{"Teknologi", true}, {"Seni dan Budaya", false}, {"Kedokteran", false}, {"Pertanian", false}}},
		{1, "Apa nama danau yang ada di dekat IT Del?",
			"Danau Toba adalah danau vulkanik terbesar di dunia yang berada dekat IT Del.",
			[]optSeed{{"Danau Toba", true}, {"Danau Ranau", false}, {"Danau Kerinci", false}, {"Danau Sentani", false}}},
		{1, "IT Del sering disebut sebagai apa oleh masyarakat sekitar?",
			"IT Del sering disebut sebagai kampus di tepi danau karena lokasinya yang indah di tepi Danau Toba.",
			[]optSeed{{"Kampus di tepi danau", true}, {"Kampus di atas bukit", false}, {"Kampus dalam hutan", false}, {"Kampus pulau terpencil", false}}},

		// ── PANTAI BULBUL (level_order=2) ─────────────────
		{2, "Di kota mana Pantai Bulbul berada?",
			"Pantai Bulbul terletak di Balige, ibu kota Kabupaten Toba.",
			[]optSeed{{"Balige", true}, {"Parapat", false}, {"Samosir", false}, {"Laguboti", false}}},
		{2, "Apa yang terkenal dari Pantai Bulbul?",
			"Pantai Bulbul terkenal dengan pasir putih halus dan pemandangan Danau Toba yang indah.",
			[]optSeed{{"Pasir putih dan pemandangan Danau Toba", true}, {"Terumbu karang yang berwarna-warni", false}, {"Ombak besar untuk surfing", false}, {"Hutan mangrove yang lebat", false}}},
		{2, "Danau Toba merupakan danau vulkanik terbesar di mana?",
			"Danau Toba adalah danau vulkanik terbesar di dunia.",
			[]optSeed{{"Di dunia", true}, {"Di Asia", false}, {"Di Indonesia", false}, {"Di Asia Tenggara", false}}},
		{2, "Kegiatan apa yang populer di Pantai Bulbul?",
			"Berenang dan menikmati pemandangan Danau Toba adalah aktivitas paling populer di Pantai Bulbul.",
			[]optSeed{{"Berenang dan menikmati pemandangan", true}, {"Selancar ombak", false}, {"Menyelam laut dalam", false}, {"Memanjat tebing", false}}},
		{2, "Balige merupakan ibu kota kabupaten apa?",
			"Balige adalah ibu kota Kabupaten Toba di Sumatera Utara.",
			[]optSeed{{"Kabupaten Toba", true}, {"Kabupaten Samosir", false}, {"Kabupaten Taput", false}, {"Kabupaten Dairi", false}}},
		{2, "Apa warna pasir di Pantai Bulbul?",
			"Pantai Bulbul memiliki pasir berwarna putih yang halus dan bersih.",
			[]optSeed{{"Putih", true}, {"Hitam", false}, {"Coklat", false}, {"Kuning keemasan", false}}},
		{2, "Danau Toba terbentuk berapa tahun lalu?",
			"Danau Toba terbentuk sekitar 74.000 tahun lalu akibat letusan supervulkan.",
			[]optSeed{{"74.000 tahun lalu", true}, {"10.000 tahun lalu", false}, {"1.000 tahun lalu", false}, {"500.000 tahun lalu", false}}},
		{2, "Apa nama festival yang sering diadakan di kawasan Danau Toba?",
			"Festival Danau Toba adalah festival budaya tahunan yang diadakan di kawasan Danau Toba.",
			[]optSeed{{"Festival Danau Toba", true}, {"Pesta Batak", false}, {"Karnaval Samosir", false}, {"Pekan Budaya Toba", false}}},
		{2, "Pantai Bulbul cocok untuk wisata keluarga karena?",
			"Pantai Bulbul cocok untuk keluarga karena airnya tenang dan pantainya bersih.",
			[]optSeed{{"Airnya tenang dan pantainya bersih", true}, {"Ombaknya besar dan menantang", false}, {"Ada wahana permainan modern", false}, {"Tempatnya sangat sepi", false}}},
		{2, "Apa nama pulau yang berada di tengah Danau Toba?",
			"Pulau Samosir adalah pulau vulkanik yang berada di tengah Danau Toba.",
			[]optSeed{{"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Batam", false}, {"Pulau Berhala", false}}},
		{2, "Balige terletak di provinsi mana?",
			"Balige berada di Provinsi Sumatera Utara.",
			[]optSeed{{"Sumatera Utara", true}, {"Sumatera Barat", false}, {"Riau", false}, {"Aceh", false}}},
		{2, "Apa yang membuat air Danau Toba berwarna biru?",
			"Air Danau Toba berwarna biru karena sangat jernih dan dalam, memantulkan warna langit.",
			[]optSeed{{"Jernih dan dalam memantulkan langit", true}, {"Banyak ganggang biru", false}, {"Pengaruh mineral khusus", false}, {"Efek cahaya matahari", false}}},
		{2, "Berapa kedalaman maksimum Danau Toba?",
			"Danau Toba memiliki kedalaman maksimum sekitar 505 meter.",
			[]optSeed{{"505 meter", true}, {"200 meter", false}, {"1.000 meter", false}, {"100 meter", false}}},
		{2, "Transportasi apa yang digunakan untuk ke Pulau Samosir dari Balige?",
			"Kapal feri digunakan untuk menyeberang dari daratan menuju Pulau Samosir.",
			[]optSeed{{"Kapal feri", true}, {"Jembatan", false}, {"Helikopter", false}, {"Perahu dayung saja", false}}},
		{2, "Apa nama suku yang mendiami kawasan Danau Toba?",
			"Suku Batak Toba adalah suku utama yang mendiami kawasan sekitar Danau Toba.",
			[]optSeed{{"Batak Toba", true}, {"Batak Karo", false}, {"Minangkabau", false}, {"Melayu", false}}},

		// ── MUSEUM BATAK (level_order=3) ──────────────────
		{3, "Apa nama lengkap Museum Batak di Balige?",
			"Museum Batak TB Silalahi Center adalah museum budaya Batak yang terletak di Balige.",
			[]optSeed{{"TB Silalahi Center", true}, {"Museum Nasional Batak", false}, {"Galeri Budaya Toba", false}, {"Rumah Adat Batak", false}}},
		{3, "Siapa yang mendirikan Museum Batak TB Silalahi Center?",
			"Museum ini didirikan oleh TB Silalahi, seorang tokoh militer dan politisi asal Batak.",
			[]optSeed{{"TB Silalahi", true}, {"Luhut Binsar Pandjaitan", false}, {"Raja Siallagan", false}, {"B.J. Habibie", false}}},
		{3, "Apa yang disimpan di Museum Batak?",
			"Museum Batak menyimpan lebih dari 1.000 koleksi benda budaya Batak Toba.",
			[]optSeed{{"Koleksi benda budaya Batak Toba", true}, {"Koleksi senjata perang dunia", false}, {"Koleksi batik nusantara", false}, {"Koleksi benda prasejarah", false}}},
		{3, "Kain tradisional Batak yang disimpan di museum disebut?",
			"Ulos adalah kain tenun tradisional Batak Toba yang menjadi koleksi utama museum.",
			[]optSeed{{"Ulos", true}, {"Batik", false}, {"Songket", false}, {"Tapis", false}}},
		{3, "Ukiran khas yang menghiasi rumah adat Batak disebut?",
			"Gorga adalah ukiran tradisional khas Batak yang biasa menghiasi rumah adat.",
			[]optSeed{{"Gorga", true}, {"Batik", false}, {"Tenun", false}, {"Wayang", false}}},
		{3, "Museum Batak berada di kota mana?",
			"Museum Batak TB Silalahi Center berada di Balige, Kabupaten Toba.",
			[]optSeed{{"Balige", true}, {"Parapat", false}, {"Samosir", false}, {"Tarutung", false}}},
		{3, "Senjata tradisional Batak apa yang ada di museum?",
			"Piso Gaja Dompak adalah pedang pusaka khas Batak Toba yang tersimpan di museum.",
			[]optSeed{{"Piso Gaja Dompak", true}, {"Keris", false}, {"Rencong", false}, {"Mandau", false}}},
		{3, "Apa fungsi museum bagi masyarakat?",
			"Museum berfungsi sebagai tempat pelestarian dan edukasi budaya bagi generasi penerus.",
			[]optSeed{{"Pelestarian dan edukasi budaya", true}, {"Tempat hiburan semata", false}, {"Pusat perbelanjaan budaya", false}, {"Tempat ibadah", false}}},
		{3, "Berapa jumlah koleksi di Museum Batak?",
			"Museum Batak TB Silalahi Center menyimpan lebih dari 1.000 koleksi benda budaya.",
			[]optSeed{{"Lebih dari 1.000", true}, {"Kurang dari 100", false}, {"Tepat 500", false}, {"Lebih dari 10.000", false}}},
		{3, "Apa nama alat musik tradisional Batak yang ada di museum?",
			"Hasapi adalah alat musik petik tradisional Batak Toba yang tersimpan di Museum Batak.",
			[]optSeed{{"Hasapi", true}, {"Gitar", false}, {"Kecapi", false}, {"Sitar", false}}},
		{3, "Pakaian adat Batak yang paling dikenal adalah?",
			"Ulos adalah pakaian adat Batak Toba yang paling dikenal dan sering dikenakan dalam upacara.",
			[]optSeed{{"Ulos", true}, {"Kebaya", false}, {"Baju Bodo", false}, {"Baju Kurung", false}}},
		{3, "Apa keistimewaan Museum Batak TB Silalahi?",
			"Museum ini memiliki koleksi terlengkap tentang budaya Batak Toba di Indonesia.",
			[]optSeed{{"Koleksi terlengkap budaya Batak Toba", true}, {"Museum terbesar di dunia", false}, {"Gratis untuk semua pengunjung", false}, {"Hanya buka malam hari", false}}},
		{3, "Di museum, pengunjung bisa melihat apa?",
			"Pengunjung bisa melihat koleksi pakaian adat, senjata, alat musik, dan peralatan rumah tangga Batak.",
			[]optSeed{{"Koleksi pakaian, senjata, dan alat musik", true}, {"Film dokumenter saja", false}, {"Pertunjukan seni langsung", false}, {"Pameran lukisan modern", false}}},
		{3, "Siapa TB Silalahi?",
			"TB Silalahi adalah tokoh militer, politisi, dan pengusaha asal Batak Toba yang mendirikan museum.",
			[]optSeed{{"Tokoh militer dan politisi Batak", true}, {"Raja Batak terakhir", false}, {"Penulis buku terkenal", false}, {"Penemu batik Batak", false}}},
		{3, "Museum Batak terletak di tepi apa?",
			"Museum Batak TB Silalahi Center terletak di tepi Danau Toba dengan pemandangan yang indah.",
			[]optSeed{{"Danau Toba", true}, {"Pantai laut", false}, {"Sungai besar", false}, {"Pegunungan", false}}},

		// ── HUTA SIALLAGAN (level_order=4) ────────────────
		{4, "Huta Siallagan terletak di pulau mana?",
			"Huta Siallagan terletak di Pulau Samosir, di tengah Danau Toba.",
			[]optSeed{{"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Batam", false}, {"Daratan Sumatera", false}}},
		{4, "Apa yang terkenal dari Huta Siallagan?",
			"Huta Siallagan terkenal dengan Batu Kursi Raja yang digunakan untuk pengadilan adat.",
			[]optSeed{{"Batu Kursi Raja untuk pengadilan adat", true}, {"Pantai pasir putih", false}, {"Air terjun tertinggi", false}, {"Hutan bambu yang lebat", false}}},
		{4, "Siapa yang biasa duduk di Batu Kursi Siallagan?",
			"Raja adat Siallagan yang duduk di batu kursi untuk mengadili perkara adat.",
			[]optSeed{{"Raja adat Siallagan", true}, {"Tamu kerajaan", false}, {"Para pedagang", false}, {"Wisatawan", false}}},
		{4, "Huta Siallagan merupakan desa adat suku apa?",
			"Huta Siallagan adalah desa adat suku Batak Toba yang masih terjaga keasliannya.",
			[]optSeed{{"Batak Toba", true}, {"Batak Karo", false}, {"Minangkabau", false}, {"Dayak", false}}},
		{4, "Apa arti kata Huta dalam bahasa Batak?",
			"Huta berarti desa atau kampung dalam bahasa Batak Toba.",
			[]optSeed{{"Desa atau kampung", true}, {"Sungai", false}, {"Hutan", false}, {"Danau", false}}},
		{4, "Pulau Samosir merupakan pulau terbesar di dalam danau di mana?",
			"Pulau Samosir adalah pulau terbesar yang berada di dalam danau di dunia.",
			[]optSeed{{"Di dunia", true}, {"Di Asia", false}, {"Di Indonesia", false}, {"Di Asia Tenggara", false}}},
		{4, "Batu Kursi di Huta Siallagan digunakan untuk?",
			"Batu Kursi digunakan sebagai tempat pengadilan adat dan permusyawaratan raja.",
			[]optSeed{{"Pengadilan adat", true}, {"Tempat bertapa", false}, {"Meja makan raja", false}, {"Tempat berdagang", false}}},
		{4, "Apa nama marga yang mendiami Huta Siallagan?",
			"Marga Siallagan adalah marga yang secara turun-temurun mendiami desa ini.",
			[]optSeed{{"Siallagan", true}, {"Sinaga", false}, {"Sitorus", false}, {"Simanjuntak", false}}},
		{4, "Luas Pulau Samosir sekitar berapa km persegi?",
			"Pulau Samosir memiliki luas sekitar 630 kilometer persegi.",
			[]optSeed{{"630 km persegi", true}, {"100 km persegi", false}, {"1.000 km persegi", false}, {"50 km persegi", false}}},
		{4, "Apa yang dimaksud dengan adat Batak?",
			"Adat Batak adalah sistem aturan dan tradisi yang mengatur kehidupan masyarakat Batak Toba.",
			[]optSeed{{"Sistem aturan dan tradisi masyarakat Batak", true}, {"Bahasa resmi Batak", false}, {"Jenis tarian Batak", false}, {"Makanan khas Batak", false}}},
		{4, "Huta Siallagan dapat dicapai dengan cara apa dari Parapat?",
			"Huta Siallagan di Pulau Samosir dapat dicapai dengan naik kapal feri dari Parapat.",
			[]optSeed{{"Naik kapal feri", true}, {"Melewati jembatan", false}, {"Naik kereta api", false}, {"Terbang dengan pesawat", false}}},
		{4, "Sistem kekerabatan Batak disebut?",
			"Dalihan Na Tolu adalah sistem kekerabatan Batak yang mengatur hubungan antar keluarga.",
			[]optSeed{{"Dalihan Na Tolu", true}, {"Marga Sistem", false}, {"Adat Minang", false}, {"Suku Adat", false}}},
		{4, "Rumah adat di Huta Siallagan disebut?",
			"Rumah Bolon adalah rumah adat tradisional Batak Toba yang ada di Huta Siallagan.",
			[]optSeed{{"Rumah Bolon", true}, {"Rumah Gadang", false}, {"Joglo", false}, {"Honai", false}}},
		{4, "Marga dalam adat Batak diturunkan dari?",
			"Marga Batak diturunkan dari garis keturunan ayah (patrilineal).",
			[]optSeed{{"Pihak ayah", true}, {"Pihak ibu", false}, {"Kedua orang tua", false}, {"Pilihan sendiri", false}}},
		{4, "Apa salam khas masyarakat Batak?",
			"Horas adalah ucapan salam khas masyarakat Batak yang berarti sehat dan sejahtera.",
			[]optSeed{{"Horas", true}, {"Mejuah-juah", false}, {"Yaahowu", false}, {"Selamat", false}}},

		// ── BUKIT HOLBUNG (level_order=5) ─────────────────
		{5, "Bukit Holbung mendapat julukan apa?",
			"Bukit Holbung dijuluki Bukit Teletubbies karena bentuknya yang bergelombang hijau.",
			[]optSeed{{"Bukit Teletubbies", true}, {"Bukit Pelangi", false}, {"Bukit Indah", false}, {"Bukit Hijau", false}}},
		{5, "Bukit Holbung terletak di kabupaten mana?",
			"Bukit Holbung terletak di Kabupaten Samosir, Sumatera Utara.",
			[]optSeed{{"Samosir", true}, {"Toba", false}, {"Dairi", false}, {"Humbang Hasundutan", false}}},
		{5, "Ketinggian Bukit Holbung sekitar berapa meter?",
			"Bukit Holbung berada pada ketinggian sekitar 1.000 meter di atas permukaan laut.",
			[]optSeed{{"1.000 meter", true}, {"500 meter", false}, {"2.000 meter", false}, {"200 meter", false}}},
		{5, "Apa yang bisa dilihat dari puncak Bukit Holbung?",
			"Dari Bukit Holbung terlihat panorama Danau Toba 360 derajat yang menakjubkan.",
			[]optSeed{{"Panorama Danau Toba 360 derajat", true}, {"Lautan luas", false}, {"Kota besar", false}, {"Gunung berapi aktif", false}}},
		{5, "Waktu terbaik mengunjungi Bukit Holbung adalah?",
			"Bukit Holbung paling indah dikunjungi saat matahari terbit dan terbenam.",
			[]optSeed{{"Saat matahari terbit dan terbenam", true}, {"Tengah malam", false}, {"Siang hari terik", false}, {"Musim hujan", false}}},
		{5, "Bukit Holbung berada di Pulau apa?",
			"Bukit Holbung berada di Pulau Samosir yang terletak di tengah Danau Toba.",
			[]optSeed{{"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Batam", false}, {"Daratan Sumatera", false}}},
		{5, "Apa yang membuat Bukit Holbung unik?",
			"Bukit Holbung unik karena bentuknya yang bergelombang seperti bukit dalam serial Teletubbies.",
			[]optSeed{{"Bentuknya bergelombang hijau", true}, {"Ada batu besar ajaib", false}, {"Tanahnya berwarna merah", false}, {"Selalu berkabut sepanjang tahun", false}}},
		{5, "Danau Toba terlihat dari Bukit Holbung karena?",
			"Danau Toba terlihat jelas dari Bukit Holbung karena posisinya yang tinggi dan pemandangan terbuka.",
			[]optSeed{{"Posisinya tinggi dengan pemandangan terbuka", true}, {"Ada teleskop raksasa di sana", false}, {"Danau Toba sangat besar", false}, {"Bukit ini tertinggi di Indonesia", false}}},
		{5, "Apa nama danau yang terlihat dari Bukit Holbung?",
			"Danau Toba adalah danau vulkanik terbesar di dunia yang terlihat dari Bukit Holbung.",
			[]optSeed{{"Danau Toba", true}, {"Danau Singkarak", false}, {"Danau Maninjau", false}, {"Danau Kerinci", false}}},
		{5, "Bukit Holbung cocok untuk kegiatan apa?",
			"Bukit Holbung cocok untuk hiking, fotografi, dan menikmati pemandangan alam.",
			[]optSeed{{"Hiking dan fotografi", true}, {"Selancar air", false}, {"Menyelam", false}, {"Berperahu", false}}},
		{5, "Samosir merupakan pulau di dalam danau terbesar di mana?",
			"Samosir adalah pulau terbesar yang berada di dalam danau di dunia.",
			[]optSeed{{"Di dunia", true}, {"Di Asia", false}, {"Di Indonesia", false}, {"Di Sumatera", false}}},
		{5, "Apa warna dominan pemandangan di Bukit Holbung?",
			"Pemandangan Bukit Holbung didominasi warna hijau dari rumput dan perbukitan.",
			[]optSeed{{"Hijau", true}, {"Merah", false}, {"Kuning", false}, {"Biru", false}}},
		{5, "Jalur menuju Bukit Holbung bisa ditempuh dari kota mana?",
			"Bukit Holbung bisa dicapai dari Pangururan, ibu kota Kabupaten Samosir.",
			[]optSeed{{"Pangururan", true}, {"Parapat", false}, {"Balige", false}, {"Tarutung", false}}},
		{5, "Bukit Holbung terletak di ketinggian berapa MDPL?",
			"Bukit Holbung berada di sekitar 1.000 meter di atas permukaan laut.",
			[]optSeed{{"Sekitar 1.000 MDPL", true}, {"Di bawah 100 MDPL", false}, {"Lebih dari 3.000 MDPL", false}, {"Tepat 500 MDPL", false}}},
		{5, "Kenapa Bukit Holbung ramai dikunjungi?",
			"Bukit Holbung ramai karena pemandangannya yang indah dan unik, cocok untuk foto.",
			[]optSeed{{"Pemandangan indah dan unik untuk foto", true}, {"Ada pertunjukan seni setiap hari", false}, {"Tiket masuknya gratis selalu", false}, {"Ada pusat perbelanjaan di sana", false}}},

		// ── DESA TOMOK (level_order=6) ────────────────────
		{6, "Desa Tomok terkenal dengan apa?",
			"Desa Tomok terkenal dengan Makam Raja Sidabutar yang bersejarah dan berusia lebih dari 500 tahun.",
			[]optSeed{{"Makam Raja Sidabutar", true}, {"Pantai indah", false}, {"Air terjun", false}, {"Hutan pinus", false}}},
		{6, "Makam Raja Sidabutar berusia sekitar berapa tahun?",
			"Makam Raja Sidabutar diperkirakan berusia lebih dari 500 tahun.",
			[]optSeed{{"Lebih dari 500 tahun", true}, {"100 tahun", false}, {"1.000 tahun", false}, {"50 tahun", false}}},
		{6, "Desa Tomok terletak di pulau mana?",
			"Desa Tomok berada di Pulau Samosir, di tengah Danau Toba.",
			[]optSeed{{"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Batam", false}, {"Daratan Sumatera", false}}},
		{6, "Apa yang bisa dibeli di Desa Tomok?",
			"Desa Tomok terkenal sebagai pusat kerajinan tangan Batak seperti ulos dan ukiran kayu.",
			[]optSeed{{"Kerajinan tangan Batak seperti ulos", true}, {"Elektronik modern", false}, {"Senjata tradisional", false}, {"Makanan impor", false}}},
		{6, "Siapa Raja Sidabutar?",
			"Raja Sidabutar adalah raja Batak Toba yang memerintah di wilayah Tomok, Samosir.",
			[]optSeed{{"Raja Batak Toba di Tomok", true}, {"Raja Majapahit", false}, {"Raja Sriwijaya", false}, {"Sultan Batak pertama", false}}},
		{6, "Sigale-gale pertama kali dibuat di mana?",
			"Sigale-gale, boneka kayu yang bisa bergerak, pertama kali dibuat di Desa Tomok.",
			[]optSeed{{"Desa Tomok", true}, {"Huta Siallagan", false}, {"Balige", false}, {"Parapat", false}}},
		{6, "Desa Tomok merupakan desa tertua ke berapa di Samosir?",
			"Desa Tomok merupakan salah satu desa tertua di Pulau Samosir.",
			[]optSeed{{"Salah satu tertua", true}, {"Desa termuda", false}, {"Desa terbesar", false}, {"Desa terkecil", false}}},
		{6, "Ulos dari Tomok dikenal sebagai?",
			"Ulos dari Tomok dikenal sebagai salah satu ulos terbaik dengan kualitas tenun yang tinggi.",
			[]optSeed{{"Salah satu ulos terbaik", true}, {"Ulos paling murah", false}, {"Ulos untuk pemula", false}, {"Ulos hanya untuk pria", false}}},
		{6, "Apa bahasa yang digunakan masyarakat Tomok sehari-hari?",
			"Masyarakat Tomok menggunakan Bahasa Batak Toba dalam kehidupan sehari-hari.",
			[]optSeed{{"Bahasa Batak Toba", true}, {"Bahasa Melayu", false}, {"Bahasa Jawa", false}, {"Bahasa Minang", false}}},
		{6, "Makam Raja Sidabutar terbuat dari apa?",
			"Makam Raja Sidabutar terbuat dari batu andesit yang diukir dengan motif Batak.",
			[]optSeed{{"Batu andesit", true}, {"Kayu jati", false}, {"Emas murni", false}, {"Beton modern", false}}},
		{6, "Desa Tomok merupakan gerbang masuk utama ke Samosir dari mana?",
			"Desa Tomok menjadi gerbang masuk utama ke Samosir dari Parapat via feri.",
			[]optSeed{{"Parapat", true}, {"Balige", false}, {"Laguboti", false}, {"Tarutung", false}}},
		{6, "Apa nama tarian yang dipentaskan di Desa Tomok?",
			"Tari Tortor adalah tarian tradisional Batak Toba yang sering dipentaskan di Tomok.",
			[]optSeed{{"Tari Tortor", true}, {"Tari Saman", false}, {"Tari Kecak", false}, {"Tari Piring", false}}},
		{6, "Sigale-gale adalah?",
			"Sigale-gale adalah patung boneka kayu yang dapat bergerak dalam upacara adat Batak.",
			[]optSeed{{"Patung boneka kayu yang bisa bergerak", true}, {"Jenis masakan Batak", false}, {"Alat musik Batak", false}, {"Nama senjata Batak", false}}},
		{6, "Kerajinan ukiran kayu Batak di Tomok biasanya bermotif apa?",
			"Ukiran kayu Batak di Tomok biasanya bermotif gorga, ukiran tradisional khas Batak.",
			[]optSeed{{"Gorga", true}, {"Batik", false}, {"Wayang", false}, {"Kaligrafi", false}}},
		{6, "Berapa lama perjalanan feri dari Parapat ke Tomok?",
			"Perjalanan feri dari Parapat ke Tomok memakan waktu sekitar 30-45 menit.",
			[]optSeed{{"30-45 menit", true}, {"5 menit", false}, {"3 jam", false}, {"1 hari", false}}},

		// ── BATU GANTUNG (level_order=7) ──────────────────
		{7, "Batu Gantung terletak di kota mana?",
			"Batu Gantung terletak di Parapat, Kabupaten Simalungun.",
			[]optSeed{{"Parapat", true}, {"Balige", false}, {"Samosir", false}, {"Laguboti", false}}},
		{7, "Apa legenda di balik Batu Gantung?",
			"Legenda Batu Gantung bercerita tentang seorang gadis bernama Seruni yang bersedih.",
			[]optSeed{{"Gadis bernama Seruni yang bersedih", true}, {"Pertempuran raja Batak", false}, {"Kisah cinta terlarang", false}, {"Asal usul Danau Toba", false}}},
		{7, "Batu Gantung adalah formasi batu yang?",
			"Batu Gantung adalah formasi batu unik yang menggantung di tepi Danau Toba.",
			[]optSeed{{"Menggantung di tepi Danau Toba", true}, {"Terapung di tengah danau", false}, {"Berada di dasar danau", false}, {"Berbentuk seperti rumah", false}}},
		{7, "Dari mana Batu Gantung bisa dilihat?",
			"Batu Gantung dapat dilihat dari kapal feri yang melintasi Danau Toba.",
			[]optSeed{{"Dari kapal feri di Danau Toba", true}, {"Hanya dari pesawat", false}, {"Hanya dari bawah air", false}, {"Dari puncak gunung saja", false}}},
		{7, "Parapat terkenal sebagai apa?",
			"Parapat terkenal sebagai kota wisata dan pintu masuk utama ke Danau Toba.",
			[]optSeed{{"Kota wisata pintu masuk Danau Toba", true}, {"Kota industri", false}, {"Kota pendidikan", false}, {"Ibukota provinsi", false}}},
		{7, "Batu Gantung berada di kabupaten mana?",
			"Batu Gantung berada di Kabupaten Simalungun, Sumatera Utara.",
			[]optSeed{{"Simalungun", true}, {"Toba", false}, {"Samosir", false}, {"Dairi", false}}},
		{7, "Nama gadis dalam legenda Batu Gantung adalah?",
			"Nama gadis dalam legenda Batu Gantung adalah Seruni.",
			[]optSeed{{"Seruni", true}, {"Butet", false}, {"Sari", false}, {"Toba", false}}},
		{7, "Batu Gantung menjadi ikon wisata kota?",
			"Batu Gantung menjadi salah satu ikon wisata kota Parapat di tepi Danau Toba.",
			[]optSeed{{"Parapat", true}, {"Balige", false}, {"Samosir", false}, {"Tarutung", false}}},
		{7, "Apa yang membuat Batu Gantung menarik secara geologi?",
			"Batu Gantung menarik secara geologi karena merupakan formasi batuan vulkanik yang unik.",
			[]optSeed{{"Formasi batuan vulkanik yang unik", true}, {"Mengandung emas", false}, {"Berubah warna setiap hari", false}, {"Selalu bergerak", false}}},
		{7, "Danau Toba di sekitar Parapat terkenal karena?",
			"Danau Toba di Parapat terkenal karena keindahannya dan menjadi destinasi wisata utama.",
			[]optSeed{{"Keindahan alam dan wisata utama", true}, {"Airnya bisa diminum langsung", false}, {"Ada monster danau", false}, {"Sering ada tsunami", false}}},
		{7, "Legenda Batu Gantung berasal dari suku apa?",
			"Legenda Batu Gantung berasal dari tradisi lisan masyarakat Batak Toba.",
			[]optSeed{{"Batak Toba", true}, {"Melayu", false}, {"Minangkabau", false}, {"Jawa", false}}},
		{7, "Jalur feri dari Parapat menuju Samosir melewati?",
			"Jalur feri dari Parapat melewati perairan Danau Toba dan melewati area Batu Gantung.",
			[]optSeed{{"Perairan Danau Toba", true}, {"Laut lepas", false}, {"Sungai besar", false}, {"Terowongan bawah air", false}}},
		{7, "Apa nama danau tempat Batu Gantung berada?",
			"Batu Gantung berada di tepi Danau Toba, danau vulkanik terbesar di dunia.",
			[]optSeed{{"Danau Toba", true}, {"Danau Singkarak", false}, {"Danau Ranau", false}, {"Danau Poso", false}}},
		{7, "Kabupaten Simalungun berbatasan dengan danau apa?",
			"Kabupaten Simalungun berbatasan langsung dengan Danau Toba.",
			[]optSeed{{"Danau Toba", true}, {"Danau Ranau", false}, {"Selat Malaka", false}, {"Danau Kerinci", false}}},
		{7, "Batu Gantung sering dikunjungi wisatawan untuk apa?",
			"Wisatawan mengunjungi Batu Gantung untuk melihat formasi batu unik dan mendengar legendanya.",
			[]optSeed{{"Melihat batu unik dan mendengar legenda", true}, {"Berenang di sekitarnya", false}, {"Mendaki ke puncaknya", false}, {"Berkemah di bawahnya", false}}},

		// ── MENARA TELE (level_order=8) ───────────────────
		{8, "Menara Tele berfungsi sebagai apa?",
			"Menara Tele berfungsi sebagai menara pandang untuk menikmati panorama Danau Toba.",
			[]optSeed{{"Menara pandang", true}, {"Menara komunikasi", false}, {"Mercusuar", false}, {"Menara air", false}}},
		{8, "Menara Tele berada di ketinggian sekitar berapa meter?",
			"Menara Tele berada di ketinggian sekitar 1.300 meter di atas permukaan laut.",
			[]optSeed{{"1.300 meter", true}, {"500 meter", false}, {"200 meter", false}, {"3.000 meter", false}}},
		{8, "Tele merupakan gerbang masuk ke pulau mana?",
			"Tele adalah gerbang masuk ke Pulau Samosir dari daratan Sumatera melalui jalan darat.",
			[]optSeed{{"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Batam", false}, {"Pulau Belitung", false}}},
		{8, "Dari Menara Tele apa yang bisa dilihat?",
			"Dari Menara Tele terlihat hamparan sawah, perbukitan, dan panorama Danau Toba.",
			[]optSeed{{"Sawah, perbukitan, dan Danau Toba", true}, {"Lautan luas", false}, {"Kota metropolitan", false}, {"Gunung berapi aktif", false}}},
		{8, "Jalan Tele dikenal sebagai?",
			"Jalan Tele dikenal sebagai salah satu jalur pegunungan terindah di Sumatera Utara.",
			[]optSeed{{"Jalur pegunungan terindah di Sumatera Utara", true}, {"Jalan tol terpanjang", false}, {"Jalan berbahaya", false}, {"Jalan buntu", false}}},
		{8, "Menara Tele berada di desa bernama?",
			"Menara Tele berada di Desa Tele, Kabupaten Samosir.",
			[]optSeed{{"Desa Tele", true}, {"Desa Tomok", false}, {"Desa Balige", false}, {"Desa Siallagan", false}}},
		{8, "Kabupaten Samosir terletak di provinsi mana?",
			"Kabupaten Samosir terletak di Provinsi Sumatera Utara.",
			[]optSeed{{"Sumatera Utara", true}, {"Sumatera Barat", false}, {"Riau", false}, {"Aceh", false}}},
		{8, "Mengapa Tele penting bagi wisatawan ke Samosir?",
			"Tele adalah pintu gerbang darat satu-satunya menuju Samosir tanpa harus naik feri.",
			[]optSeed{{"Pintu gerbang darat ke Samosir", true}, {"Tempat belanja oleh-oleh", false}, {"Bandara kecil", false}, {"Terminal bus utama", false}}},
		{8, "Pemandangan sawah dari Menara Tele terlihat karena?",
			"Ketinggian Menara Tele yang mencapai 1.300 MDPL memungkinkan melihat hamparan sawah.",
			[]optSeed{{"Ketinggian 1.300 MDPL", true}, {"Ada kamera canggih", false}, {"Lokasinya di tepi danau", false}, {"Ada teleskop di menara", false}}},
		{8, "Apa nama danau yang terlihat dari Menara Tele?",
			"Danau Toba adalah danau vulkanik terbesar di dunia yang terlihat dari Menara Tele.",
			[]optSeed{{"Danau Toba", true}, {"Danau Singkarak", false}, {"Danau Maninjau", false}, {"Danau Ranau", false}}},
		{8, "Menara Tele cocok dikunjungi saat?",
			"Menara Tele paling indah dikunjungi saat cuaca cerah agar pemandangan terlihat jelas.",
			[]optSeed{{"Cuaca cerah", true}, {"Malam hari", false}, {"Musim hujan", false}, {"Tengah malam", false}}},
		{8, "Berapa lama perjalanan dari Medan ke Menara Tele?",
			"Perjalanan dari Medan ke Menara Tele sekitar 5-6 jam dengan kendaraan darat.",
			[]optSeed{{"5-6 jam", true}, {"30 menit", false}, {"1 hari penuh", false}, {"2 jam", false}}},
		{8, "Apa yang dijual di sekitar Menara Tele?",
			"Di sekitar Menara Tele biasa dijual makanan khas Batak dan oleh-oleh Samosir.",
			[]optSeed{{"Makanan khas Batak dan oleh-oleh", true}, {"Elektronik", false}, {"Kendaraan bermotor", false}, {"Buku pelajaran", false}}},
		{8, "Tele berarti apa dalam bahasa setempat?",
			"Tele adalah nama tempat yang berasal dari bahasa Batak lokal.",
			[]optSeed{{"Nama tempat dalam bahasa Batak", true}, {"Berarti danau", false}, {"Berarti menara", false}, {"Berarti puncak gunung", false}}},
		{8, "Pulau Samosir bisa dicapai dari Tele dengan?",
			"Dari Tele, Pulau Samosir bisa dicapai melalui jalan darat yang melintasi perbatasan.",
			[]optSeed{{"Jalan darat", true}, {"Kapal feri", false}, {"Perahu dayung", false}, {"Jembatan gantung", false}}},

		// ── AIR TERJUN SITUMURUN (level_order=9) ──────────
		{9, "Air Terjun Situmurun terletak di kabupaten mana?",
			"Air Terjun Situmurun terletak di Kabupaten Toba, Sumatera Utara.",
			[]optSeed{{"Kabupaten Toba", true}, {"Kabupaten Samosir", false}, {"Kabupaten Simalungun", false}, {"Kabupaten Dairi", false}}},
		{9, "Apa keunikan Air Terjun Situmurun?",
			"Air Terjun Situmurun unik karena airnya langsung jatuh ke perairan Danau Toba.",
			[]optSeed{{"Airnya jatuh langsung ke Danau Toba", true}, {"Berwarna merah", false}, {"Mengalir ke atas", false}, {"Berbentuk spiral", false}}},
		{9, "Situmurun berarti apa dalam bahasa Batak?",
			"Situmurun berarti turun bertahap dalam bahasa Batak Toba.",
			[]optSeed{{"Turun bertahap", true}, {"Air biru", false}, {"Jatuh deras", false}, {"Sungai indah", false}}},
		{9, "Ketinggian Air Terjun Situmurun sekitar berapa meter?",
			"Air Terjun Situmurun memiliki ketinggian sekitar 70 meter.",
			[]optSeed{{"70 meter", true}, {"10 meter", false}, {"200 meter", false}, {"500 meter", false}}},
		{9, "Nama lain Air Terjun Situmurun adalah?",
			"Air Terjun Situmurun juga dikenal dengan nama Binangalom.",
			[]optSeed{{"Binangalom", true}, {"Sipisopiso", false}, {"Sigura-gura", false}, {"Ponot", false}}},
		{9, "Wisatawan bisa melakukan apa di Air Terjun Situmurun?",
			"Wisatawan bisa berenang di kolam alami dan menikmati kesegaran air terjun.",
			[]optSeed{{"Berenang di kolam alami", true}, {"Mendaki ke puncak", false}, {"Berselancar", false}, {"Memancing di laut", false}}},
		{9, "Cara terbaik mencapai Air Terjun Situmurun adalah?",
			"Air Terjun Situmurun paling mudah dicapai dengan perahu dari tepi Danau Toba.",
			[]optSeed{{"Dengan perahu dari tepi danau", true}, {"Dengan kereta api", false}, {"Dengan pesawat helikopter", false}, {"Dengan jalan kaki dari Medan", false}}},
		{9, "Air Terjun Situmurun mengalir ke mana?",
			"Air Terjun Situmurun mengalir langsung ke perairan Danau Toba.",
			[]optSeed{{"Danau Toba", true}, {"Laut Sumatera", false}, {"Sungai Asahan", false}, {"Sawah di bawahnya", false}}},
		{9, "Apa nama danau yang menjadi tujuan aliran air terjun ini?",
			"Danau Toba adalah danau vulkanik terbesar di dunia yang menjadi tujuan aliran Situmurun.",
			[]optSeed{{"Danau Toba", true}, {"Danau Singkarak", false}, {"Danau Ranau", false}, {"Danau Poso", false}}},
		{9, "Air Terjun Situmurun berada di provinsi mana?",
			"Air Terjun Situmurun berada di Provinsi Sumatera Utara.",
			[]optSeed{{"Sumatera Utara", true}, {"Sumatera Barat", false}, {"Riau", false}, {"Jambi", false}}},
		{9, "Pemandangan di sekitar Air Terjun Situmurun didominasi oleh?",
			"Pemandangan sekitar Air Terjun Situmurun didominasi oleh hutan hijau dan perairan Danau Toba.",
			[]optSeed{{"Hutan hijau dan Danau Toba", true}, {"Gedung-gedung tinggi", false}, {"Padang pasir", false}, {"Salju abadi", false}}},
		{9, "Air Terjun Situmurun termasuk kategori wisata apa?",
			"Situmurun termasuk wisata alam yang menggabungkan keindahan air terjun dan danau.",
			[]optSeed{{"Wisata alam air terjun dan danau", true}, {"Wisata belanja", false}, {"Wisata kuliner", false}, {"Wisata sejarah", false}}},
		{9, "Kolam alami di bawah Air Terjun Situmurun terbentuk dari?",
			"Kolam alami terbentuk dari pertemuan air terjun dengan permukaan Danau Toba.",
			[]optSeed{{"Pertemuan air terjun dan Danau Toba", true}, {"Penggalian manusia", false}, {"Kawah gunung berapi", false}, {"Banjir tahunan", false}}},
		{9, "Binangalom adalah nama lain dari?",
			"Binangalom adalah nama lain dari Air Terjun Situmurun di Kabupaten Toba.",
			[]optSeed{{"Air Terjun Situmurun", true}, {"Danau Toba", false}, {"Pulau Samosir", false}, {"Bukit Holbung", false}}},
		{9, "Apa yang membuat Air Terjun Situmurun berbeda dari air terjun lain?",
			"Keunikannya adalah langsung jatuh ke danau, bukan ke sungai atau kolam biasa.",
			[]optSeed{{"Langsung jatuh ke danau", true}, {"Airnya berwarna biru", false}, {"Bisa diminum langsung", false}, {"Mengalir tanpa henti sepanjang tahun", false}}},

		// ── SIGALE-GALE (level_order=10) ──────────────────
		{10, "Sigale-gale adalah?",
			"Sigale-gale adalah patung boneka kayu yang dapat bergerak dalam upacara adat Batak Toba.",
			[]optSeed{{"Patung boneka kayu yang bisa bergerak", true}, {"Masakan khas Batak", false}, {"Alat musik Batak", false}, {"Tarian Batak", false}}},
		{10, "Untuk apa Sigale-gale dibuat?",
			"Sigale-gale dibuat sebagai pengganti anak bagi raja yang tidak memiliki keturunan.",
			[]optSeed{{"Pengganti anak bagi raja tanpa keturunan", true}, {"Hiasan istana", false}, {"Senjata perang", false}, {"Media pertanian", false}}},
		{10, "Bagaimana Sigale-gale bisa bergerak?",
			"Sigale-gale digerakkan dengan mekanisme tali yang dikendalikan oleh manusia.",
			[]optSeed{{"Mekanisme tali yang dikendalikan manusia", true}, {"Tenaga listrik", false}, {"Angin", false}, {"Kekuatan gaib saja", false}}},
		{10, "Upacara Sigale-gale biasanya diiringi musik apa?",
			"Upacara Sigale-gale biasanya diiringi musik gondang Batak Toba.",
			[]optSeed{{"Gondang", true}, {"Gamelan", false}, {"Dangdut", false}, {"Kolintang", false}}},
		{10, "Gondang adalah?",
			"Gondang adalah alat musik perkusi tradisional Batak Toba yang digunakan dalam upacara.",
			[]optSeed{{"Alat musik perkusi tradisional Batak", true}, {"Jenis makanan Batak", false}, {"Nama suku Batak", false}, {"Pakaian adat Batak", false}}},
		{10, "Sigale-gale pertama kali ada di desa mana?",
			"Sigale-gale pertama kali dibuat dan digunakan di Desa Tomok, Pulau Samosir.",
			[]optSeed{{"Desa Tomok", true}, {"Huta Siallagan", false}, {"Balige", false}, {"Parapat", false}}},
		{10, "Upacara Sigale-gale termasuk dalam ritual apa?",
			"Upacara Sigale-gale termasuk dalam ritual pemakaman adat Batak Toba.",
			[]optSeed{{"Ritual pemakaman adat Batak", true}, {"Ritual pernikahan", false}, {"Ritual panen", false}, {"Ritual kelahiran", false}}},
		{10, "Tari apa yang dilakukan bersama Sigale-gale?",
			"Tari Tortor adalah tarian yang dilakukan bersama Sigale-gale dalam upacara adat.",
			[]optSeed{{"Tari Tortor", true}, {"Tari Saman", false}, {"Tari Kecak", false}, {"Tari Jaipong", false}}},
		{10, "Sigale-gale terbuat dari bahan apa?",
			"Sigale-gale terbuat dari kayu yang diukir menyerupai manusia.",
			[]optSeed{{"Kayu", true}, {"Batu", false}, {"Tanah liat", false}, {"Logam", false}}},
		{10, "Apa makna Sigale-gale bagi masyarakat Batak?",
			"Sigale-gale merupakan simbol penghormatan kepada leluhur dan arwah yang telah meninggal.",
			[]optSeed{{"Penghormatan kepada leluhur", true}, {"Lambang kekayaan", false}, {"Simbol keberanian", false}, {"Tanda kesuburan tanah", false}}},
		{10, "Di mana wisatawan bisa melihat pertunjukan Sigale-gale?",
			"Wisatawan bisa melihat pertunjukan Sigale-gale di Desa Tomok, Pulau Samosir.",
			[]optSeed{{"Desa Tomok, Pulau Samosir", true}, {"Kota Medan", false}, {"Jakarta", false}, {"Museum Nasional", false}}},
		{10, "Sigale-gale menari mengikuti irama musik apa?",
			"Sigale-gale menari mengikuti irama gondang Sabangunan yang khas.",
			[]optSeed{{"Gondang Sabangunan", true}, {"Gamelan Jawa", false}, {"Jazz modern", false}, {"Pop Indonesia", false}}},
		{10, "Apa nama boneka kayu yang bisa menari dalam adat Batak?",
			"Sigale-gale adalah nama boneka kayu yang bisa bergerak dan menari dalam upacara Batak.",
			[]optSeed{{"Sigale-gale", true}, {"Ondel-ondel", false}, {"Barong", false}, {"Wayang", false}}},
		{10, "Sigale-gale menjadi daya tarik wisata karena?",
			"Sigale-gale menarik wisatawan karena keunikannya sebagai boneka kayu yang bisa menari.",
			[]optSeed{{"Keunikan boneka kayu yang bisa menari", true}, {"Harganya sangat mahal", false}, {"Bisa berbicara sendiri", false}, {"Dibuat dari emas", false}}},
		{10, "Pulau tempat Sigale-gale berasal adalah?",
			"Sigale-gale berasal dari Pulau Samosir yang terletak di tengah Danau Toba.",
			[]optSeed{{"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Batam", false}, {"Pulau Jawa", false}}},
	}

	count := 0
	for _, q := range questions {
		var loc models.Location
		config.DB.Where("level_order = ?", q.LocationOrder).First(&loc)
		if loc.ID == 0 {
			continue
		}

		var existing models.LocationQuestion
		if config.DB.Where("location_id = ? AND question = ?", loc.ID, q.Question).
			First(&existing).Error != nil {
			lq := models.LocationQuestion{
				LocationID:  loc.ID,
				Question:    q.Question,
				Explanation: q.Explanation,
				IsActive:    true,
			}
			config.DB.Create(&lq)
			for _, opt := range q.Options {
				config.DB.Create(&models.LocationQuestionOption{
					QuestionID: lq.ID,
					OptionText: opt.Text,
					IsCorrect:  opt.IsCorrect,
				})
			}
			count++
		}
	}
	log.Printf("  ✓ Location Questions (%d soal untuk 10 lokasi)", count)
}
