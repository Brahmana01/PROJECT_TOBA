package database

import (
	"errors"
	"fmt"
	"log"
	"strings"
	"time"
	"toba-backend/config"
	"toba-backend/models"

	"golang.org/x/crypto/bcrypt"
	"gorm.io/gorm"
)

func SeedAll() {
	SeedCharacters()
	SeedTitles()
	SeedLocations()
	SeedLocationFacts()
	SeedLocationQuestions()
	SeedTriviaCategories()
	SeedTriviaQuestions()
	SeedBattleQuestions()
	SeedAchievements()
	SeedGalleryCards()
	SeedGameSettings()
	SeedWeeklyChallenges()
	SeedAdmin()
	log.Println("ℹ️ Seluruh fungsi seed selesai dipanggil; periksa log masing-masing untuk memastikan tidak ada error.")
}

// ============================================================
// CHARACTERS
// ============================================================
func SeedCharacters() {
	characters := []models.Character{
		{ID: 1, Name: "Ucok", Image: "char_ucok.png", Description: "Anak laki-laki Batak yang cerdas dan pemberani"},
		{ID: 2, Name: "Butet", Image: "char_butet.png", Description: "Anak perempuan Batak yang ceria dan pintar"},
	}

	for _, character := range characters {
		var existing models.Character
		err := config.DB.First(&existing, character.ID).Error

		if errors.Is(err, gorm.ErrRecordNotFound) {
			if err := config.DB.Create(&character).Error; err != nil {
				log.Printf("❌ Gagal membuat character %s: %v", character.Name, err)
				continue
			}
			log.Printf("✅ Character ditambahkan: %s", character.Name)
			continue
		}

		if err != nil {
			log.Printf("❌ Gagal mengecek character %s: %v", character.Name, err)
			continue
		}

		log.Printf("ℹ️ Character sudah ada, dipertahankan: %s", character.Name)
	}
}

// ============================================================
// TITLES — sesuai field Title: TitleName, MinimumLevel
// ============================================================
func SeedTitles() {
	titles := []models.Title{
		{ID: 1, TitleName: "Wisatawan Pemula", MinimumLevel: 1},
		{ID: 2, TitleName: "Penjelajah Budaya", MinimumLevel: 5},
		{ID: 3, TitleName: "Sahabat Budaya", MinimumLevel: 10},
		{ID: 4, TitleName: "Ahli Budaya", MinimumLevel: 15},
		{ID: 5, TitleName: "Maestro Budaya", MinimumLevel: 20},
	}

	for _, title := range titles {
		var existing models.Title
		err := config.DB.First(&existing, title.ID).Error

		if errors.Is(err, gorm.ErrRecordNotFound) {
			if err := config.DB.Create(&title).Error; err != nil {
				log.Printf("❌ Gagal membuat title %s: %v", title.TitleName, err)
				continue
			}
			log.Printf("✅ Title ditambahkan: %s", title.TitleName)
			continue
		}

		if err != nil {
			log.Printf("❌ Gagal mengecek title %s: %v", title.TitleName, err)
			continue
		}

		log.Printf("ℹ️ Title sudah ada, dipertahankan: %s", title.TitleName)
	}
}

// ============================================================
// LOCATIONS
// ============================================================
func SeedLocations() {
	locations := []models.Location{
		{ID: 1, Name: "IT Del", LevelOrder: 1, Description: "Institut Teknologi Del adalah kampus asri di Laguboti, Kabupaten Toba. Berdiri di tepi Danau Toba, kampus ini menjadi simbol pendidikan teknologi terbaik di Sumatera Utara.", Image: "itdel.png", RewardXP: 50, RewardStars: 1, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 2, Name: "Taman Eden 100", LevelOrder: 2, Description: "Kawasan wisata alam di Kabupaten Toba yang menawarkan hutan tropis, air terjun, dan pemandangan alam yang memukau. Paru-paru hijau kawasan Danau Toba.", Image: "tamaneden.png", RewardXP: 60, RewardStars: 1, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 3, Name: "Kaldera Toba", LevelOrder: 3, Description: "Supervolcano terbesar di dunia yang kini menjadi danau vulkanik terbesar. Kawasan ini sering menjadi tempat pelaksanaan upacara adat penting masyarakat Batak Toba.", Image: "kalderatoba.png", RewardXP: 70, RewardStars: 1, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 4, Name: "Parapat", LevelOrder: 4, Description: "Kota wisata di tepi Danau Toba yang terkenal dengan kuliner khas Batak, souvenir, dan pemandangan danau yang memukau. Pintu gerbang utama menuju Pulau Samosir.", Image: "parapat.png", RewardXP: 80, RewardStars: 2, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 5, Name: "Ajibata", LevelOrder: 5, Description: "Pelabuhan utama di sisi Parapat yang menghubungkan Sumatera dengan Pulau Samosir melalui kapal ferry. Pemandangan Danau Toba dari sini sangat memesona.", Image: "ajibata.png", RewardXP: 90, RewardStars: 2, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 6, Name: "Tomok", LevelOrder: 6, Description: "Desa wisata di Pulau Samosir terkenal dengan pasar souvenir khas Batak dan pertunjukan tari Sigale-gale. Terdapat pula makam Raja Sidabutar yang bersejarah.", Image: "tomok.png", RewardXP: 100, RewardStars: 2, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 7, Name: "Tuktuk", LevelOrder: 7, Description: "Semenanjung di Pulau Samosir dengan pemandangan Danau Toba yang spektakuler. Pusat wisata dengan berbagai penginapan, restoran, dan budaya lokal yang kaya.", Image: "tuktuk.png", RewardXP: 110, RewardStars: 2, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 8, Name: "Batuhoda", LevelOrder: 8, Description: "Pantai indah di Pulau Samosir dengan pasir putih bersih dan air danau yang jernih. Cocok untuk berenang dan menikmati keindahan Danau Toba.", Image: "batuhoda.png", RewardXP: 120, RewardStars: 3, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 9, Name: "Tanah Ponggol", LevelOrder: 9, Description: "Kawasan di Samosir dengan jembatan ikonik penghubung daratan ke Pulau Samosir. Dari sini wisatawan bisa memasuki pulau tanpa naik kapal ferry.", Image: "tanahponggol.png", RewardXP: 130, RewardStars: 3, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
		{ID: 10, Name: "Sibea-bea", LevelOrder: 10, Description: "Destinasi ikonik di Pulau Samosir dengan patung Yesus Kristus setinggi 61 meter. Pemandangan Danau Toba dari ketinggian ini sangat memukau wisatawan.", Image: "sibeabea.png", RewardXP: 150, RewardStars: 3, QuestionCount: 5, MinimumCorrect: 3, IsActive: true},
	}
	for _, l := range locations {
		var existing models.Location
		result := config.DB.Where("level_order = ?", l.LevelOrder).First(&existing)
		if result.Error == nil {
			log.Printf("ℹ️  Location sudah ada: %s", l.Name)
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek location %s: %v", l.Name, result.Error)
			continue
		}
		if err := config.DB.Create(&l).Error; err != nil {
			log.Printf("❌ Gagal seed location %s: %v", l.Name, err)
			continue
		}
		log.Printf("✅ Location ditambahkan: %s", l.Name)
	}
	log.Println("✅ Locations selesai")
}

// ============================================================
// LOCATION FACTS (4 fakta per lokasi = 40 total)
// ============================================================
func SeedLocationFacts() {
	facts := []models.LocationFact{
		// IT DEL
		{LocationID: 1, Fact: "IT Del didirikan pada tahun 2001 atas inisiatif BJ Habibie sebagai wujud kepedulian terhadap pendidikan di kawasan Danau Toba."},
		{LocationID: 1, Fact: "Kampus IT Del terletak di Laguboti, Kabupaten Toba, tepat di tepi Danau Toba yang memiliki pemandangan spektakuler."},
		{LocationID: 1, Fact: "IT Del adalah singkatan dari Institut Teknologi Del, perguruan tinggi swasta berbasis teknologi di Sumatera Utara."},
		{LocationID: 1, Fact: "IT Del menjadi salah satu institusi pendidikan tinggi yang berfokus pada pengembangan ilmu pengetahuan dan teknologi di kawasan Danau Toba."},

		// TAMAN EDEN 100
		{LocationID: 2, Fact: "Taman Eden 100 memiliki lebih dari 100 hektar hutan tropis yang dijaga kelestariannya oleh masyarakat lokal Batak."},
		{LocationID: 2, Fact: "Di Taman Eden 100 terdapat beragam flora dan fauna endemik Sumatera yang langka dan dilindungi."},
		{LocationID: 2, Fact: "Kawasan ini dikenal dengan program konservasi hutan yang melibatkan aktif masyarakat Batak setempat."},
		{LocationID: 2, Fact: "Taman Eden 100 menawarkan wisata alam berupa kawasan hutan dan air terjun."},

		// KALDERA TOBA
		{LocationID: 3, Fact: "Danau Toba terbentuk dari letusan supervolcano sekitar 74.000 tahun lalu, salah satu letusan terbesar dalam sejarah Bumi."},
		{LocationID: 3, Fact: "Kaldera Toba adalah kaldera terbesar di dunia dengan panjang 100 km, lebar 30 km, dan kedalaman hingga 505 meter."},
		{LocationID: 3, Fact: "Di tengah Danau Toba terdapat Pulau Samosir yang merupakan pulau vulkanik terbesar di dunia yang berada di dalam danau."},
		{LocationID: 3, Fact: "Danau Toba merupakan danau vulkanik yang terbentuk dari aktivitas letusan gunung api purba."},

		// PARAPAT
		{LocationID: 4, Fact: "Parapat berjarak sekitar 176 km dari Medan dan bisa ditempuh dalam waktu 4-5 jam perjalanan darat."},
		{LocationID: 4, Fact: "Pelabuhan Parapat adalah titik keberangkatan utama kapal ferry menuju Pulau Samosir yang beroperasi setiap hari."},
		{LocationID: 4, Fact: "Mie Gomak adalah kuliner khas Parapat yang sering disebut spaghetti-nya orang Batak karena bentuk mienya yang tebal."},
		{LocationID: 4, Fact: "Parapat dikenal sebagai salah satu pintu gerbang wisata menuju Pulau Samosir."},

		// AJIBATA
		{LocationID: 5, Fact: "Pelabuhan Ajibata adalah pelabuhan terbesar di sisi Parapat yang melayani penyeberangan ferry ke Pulau Samosir."},
		{LocationID: 5, Fact: "Dari Ajibata, perjalanan naik ferry ke Tomok di Pulau Samosir memakan waktu sekitar 30-45 menit."},
		{LocationID: 5, Fact: "Di kawasan Ajibata terdapat berbagai rumah makan yang menyajikan ikan bakar khas Danau Toba yang lezat."},
		{LocationID: 5, Fact: "Pelabuhan Ajibata menjadi salah satu jalur transportasi penting yang menghubungkan kawasan Parapat dengan Pulau Samosir."},

		// TOMOK
		{LocationID: 6, Fact: "Tomok adalah desa pertama yang dikunjungi wisatawan saat tiba di Pulau Samosir dari Pelabuhan Ajibata."},
		{LocationID: 6, Fact: "Di Tomok terdapat makam Raja Sidabutar, raja pertama Tomok, yang diperkirakan berusia lebih dari 500 tahun."},
		{LocationID: 6, Fact: "Pertunjukan Sigale-gale di Tomok adalah atraksi boneka kayu yang bisa menari mengikuti irama musik Batak."},
		{LocationID: 6, Fact: "Tomok dikenal sebagai kawasan wisata yang memiliki pasar souvenir khas Batak."},

		// TUKTUK
		{LocationID: 7, Fact: "Tuktuk adalah semenanjung berbentuk unik di Pulau Samosir yang dikelilingi Danau Toba dari hampir semua sisi."},
		{LocationID: 7, Fact: "Kawasan Tuktuk menjadi pusat akomodasi dan pariwisata di Pulau Samosir dengan banyak hotel dan resort tepi danau."},
		{LocationID: 7, Fact: "Dari Tuktuk wisatawan bisa menyewa sepeda motor atau sepeda untuk mengelilingi Pulau Samosir yang indah."},
		{LocationID: 7, Fact: "Pemandangan Danau Toba menjadi salah satu daya tarik wisata utama kawasan Tuktuk."},

		// BATUHODA
		{LocationID: 8, Fact: "Batuhoda berasal dari kata Batu dan Hoda (kuda dalam Bahasa Batak) karena ada batu berbentuk kuda di kawasan ini."},
		{LocationID: 8, Fact: "Pantai Batuhoda memiliki air danau yang sangat jernih dan dangkal sehingga cocok untuk berenang keluarga."},
		{LocationID: 8, Fact: "Aksara Batak Toba memiliki 19 huruf induk yang masing-masing memiliki bunyi vokal a secara default."},
		{LocationID: 8, Fact: "Kawasan Pantai Batuhoda memiliki fasilitas wisata seperti restoran dan penginapan."},

		// TANAH PONGGOL
		{LocationID: 9, Fact: "Jembatan Tanah Ponggol menghubungkan daratan Kabupaten Samosir dengan Pulau Samosir sepanjang sekitar 100 meter."},
		{LocationID: 9, Fact: "Melalui Tanah Ponggol wisatawan bisa mencapai Pulau Samosir dengan kendaraan tanpa harus naik kapal ferry."},
		{LocationID: 9, Fact: "Upacara adat pernikahan Batak Toba disebut pesta unjuk yang melibatkan banyak tahapan dan kedua keluarga besar."},
		{LocationID: 9, Fact: "Hasapi merupakan alat musik tradisional Batak Toba yang dimainkan dengan cara dipetik."},

		// SIBEA-BEA
		{LocationID: 10, Fact: "Patung Yesus Kristus di Sibea-bea setinggi 61 meter menjadikannya salah satu patung Yesus tertinggi di Asia Tenggara."},
		{LocationID: 10, Fact: "Dari Sibea-bea wisatawan bisa menikmati panorama Danau Toba 360 derajat yang sangat memukau dari ketinggian."},
		{LocationID: 10, Fact: "Tulila adalah alat musik tiup tradisional Batak Toba yang terbuat dari bambu dan menghasilkan suara lembut dan merdu."},
		{LocationID: 10, Fact: "Tulila dimainkan dengan cara ditiup dan dikenal menghasilkan alunan musik yang lembut serta penuh perasaan."},
	}

	for _, f := range facts {
		var existing models.LocationFact

		result := config.DB.Where(
			"location_id = ? AND fact = ?",
			f.LocationID,
			f.Fact,
		).First(&existing)

		if result.Error == nil {
			continue
		}

		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek fact: %v", result.Error)
			continue
		}

		if err := config.DB.Create(&f).Error; err != nil {
			log.Printf("❌ Gagal seed fact: %v", err)
			continue
		}
	}

	log.Printf("✅ Location Facts selesai (%d fakta)", len(facts))
}

// ============================================================
// LOCATION QUESTIONS (15 soal per lokasi = 150 total)
// Sesuai soal yang diberikan + dummy untuk melengkapi
// ============================================================
func SeedLocationQuestions() {

	type Opt struct {
		Text    string
		Correct bool
	}
	type Q struct {
		LocID       uint
		Question    string
		Explanation string
		Opts        []Opt
	}

	questions := []Q{
		// ══════════════════════════════════════════════════════
		// LOKASI 1: IT DEL (15 soal)
		// ══════════════════════════════════════════════════════
		{1, "'Au lao tu IT Del'. Kata 'lao' dalam Bahasa Batak berarti...", "'Lao' berarti 'pergi'. Kalimat tersebut artinya Saya pergi ke IT Del.", []Opt{{"Berasal", false}, {"Pergi", true}, {"Suka", false}, {"Senang", false}}},
		{1, "Pilih kata yang tepat untuk 'belajar' dalam Bahasa Batak:", "'Marsiajar' adalah kata Bahasa Batak yang berarti belajar.", []Opt{{"Marsiajar", true}, {"Marsitopak", false}, {"Mangalehon", false}, {"Manggokhon", false}}},
		{1, "Kata 'sikolla' dalam Bahasa Batak berarti...", "'Sikolla' dalam Bahasa Batak berarti sekolah.", []Opt{{"Pasar", false}, {"Danau", false}, {"Sekolah", true}, {"Sawah", false}}},
		{1, "Pilih kata yang tepat untuk 'menulis' dalam Bahasa Batak:", "'Manurat' adalah kata Bahasa Batak yang berarti menulis.", []Opt{{"Manjaha", false}, {"Manurat", true}, {"Manirat", false}, {"Manapu", false}}},
		{1, "Apa arti dari kata 'ulos'?", "Ulos adalah kain tradisional tenun khas Batak Toba yang memiliki nilai budaya dan spiritual tinggi.", []Opt{{"Topi", false}, {"Sarung", false}, {"Kain tradisional", true}, {"Sepatu", false}}},
		{1, "Kata 'marhusip' dalam Bahasa Batak berarti...", "'Marhusip' berarti berbisik atau berbicara pelan dalam Bahasa Batak.", []Opt{{"Berlari", false}, {"Berbisik", true}, {"Bernyanyi", false}, {"Berteriak", false}}},
		{1, "Pilih kata yang tepat untuk 'membaca' dalam Bahasa Batak:", "'Manjaha' adalah kata Bahasa Batak yang berarti membaca.", []Opt{{"Manjaha", true}, {"Manurat", false}, {"Marsiajar", false}, {"Mandok", false}}},
		{1, "Kata 'guru' dalam Bahasa Batak disebut...", "Dalam Bahasa Batak, guru disebut 'guru' atau 'pangajari' yang berarti orang yang mengajar.", []Opt{{"Guru", true}, {"Parsikola", false}, {"Panuturi", false}, {"Parjolo", false}}},
		{1, "Pilih kata yang tepat untuk 'pintar' dalam Bahasa Batak:", "'Malo' adalah kata Bahasa Batak yang berarti pintar atau cerdas.", []Opt{{"Timbo", false}, {"Malo", true}, {"Uli", false}, {"Torop", false}}},
		{1, "Apa arti kata 'bagas' dalam Bahasa Batak?", "'Bagas' dalam Bahasa Batak berarti rumah. Bagas adalah tempat tinggal keluarga Batak.", []Opt{{"Sawah", false}, {"Rumah", true}, {"Hutan", false}, {"Danau", false}}},
		{1, "Kata 'parjolo' dalam Bahasa Batak berarti...", "'Parjolo' berarti yang pertama atau terdepan dalam Bahasa Batak.", []Opt{{"Terakhir", false}, {"Pertama", true}, {"Tengah", false}, {"Terbesar", false}}},
		{1, "Pilih kata yang tepat untuk 'teman' dalam Bahasa Batak:", "'Dongan' adalah kata Bahasa Batak yang berarti teman atau kawan.", []Opt{{"Hula", false}, {"Dongan", true}, {"Boru", false}, {"Amang", false}}},
		{1, "Apa arti dari kata 'lomo' dalam Bahasa Batak?", "'Lomo' dalam Bahasa Batak berarti suka atau senang terhadap sesuatu.", []Opt{{"Tidak suka", false}, {"Suka/Senang", true}, {"Sedih", false}, {"Marah", false}}},
		{1, "Kata 'naeng' dalam Bahasa Batak berarti...", "'Naeng' dalam Bahasa Batak berarti mau atau ingin.", []Opt{{"Tidak mau", false}, {"Mau/Ingin", true}, {"Sudah", false}, {"Belum", false}}},
		{1, "Pilih kata yang tepat untuk 'cepat' dalam Bahasa Batak:", "'Sura-sura' atau 'longang' dalam Bahasa Batak berarti cepat.", []Opt{{"Lambat", false}, {"Longang", true}, {"Timbo", false}, {"Torop", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 2: TAMAN EDEN 100 (15 soal)
		// ══════════════════════════════════════════════════════
		{2, "Kata 'harangan' dalam Bahasa Batak berarti...", "'Harangan' berarti hutan. Taman Eden 100 memiliki kawasan harangan yang sangat luas.", []Opt{{"Gunung", false}, {"Hutan", true}, {"Lembah", false}, {"Bukit", false}}},
		{2, "Pilih kata yang tepat untuk 'indah' dalam Bahasa Batak:", "'Uli' dalam Bahasa Batak berarti indah. Pemandangan Taman Eden sangat uli!", []Opt{{"Uli", true}, {"Jempek", false}, {"Timbo", false}, {"Bidang", false}}},
		{2, "Kata 'mangombak' dalam Bahasa Batak berarti...", "'Mangombak' dalam Bahasa Batak berarti mencangkul, bukan ombak seperti yang terdengar.", []Opt{{"Ombak", false}, {"Angin", false}, {"Mencangkul", true}, {"Menenun", false}}},
		{2, "Pilih kata yang tepat untuk 'berenang' dalam Bahasa Batak:", "'Marlange' adalah kata Bahasa Batak yang berarti berenang.", []Opt{{"Marmeam", false}, {"Marlange", true}, {"Marsiajar", false}, {"Marlojong", false}}},
		{2, "Apa nama tarian khas dari adat Batak?", "Tari Tortor adalah tarian tradisional khas Batak Toba yang ditampilkan dalam berbagai upacara adat.", []Opt{{"Tari Piring", false}, {"Serampang Dua Belas", false}, {"Tari Tortor", true}, {"Tari Topeng", false}}},
		{2, "Kata 'dolok' dalam Bahasa Batak berarti...", "'Dolok' dalam Bahasa Batak berarti gunung atau bukit yang tinggi.", []Opt{{"Danau", false}, {"Gunung", true}, {"Sungai", false}, {"Lembah", false}}},
		{2, "Pilih kata yang tepat untuk 'air' dalam Bahasa Batak:", "'Aek' adalah kata Bahasa Batak yang berarti air.", []Opt{{"Tao", false}, {"Aek", true}, {"Dolok", false}, {"Harangan", false}}},
		{2, "Kata 'tao' dalam Bahasa Batak berarti...", "'Tao' dalam Bahasa Batak berarti danau. Danau Toba disebut Tao Toba.", []Opt{{"Sungai", false}, {"Laut", false}, {"Danau", true}, {"Rawa", false}}},
		{2, "Pilih kata yang tepat untuk 'burung' dalam Bahasa Batak:", "'Manuk-manuk' adalah kata Bahasa Batak yang berarti burung.", []Opt{{"Lombu", false}, {"Manuk-manuk", true}, {"Horbo", false}, {"Biang", false}}},
		{2, "Kata 'marlojong' dalam Bahasa Batak berarti...", "'Marlojong' dalam Bahasa Batak berarti berlari atau lari-lari.", []Opt{{"Berjalan", false}, {"Berlari", true}, {"Berenang", false}, {"Terbang", false}}},
		{2, "Apa arti kata 'timbo' dalam Bahasa Batak?", "'Timbo' dalam Bahasa Batak berarti tinggi.", []Opt{{"Pendek", false}, {"Tinggi", true}, {"Panjang", false}, {"Lebar", false}}},
		{2, "Pilih kata yang tepat untuk 'besar' dalam Bahasa Batak:", "'Bontar' dalam Bahasa Batak berarti putih, sedangkan 'godang' berarti besar.", []Opt{{"Otik", false}, {"Godang", true}, {"Bontar", false}, {"Birong", false}}},
		{2, "Kata 'hosa' dalam Bahasa Batak berarti...", "'Hosa' dalam Bahasa Batak berarti napas atau bernafas.", []Opt{{"Makan", false}, {"Napas", true}, {"Tidur", false}, {"Berjalan", false}}},
		{2, "Pilih kata untuk 'pohon' dalam Bahasa Batak:", "'Hau' dalam Bahasa Batak berarti pohon atau kayu.", []Opt{{"Rura", false}, {"Hau", true}, {"Aek", false}, {"Tano", false}}},
		{2, "Kata 'tano' dalam Bahasa Batak berarti...", "'Tano' dalam Bahasa Batak berarti tanah atau bumi.", []Opt{{"Air", false}, {"Tanah", true}, {"Api", false}, {"Angin", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 3: KALDERA TOBA (15 soal)
		// ══════════════════════════════════════════════════════
		{3, "Kata 'dongan tubu' dalam Bahasa Batak berarti...", "'Dongan tubu' berarti teman satu marga atau orang yang lahir dari garis keturunan yang sama.", []Opt{{"Orangtua", false}, {"Teman baik", false}, {"Teman satu marga", true}, {"Kakak perempuan", false}}},
		{3, "Pilih kata yang tepat untuk 'pesta' dalam Bahasa Batak:", "'Ulaon' dalam Bahasa Batak berarti pesta atau upacara.", []Opt{{"Ulaon", true}, {"Onan", false}, {"Huta", false}, {"Modom", false}}},
		{3, "Kata 'mangoli' dalam Bahasa Batak berarti...", "'Mangoli' dalam Bahasa Batak berarti menikah.", []Opt{{"Bekerja", false}, {"Bertunangan", false}, {"Menikah", true}, {"Bertemu", false}}},
		{3, "Pilih kata yang tepat untuk 'gendang' dalam Bahasa Batak:", "'Gondang' adalah kata Bahasa Batak yang berarti gendang.", []Opt{{"Godang", false}, {"Gondang", true}, {"Gok", false}, {"Geok", false}}},
		{3, "Pilih kata yang tepat untuk 'ramai' dalam Bahasa Batak:", "'Torop' dalam Bahasa Batak berarti ramai atau banyak.", []Opt{{"Sadari", false}, {"Satokkin", false}, {"Saotik", false}, {"Torop", true}}},
		{3, "Apa yang dimaksud dengan 'Dalihan Na Tolu' dalam adat Batak?", "Dalihan Na Tolu berarti tiga tungku yang melambangkan tiga pilar relasi sosial: hula-hula, dongan tubu, dan boru.", []Opt{{"Tiga macam ulos", false}, {"Tiga jenis makanan", false}, {"Tiga pilar relasi sosial", true}, {"Tiga generasi marga", false}}},
		{3, "Kata 'huta' dalam Bahasa Batak berarti...", "'Huta' dalam Bahasa Batak berarti kampung atau desa.", []Opt{{"Kota", false}, {"Kampung/Desa", true}, {"Hutan", false}, {"Pasar", false}}},
		{3, "Pilih kata yang tepat untuk 'pasar' dalam Bahasa Batak:", "'Onan' dalam Bahasa Batak berarti pasar tradisional.", []Opt{{"Huta", false}, {"Onan", true}, {"Bagas", false}, {"Tano", false}}},
		{3, "Kata 'marsangap' dalam Bahasa Batak berarti...", "'Marsangap' dalam Bahasa Batak berarti mulia atau terhormat.", []Opt{{"Miskin", false}, {"Mulia/Terhormat", true}, {"Rendah", false}, {"Lemah", false}}},
		{3, "Siapakah yang disebut 'hula-hula' dalam adat Batak?", "Hula-hula adalah keluarga dari pihak istri yang memiliki posisi terhormat dalam adat Batak.", []Opt{{"Saudara ayah", false}, {"Keluarga dari istri", true}, {"Anak laki-laki", false}, {"Saudara ibu", false}}},
		{3, "Kata 'modom' dalam Bahasa Batak berarti...", "'Modom' dalam Bahasa Batak berarti tidur.", []Opt{{"Makan", false}, {"Minum", false}, {"Tidur", true}, {"Bermain", false}}},
		{3, "Pilih kata untuk 'bekerja' dalam Bahasa Batak:", "'Manogihon' atau 'mardege' dalam Bahasa Batak berarti bekerja.", []Opt{{"Manogihon", true}, {"Marlange", false}, {"Marsiajar", false}, {"Mangan", false}}},
		{3, "Apa arti kata 'borhat' dalam Bahasa Batak?", "'Borhat' dalam Bahasa Batak berarti berangkat atau pergi.", []Opt{{"Datang", false}, {"Berangkat", true}, {"Pulang", false}, {"Berhenti", false}}},
		{3, "Kata 'ro' dalam Bahasa Batak berarti...", "'Ro' dalam Bahasa Batak berarti datang atau tiba.", []Opt{{"Pergi", false}, {"Datang", true}, {"Berlari", false}, {"Berhenti", false}}},
		{3, "Pilih kata untuk 'senang' dalam Bahasa Batak:", "'Marasi' atau 'las roha' dalam Bahasa Batak berarti senang atau gembira.", []Opt{{"Marsak", false}, {"Las roha", true}, {"Dombak", false}, {"Rimas", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 4: PARAPAT (15 soal)
		// ══════════════════════════════════════════════════════
		{4, "Gendang khas Batak Toba disebut...", "Taganing adalah gendang khas Batak Toba yang terdiri dari 5 buah gendang berbeda ukuran.", []Opt{{"Taganing", true}, {"Drum", false}, {"Sangkakala", false}, {"Biola", false}}},
		{4, "Makanan yang disebut 'sushi-nya orang Batak' adalah...", "Naniura disebut sushi Batak karena berbahan ikan mas mentah yang direndam bumbu tanpa dimasak api.", []Opt{{"Omok-lomok", false}, {"Naniura", true}, {"Na tinombur", false}, {"Itak gurgur", false}}},
		{4, "Aksesoris kepala tradisional untuk pria dalam pakaian adat Batak disebut...", "Sortali adalah ikat kepala tradisional khas pria Batak Toba dalam berbagai upacara adat.", []Opt{{"Sortali", true}, {"Tali-tali", false}, {"Songket", false}, {"Sarung", false}}},
		{4, "Pilih kata yang tepat untuk 'makan' dalam Bahasa Batak:", "'Mangan' adalah kata Bahasa Batak yang berarti makan.", []Opt{{"Manang", false}, {"Mangan", true}, {"Mampe", false}, {"Marsangap", false}}},
		{4, "Apa nama mie khas Batak yang disebut 'spaghetti Batak'?", "Mie Gomak adalah mie khas Batak Toba yang tebal dan panjang seperti spaghetti.", []Opt{{"Mie Ayam", false}, {"Mie Aceh", false}, {"Ifu Mie", false}, {"Mie Gomak", true}}},
		{4, "Apa makanan Batak yang berbahan dasar ikan mas dimasak dengan andaliman?", "Arsik adalah masakan khas Batak Toba berupa ikan mas yang dimasak dengan bumbu kuning dan andaliman.", []Opt{{"Saksang", false}, {"Naniura", false}, {"Arsik", true}, {"Lappet", false}}},
		{4, "Lappet biasanya dibungkus dengan...", "Lappet adalah kue tradisional Batak yang terbuat dari tepung beras dan dibungkus dengan daun pisang.", []Opt{{"Daun singkong", false}, {"Daun kelapa", false}, {"Daun pisang", true}, {"Daun sirih", false}}},
		{4, "Makanan khas Batak berbahan daging dan darah adalah...", "Saksang adalah masakan khas Batak yang menggunakan daging dengan bumbu darah dan rempah khas.", []Opt{{"Naniura", false}, {"Arsik", false}, {"Saksang", true}, {"Itak gurgur", false}}},
		{4, "Dali ni horbo terbuat dari...", "Dali ni horbo adalah keju khas Batak yang terbuat dari susu kerbau yang dipadatkan.", []Opt{{"Susu kerbau", true}, {"Susu sapi", false}, {"Kelapa", false}, {"Air jeruk", false}}},
		{4, "Makanan berbahan beras dan parutan kelapa disebut...", "Itak gurgur adalah makanan tradisional Batak Toba berbahan beras dan parutan kelapa.", []Opt{{"Itak gurgur", true}, {"Ombus-ombus", false}, {"Na tinombur", false}, {"Lappet", false}}},
		{4, "Ombus-ombus biasanya dimakan saat...", "Ombus-ombus adalah kue tradisional Batak yang biasanya dihidangkan saat pesta adat.", []Opt{{"Ujian", false}, {"Pesta adat", true}, {"Upacara kematian", false}, {"Acara resmi", false}}},
		{4, "Apa arti dari kalimat 'Male au'?", "'Male au' dalam Bahasa Batak berarti Saya lapar.", []Opt{{"Saya kenyang", false}, {"Saya lapar", true}, {"Saya haus", false}, {"Saya mengantuk", false}}},
		{4, "Kalimat Bahasa Batak untuk 'Saya mau makan' adalah...", "'Naeng mangan au' artinya Saya mau makan. Naeng berarti mau/ingin.", []Opt{{"Sai ro au mangan", false}, {"Naeng mangan au", true}, {"Mangan ho do", false}, {"Au do manang", false}}},
		{4, "Apa arti kata 'butong' dalam Bahasa Batak?", "'Butong' dalam Bahasa Batak berarti kenyang.", []Opt{{"Lapar", false}, {"Haus", false}, {"Kenyang", true}, {"Mengantuk", false}}},
		{4, "Kata 'mahuas' dalam Bahasa Batak berarti...", "'Mahuas' dalam Bahasa Batak berarti haus.", []Opt{{"Lapar", false}, {"Haus", true}, {"Kenyang", false}, {"Pusing", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 5: AJIBATA (15 soal)
		// ══════════════════════════════════════════════════════
		{5, "Apa kata 'pelabuhan' dalam Bahasa Batak?", "'Hopal' dalam Bahasa Batak berarti pelabuhan.", []Opt{{"Pelabuhan", false}, {"Hopal", true}, {"Dermaga", false}, {"Stasiun", false}}},
		{5, "Kata perahu dalam Bahasa Batak adalah...", "'Parau' adalah kata Bahasa Batak yang berarti perahu atau kapal.", []Opt{{"Parau", true}, {"Hapal", false}, {"Kapal", false}, {"Perahu", false}}},
		{5, "'Panompang hopal paimahon di bontean'. Kata 'Panompang' berarti...", "'Panompang' dalam Bahasa Batak berarti penumpang.", []Opt{{"Kapal", false}, {"Pelabuhan", false}, {"Dermaga", false}, {"Penumpang", true}}},
		{5, "'Parau nunga borhat tu Tomok'. Kata 'nunga' berarti...", "'Nunga' dalam Bahasa Batak berarti sudah. Kalimat ini berarti Perahu sudah berangkat ke Tomok.", []Opt{{"Tomok", false}, {"Berangkat", false}, {"Akan", false}, {"Sudah", true}}},
		{5, "'Supir hopal' memiliki makna...", "'Supir hopal' berarti pengemudi kapal karena hopal berarti kapal/pelabuhan.", []Opt{{"Pengemudi", false}, {"Masinis", false}, {"Kondektur", false}, {"Pengemudi kapal", true}}},
		{5, "Kata 'bontean' dalam Bahasa Batak berarti...", "'Bontean' dalam Bahasa Batak berarti pelabuhan atau dermaga.", []Opt{{"Pasar", false}, {"Pelabuhan", true}, {"Rumah", false}, {"Hutan", false}}},
		{5, "Pilih kata untuk 'menyeberang' dalam Bahasa Batak:", "'Mambege' dalam Bahasa Batak berarti menyeberang.", []Opt{{"Marlange", false}, {"Mambege", true}, {"Marlojong", false}, {"Borhat", false}}},
		{5, "Kata 'kapal' dalam Bahasa Batak adalah...", "Dalam Bahasa Batak, kapal disebut 'hopal' yang merujuk pada kendaraan air besar.", []Opt{{"Parau", false}, {"Hopal", true}, {"Bontean", false}, {"Topi", false}}},
		{5, "Apa arti 'sian' dalam Bahasa Batak?", "'Sian' dalam Bahasa Batak berarti dari atau berasal dari.", []Opt{{"Ke", false}, {"Dari", true}, {"Di", false}, {"Untuk", false}}},
		{5, "Kata 'tu' dalam Bahasa Batak berarti...", "'Tu' dalam Bahasa Batak berarti ke atau menuju.", []Opt{{"Dari", false}, {"Ke", true}, {"Di", false}, {"Dan", false}}},
		{5, "Pilih kata untuk 'berangkat' dalam Bahasa Batak:", "'Borhat' dalam Bahasa Batak berarti berangkat atau pergi.", []Opt{{"Ro", false}, {"Borhat", true}, {"Mago", false}, {"Sae", false}}},
		{5, "Kata 'jolma' dalam Bahasa Batak berarti...", "'Jolma' dalam Bahasa Batak berarti orang atau manusia.", []Opt{{"Hewan", false}, {"Orang/Manusia", true}, {"Tumbuhan", false}, {"Benda", false}}},
		{5, "Apa arti 'torop jolma' dalam Bahasa Batak?", "'Torop jolma' dalam Bahasa Batak berarti banyak orang karena torop = banyak dan jolma = orang.", []Opt{{"Sedikit orang", false}, {"Banyak orang", true}, {"Satu orang", false}, {"Orang asing", false}}},
		{5, "Kata 'paidaida' dalam Bahasa Batak berarti...", "'Paidaida' dalam Bahasa Batak berarti melihat-lihat atau menonton.", []Opt{{"Makan-makan", false}, {"Melihat-lihat", true}, {"Jalan-jalan", false}, {"Beli-beli", false}}},
		{5, "Pilih kata untuk 'pulang' dalam Bahasa Batak:", "'Mulak' dalam Bahasa Batak berarti pulang atau kembali.", []Opt{{"Borhat", false}, {"Mulak", true}, {"Ro", false}, {"Sae", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 6: TOMOK (15 soal)
		// ══════════════════════════════════════════════════════
		{6, "Apa Bahasa Indonesia dari kata 'bontean'?", "'Bontean' dalam Bahasa Batak berarti pelabuhan.", []Opt{{"Pelabuhan", true}, {"Hopal", false}, {"Dermaga", false}, {"Stasiun", false}}},
		{6, "'Laho maredang-edang tu Tomok'. Arti 'maredang-edang' adalah...", "'Maredang-edang' dalam Bahasa Batak berarti jalan-jalan atau berwisata santai.", []Opt{{"Jalan-jalan", true}, {"Lari-lari", false}, {"Berenang", false}, {"Menyeberang", false}}},
		{6, "'Torop halak martiga-tiga di Tomok'. Kata 'martiga-tiga' berarti...", "'Martiga-tiga' dalam Bahasa Batak berarti berdagang.", []Opt{{"Bernyanyi", false}, {"Berdagang", true}, {"Bertani", false}, {"Bersepeda", false}}},
		{6, "'Godang halak paidaida Sigale-gale'. Godang sama artinya dengan...", "'Godang' dalam Bahasa Batak berarti banyak, sama dengan kata bahat.", []Opt{{"Santopik", false}, {"Saotik", false}, {"Otik", false}, {"Bahat", true}}},
		{6, "'Rarat hian parbornginan di Tomok'. Arti 'parbornginan' adalah...", "'Parbornginan' dalam Bahasa Batak berarti hotel atau tempat menginap.", []Opt{{"Gubuk", false}, {"Rumah", false}, {"Hotel", true}, {"Restoran", false}}},
		{6, "Di Tomok terdapat makam raja pertama yang disebut...", "Makam Raja Sidabutar adalah situs bersejarah di Tomok yang berusia lebih dari 500 tahun.", []Opt{{"Raja Sirait", false}, {"Raja Sidabutar", true}, {"Raja Simanjuntak", false}, {"Raja Sinaga", false}}},
		{6, "Pertunjukan boneka kayu yang bisa menari di Tomok disebut...", "Sigale-gale adalah pertunjukan boneka kayu khas Batak Toba yang bisa menari mengikuti musik.", []Opt{{"Gondang", false}, {"Sigale-gale", true}, {"Tortor", false}, {"Gondang Sabangunan", false}}},
		{6, "Kata 'marbaju' dalam Bahasa Batak berarti...", "'Marbaju' dalam Bahasa Batak berarti memakai baju atau berpakaian.", []Opt{{"Membeli baju", false}, {"Memakai baju", true}, {"Mencuci baju", false}, {"Menjahit baju", false}}},
		{6, "Pilih kata untuk 'belanja' dalam Bahasa Batak:", "'Mamboli' dalam Bahasa Batak berarti membeli atau berbelanja.", []Opt{{"Mamboli", true}, {"Mangallang", false}, {"Mangan", false}, {"Mandok", false}}},
		{6, "Apa arti 'souvenir' dalam konteks budaya Batak?", "Souvenir khas Batak di Tomok meliputi ulos, patung kayu, dan berbagai kerajinan tangan tradisional.", []Opt{{"Makanan khas", false}, {"Kerajinan tangan khas", true}, {"Alat musik", false}, {"Pakaian adat", false}}},
		{6, "Kata 'na sae' dalam Bahasa Batak berarti...", "'Na sae' dalam Bahasa Batak berarti sudah selesai atau sudah habis.", []Opt{{"Belum selesai", false}, {"Sudah selesai", true}, {"Sedang berjalan", false}, {"Belum mulai", false}}},
		{6, "Pilih kata untuk 'enak' dalam Bahasa Batak:", "'Tabo' dalam Bahasa Batak berarti enak atau lezat.", []Opt{{"Tabo", true}, {"Bura", false}, {"Oto", false}, {"Malo", false}}},
		{6, "Kata 'na uli' dalam Bahasa Batak berarti...", "'Na uli' dalam Bahasa Batak berarti yang indah atau cantik.", []Opt{{"Yang jelek", false}, {"Yang indah", true}, {"Yang besar", false}, {"Yang kecil", false}}},
		{6, "Apa arti 'marsijalangan' dalam Bahasa Batak?", "'Marsijalangan' dalam Bahasa Batak berarti saling mengunjungi.", []Opt{{"Saling bertengkar", false}, {"Saling mengunjungi", true}, {"Saling berjauhan", false}, {"Saling berdiam", false}}},
		{6, "Kata 'hita' dalam Bahasa Batak berarti...", "'Hita' dalam Bahasa Batak berarti kita semua (inklusif termasuk lawan bicara).", []Opt{{"Saya", false}, {"Kamu", false}, {"Kita", true}, {"Mereka", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 7: TUKTUK (15 soal)
		// ══════════════════════════════════════════════════════
		{7, "'Beta mardalani to Tuktuk'. Arti 'mardalani' adalah...", "'Mardalani' dalam Bahasa Batak berarti jalan-jalan atau berkeliling.", []Opt{{"Jalan-jalan", true}, {"Lari-lari", false}, {"Berenang", false}, {"Menyeberang", false}}},
		{7, "'Panatapan na uli di topi Tao Toba'. Arti 'panatapan' adalah...", "'Panatapan' dalam Bahasa Batak berarti pemandangan. Tuktuk terkenal dengan panatapan Danau Toba.", []Opt{{"Gunung", false}, {"Sawah", false}, {"Pemandangan", true}, {"Menyeberang", false}}},
		{7, "'Hami markapal sian Tomok to Tuktuk'. Arti kalimat tersebut adalah...", "Kalimat ini berarti Kami naik kapal dari Tomok ke Tuktuk.", []Opt{{"Kami naik kapal dari Tomok ke Tuktuk", true}, {"Kami bertani dari Tomok ke Tuktuk", false}, {"Kami berjalan dari Tomok ke Tuktuk", false}, {"Kami berkelana ke Tuktuk", false}}},
		{7, "'Godang halak Batak maredang-edang tu Samosir'. Halak berarti...", "'Halak' dalam Bahasa Batak berarti orang atau jolma.", []Opt{{"Jolma", true}, {"Baoa", false}, {"Borua", false}, {"Hita", false}}},
		{7, "'Andigan hamu lao tu Tuktuk'. Arti 'andigan' adalah...", "'Andigan' dalam Bahasa Batak berarti kapan. Kalimat ini berarti Kapan kalian pergi ke Tuktuk?", []Opt{{"Mengapa", false}, {"Siapa", false}, {"Kapan", true}, {"Dimana", false}}},
		{7, "Kata 'topi tao' dalam Bahasa Batak berarti...", "'Topi tao' dalam Bahasa Batak berarti tepi danau atau pinggir danau.", []Opt{{"Tengah danau", false}, {"Tepi danau", true}, {"Bawah danau", false}, {"Atas danau", false}}},
		{7, "Pilih kata untuk 'melihat' dalam Bahasa Batak:", "'Panatapan' berasal dari kata 'tangkop' atau 'ida' yang berarti melihat.", []Opt{{"Mandok", false}, {"Mangida", true}, {"Mangan", false}, {"Marlange", false}}},
		{7, "Kata 'hami' dalam Bahasa Batak berarti...", "'Hami' dalam Bahasa Batak berarti kami (eksklusif, tidak termasuk lawan bicara).", []Opt{{"Saya", false}, {"Kamu", false}, {"Kami", true}, {"Kalian", false}}},
		{7, "Kata 'hamu' dalam Bahasa Batak berarti...", "'Hamu' dalam Bahasa Batak berarti kalian atau kamu semua.", []Opt{{"Saya", false}, {"Kalian", true}, {"Kami", false}, {"Mereka", false}}},
		{7, "Pilih kata untuk 'senang' dalam Bahasa Batak:", "'Las roha' dalam Bahasa Batak berarti senang atau gembira hati.", []Opt{{"Marsak roha", false}, {"Las roha", true}, {"Dombak roha", false}, {"Tiar roha", false}}},
		{7, "Apa arti 'tao Toba' dalam Bahasa Batak?", "'Tao Toba' berarti Danau Toba. Tao berarti danau dalam Bahasa Batak.", []Opt{{"Pulau Toba", false}, {"Gunung Toba", false}, {"Danau Toba", true}, {"Sungai Toba", false}}},
		{7, "Kata 'marsibaen' dalam Bahasa Batak berarti...", "'Marsibaen' dalam Bahasa Batak berarti bekerja sama atau saling membantu.", []Opt{{"Bersaing", false}, {"Bekerja sama", true}, {"Bermusuhan", false}, {"Berpisah", false}}},
		{7, "Pilih kata untuk 'foto' dalam Bahasa Batak:", "Dalam Bahasa Batak modern, foto disebut 'foto' tapi tempat berfoto disebut 'paidaida' (tempat melihat).", []Opt{{"Paidaida", false}, {"Foto", true}, {"Panatapan", false}, {"Pandaida", false}}},
		{7, "Kata 'di jolo ni' dalam Bahasa Batak berarti...", "'Di jolo ni' dalam Bahasa Batak berarti di depan sesuatu.", []Opt{{"Di belakang", false}, {"Di depan", true}, {"Di samping", false}, {"Di atas", false}}},
		{7, "Pilih kata untuk 'cantik' dalam Bahasa Batak:", "'Uli' atau 'uli rupana' dalam Bahasa Batak berarti cantik atau tampan.", []Opt{{"Uli", true}, {"Malo", false}, {"Timbo", false}, {"Godang", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 8: BATUHODA (15 soal)
		// Lima soal awal mengikuti materi Batuhoda yang tersedia.
		// Catatan: glyph khusus untuk kata "panatapan" tidak tersedia
		// pada berkas sumber yang diterima; karena itu soal ke-4
		// menguji arti kata yang memang tercantum pada materi Tuktuk,
		// bukan mengarang bentuk aksara Bataknya.
		// ══════════════════════════════════════════════════════
		{8, "Penulisan 'Batuhoda' dalam aksara Batak adalah...", "Materi sumber menandai bentuk ᯅ ᯗᯂᯑ sebagai jawaban yang benar.", []Opt{{"ᯅ ᯗᯂᯑ", true}, {"ᯔ ᯗ", false}, {"ᯅ ᯇ ᯉ", false}, {"ᯅ ᯉ", false}}},
		{8, "Kata 'saksang' dalam aksara Batak adalah...", "Materi sumber menandai bentuk ᯘᯐᯘᯉᯱ sebagai jawaban yang benar.", []Opt{{"ᯔᯘᯰᯘᯰ", false}, {"ᯘᯘᯔᯰᯘ", false}, {"ᯘᯘᯘᯰ", false}, {"ᯘᯐᯘᯉᯱ", true}}},
		{8, "Tulisan ᯔ ᯑ ᯔ dibaca...", "Bank soal sebelumnya pada file seed mengaitkan rangkaian aksara ᯔ ᯑ ᯔ dengan kata mangan (makan).", []Opt{{"Bapana", false}, {"Mangan", true}, {"Mata", false}, {"Amon-amon", false}}},
		{8, "Apa arti kata 'panatapan' dalam Bahasa Batak?", "Materi Tuktuk yang diberikan pengguna mencantumkan panatapan dengan arti pemandangan. Glyph untuk penulisan kata ini tidak tersedia pada sumber Batuhoda yang dapat diperiksa.", []Opt{{"Pelabuhan", false}, {"Jalan-jalan", false}, {"Pemandangan", true}, {"Berdagang", false}}},
		{8, "Kata pakaian (amon-amon) dalam aksara Batak adalah...", "Materi sumber menandai bentuk ᯀᯔ᯲ᯀᯔ᯲ sebagai jawaban yang benar.", []Opt{{"ᯀᯔ᯲ᯀᯔ᯲", true}, {"ᯀᯝᯀᯝ", false}, {"ᯀᯉ᯲ᯀᯉ᯲", false}, {"ᯀᯔᯰᯀᯔᯰ", false}}},

		// Sepuluh soal tambahan berdasarkan materi lokasi dan aksara.
		{8, "Pantai Batuhoda berada di pulau...", "Materi lokasi menyebut Batuhoda berada di Pulau Samosir.", []Opt{{"Pulau Jawa", false}, {"Pulau Samosir", true}, {"Pulau Nias", false}, {"Pulau Bangka", false}}},
		{8, "Apa ciri pasir yang disebutkan pada materi Pantai Batuhoda?", "Materi Batuhoda menyebut pantai dengan pasir putih.", []Opt{{"Pasir hitam", false}, {"Pasir berbatu", false}, {"Pasir putih", true}, {"Pasir merah", false}}},
		{8, "Bagaimana kondisi air danau di kawasan Pantai Batuhoda menurut materi?", "Materi menyebut air danau yang jernih.", []Opt{{"Keruh", false}, {"Berlumpur", false}, {"Kering", false}, {"Jernih", true}}},
		{8, "Kegiatan yang cocok dilakukan di danau kawasan Batuhoda adalah...", "Materi menyebut danau di kawasan Batuhoda cocok untuk berenang.", []Opt{{"Berenang", true}, {"Berselancar di laut", false}, {"Menyelam di samudra", false}, {"Bermain ski salju", false}}},
		{8, "Fasilitas wisata yang disebutkan tersedia di kawasan Pantai Batuhoda adalah...", "Materi menyebut adanya restoran dan penginapan.", []Opt{{"Bandara dan terminal", false}, {"Restoran dan penginapan", true}, {"Pabrik dan gudang", false}, {"Stasiun kereta", false}}},
		{8, "Salah satu kegiatan yang sesuai dilakukan pengunjung di Pantai Batuhoda adalah...", "Materi menyebut kawasan Batuhoda cocok untuk berfoto.", []Opt{{"Mendaki gunung es", false}, {"Berkemah di gurun", false}, {"Berfoto", true}, {"Berselancar di samudra", false}}},
		{8, "Berapa jumlah induk aksara dalam aksara Batak Toba menurut materi?", "Materi aksara Batak Toba menyebut 19 induk aksara.", []Opt{{"15", false}, {"17", false}, {"21", false}, {"19", true}}},
		{8, "Penggabungan induk aksara dengan anak surat digunakan untuk...", "Anak surat ditambahkan pada induk aksara untuk membentuk bunyi dengan vokal yang sesuai.", []Opt{{"Mengubah bentuk huruf menjadi angka", false}, {"Menghapus seluruh konsonan", false}, {"Membentuk bunyi aksara dengan vokal tertentu", true}, {"Mengubah arah penulisan", false}}},
		{8, "Anak surat '-i' pada aksara Batak digunakan untuk...", "Anak surat -i mengubah bunyi vokal pada induk aksara menjadi i.", []Opt{{"Menghilangkan semua konsonan", false}, {"Menambah bunyi i", true}, {"Menambah bunyi u", false}, {"Mengubah aksara menjadi angka", false}}},
		{8, "Apa fungsi tanda pangolat dalam aksara Batak?", "Pangolat atau tanda bunuh digunakan untuk menghilangkan bunyi vokal a pada akhir suku kata.", []Opt{{"Menambah bunyi vokal a", false}, {"Mengubah huruf menjadi angka", false}, {"Menghilangkan bunyi vokal a pada akhir suku kata", true}, {"Menambah bunyi vokal i", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 9: TANAH PONGGOL (15 soal)
		// ══════════════════════════════════════════════════════
		{9, "Pesta pernikahan dalam adat Batak Toba...", "Pesta pernikahan adat Batak Toba terdiri dari lebih dari satu tahapan mulai dari pemberkatan hingga ulaon sadari.", []Opt{{"Merupakan acara dukacita", false}, {"Dirayakan selama tiga hari berturut-turut", false}, {"Terdiri dari lebih dari satu tahapan", true}, {"Harus diadakan pada akhir tahun", false}}},
		{9, "Jika pengantin laki-laki dari keluarga Sinaga, maka...", "Dalam adat Batak, pihak paranak adalah pihak laki-laki. Jika pengantin laki-laki bermarga Sinaga, maka paranak bermarga Sinaga.", []Opt{{"Pihak parboru bermarga Sinaga", false}, {"Pihak parboru tidak bermarga", false}, {"Pihak paranak bermarga Sinaga", true}, {"Pihak paranak tidak bermarga", false}}},
		{9, "Hasapi adalah alat musik seperti...", "Hasapi adalah alat musik tradisional Batak Toba yang bentuknya menyerupai gitar kecil dengan dua senar.", []Opt{{"Gitar kecil", true}, {"Drum", false}, {"Piano", false}, {"Seruling", false}}},
		{9, "Hasapi dimainkan dengan cara...", "Hasapi dimainkan dengan cara dipetik menggunakan jari, menghasilkan suara yang lembut.", []Opt{{"Digesek", false}, {"Dipukul", false}, {"Ditiup", false}, {"Dipetik", true}}},
		{9, "Dalam Bahasa Batak, teras rumah disebut...", "'Di joloni jabu' dalam Bahasa Batak berarti di teras rumah atau di depan rumah.", []Opt{{"Di bagas", false}, {"Di sikkola", false}, {"Di joloni jabu", true}, {"Di kamar", false}}},
		{9, "Apa yang dimaksud dengan 'martumpol' dalam adat Batak?", "Martumpol adalah upacara pertunangan secara adat Batak sebelum pernikahan dilaksanakan.", []Opt{{"Pesta setelah menikah", false}, {"Upacara pertunangan", true}, {"Upacara kematian", false}, {"Penabalan marga", false}}},
		{9, "Apa arti 'parboru' dalam adat Batak?", "Parboru adalah pihak perempuan atau keluarga pengantin perempuan dalam adat pernikahan Batak.", []Opt{{"Pihak laki-laki", false}, {"Pihak perempuan", true}, {"Orang tua pengantin", false}, {"Pengantin baru", false}}},
		{9, "Apa arti 'paranak' dalam adat Batak?", "Paranak adalah pihak laki-laki atau keluarga pengantin laki-laki dalam adat pernikahan Batak.", []Opt{{"Pihak perempuan", false}, {"Pihak laki-laki", true}, {"Keluarga mertua", false}, {"Tamu undangan", false}}},
		{9, "Upacara 'marhata sinamot' dalam adat Batak berkaitan dengan...", "Marhata sinamot adalah proses pembicaraan mas kawin antara keluarga pengantin laki-laki dan perempuan.", []Opt{{"Pembagian ulos", false}, {"Pembicaraan mas kawin", true}, {"Penabalan gelar", false}, {"Pesta setelah menikah", false}}},
		{9, "Kata 'pasu-pasu' dalam adat Batak berarti...", "'Pasu-pasu' dalam adat Batak berarti pemberkatan atau doa restu.", []Opt{{"Hukuman adat", false}, {"Pemberkatan/Doa restu", true}, {"Larangan adat", false}, {"Pembagian warisan", false}}},
		{9, "Jembatan Tanah Ponggol menghubungkan...", "Jembatan Tanah Ponggol menghubungkan daratan Kabupaten Samosir dengan Pulau Samosir.", []Opt{{"Parapat dengan Tomok", false}, {"Daratan dengan Pulau Samosir", true}, {"Ajibata dengan Tuktuk", false}, {"Samosir dengan Sumatera", false}}},
		{9, "Kata 'marsibuha-buhai' dalam Bahasa Batak berarti...", "'Marsibuha-buhai' dalam Bahasa Batak berarti bergotong royong atau bekerja bersama-sama.", []Opt{{"Berdiam diri", false}, {"Bergotong royong", true}, {"Berselisih paham", false}, {"Berpencar", false}}},
		{9, "Dalam adat Batak, 'tudu-tudu sipanganon' adalah...", "Tudu-tudu sipanganon adalah sajian makanan adat yang disajikan dalam upacara pernikahan Batak.", []Opt{{"Tarian adat", false}, {"Sajian makanan adat", true}, {"Pemberian ulos", false}, {"Nyanyian adat", false}}},
		{9, "Kata 'dengke' dalam Bahasa Batak berarti...", "'Dengke' dalam Bahasa Batak berarti ikan. Dengke simudur-udur adalah ikan yang disajikan dalam upacara adat.", []Opt{{"Ayam", false}, {"Ikan", true}, {"Daging", false}, {"Sayur", false}}},
		{9, "Apa arti 'manjalo tumpak' dalam adat pernikahan Batak?", "'Manjalo tumpak' berarti menerima sumbangan atau pemberian dari tamu dalam upacara adat.", []Opt{{"Membagikan makanan", false}, {"Menerima sumbangan", true}, {"Memberikan ulos", false}, {"Melakukan tarian", false}}},

		// ══════════════════════════════════════════════════════
		// LOKASI 10: SIBEA-BEA (15 soal)
		// ══════════════════════════════════════════════════════
		{10, "Nama alat musik tiup yang sering dimainkan di Sibea-bea adalah...", "Tulila adalah alat musik tiup tradisional Batak Toba yang terbuat dari bambu.", []Opt{{"Tulila", true}, {"Hasapi", false}, {"Taganing", false}, {"Sarune", false}}},
		{10, "Cara memainkan Tulila adalah dengan...", "Tulila dimainkan dengan cara ditiup. Ini adalah alat musik aerophone Batak Toba.", []Opt{{"Digesek", false}, {"Dipukul", false}, {"Dipetik", false}, {"Ditiup", true}}},
		{10, "Suara dari Tulila terdengar seperti...", "Tulila menghasilkan suara yang lembut dan penuh perasaan, cocok untuk acara sakral.", []Opt{{"Lembut dan penuh perasaan", true}, {"Keras dan ramai", false}, {"Seperti drum", false}, {"Seperti peluit", false}}},
		{10, "Tulila biasa dimainkan pada saat...", "Tulila biasanya dimainkan dalam upacara atau acara-acara khusus adat Batak Toba.", []Opt{{"Upacara atau acara khusus", true}, {"Olahraga", false}, {"Sekolah", false}, {"Makan malam", false}}},
		{10, "Alat musik Tulila berasal dari daerah...", "Tulila adalah alat musik tradisional dari daerah Batak Toba di Sumatera Utara.", []Opt{{"Bali", false}, {"Batak", true}, {"Jawa", false}, {"Kalimantan", false}}},
		{10, "Patung Yesus Kristus di Sibea-bea memiliki tinggi...", "Patung Yesus di Sibea-bea memiliki tinggi 61 meter, menjadikannya salah satu patung Yesus tertinggi di Asia Tenggara.", []Opt{{"41 meter", false}, {"51 meter", false}, {"61 meter", true}, {"71 meter", false}}},
		{10, "Sibea-bea merupakan destinasi ikonik di...", "Sibea-bea terletak di Pulau Samosir, salah satu destinasi favorit wisatawan domestik dan internasional.", []Opt{{"Pulau Nias", false}, {"Pulau Samosir", true}, {"Kota Medan", false}, {"Kota Parapat", false}}},
		{10, "Apa nama ansambel musik tradisional Batak Toba yang paling terkenal?", "Gondang Sabangunan adalah ansambel musik tradisional Batak Toba yang lengkap dengan berbagai alat musik.", []Opt{{"Gondang Sabangunan", true}, {"Gondang Hasapi", false}, {"Taganing Ensemble", false}, {"Sarune Band", false}}},
		{10, "Alat musik Batak yang terbuat dari tanduk kerbau adalah...", "Sarune Bolon adalah alat tiup tradisional Batak yang terbuat dari tanduk kerbau dengan 4 lubang.", []Opt{{"Taganing", false}, {"Sarune Bolon", true}, {"Hasapi", false}, {"Gondang", false}}},
		{10, "Garantung dalam musik Batak Toba mirip dengan alat musik modern...", "Garantung mirip xylophone karena terdiri dari bilah kayu yang dipukul untuk menghasilkan nada.", []Opt{{"Drum", false}, {"Xylophone", true}, {"Gitar", false}, {"Piano", false}}},
		{10, "Apa fungsi Ogung dalam ansambel musik Batak?", "Ogung berfungsi sebagai pemberi irama dasar yang tetap (ritme) dalam ansambel musik Batak Toba.", []Opt{{"Memainkan melodi", false}, {"Memberi irama dasar", true}, {"Bernyanyi", false}, {"Memimpin ansambel", false}}},
		{10, "Hesek dalam musik Batak terbuat dari...", "Hesek adalah alat musik sederhana Batak yang terbuat dari botol dan logam kecil untuk menambah semangat musik.", []Opt{{"Botol dan logam kecil", true}, {"Kayu dan rotan", false}, {"Bambu dan daun", false}, {"Kulit hewan", false}}},
		{10, "Kata 'gondang' dalam Bahasa Batak berarti...", "'Gondang' dalam Bahasa Batak berarti gendang atau musik tradisional Batak.", []Opt{{"Suling", false}, {"Gendang/Musik tradisional", true}, {"Tari", false}, {"Nyanyian", false}}},
		{10, "Alat musik Batak yang dipukul dengan stik dan berfungsi sebagai dirigen adalah...", "Taganing berfungsi sebagai dirigen dalam ansambel Gondang Sabangunan karena penabuhnya yang memimpin irama.", []Opt{{"Ogung", false}, {"Taganing", true}, {"Hesek", false}, {"Gondang", false}}},
		{10, "Apa arti 'horas' dalam Bahasa Batak?", "'Horas' adalah salam khas Batak Toba yang berarti selamat, sehat, dan sejahtera.", []Opt{{"Selamat tinggal", false}, {"Selamat/Sehat/Sejahtera", true}, {"Terima kasih", false}, {"Maaf", false}}},
	}

	for _, q := range questions {
		var existing models.LocationQuestion
		result := config.DB.Where("location_id = ? AND question = ?", q.LocID, q.Question).First(&existing)
		if result.Error == nil {
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek soal lokasi: %v", result.Error)
			continue
		}

		newQ := models.LocationQuestion{
			LocationID:  q.LocID,
			Question:    q.Question,
			Explanation: q.Explanation,
			IsActive:    true,
		}
		if err := config.DB.Create(&newQ).Error; err != nil {
			log.Printf("❌ Gagal seed soal lokasi: %v", err)
			continue
		}
		for _, opt := range q.Opts {
			config.DB.Create(&models.LocationQuestionOption{
				QuestionID: newQ.ID,
				OptionText: opt.Text,
				IsCorrect:  opt.Correct,
			})
		}
	}
	log.Println("✅ Location Questions selesai (150 soal)")
}

// ============================================================
// TRIVIA CATEGORIES (5 KATEGORI FINAL)
// ============================================================
func SeedTriviaCategories() {
	categories := []models.TriviaCategory{
		{
			ID:           1,
			Name:         "Bahasa dan Kosakata Batak",
			Description:  "Pelajari kosakata, ungkapan, dan istilah Bahasa Batak Toba.",
			DisplayOrder: 1,
			IsActive:     true,
		},
		{
			ID:           2,
			Name:         "Makanan dan Unsur Adat Batak Toba",
			Description:  "Kenali makanan tradisional dan unsur adat Batak Toba.",
			DisplayOrder: 2,
			IsActive:     true,
		},
		{
			ID:           3,
			Name:         "Musik Tradisional",
			Description:  "Kenali alat musik, teknik permainan, dan ansambel musik Batak Toba.",
			DisplayOrder: 3,
			IsActive:     true,
		},
		{
			ID:           4,
			Name:         "Ulos dan Adat Pernikahan",
			Description:  "Pelajari jenis, makna, dan penggunaan ulos serta adat pernikahan Batak Toba.",
			DisplayOrder: 4,
			IsActive:     true,
		},
		{
			ID:           5,
			Name:         "Kekerabatan dan Falsafah Adat",
			Description:  "Pelajari partuturan, tarombo, Dalihan Na Tolu, dan falsafah adat Batak Toba.",
			DisplayOrder: 5,
			IsActive:     true,
		},
	}

	for _, c := range categories {
		var existing models.TriviaCategory
		err := config.DB.First(&existing, c.ID).Error

		if errors.Is(err, gorm.ErrRecordNotFound) {
			if err := config.DB.Create(&c).Error; err != nil {
				log.Printf("❌ Gagal membuat kategori %s: %v", c.Name, err)
				continue
			}
			log.Printf("✅ Kategori ditambahkan: %s", c.Name)
			continue
		}

		if err != nil {
			log.Printf("❌ Gagal mengecek kategori %s: %v", c.Name, err)
			continue
		}

		if err := config.DB.Model(&existing).Updates(map[string]interface{}{
			"name":          c.Name,
			"description":   c.Description,
			"display_order": c.DisplayOrder,
			"is_active":     c.IsActive,
		}).Error; err != nil {
			log.Printf("❌ Gagal memperbarui kategori %s: %v", c.Name, err)
			continue
		}

		log.Printf("✅ Kategori diperbarui: %s", c.Name)
	}

	log.Println("✅ Trivia Categories selesai (5 kategori)")
}

// ============================================================
// TRIVIA QUESTIONS
// 5 soal per kategori = 25 soal
// Sumber dan kunci jawaban sudah ditinjau
// ============================================================
func SeedTriviaQuestions() {
	type Opt struct {
		Text    string
		Correct bool
	}

	type Q struct {
		CatID       uint
		Question    string
		Explanation string
		Opts        []Opt
	}

	questions := []Q{

		// ====================================================
		// KATEGORI 1: BAHASA DAN KOSAKATA BATAK
		// ====================================================

		{
			1,
			"Apa arti kata dior?",
			"Menurut Kamus Hata Batak Toba karya Op. Faustin Panjaitan, dior berarti mencari.",
			[]Opt{
				{"Mengawali", false},
				{"Mencari", true},
				{"Mengawasi", false},
				{"Mengeluarkan", false},
			},
		},
		{
			1,
			"Manakah pasangan kata dan arti yang tepat?",
			"Panjar berarti uang muka atau pembayaran awal.",
			[]Opt{
				{"Panjar — pancaran air", false},
				{"Panjar — uang muka", true},
				{"Panjar — tempat tinggi", false},
				{"Panjar — keturunan", false},
			},
		},
		{
			1,
			"Seseorang disebut nunut ketika ia melakukan pekerjaan dengan ...",
			"Nunut berkaitan dengan ketekunan dan keuletan. Ulaon nunut merupakan pekerjaan yang membutuhkan kesabaran dan ketekunan.",
			[]Opt{
				{"Cepat tanpa memperhatikan hasil", false},
				{"Sesekali saja", false},
				{"Tekun dan sabar", true},
				{"Terpaksa karena disuruh", false},
			},
		},
		{
			1,
			"Jika arah yang dimaksud adalah timur, nama arah dalam Desa Na Ualu adalah ...",
			"Dalam pembagian delapan arah mata angin Batak Toba, Purba menunjukkan arah timur.",
			[]Opt{
				{"Manabia", false},
				{"Purba", true},
				{"Pastima", false},
				{"Dangsina", false},
			},
		},
		{
			1,
			"Es yang dibiarkan di bawah panas matahari berubah menjadi air. Kata yang paling sesuai dengan perubahan tersebut adalah ...",
			"Maderder berarti meleleh atau mencair.",
			[]Opt{
				{"Masarsar", false},
				{"Maderder", true},
				{"Suhar", false},
				{"Sundar", false},
			},
		},

		// ====================================================
		// KATEGORI 2: MAKANAN DAN UNSUR ADAT BATAK TOBA
		// ====================================================

		{
			2,
			"Apa kesimpulan yang paling tepat dari dicantumkannya sipitu dai dalam inventarisasi makanan adat Batak Toba?",
			"Inventarisasi makanan adat Batak Toba mencakup beragam hidangan, seperti sipitu dai, naniura, nani arsik, dan makanan tradisional lainnya.",
			[]Opt{
				{"Semua makanan adat Batak Toba harus berbahan ikan.", false},
				{"Ragam makanan adat Batak Toba mencakup hidangan selain arsik dan naniura.", true},
				{"Semua makanan adat Batak Toba disajikan sebagai camilan.", false},
				{"Sipitu dai merupakan nama lain dari dekke naniura.", false},
			},
		},
		{
			2,
			"Apa perbedaan utama dekke naniura dengan dekke natinombur?",
			"Perbedaan pengolahannya terletak pada proses pemasakan. Naniura diolah tanpa pemasakan panas, sedangkan natinombur dipanggang terlebih dahulu sebelum diberi bumbu.",
			[]Opt{
				{"Naniura diasamkan tanpa pemasakan panas, sedangkan natinombur dipanggang terlebih dahulu.", true},
				{"Naniura dipanggang terlebih dahulu, sedangkan natinombur diasamkan tanpa pemasakan panas.", false},
				{"Naniura direbus dengan bumbu, sedangkan natinombur dikukus dengan rempah.", false},
				{"Naniura digoreng dengan bumbu, sedangkan natinombur direbus dengan asam.", false},
			},
		},
		{
			2,
			"Indahan sipaet-paet tercatat sebagai salah satu ...",
			"Indahan sipaet-paet termasuk makanan tradisional Batak Toba yang berkaitan dengan tradisi mangapuli atau penghiburan dalam suasana duka.",
			[]Opt{
				{"Jenis pakaian adat", false},
				{"Jenis tarian tradisional", false},
				{"Alat musik tradisional", false},
				{"Makanan adat Batak Toba", true},
			},
		},
		{
			2,
			"Pernyataan manakah yang paling tepat tentang makanan tradisional dan kegiatan adat Batak Toba?",
			"Makanan tradisional Batak Toba mempunyai ragam bahan, cara pengolahan, serta fungsi yang dapat berkaitan dengan kegiatan adat dan konteks penyajiannya.",
			[]Opt{
				{"Semua makanan tradisional hanya boleh dimakan saat upacara.", false},
				{"Semua makanan sehari-hari memiliki fungsi adat yang sama.", false},
				{"Makanan adat selalu menggunakan bahan yang mahal.", false},
				{"Jenis makanan dan cara penyajiannya dapat berkaitan dengan konteks acara.", true},
			},
		},
		{
			2,
			"Apa makna penggunaan kerbau dalam upacara kematian Saur Matua pada masyarakat Batak Toba?",
			"Penggunaan kerbau dalam upacara Saur Matua berkaitan dengan kedudukan adat serta status sosial dan ekonomi orang yang meninggal.",
			[]Opt{
				{"Menunjukkan hubungan antarkeluarga", false},
				{"Menandakan kedudukan adat dan sosial orang yang meninggal", true},
				{"Menunjukkan perbedaan mata pencaharian setiap keluarga", false},
				{"Menjadi syarat untuk seluruh kegiatan adat Batak Toba", false},
			},
		},

		// ====================================================
		// KATEGORI 3: MUSIK TRADISIONAL
		// ====================================================

		{
			3,
			"Bagaimana cara memainkan alat musik tradisional Batak Toba yang bernama hasapi?",
			"Hasapi merupakan alat musik berdawai dalam tradisi Batak Toba yang dimainkan dengan teknik petikan.",
			[]Opt{
				{"Dipetik", true},
				{"Ditiup", false},
				{"Digesek", false},
				{"Dipukul", false},
			},
		},
		{
			3,
			"Dalam ansambel Gondang Hasapi, alat musik yang berperan menghasilkan melodi melalui tiupan adalah ...",
			"Sulim merupakan alat musik tiup yang menjadi bagian dari formasi instrumen Gondang Hasapi.",
			[]Opt{
				{"Hasapi", false},
				{"Ogung", false},
				{"Sulim", true},
				{"Hesek", false},
			},
		},
		{
			3,
			"Alat musik Batak Toba yang memiliki beberapa gendang dengan tinggi nada berbeda adalah ...",
			"Taganing merupakan perangkat gendang Batak Toba yang terdiri dari lima gendang dengan ukuran berbeda.",
			[]Opt{
				{"Gordang", false},
				{"Taganing", true},
				{"Garantung", false},
				{"Hesek", false},
			},
		},
		{
			3,
			"Bagaimana cara memainkan alat musik tradisional Batak Toba yang bernama ogung?",
			"Ogung merupakan alat musik gong dalam ansambel musik Batak Toba yang dimainkan dengan cara dipukul.",
			[]Opt{
				{"Digesek", false},
				{"Dipetik", false},
				{"Ditiup", false},
				{"Dipukul", true},
			},
		},
		{
			3,
			"Teknik marsiulak hosa berkaitan dengan permainan alat musik ...",
			"Marsiulak hosa merupakan teknik pernapasan melingkar atau circular breathing yang digunakan pemain Sarune Bolon untuk memainkan frasa musik panjang tanpa jeda napas dalam Gondang Sabangunan.",
			[]Opt{
				{"Tarian tradisional", false},
				{"Ansambel musik tradisional", true},
				{"Pakaian adat", false},
				{"Upacara adat", false},
			},
		},

		// ====================================================
		// KATEGORI 4: ULOS DAN ADAT PERNIKAHAN
		// ====================================================

		{
			4,
			"Dalam upacara pernikahan adat Batak Toba, siapa yang pertama kali memberikan ulos dalam rangkaian marunjuk?",
			"Pada tahapan pemberian ulos dalam marunjuk, pemberian pertama dilakukan oleh orang tua pengantin wanita kepada orang tua pengantin pria.",
			[]Opt{
				{"Orang tua pengantin wanita", true},
				{"Orang tua pengantin pria", false},
				{"Kedua mempelai", false},
				{"Para tamu undangan", false},
			},
		},
		{
			4,
			"Bagaimana cara tradisional membuat kain ulos?",
			"Kain ulos dibuat melalui proses menenun benang oleh para penenun.",
			[]Opt{
				{"Dengan mengukir kayu", false},
				{"Dengan mencetak tanah liat", false},
				{"Dengan menenun benang", true},
				{"Dengan menganyam daun", false},
			},
		},
		{
			4,
			"Manakah yang termasuk jenis ulos dalam upacara marunjuk?",
			"Ulos Passamot merupakan salah satu jenis ulos yang digunakan dalam rangkaian upacara adat marunjuk.",
			[]Opt{
				{"Ulos Passamot", true},
				{"Ulos Tujung", false},
				{"Ulos Saput", false},
				{"Ulos Bolean", false},
			},
		},
		{
			4,
			"Nama Ulos Ragi Idup berkaitan dengan simbol ...",
			"Ragi berarti pola, sedangkan idup berarti kehidupan. Ulos Ragi Idup berkaitan dengan simbol kehidupan.",
			[]Opt{
				{"Perdagangan", false},
				{"Kedudukan", false},
				{"Kehidupan", true},
				{"Peperangan", false},
			},
		},
		{
			4,
			"Ulos Ragi Hotang digunakan dengan harapan agar penerimanya ...",
			"Ulos Ragi Hotang diberikan dengan harapan agar penerimanya memperoleh hasil yang baik dan rajin bekerja.",
			[]Opt{
				{"Memiliki kedudukan tinggi.", false},
				{"Mendapat hasil baik dan rajin bekerja.", true},
				{"Menjadi pemimpin adat.", false},
				{"Selalu memperoleh kekayaan.", false},
			},
		},

		// ====================================================
		// KATEGORI 5: KEKERABATAN DAN FALSAFAH ADAT
		// ====================================================

		{
			5,
			"Menurut sistem partuturan Batak Toba, siapakah yang disebut bona ni ari?",
			"Bona ni ari merupakan istilah kekerabatan yang berkaitan dengan kelompok marga dari garis kerabat leluhur, termasuk saudara laki-laki dari pihak yang melahirkan kakek. Hubungan ini memiliki variasi penjelasan dalam silsilah adat.",
			[]Opt{
				{"Tulang dari pihak ibu kita", false},
				{"Tulang dari pihak ayah kita", false},
				{"Tulang dari oppung doli kita", true},
				{"Tulang dari pihak istri kita", false},
			},
		},
		{
			5,
			"Apa yang paling membedakan pembagian jambar dalam adat dari pembagian makanan biasa?",
			"Parjambaron merupakan pembagian daging dalam pesta adat yang mengikuti aturan penghormatan dan kedudukan kekerabatan berdasarkan Dalihan Na Tolu.",
			[]Opt{
				{"Jambar selalu dibagikan kepada orang yang datang pertama", false},
				{"Jambar memiliki aturan pembagian yang berkaitan dengan adat", true},
				{"Jambar hanya boleh diberikan kepada keluarga satu marga", false},
				{"Jambar selalu dibagikan dalam jumlah yang sama kepada setiap orang", false},
			},
		},
		{
			5,
			"Siapakah leluhur yang dikenal dalam berbagai tradisi tarombo Batak Toba?",
			"Si Raja Batak dikenal sebagai leluhur dalam berbagai tradisi silsilah atau tarombo Batak Toba.",
			[]Opt{
				{"Gajah Mada", false},
				{"Si Raja Batak", true},
				{"Pangeran Diponegoro", false},
				{"Pattimura", false},
			},
		},
		{
			5,
			"Menurut tradisi tarombo Batak Toba yang umum dikenal, siapa dua putra Si Raja Batak?",
			"Dalam tradisi tarombo Batak Toba, Si Raja Batak dikenal memiliki dua putra, yaitu Guru Tatea Bulan dan Raja Isombaon.",
			[]Opt{
				{"Guru Tatea Bulan dan Raja Isombaon", true},
				{"Raja Sisingamangaraja dan Si Singamangaraja XII", false},
				{"Raja Uti dan Raja Ihat Manisia", false},
				{"Raja Marpaung dan Raja Simanjuntak", false},
			},
		},
		{
			5,
			"Dalam suatu pesta adat, seseorang harus menghormati keluarga dari pihak pemberi istri. Dalam Dalihan Na Tolu, pihak tersebut disebut ...",
			"Hula-hula adalah pihak keluarga pemberi istri yang memiliki kedudukan terhormat dalam sistem kekerabatan Dalihan Na Tolu.",
			[]Opt{
				{"Boru", false},
				{"Dongan Tubu", false},
				{"Hula-hula", true},
				{"Pargonsi", false},
			},
		},
	}

	// Validasi jumlah soal dan jawaban sebelum masuk database.
	for _, q := range questions {
		if len(q.Opts) != 4 {
			log.Printf("❌ Soal tidak memiliki 4 opsi: %s", q.Question)
			return
		}

		correctCount := 0
		for _, opt := range q.Opts {
			if opt.Correct {
				correctCount++
			}
		}

		if correctCount != 1 {
			log.Printf("❌ Soal harus memiliki tepat 1 jawaban benar: %s", q.Question)
			return
		}
	}

	// Tandai soal lama yang tidak termasuk bank soal V2 sebagai nonaktif.
	wanted := make(map[string]bool)
	for _, q := range questions {
		wanted[fmt.Sprintf("%d|%s", q.CatID, q.Question)] = true
	}

	if err := config.DB.Transaction(func(tx *gorm.DB) error {
		var activeQuestions []models.TriviaQuestion

		if err := tx.Where("is_active = ?", true).Find(&activeQuestions).Error; err != nil {
			return err
		}

		for _, oldQ := range activeQuestions {
			key := fmt.Sprintf("%d|%s", oldQ.CategoryID, oldQ.Question)

			if !wanted[key] {
				if err := tx.Model(&oldQ).Update("is_active", false).Error; err != nil {
					return err
				}
			}
		}

		for _, q := range questions {
			var existing models.TriviaQuestion

			err := tx.Where(
				"category_id = ? AND question = ? AND is_active = ?",
				q.CatID,
				q.Question,
				true,
			).First(&existing).Error

			if err == nil {
				var oldOptions []models.TriviaQuestionOption

				if err := tx.Where(
					"question_id = ?",
					existing.ID,
				).Order("id ASC").Find(&oldOptions).Error; err != nil {
					return err
				}

				same := existing.Explanation == q.Explanation &&
					len(oldOptions) == len(q.Opts)

				if same {
					for i, opt := range q.Opts {
						if oldOptions[i].OptionText != opt.Text ||
							oldOptions[i].IsCorrect != opt.Correct {
							same = false
							break
						}
					}
				}

				if same {
					continue
				}

				// Versi lama dinonaktifkan, riwayat jawabannya tetap tersimpan.
				if err := tx.Model(&existing).Update("is_active", false).Error; err != nil {
					return err
				}

			} else if !errors.Is(err, gorm.ErrRecordNotFound) {
				return err
			}

			newQ := models.TriviaQuestion{
				CategoryID:  q.CatID,
				Question:    q.Question,
				Explanation: q.Explanation,
				IsActive:    true,
			}

			if err := tx.Create(&newQ).Error; err != nil {
				return err
			}

			for _, opt := range q.Opts {
				newOpt := models.TriviaQuestionOption{
					QuestionID: newQ.ID,
					OptionText: opt.Text,
					IsCorrect:  opt.Correct,
				}

				if err := tx.Create(&newOpt).Error; err != nil {
					return err
				}
			}
		}

		return nil
	}); err != nil {
		log.Printf("❌ Gagal menjalankan Trivia Questions V2: %v", err)
		return
	}

	log.Println("✅ Trivia Questions V2 selesai (25 soal, 5 kategori)")
}

// ============================================================
// BATTLE QUESTIONS (25 SOAL BANK)
// 5 soal dipilih untuk setiap pertandingan
// ============================================================
func SeedBattleQuestions() {
	type Opt struct {
		Text    string
		Correct bool
	}

	type Q struct {
		Question    string
		Explanation string
		Opts        []Opt
	}

	questions := []Q{

		// ====================================================
		// SOAL 1-15: BANK SOAL LAMA (DIPERTAHANKAN)
		// ====================================================

		{
			"Apa arti kata 'horas' dalam Bahasa Batak?",
			"'Horas' adalah salam khas Batak Toba yang berarti selamat, sehat, dan sejahtera.",
			[]Opt{
				{"Selamat/Sehat", true},
				{"Terima kasih", false},
				{"Sampai jumpa", false},
				{"Maaf", false},
			},
		},
		{
			"Danau Toba terletak di provinsi...",
			"Danau Toba adalah danau vulkanik terbesar di dunia yang terletak di Sumatera Utara.",
			[]Opt{
				{"Sumatera Barat", false},
				{"Sumatera Utara", true},
				{"Riau", false},
				{"Aceh", false},
			},
		},
		{
			"'Mauliate' dalam Bahasa Batak berarti...",
			"'Mauliate' adalah ungkapan terima kasih dalam Bahasa Batak Toba.",
			[]Opt{
				{"Maaf", false},
				{"Halo", false},
				{"Terima kasih", true},
				{"Selamat datang", false},
			},
		},
		{
			"Apa nama sistem kekerabatan khas Batak yang terdiri dari tiga unsur?",
			"Dalihan Na Tolu adalah sistem kekerabatan Batak yang terdiri dari hula-hula, dongan tubu, dan boru.",
			[]Opt{
				{"Dalihan Na Tolu", true},
				{"Gorga Batak", false},
				{"Adat Sinamot", false},
				{"Marga Tiga", false},
			},
		},
		{
			"Pulau yang berada di tengah Danau Toba disebut...",
			"Pulau Samosir adalah pulau vulkanik yang berada di tengah Danau Toba.",
			[]Opt{
				{"Pulau Nias", false},
				{"Pulau Samosir", true},
				{"Pulau Berhala", false},
				{"Pulau Sibolga", false},
			},
		},
		{
			"Apa yang dimaksud dengan 'marga' dalam adat Batak?",
			"Marga adalah nama klan atau kelompok kekerabatan patrilineal dalam masyarakat Batak.",
			[]Opt{
				{"Nama desa", false},
				{"Nama klan/garis keturunan", true},
				{"Nama gelar bangsawan", false},
				{"Nama upacara adat", false},
			},
		},
		{
			"Alat musik Batak yang terdiri dari 5 gendang adalah...",
			"Taganing adalah gendang khas Batak Toba yang terdiri dari 5 buah gendang berbeda ukuran.",
			[]Opt{
				{"Gondang", false},
				{"Taganing", true},
				{"Ogung", false},
				{"Sarune", false},
			},
		},
		{
			"Arsik adalah masakan Batak berbahan dasar...",
			"Arsik adalah masakan khas Batak Toba yang berbahan dasar ikan mas yang dimasak dengan bumbu kuning.",
			[]Opt{
				{"Daging kerbau", false},
				{"Ikan mas", true},
				{"Ayam kampung", false},
				{"Daging babi", false},
			},
		},
		{
			"Rumah adat Batak Toba disebut...",
			"Jabu atau Ruma Adat adalah rumah tradisional Batak Toba yang memiliki atap melengkung.",
			[]Opt{
				{"Joglo", false},
				{"Gadang", false},
				{"Jabu/Ruma Batak", true},
				{"Honai", false},
			},
		},
		{
			"Apa arti kata 'au' dalam Bahasa Batak?",
			"'Au' dalam Bahasa Batak berarti saya atau aku.",
			[]Opt{
				{"Kamu", false},
				{"Kita", false},
				{"Saya/Aku", true},
				{"Mereka", false},
			},
		},
		{
			"Ornamen ukiran khas yang menghiasi rumah adat Batak disebut...",
			"Gorga adalah ornamen atau ukiran tradisional khas Batak Toba yang menghiasi rumah adat.",
			[]Opt{
				{"Batik", false},
				{"Gorga", true},
				{"Tenun", false},
				{"Songket", false},
			},
		},
		{
			"Kata 'inang' dalam Bahasa Batak berarti...",
			"'Inang' dalam Bahasa Batak berarti ibu.",
			[]Opt{
				{"Ayah", false},
				{"Kakek", false},
				{"Ibu", true},
				{"Nenek", false},
			},
		},
		{
			"Apa nama festival budaya Batak yang dirayakan di Danau Toba?",
			"Pesta Danau Toba adalah festival budaya yang merayakan kekayaan budaya Batak Toba.",
			[]Opt{
				{"Pesta Danau Toba", true},
				{"Festival Nias", false},
				{"Karnaval Batak", false},
				{"Pekan Budaya Melayu", false},
			},
		},
		{
			"'Amang' dalam Bahasa Batak berarti...",
			"'Amang' dalam Bahasa Batak berarti ayah.",
			[]Opt{
				{"Ibu", false},
				{"Ayah", true},
				{"Kakak", false},
				{"Paman", false},
			},
		},
		{
			"Alat musik modern yang mirip dengan Garantung Batak adalah...",
			"Garantung mirip xylophone karena terdiri dari bilah kayu yang dipukul untuk menghasilkan nada.",
			[]Opt{
				{"Drum", false},
				{"Xylophone", true},
				{"Gitar", false},
				{"Piano", false},
			},
		},

		// ====================================================
		// SOAL 16-25: TAMBAHAN BANK SOAL
		// Berdasarkan materi Trivia yang sudah tersedia
		// ====================================================

		{
			"Dalam ansambel Gondang Hasapi, alat musik yang menghasilkan melodi melalui tiupan adalah...",
			"Sulim merupakan alat musik tiup yang menjadi bagian dari formasi instrumen Gondang Hasapi.",
			[]Opt{
				{"Hasapi", false},
				{"Ogung", false},
				{"Sulim", true},
				{"Hesek", false},
			},
		},
		{
			"Perbedaan utama dekke naniura dengan dekke natinombur terletak pada...",
			"Naniura diolah tanpa pemasakan panas, sedangkan natinombur dipanggang terlebih dahulu sebelum diberi bumbu.",
			[]Opt{
				{"Cara pengolahan ikan", true},
				{"Jenis kain yang digunakan", false},
				{"Alat musik pengiring", false},
				{"Bentuk rumah adat", false},
			},
		},
		{
			"Indahan sipaet-paet tercatat sebagai salah satu...",
			"Indahan sipaet-paet termasuk makanan tradisional Batak Toba yang berkaitan dengan tradisi mangapuli atau penghiburan dalam suasana duka.",
			[]Opt{
				{"Jenis pakaian adat", false},
				{"Jenis tarian tradisional", false},
				{"Alat musik tradisional", false},
				{"Makanan tradisional Batak Toba", true},
			},
		},
		{
			"Nama Ulos Ragi Idup berkaitan dengan simbol...",
			"Ragi berarti pola, sedangkan idup berarti kehidupan. Ulos Ragi Idup berkaitan dengan simbol kehidupan.",
			[]Opt{
				{"Perdagangan", false},
				{"Kehidupan", true},
				{"Peperangan", false},
				{"Perjalanan", false},
			},
		},
		{
			"Bagaimana cara tradisional membuat kain ulos?",
			"Kain ulos dibuat melalui proses menenun benang oleh para penenun.",
			[]Opt{
				{"Dengan mengukir kayu", false},
				{"Dengan mencetak tanah liat", false},
				{"Dengan menenun benang", true},
				{"Dengan menganyam daun", false},
			},
		},
		{
			"Dalam upacara pernikahan adat Batak Toba, siapa yang pertama kali memberikan ulos dalam rangkaian marunjuk?",
			"Pada tahapan pemberian ulos dalam marunjuk, pemberian pertama dilakukan oleh orang tua pengantin wanita kepada orang tua pengantin pria.",
			[]Opt{
				{"Orang tua pengantin pria", false},
				{"Kedua mempelai", false},
				{"Para tamu undangan", false},
				{"Orang tua pengantin wanita", true},
			},
		},
		{
			"Ulos Passamot merupakan salah satu jenis ulos yang digunakan dalam rangkaian...",
			"Ulos Passamot merupakan salah satu jenis ulos yang digunakan dalam rangkaian upacara adat marunjuk.",
			[]Opt{
				{"Marunjuk", true},
				{"Upacara kematian", false},
				{"Penyambutan wisatawan", false},
				{"Permainan tradisional", false},
			},
		},
		{
			"Menurut tradisi tarombo Batak Toba yang umum dikenal, dua putra Si Raja Batak adalah...",
			"Dalam tradisi tarombo Batak Toba, Si Raja Batak dikenal memiliki dua putra, yaitu Guru Tatea Bulan dan Raja Isombaon.",
			[]Opt{
				{"Raja Uti dan Raja Ihat Manisia", false},
				{"Raja Sisingamangaraja dan Raja Isombaon", false},
				{"Guru Tatea Bulan dan Raja Isombaon", true},
				{"Guru Tatea Bulan dan Raja Uti", false},
			},
		},
		{
			"Dalam pembagian delapan arah mata angin Batak Toba, Purba menunjukkan arah...",
			"Dalam pembagian delapan arah mata angin Batak Toba, Purba menunjukkan arah timur.",
			[]Opt{
				{"Barat", false},
				{"Timur", true},
				{"Utara", false},
				{"Selatan", false},
			},
		},
		{
			"Penggunaan kerbau dalam upacara kematian Saur Matua berkaitan dengan...",
			"Penggunaan kerbau dalam upacara Saur Matua berkaitan dengan kedudukan adat serta status sosial dan ekonomi orang yang meninggal.",
			[]Opt{
				{"Hubungan perdagangan antarwilayah", false},
				{"Penentuan arah rumah adat", false},
				{"Penanda jenis alat musik", false},
				{"Kedudukan adat dan sosial-ekonomi orang yang meninggal", true},
			},
		},
	}

	// ========================================================
	// VALIDASI BANK SOAL SEBELUM DISIMPAN
	// ========================================================

	if len(questions) != 25 {
		log.Printf("❌ Jumlah Battle Questions harus 25, ditemukan %d", len(questions))
		return
	}

	seenQuestions := make(map[string]bool)

	for _, q := range questions {
		if q.Question == "" || q.Explanation == "" {
			log.Printf("❌ Pertanyaan atau penjelasan kosong: %s", q.Question)
			return
		}

		if seenQuestions[q.Question] {
			log.Printf("❌ Pertanyaan duplikat: %s", q.Question)
			return
		}
		seenQuestions[q.Question] = true

		if len(q.Opts) != 4 {
			log.Printf("❌ Soal harus memiliki 4 opsi: %s", q.Question)
			return
		}

		correctCount := 0
		seenOptions := make(map[string]bool)

		for _, opt := range q.Opts {
			if opt.Text == "" || seenOptions[opt.Text] {
				log.Printf("❌ Opsi kosong atau duplikat: %s", q.Question)
				return
			}

			seenOptions[opt.Text] = true

			if opt.Correct {
				correctCount++
			}
		}

		if correctCount != 1 {
			log.Printf("❌ Soal harus memiliki tepat 1 jawaban benar: %s", q.Question)
			return
		}
	}

	// ========================================================
	// SINKRONISASI BANK SOAL BATTLE (25 SOAL AKTIF)
	// Riwayat pertandingan lama tetap dipertahankan.
	// ========================================================

	wanted := make(map[string]struct{}, len(questions))
	for _, q := range questions {
		wanted[q.Question] = struct{}{}
	}

	if err := config.DB.Transaction(func(tx *gorm.DB) error {

		// 1. Nonaktifkan soal lama yang tidak masuk bank terbaru.
		var allQuestions []models.BattleQuestion

		if err := tx.Find(&allQuestions).Error; err != nil {
			return err
		}

		for _, oldQ := range allQuestions {
			if _, exists := wanted[oldQ.Question]; !exists && oldQ.IsActive {
				if err := tx.Model(&models.BattleQuestion{}).
					Where("id = ?", oldQ.ID).
					Update("is_active", false).Error; err != nil {
					return fmt.Errorf("gagal mengarsipkan soal ID %d: %w", oldQ.ID, err)
				}
			}
		}

		// 2. Sinkronkan setiap soal dalam bank terbaru.
		for _, q := range questions {

			var candidates []models.BattleQuestion

			if err := tx.Where(
				"question = ?",
				q.Question,
			).Order("id DESC").Find(&candidates).Error; err != nil {
				return err
			}

			var exactMatch *models.BattleQuestion

			// Cari versi soal yang konten dan opsinya identik.
			for i := range candidates {

				var oldOptions []models.BattleQuestionOption

				if err := tx.Where(
					"question_id = ?",
					candidates[i].ID,
				).Order("id ASC").Find(&oldOptions).Error; err != nil {
					return err
				}

				same := candidates[i].Explanation == q.Explanation &&
					len(oldOptions) == len(q.Opts)

				if same {
					for j, opt := range q.Opts {
						if oldOptions[j].OptionText != opt.Text ||
							oldOptions[j].IsCorrect != opt.Correct {
							same = false
							break
						}
					}
				}

				if same {
					if exactMatch == nil ||
						(!exactMatch.IsActive && candidates[i].IsActive) {

						candidate := candidates[i]
						exactMatch = &candidate
					}
				}
			}

			// 3. Nonaktifkan versi lain dari pertanyaan yang sama.
			for _, oldQ := range candidates {
				if oldQ.IsActive &&
					(exactMatch == nil || oldQ.ID != exactMatch.ID) {

					if err := tx.Model(&models.BattleQuestion{}).
						Where("id = ?", oldQ.ID).
						Update("is_active", false).Error; err != nil {
						return err
					}
				}
			}

			// 4. Gunakan kembali soal jika kontennya identik.
			if exactMatch != nil {

				if !exactMatch.IsActive {
					if err := tx.Model(&models.BattleQuestion{}).
						Where("id = ?", exactMatch.ID).
						Update("is_active", true).Error; err != nil {
						return err
					}
				}

				continue
			}

			// 5. Buat versi baru jika belum ada konten identik.
			newQ := models.BattleQuestion{
				Question:    q.Question,
				Explanation: q.Explanation,
				IsActive:    true,
			}

			if err := tx.Create(&newQ).Error; err != nil {
				return fmt.Errorf("gagal membuat soal Battle: %w", err)
			}

			for _, opt := range q.Opts {

				newOpt := models.BattleQuestionOption{
					QuestionID: newQ.ID,
					OptionText: opt.Text,
					IsCorrect:  opt.Correct,
				}

				if err := tx.Create(&newOpt).Error; err != nil {
					return fmt.Errorf("gagal membuat opsi Battle: %w", err)
				}
			}
		}

		// 6. Pastikan jumlah soal aktif tepat 25.
		var activeCount int64

		if err := tx.Model(&models.BattleQuestion{}).
			Where("is_active = ?", true).
			Count(&activeCount).Error; err != nil {
			return err
		}

		if activeCount != int64(len(questions)) {
			return fmt.Errorf(
				"jumlah soal Battle aktif harus %d, ditemukan %d",
				len(questions),
				activeCount,
			)
		}

		return nil

	}); err != nil {
		log.Printf("❌ Gagal menjalankan Battle Questions V2: %v", err)
		return
	}

	log.Println("✅ Battle Questions V2 selesai (25 soal aktif)")
}

// ============================================================
// ACHIEVEMENTS (14 achievement)
// ============================================================
func SeedAchievements() {
	achievements := []models.Achievement{
		{ID: 1, Name: "Penjelajah Pemula", Description: "Selesaikan lokasi pertama di Jelajah Budaya", Icon: "🗺️", ConditionType: "LOCATION", ConditionValue: 1, RewardXP: 50, RewardStars: 1},
		{ID: 2, Name: "Sahabat Budaya", Description: "Capai Level 5", Icon: "⭐", ConditionType: "LEVEL", ConditionValue: 5, RewardXP: 100, RewardStars: 2},
		{ID: 3, Name: "Penjelajah Sejati", Description: "Selesaikan 5 lokasi di Jelajah Budaya", Icon: "🏔️", ConditionType: "LOCATION", ConditionValue: 5, RewardXP: 150, RewardStars: 3},
		{ID: 4, Name: "Master Penjelajah", Description: "Selesaikan semua 10 lokasi Jelajah Budaya", Icon: "🏆", ConditionType: "LOCATION", ConditionValue: 10, RewardXP: 300, RewardStars: 5},
		{ID: 5, Name: "Kuiz Perdana", Description: "Main Trivia untuk pertama kali", Icon: "📚", ConditionType: "TRIVIA", ConditionValue: 1, RewardXP: 30, RewardStars: 1},
		{ID: 6, Name: "Pecinta Trivia", Description: "Main Trivia sebanyak 10 kali", Icon: "📖", ConditionType: "TRIVIA", ConditionValue: 10, RewardXP: 100, RewardStars: 2},
		{ID: 7, Name: "Master Trivia", Description: "Jawab 100 soal trivia dengan benar", Icon: "🎓", ConditionType: "TRIVIA_CORRECT", ConditionValue: 100, RewardXP: 200, RewardStars: 3},
		{ID: 8, Name: "Pendekar Battle", Description: "Menangkan 1 Battle Quiz", Icon: "⚔️", ConditionType: "BATTLE_WIN", ConditionValue: 1, RewardXP: 75, RewardStars: 2},
		{ID: 9, Name: "Raja Battle", Description: "Menangkan 5 Battle Quiz", Icon: "👑", ConditionType: "BATTLE_WIN", ConditionValue: 5, RewardXP: 200, RewardStars: 4},
		{ID: 10, Name: "Kolektor Bintang", Description: "Kumpulkan 20 bintang", Icon: "⭐", ConditionType: "STARS", ConditionValue: 20, RewardXP: 100, RewardStars: 2},
		{ID: 11, Name: "Bintang Sejati", Description: "Kumpulkan 50 bintang", Icon: "🌟", ConditionType: "STARS", ConditionValue: 50, RewardXP: 250, RewardStars: 5},
		{ID: 12, Name: "Naik Level", Description: "Capai Level 5", Icon: "📈", ConditionType: "LEVEL", ConditionValue: 5, RewardXP: 100, RewardStars: 2},
		{ID: 13, Name: "Duta Budaya", Description: "Capai Level 15", Icon: "🎖️", ConditionType: "LEVEL", ConditionValue: 15, RewardXP: 300, RewardStars: 5},
		{ID: 14, Name: "Master Budaya Batak", Description: "Selesaikan semua achievement lainnya", Icon: "🏅", ConditionType: "ALL_ACHIEVEMENT", ConditionValue: 13, RewardXP: 500, RewardStars: 10},
	}

	for _, a := range achievements {
		var existing models.Achievement
		result := config.DB.Where("name = ?", a.Name).First(&existing)

		if result.Error == nil {
			err := config.DB.Model(&existing).Updates(map[string]interface{}{
				"description":     a.Description,
				"icon":            a.Icon,
				"condition_type":  a.ConditionType,
				"condition_value": a.ConditionValue,
				"reward_xp":       a.RewardXP,
				"reward_stars":    a.RewardStars,
				"is_active":       true,
			}).Error

			if err != nil {
				log.Printf("❌ Gagal update achievement %s: %v", a.Name, err)
			} else {
				log.Printf("✅ Achievement diperbarui: %s", a.Name)
			}
			continue
		}

		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek achievement %s: %v", a.Name, result.Error)
			continue
		}

		// Gunakan ID otomatis PostgreSQL, bukan ID dari seed.
		a.ID = 0

		if err := config.DB.Create(&a).Error; err != nil {
			log.Printf("❌ Gagal seed achievement %s: %v", a.Name, err)
			continue
		}

		log.Printf("✅ Achievement baru ditambahkan: %s", a.Name)
	}
	// Nonaktifkan achievement lama yang tidak digunakan lagi.
	// Record dan riwayat pemain tetap dipertahankan.
	legacyNames := []string{
		"Maestro Toba",
		"XP Pertama",
		"Pencari Ilmu",
	}

	if err := config.DB.Model(&models.Achievement{}).
		Where("name IN ?", legacyNames).
		Update("is_active", false).Error; err != nil {
		log.Printf("❌ Gagal menonaktifkan achievement lama: %v", err)
	}

	log.Println("✅ Achievements selesai disinkronkan")

}

// ============================================================
// GALLERY CARDS (20 kartu)
// ============================================================
func SeedGalleryCards() {
	cards := []models.GalleryCard{
		{ID: 1, LocationID: 1, Title: "IT Del", Image: "gallery_itdel.png", Description: "Institut Teknologi Del, kampus teknologi bergengsi di tepi Danau Toba yang didirikan oleh BJ Habibie."},
		{ID: 2, LocationID: 1, Title: "Aksara Batak", Image: "gallery_aksara.png", Description: "Sistem tulisan tradisional Batak Toba dengan 19 huruf induk yang unik dan kaya makna."},
		{ID: 3, LocationID: 2, Title: "Taman Eden 100", Image: "gallery_tamaneden.png", Description: "Kawasan hutan tropis seluas 100 hektar yang menjadi paru-paru hijau kawasan Danau Toba."},
		{ID: 4, LocationID: 2, Title: "Flora Endemik Toba", Image: "gallery_flora.png", Description: "Beragam tanaman endemik Sumatera yang langka dan dilindungi terdapat di kawasan Taman Eden 100."},
		{ID: 5, LocationID: 3, Title: "Kaldera Toba", Image: "gallery_kaldera.png", Description: "Supervolcano terbesar di dunia yang kini menjadi danau vulkanik terbesar seluas 1.130 km²."},
		{ID: 6, LocationID: 3, Title: "Pulau Samosir", Image: "gallery_samosir.png", Description: "Pulau vulkanik di tengah Danau Toba yang merupakan pulau dalam danau terbesar di dunia."},
		{ID: 7, LocationID: 4, Title: "Parapat", Image: "gallery_parapat.png", Description: "Kota wisata di tepi Danau Toba yang menjadi pintu gerbang menuju Pulau Samosir."},
		{ID: 8, LocationID: 4, Title: "Mie Gomak", Image: "gallery_miegomak.png", Description: "Mie khas Batak Toba yang tebal dan panjang, dijuluki spaghetti Batak dan menjadi kuliner ikonik Parapat."},
		{ID: 9, LocationID: 5, Title: "Pelabuhan Ajibata", Image: "gallery_ajibata.png", Description: "Pelabuhan utama yang menghubungkan Parapat dengan Pulau Samosir melalui kapal ferry Batak."},
		{ID: 10, LocationID: 5, Title: "Ferry Danau Toba", Image: "gallery_ferry.png", Description: "Kapal ferry tradisional yang menjadi transportasi utama menyeberangi Danau Toba menuju Pulau Samosir."},
		{ID: 11, LocationID: 6, Title: "Desa Tomok", Image: "gallery_tomok.png", Description: "Desa wisata di Pulau Samosir dengan pasar souvenir khas Batak dan makam Raja Sidabutar."},
		{ID: 12, LocationID: 6, Title: "Sigale-gale", Image: "gallery_sigalegale.png", Description: "Boneka kayu ajaib dari Tomok yang bisa menari mengikuti irama musik Gondang Batak Toba."},
		{ID: 13, LocationID: 7, Title: "Tuktuk Siadong", Image: "gallery_tuktuk.png", Description: "Semenanjung indah di Pulau Samosir dengan pemandangan Danau Toba yang spektakuler 360 derajat."},
		{ID: 14, LocationID: 7, Title: "Ulos Batak", Image: "gallery_ulos.png", Description: "Kain tenun tradisional khas Batak Toba yang memiliki nilai budaya dan spiritual sangat tinggi."},
		{ID: 15, LocationID: 8, Title: "Pantai Batuhoda", Image: "gallery_batuhoda.png", Description: "Pantai cantik di Pulau Samosir dengan pasir putih bersih dan air Danau Toba yang jernih."},
		{ID: 16, LocationID: 8, Title: "Gondang Sabangunan", Image: "gallery_gondang.png", Description: "Ansambel musik tradisional Batak Toba yang lengkap terdiri dari Taganing, Sarune, Ogung, dan Hesek."},
		{ID: 17, LocationID: 9, Title: "Jembatan Tanah Ponggol", Image: "gallery_ponggol.png", Description: "Jembatan ikonik yang menghubungkan daratan Kabupaten Samosir dengan Pulau Samosir."},
		{ID: 18, LocationID: 9, Title: "Adat Pernikahan Batak", Image: "gallery_pernikahan.png", Description: "Upacara adat pernikahan Batak Toba yang kaya ritual dan penuh makna filosofis mendalam."},
		{ID: 19, LocationID: 10, Title: "Patung Yesus Sibea-bea", Image: "gallery_sibeabea.png", Description: "Patung Yesus Kristus setinggi 61 meter di Sibea-bea, salah satu yang tertinggi di Asia Tenggara."},
		{ID: 20, LocationID: 10, Title: "Panorama Danau Toba", Image: "gallery_panorama.png", Description: "Pemandangan Danau Toba dari ketinggian Sibea-bea yang memukau dengan keindahan alam yang tiada tara."},
	}
	for _, c := range cards {
		var existing models.GalleryCard
		result := config.DB.Where("location_id = ? AND title = ?", c.LocationID, c.Title).First(&existing)
		if result.Error == nil {
			log.Printf("ℹ️  Gallery sudah ada: %s", c.Title)
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek gallery %s: %v", c.Title, result.Error)
			continue
		}
		if err := config.DB.Create(&c).Error; err != nil {
			log.Printf("❌ Gagal seed gallery %s: %v", c.Title, err)
			continue
		}
		log.Printf("✅ Gallery ditambahkan: %s", c.Title)
	}
	log.Println("✅ Gallery Cards selesai (20 kartu)")
}

// ============================================================
// GAME SETTINGS
// ============================================================
func SeedGameSettings() {
	settings := []models.GameSetting{
		{SettingKey: "quiz_time_limit", SettingValue: "15", Description: "Batas waktu soal quiz dalam detik"},
		{SettingKey: "quiz_question_count", SettingValue: "5", Description: "Jumlah soal per sesi quiz"},
		{SettingKey: "min_correct_to_pass", SettingValue: "3", Description: "Jumlah minimum jawaban benar untuk lulus"},
		{SettingKey: "battle_question_count", SettingValue: "5", Description: "Jumlah soal per sesi battle"},
		{SettingKey: "battle_time_per_question", SettingValue: "15", Description: "Batas waktu per soal battle dalam detik"},
		{SettingKey: "xp_per_level", SettingValue: "500", Description: "XP yang dibutuhkan untuk naik satu level"},
		{SettingKey: "matchmaking_timeout", SettingValue: "30", Description: "Waktu tunggu matchmaking sebelum vs Bot dalam detik"},
		{SettingKey: "app_version", SettingValue: "1.0.0", Description: "Versi aplikasi saat ini"},
	}
	for _, s := range settings {
		var existing models.GameSetting
		if config.DB.Where("setting_key = ?", s.SettingKey).First(&existing).Error != nil {
			config.DB.Create(&s)
		}
	}
	log.Println("✅ Game Settings selesai")
}

// ============================================================
// WEEKLY CHALLENGES (3 tantangan contoh)
// ============================================================
func SeedWeeklyChallenges() {
	now := time.Now()
	nextWeek := now.AddDate(0, 0, 7)

	challenges := []models.WeeklyChallenge{
		{
			Title:       "Cocokkan Kata Batak!",
			Description: "Pasangkan kata Bahasa Batak dengan artinya dalam Bahasa Indonesia.",
			GameType:    "MATCHING",
			RewardXP:    100,
			RewardStars: 2,
			IsActive:    true,
			StartDate:   now,
			EndDate:     nextWeek,
			DataJSON:    `{"pairs":[{"batak":"Horas","indo":"Selamat/Sehat"},{"batak":"Mauliate","indo":"Terima kasih"},{"batak":"Mangan","indo":"Makan"},{"batak":"Bagas","indo":"Rumah"},{"batak":"Tao","indo":"Danau"}]}`,
		},
		{
			Title:       "Susun Kata Batak!",
			Description: "Susun huruf-huruf acak menjadi kata Bahasa Batak yang benar.",
			GameType:    "WORD_ARRANGE",
			RewardXP:    120,
			RewardStars: 2,
			IsActive:    false,
			StartDate:   now.AddDate(0, 0, -7),
			EndDate:     now.AddDate(0, 0, -1),
			DataJSON:    `{"words":[{"scrambled":"SOLU","answer":"ULOS","hint":"Kain tenun khas Batak"},{"scrambled":"SAROH","answer":"HORAS","hint":"Salam khas Batak"},{"scrambled":"NAGMAN","answer":"MANGAN","hint":"Kegiatan makan"}]}`,
		},
		{
			Title:       "Memory Card Budaya Batak!",
			Description: "Temukan pasangan kartu budaya Batak yang cocok.",
			GameType:    "MEMORY_CARD",
			RewardXP:    150,
			RewardStars: 3,
			IsActive:    false,
			StartDate:   now.AddDate(0, 0, -14),
			EndDate:     now.AddDate(0, 0, -8),
			DataJSON:    `{"cards":[{"id":"taganing","label":"Taganing","emoji":"🥁"},{"id":"hasapi","label":"Hasapi","emoji":"🎸"},{"id":"ulos","label":"Ulos","emoji":"🧣"},{"id":"gondang","label":"Gondang","emoji":"🎵"}]}`,
		},
	}

	for _, challenge := range challenges {
		var existing models.WeeklyChallenge
		result := config.DB.Where(
			"title = ? AND game_type = ?",
			challenge.Title,
			challenge.GameType,
		).First(&existing)

		if result.Error == nil {
			// Perbaiki hanya typo scramble lama pada record yang sudah ada.
			// Data challenge lain dan tanggal/progres yang sudah tersimpan dipertahankan.
			if challenge.GameType == "WORD_ARRANGE" && strings.Contains(existing.DataJSON, "NAGNOM") {
				updatedData := strings.ReplaceAll(existing.DataJSON, "NAGNOM", "NAGMAN")
				if err := config.DB.Model(&existing).Update("data_json", updatedData).Error; err != nil {
					log.Printf("❌ Gagal memperbaiki DataJSON Weekly Challenge %s: %v", challenge.Title, err)
				} else {
					log.Printf("✅ Typo scramble Weekly Challenge diperbaiki: %s", challenge.Title)
				}
			} else {
				log.Printf("ℹ️ Weekly Challenge sudah ada, dipertahankan: %s", challenge.Title)
			}
			continue
		}

		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek Weekly Challenge %s: %v", challenge.Title, result.Error)
			continue
		}

		if err := config.DB.Create(&challenge).Error; err != nil {
			log.Printf("❌ Gagal seed Weekly Challenge %s: %v", challenge.Title, err)
			continue
		}
		log.Printf("✅ Weekly Challenge ditambahkan: %s", challenge.Title)
	}

	log.Println("✅ Weekly Challenges selesai")
}

// ============================================================
// ADMIN — sesuai field Admin struct
// ============================================================
func SeedAdmin() {
	var existing models.Admin
	result := config.DB.Where("username = ?", "admin").First(&existing)
	if result.Error == nil {
		log.Println("ℹ️  Admin sudah ada, skip")
		return
	}
	if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
		log.Printf("❌ Gagal mengecek admin: %v", result.Error)
		return
	}

	hashedPass, err := bcrypt.GenerateFromPassword([]byte("admin123"), bcrypt.DefaultCost)
	if err != nil {
		log.Printf("❌ Gagal hash password admin: %v", err)
		return
	}

	admin := models.Admin{
		Fullname:     "Admin TOBA",
		Username:     "admin",
		PasswordHash: string(hashedPass),
		Role:         "admin",
	}

	if err := config.DB.Create(&admin).Error; err != nil {
		log.Printf("❌ Gagal seed admin: %v", err)
		return
	}

	fmt.Println("✅ Admin default dibuat")
	fmt.Println("   Username : admin")
	fmt.Println("   Password : admin123")
	fmt.Println("   ⚠️  Ganti password setelah login pertama!")
}
