package database

import (
	"errors"
	"fmt"
	"log"
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
	log.Println("✅ Semua seed data berhasil!")
}

// ============================================================
// CHARACTERS
// ============================================================
func SeedCharacters() {
	var count int64
	config.DB.Model(&models.Character{}).Count(&count)
	if count > 0 {
		log.Println("ℹ️  Characters sudah ada, skip")
		return
	}
	characters := []models.Character{
		{ID: 1, Name: "Ucok", Image: "char_ucok.png", Description: "Anak laki-laki Batak yang cerdas dan pemberani"},
		{ID: 2, Name: "Butet", Image: "char_butet.png", Description: "Anak perempuan Batak yang ceria dan pintar"},
	}
	for _, c := range characters {
		config.DB.Create(&c)
	}
	log.Println("✅ Characters selesai")
}

// ============================================================
// TITLES — sesuai field Title: TitleName, MinimumLevel
// ============================================================
func SeedTitles() {
	var count int64
	config.DB.Model(&models.Title{}).Count(&count)
	if count > 0 {
		log.Println("ℹ️  Titles sudah ada, skip")
		return
	}
	titles := []models.Title{
		{ID: 1, TitleName: "Wisatawan Pemula", MinimumLevel: 1},
		{ID: 2, TitleName: "Penjelajah Budaya", MinimumLevel: 5},
		{ID: 3, TitleName: "Sahabat Budaya", MinimumLevel: 10},
		{ID: 4, TitleName: "Ahli Budaya", MinimumLevel: 15},
		{ID: 5, TitleName: "Maestro Budaya", MinimumLevel: 20},
	}
	for _, t := range titles {
		config.DB.Create(&t)
	}
	log.Println("✅ Titles selesai")
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
// LOCATION FACTS (3 fakta per lokasi = 30 total)
// ============================================================
func SeedLocationFacts() {
	facts := []models.LocationFact{
		// IT Del
		{LocationID: 1, Fact: "IT Del didirikan pada tahun 2001 atas inisiatif BJ Habibie sebagai wujud kepedulian terhadap pendidikan di kawasan Danau Toba."},
		{LocationID: 1, Fact: "Kampus IT Del terletak di Laguboti, Kabupaten Toba, tepat di tepi Danau Toba yang memiliki pemandangan spektakuler."},
		{LocationID: 1, Fact: "IT Del adalah singkatan dari Institut Teknologi Del, perguruan tinggi swasta berbasis teknologi di Sumatera Utara."},
		// Taman Eden 100
		{LocationID: 2, Fact: "Taman Eden 100 memiliki lebih dari 100 hektar hutan tropis yang dijaga kelestariannya oleh masyarakat lokal Batak."},
		{LocationID: 2, Fact: "Di Taman Eden 100 terdapat beragam flora dan fauna endemik Sumatera yang langka dan dilindungi."},
		{LocationID: 2, Fact: "Kawasan ini dikenal dengan program konservasi hutan yang melibatkan aktif masyarakat Batak setempat."},
		// Kaldera Toba
		{LocationID: 3, Fact: "Danau Toba terbentuk dari letusan supervulcano sekitar 74.000 tahun lalu, salah satu letusan terbesar dalam sejarah Bumi."},
		{LocationID: 3, Fact: "Kaldera Toba adalah kaldera terbesar di dunia dengan panjang 100 km, lebar 30 km, dan kedalaman hingga 505 meter."},
		{LocationID: 3, Fact: "Di tengah Danau Toba terdapat Pulau Samosir yang merupakan pulau vulkanik terbesar di dunia yang berada di dalam danau."},
		// Parapat
		{LocationID: 4, Fact: "Parapat berjarak sekitar 176 km dari Medan dan bisa ditempuh dalam waktu 4-5 jam perjalanan darat."},
		{LocationID: 4, Fact: "Pelabuhan Parapat adalah titik keberangkatan utama kapal ferry menuju Pulau Samosir yang beroperasi setiap hari."},
		{LocationID: 4, Fact: "Mie Gomak adalah kuliner khas Parapat yang sering disebut spaghetti-nya orang Batak karena bentuk mienya yang tebal."},
		// Ajibata
		{LocationID: 5, Fact: "Pelabuhan Ajibata adalah pelabuhan terbesar di sisi Parapat yang melayani penyeberangan ferry ke Pulau Samosir."},
		{LocationID: 5, Fact: "Dari Ajibata, perjalanan naik ferry ke Tomok di Pulau Samosir memakan waktu sekitar 30-45 menit."},
		{LocationID: 5, Fact: "Di kawasan Ajibata terdapat berbagai rumah makan yang menyajikan ikan bakar khas Danau Toba yang lezat."},
		// Tomok
		{LocationID: 6, Fact: "Tomok adalah desa pertama yang dikunjungi wisatawan saat tiba di Pulau Samosir dari Pelabuhan Ajibata."},
		{LocationID: 6, Fact: "Di Tomok terdapat makam Raja Sidabutar, raja pertama Tomok, yang diperkirakan berusia lebih dari 500 tahun."},
		{LocationID: 6, Fact: "Pertunjukan Sigale-gale di Tomok adalah atraksi boneka kayu yang bisa menari mengikuti irama musik Batak."},
		// Tuktuk
		{LocationID: 7, Fact: "Tuktuk adalah semenanjung berbentuk unik di Pulau Samosir yang dikelilingi Danau Toba dari hampir semua sisi."},
		{LocationID: 7, Fact: "Kawasan Tuktuk menjadi pusat akomodasi dan pariwisata di Pulau Samosir dengan banyak hotel dan resort tepi danau."},
		{LocationID: 7, Fact: "Dari Tuktuk wisatawan bisa menyewa sepeda motor atau sepeda untuk mengelilingi Pulau Samosir yang indah."},
		// Batuhoda
		{LocationID: 8, Fact: "Batuhoda berasal dari kata Batu dan Hoda (kuda dalam Bahasa Batak) karena ada batu berbentuk kuda di kawasan ini."},
		{LocationID: 8, Fact: "Pantai Batuhoda memiliki air danau yang sangat jernih dan dangkal sehingga sangat aman untuk berenang keluarga."},
		{LocationID: 8, Fact: "Aksara Batak Toba memiliki 19 huruf induk yang masing-masing memiliki bunyi vokal a secara default."},
		// Tanah Ponggol
		{LocationID: 9, Fact: "Jembatan Tanah Ponggol menghubungkan daratan Kabupaten Samosir dengan Pulau Samosir sepanjang sekitar 100 meter."},
		{LocationID: 9, Fact: "Melalui Tanah Ponggol wisatawan bisa mencapai Pulau Samosir dengan kendaraan tanpa harus naik kapal ferry."},
		{LocationID: 9, Fact: "Upacara adat pernikahan Batak Toba disebut pesta unjuk yang melibatkan banyak tahapan dan kedua keluarga besar."},
		// Sibea-bea
		{LocationID: 10, Fact: "Patung Yesus Kristus di Sibea-bea setinggi 61 meter menjadikannya salah satu patung Yesus tertinggi di Asia Tenggara."},
		{LocationID: 10, Fact: "Dari Sibea-bea wisatawan bisa menikmati panorama Danau Toba 360 derajat yang sangat memukau dari ketinggian."},
		{LocationID: 10, Fact: "Tulila adalah alat musik tiup tradisional Batak Toba yang terbuat dari bambu dan menghasilkan suara lembut dan merdu."},
	}
	for _, f := range facts {
		var existing models.LocationFact
		result := config.DB.Where("location_id = ? AND fact = ?", f.LocationID, f.Fact).First(&existing)
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
	log.Println("✅ Location Facts selesai (30 fakta)")
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
		// ══════════════════════════════════════════════════════
		{8, "Kata 'saksang' dalam aksara Batak adalah...", "Saksang ditulis dalam aksara Batak sebagai ᯘᯐᯘᯉᯱ.", []Opt{{"ᯔᯘᯰᯘᯰ", false}, {"ᯘᯘᯔᯰᯘ", false}, {"ᯘᯘᯘᯰ", false}, {"ᯘᯐᯘᯉᯱ", true}}},
		{8, "Kata 'pakaian' (amon-amon) dalam aksara Batak adalah...", "'Amon-amon' ditulis dalam aksara Batak sebagai ᯀᯔ᯲ᯀᯔ᯲.", []Opt{{"ᯀᯔ᯲ᯀᯔ᯲", true}, {"ᯀᯝᯀᯝ", false}, {"ᯀᯉ᯲ᯀᯉ᯲", false}, {"ᯀᯔᯰᯀᯔᯰ", false}}},
		{8, "Aksara Batak terdiri dari huruf-huruf yang disebut...", "Huruf dalam aksara Batak disebut 'surat'. Ada 19 induk surat dalam aksara Batak Toba.", []Opt{{"Aksara", false}, {"Surat", true}, {"Huruf", false}, {"Karakter", false}}},
		{8, "Tanda untuk menghilangkan bunyi vokal 'a' dalam aksara Batak disebut...", "Tanda bunuh atau pangolat digunakan untuk menghilangkan bunyi vokal a pada akhir suku kata.", []Opt{{"Pangolat", true}, {"Anak surat", false}, {"Induk surat", false}, {"Pangkat", false}}},
		{8, "Batuhoda berasal dari kata Batak Batu dan Hoda yang berarti...", "'Hoda' dalam Bahasa Batak berarti kuda. Batuhoda dinamai karena ada batu menyerupai kuda.", []Opt{{"Batu karang", false}, {"Batu dan kuda", true}, {"Batu hitam", false}, {"Batu besar", false}}},
		{8, "Berapa jumlah induk aksara (huruf utama) dalam aksara Batak Toba?", "Aksara Batak Toba memiliki 19 huruf induk yang masing-masing bervokal a secara default.", []Opt{{"15", false}, {"17", false}, {"19", true}, {"21", false}}},
		{8, "Anak surat '-i' dalam aksara Batak digunakan untuk...", "Anak surat -i dalam aksara Batak ditambahkan ke huruf induk untuk mengubah bunyi vokal menjadi i.", []Opt{{"Menambah bunyi a", false}, {"Menambah bunyi i", true}, {"Menambah bunyi u", false}, {"Menambah bunyi e", false}}},
		{8, "Kata 'indahan' (nasi) dalam aksara Batak adalah...", "Indahan ditulis dalam aksara Batak sebagai ᯤᯉᯑᯂᯉᯉᯱ.", []Opt{{"ᯤᯉᯑᯂᯉᯉᯱ", true}, {"ᯉᯑᯂᯉ", false}, {"ᯤᯉᯑᯂ", false}, {"ᯉᯑᯂᯉᯉ", false}}},
		{8, "Aksara Batak Toba termasuk dalam kategori aksara...", "Aksara Batak Toba adalah abugida, yaitu sistem tulisan yang setiap huruf mewakili konsonan dengan vokal inheren.", []Opt{{"Alfabet", false}, {"Abugida", true}, {"Abjad", false}, {"Logografi", false}}},
		{8, "Kata 'mangan' (makan) dalam aksara Batak adalah...", "Mangan ditulis dalam aksara Batak dengan kombinasi induk surat ma-nga-n.", []Opt{{"ᯔ ᯑ ᯔ", true}, {"ᯔ ᯉ ᯔ", false}, {"ᯔ ᯉ ᯑ", false}, {"ᯔ ᯑ ᯉ", false}}},
		{8, "Penulisan 'Batuhoda' dalam aksara Batak adalah...", "Batuhoda dalam aksara Batak ditulis dengan kombinasi ba-tu-ho-da menggunakan anak surat -u dan -o.", []Opt{{"ᯅ ᯗᯂᯑ", true}, {"ᯅ ᯗ ᯂ ᯑ", false}, {"ᯅᯗᯂᯑ", false}, {"ᯅ ᯗᯂ ᯑ", false}}},
		{8, "Anak surat '-u' dalam aksara Batak digunakan untuk...", "Anak surat -u ditambahkan ke huruf induk untuk mengubah bunyi vokal menjadi u.", []Opt{{"Menambah bunyi a", false}, {"Menambah bunyi i", false}, {"Menambah bunyi u", true}, {"Menambah bunyi e", false}}},
		{8, "Kata 'lappet' dalam aksara Batak adalah...", "Lappet ditulis dalam aksara Batak sebagai ᯖᯇᯇᯄ᯲.", []Opt{{"ᯂᯇᯇ᯲", false}, {"ᯂᯇᯇ᯲ᯖ", false}, {"ᯖᯇᯇᯄ᯲", true}, {"ᯂᯇᯇᯖ᯲", false}}},
		{8, "Aksara Batak ditulis dari arah...", "Aksara Batak ditulis dari kiri ke kanan, sama seperti tulisan Latin.", []Opt{{"Kanan ke kiri", false}, {"Kiri ke kanan", true}, {"Atas ke bawah", false}, {"Bawah ke atas", false}}},
		{8, "Kata 'dali' dalam aksara Batak adalah...", "Dali ditulis dalam aksara Batak sebagai ᯑᯖᯤ.", []Opt{{"ᯑᯖᯤ", true}, {"ᯑᯂᯤ", false}, {"ᯑᯖᯪ", false}, {"ᯑᯖᯮ", false}}},

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
// TRIVIA CATEGORIES
// ============================================================
func SeedTriviaCategories() {
	categories := []models.TriviaCategory{
		{ID: 1, Name: "Makanan Khas", Description: "Kenali makanan tradisional khas Batak Toba dan bahasanya", DisplayOrder: 1, IsActive: true},
		{ID: 2, Name: "Pakaian Adat", Description: "Pelajari ulos dan pakaian tradisional Batak Toba", DisplayOrder: 2, IsActive: true},
		{ID: 3, Name: "Alat Musik Tradisional", Description: "Kenali alat musik khas Batak Toba", DisplayOrder: 3, IsActive: true},
		{ID: 4, Name: "Adat Istiadat", Description: "Pelajari tradisi dan adat budaya Batak Toba", DisplayOrder: 4, IsActive: true},
		{ID: 5, Name: "Aksara Batak", Description: "Belajar membaca dan menulis aksara Batak Toba", DisplayOrder: 5, IsActive: true},
	}
	for _, c := range categories {
		var existing models.TriviaCategory
		result := config.DB.Where("id = ?", c.ID).First(&existing)
		if result.Error == nil {
			log.Printf("ℹ️  Trivia Category sudah ada: %s", c.Name)
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek trivia category %s: %v", c.Name, result.Error)
			continue
		}
		if err := config.DB.Create(&c).Error; err != nil {
			log.Printf("❌ Gagal seed trivia category %s: %v", c.Name, err)
			continue
		}
	}
	log.Println("✅ Trivia Categories selesai")
}

// ============================================================
// TRIVIA QUESTIONS (10 soal per kategori = 50 total)
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
		// ── KATEGORI 1: MAKANAN KHAS (10 soal) ───────────────
		{1, "Apa makanan Batak berbahan dasar ikan mas yang tidak dimasak dengan api?", "Naniura adalah hidangan ikan mas khas Batak Toba yang dimasak tanpa api menggunakan bumbu dan asam jeruk.", []Opt{{"Saksang", false}, {"Arsik", false}, {"Naniura", true}, {"Lappet", false}}},
		{1, "Dali ni horbo terbuat dari...", "Dali ni horbo adalah keju khas Batak Toba yang terbuat dari susu kerbau yang dipadatkan.", []Opt{{"Susu kerbau", true}, {"Susu sapi", false}, {"Kelapa", false}, {"Air jeruk", false}}},
		{1, "Makanan berbahan beras dan parutan kelapa disebut...", "Itak gurgur adalah makanan tradisional Batak Toba berbahan beras dan parutan kelapa.", []Opt{{"Itak gurgur", true}, {"Ombus-ombus", false}, {"Na tinombur", false}, {"Lappet", false}}},
		{1, "Ombus-ombus biasanya dimakan saat...", "Ombus-ombus dihidangkan saat pesta adat Batak Toba.", []Opt{{"Ujian", false}, {"Pesta adat", true}, {"Upacara kematian", false}, {"Acara resmi", false}}},
		{1, "Lappet biasanya dibungkus dengan...", "Lappet adalah kue tradisional Batak yang dibungkus dengan daun pisang.", []Opt{{"Daun singkong", false}, {"Daun kelapa", false}, {"Daun pisang", true}, {"Daun sirih", false}}},
		{1, "Makanan khas Batak berbahan daging dan darah adalah...", "Saksang adalah masakan khas Batak menggunakan daging dengan bumbu darah dan rempah khas.", []Opt{{"Naniura", false}, {"Arsik", false}, {"Saksang", true}, {"Itak gurgur", false}}},
		{1, "Apa arti dari kalimat 'Male au'?", "'Male au' berarti Saya lapar dalam Bahasa Batak.", []Opt{{"Saya kenyang", false}, {"Saya lapar", true}, {"Saya haus", false}, {"Saya mengantuk", false}}},
		{1, "Kalimat Bahasa Batak untuk 'Saya mau makan' adalah...", "'Naeng mangan au' artinya Saya mau makan.", []Opt{{"Sai ro au mangan", false}, {"Naeng mangan au", true}, {"Mangan ho do", false}, {"Au do manang", false}}},
		{1, "Apa nama mie khas Batak yang disebut 'spaghetti Batak'?", "Mie Gomak adalah mie khas Batak Toba yang tebal dan panjang seperti spaghetti.", []Opt{{"Mie Ayam", false}, {"Mie Aceh", false}, {"Mie Gomak", true}, {"Mie Saksang", false}}},
		{1, "'Butong au' dalam Bahasa Batak berarti...", "'Butong' berarti kenyang. Butong au artinya Saya kenyang.", []Opt{{"Saya lapar", false}, {"Saya haus", false}, {"Saya kenyang", true}, {"Saya tidur", false}}},

		// ── KATEGORI 2: PAKAIAN ADAT (10 soal) ───────────────
		{2, "Apa arti dari kata 'ulos'?", "Ulos adalah kain tenun tradisional khas Batak Toba yang memiliki nilai budaya dan spiritual tinggi.", []Opt{{"Topi", false}, {"Sarung", false}, {"Kain tradisional", true}, {"Sepatu", false}}},
		{2, "Kalimat Bahasa Batak untuk 'Saya memakai ulos' adalah...", "'Au Mamakke Ulos' berarti Saya memakai ulos.", []Opt{{"Au do mangulosi", false}, {"Mangulosi do ho", false}, {"Au Mamakke Ulos", true}, {"Sai mangulosi ho", false}}},
		{2, "Ulos yang dipakai saat pernikahan adat Batak disebut...", "Ulos Ragi Hotang adalah jenis ulos yang paling sering digunakan dalam upacara pernikahan adat Batak Toba.", []Opt{{"Ulos Sadum", false}, {"Ulos Ragi Hotang", true}, {"Ulos Mangiring", false}, {"Ulos Tumtuman", false}}},
		{2, "Apa fungsi utama ulos dalam upacara adat Batak?", "Ulos berfungsi sebagai simbol kasih sayang dan restu dari pemberi kepada penerima.", []Opt{{"Pelindung dari hujan", false}, {"Hiasan rumah", false}, {"Simbol kasih dan restu", true}, {"Bahan pertunjukan seni", false}}},
		{2, "Siapa yang biasanya memberikan ulos dalam acara adat (mangulosi)?", "Dalam adat Batak ulos diberikan oleh orang yang lebih tua kepada yang lebih muda.", []Opt{{"Anak kepada orang tua", false}, {"Tamu kepada tuan rumah", false}, {"Orang yang lebih tua kepada yang lebih muda", true}, {"Teman kepada sahabat", false}}},
		{2, "Ulos Ragi Hotang melambangkan...", "Ulos Ragi Hotang melambangkan persatuan dan kekuatan.", []Opt{{"Kesuburan", false}, {"Keindahan", false}, {"Persatuan dan kekuatan", true}, {"Kekayaan", false}}},
		{2, "Aksesoris kepala tradisional pria Batak disebut...", "Sortali adalah ikat kepala tradisional khas pria Batak Toba dalam berbagai upacara adat.", []Opt{{"Sortali", true}, {"Tali-tali", false}, {"Songket", false}, {"Sarung", false}}},
		{2, "Dalam struktur adat Batak, pemberi ulos disebut...", "Hula-hula adalah kelompok yang memberikan ulos kepada boru dalam adat Batak.", []Opt{{"Boru", false}, {"Hula-hula", true}, {"Dongan tubu", false}, {"Pariban", false}}},
		{2, "Mengapa ulos tidak boleh dipakai sembarangan?", "Ulos memiliki nilai sakral dan spiritual yang tinggi, penggunaannya terikat dengan konteks upacara adat.", []Opt{{"Karena mudah rusak", false}, {"Karena ulos memiliki nilai sakral dan digunakan dalam upacara adat", true}, {"Karena hanya boleh dipakai oleh raja", false}, {"Karena hanya wanita yang boleh memakainya", false}}},
		{2, "Ulos Batak dibuat menggunakan alat tradisional yang disebut...", "Sorha adalah alat tenun tradisional Batak yang digunakan untuk membuat ulos.", []Opt{{"Tenun ikat", false}, {"Gorga", false}, {"Sorha", true}, {"Giling pahat", false}}},

		// ── KATEGORI 3: ALAT MUSIK (10 soal) ─────────────────
		{3, "Taganing dimainkan dengan cara...", "Taganing dimainkan dengan dipukul stik dan berfungsi sebagai pembawa melodi dan ritme.", []Opt{{"Dipukul dengan stik", true}, {"Ditiup seperti seruling", false}, {"Dipetik seperti gitar", false}, {"Digesek seperti biola", false}}},
		{3, "Taganing terdiri dari berapa gendang?", "Taganing terdiri dari 5 gendang berukuran berbeda.", []Opt{{"3", false}, {"5", true}, {"7", false}, {"9", false}}},
		{3, "Siapa yang disebut 'dirigen' di ansambel Gondang Sabangunan?", "Penabuh Taganing berperan sebagai dirigen karena menentukan tempo dan irama musik.", []Opt{{"Peniup Sarune", false}, {"Penabuh Taganing", true}, {"Penabuh Ogung", false}, {"Penabuh Hasapi", false}}},
		{3, "Bahan utama Sarune Bolon adalah...", "Sarune Bolon adalah alat tiup tradisional Batak yang terbuat dari tanduk kerbau.", []Opt{{"Tanduk kerbau", true}, {"Bambu", false}, {"Kayu nangka", false}, {"Kulit sapi", false}}},
		{3, "Sarune Bolon memiliki lubang sebanyak...", "Sarune Bolon memiliki 4 lubang untuk mengatur nada.", []Opt{{"2", false}, {"4", true}, {"6", false}, {"8", false}}},
		{3, "Hasapi adalah alat musik seperti...", "Hasapi adalah alat musik tradisional Batak yang menyerupai gitar kecil dengan dua senar.", []Opt{{"Gitar kecil", true}, {"Drum", false}, {"Piano", false}, {"Seruling", false}}},
		{3, "Ogung terbuat dari bahan apa?", "Ogung atau gong adalah alat musik Batak yang terbuat dari logam kuningan.", []Opt{{"Logam (kuningan)", true}, {"Kayu", false}, {"Bambu", false}, {"Plastik", false}}},
		{3, "Garantung dimainkan dengan cara...", "Garantung dimainkan dengan dipukul menggunakan bilah kayu kecil dan mirip xylophone.", []Opt{{"Ditiup", false}, {"Dipetik", false}, {"Dipukul dengan bilah kayu", true}, {"Digesek", false}}},
		{3, "Cara memainkan Tulila adalah dengan...", "Tulila adalah alat musik tiup tradisional Batak Toba yang ditiup.", []Opt{{"Digesek", false}, {"Dipukul", false}, {"Dipetik", false}, {"Ditiup", true}}},
		{3, "Hesek terbuat dari bahan...", "Hesek adalah alat musik sederhana Batak Toba yang terbuat dari botol dan logam kecil.", []Opt{{"Botol dan logam kecil", true}, {"Kayu dan tali", false}, {"Plastik dan kertas", false}, {"Daun dan bambu", false}}},

		// ── KATEGORI 4: ADAT ISTIADAT (10 soal) ──────────────
		{4, "Apa arti dari 'Dalihan Na Tolu' dalam adat Batak?", "Dalihan Na Tolu berarti tiga tungku yang melambangkan tiga pilar relasi sosial: hula-hula, dongan tubu, dan boru.", []Opt{{"Tiga macam ulos", false}, {"Tiga jenis makanan", false}, {"Tiga pilar relasi sosial", true}, {"Tiga generasi marga", false}}},
		{4, "Siapakah yang disebut 'hula-hula' dalam adat Batak?", "Hula-hula adalah keluarga dari pihak istri yang sangat dihormati dalam adat Batak.", []Opt{{"Saudara dari pihak ibu", false}, {"Saudara ayah", false}, {"Keluarga dari istri", true}, {"Anak laki-laki", false}}},
		{4, "Istilah 'martumpol' berarti...", "Martumpol adalah upacara pertunangan secara adat Batak sebelum pernikahan.", []Opt{{"Pesta adat", false}, {"Lamaran secara adat Batak", true}, {"Upacara kematian", false}, {"Penabalan marga", false}}},
		{4, "Apa yang dimaksud dengan 'marhata sinamot'?", "Marhata sinamot adalah proses pembicaraan mas kawin antara keluarga kedua pengantin.", []Opt{{"Penabalan gelar", false}, {"Pembicaraan tentang mas kawin", true}, {"Penaburan bunga", false}, {"Pembagian ulos", false}}},
		{4, "Apa nama tarian tradisional dalam upacara adat Batak?", "Tor-tor adalah tarian tradisional khas Batak Toba yang ditampilkan dalam berbagai upacara adat.", []Opt{{"Tor-tor", true}, {"Poco-poco", false}, {"Saman", false}, {"Serimpi", false}}},
		{4, "Upacara adat menggali tulang leluhur dalam Batak disebut...", "Mangokkal holi adalah tradisi menggali kembali tulang belulang leluhur untuk dipindahkan.", []Opt{{"Manortor", false}, {"Mangulosi", false}, {"Mangokkal holi", true}, {"Marunjuk", false}}},
		{4, "Dalam adat Batak, 'dongan tubu' berarti...", "Dongan tubu berarti teman satu marga atau orang yang lahir dari garis keturunan yang sama.", []Opt{{"Teman kerja", false}, {"Teman sekampung", false}, {"Satu marga dari garis keturunan yang sama", true}, {"Tetangga dekat", false}}},
		{4, "Dalam pemberian ulos, posisi tangan menunjukkan...", "Posisi tangan dalam pemberian ulos menunjukkan hierarki sosial pemberi dan penerima.", []Opt{{"Keberanian", false}, {"Hierarki sosial", true}, {"Jumlah anggota marga", false}, {"Besar cinta kasih", false}}},
		{4, "Dalam struktur adat Batak, siapakah 'anak boru'?", "Anak boru atau boru adalah keluarga dari pihak istri yang berperan melayani dalam upacara adat.", []Opt{{"Keluarga dari suami", false}, {"Keluarga dari istri", true}, {"Saudara laki-laki", false}, {"Penari adat", false}}},
		{4, "Ulos sering digunakan pada saat...", "Ulos digunakan dalam upacara kelahiran, pernikahan, dan kematian dalam adat Batak.", []Opt{{"Bekerja di ladang", false}, {"Makan malam bersama", false}, {"Upacara kelahiran, pernikahan, dan kematian", true}, {"Festival musik", false}}},

		// ── KATEGORI 5: AKSARA BATAK (10 soal) ───────────────
		{5, "ᯤᯉᯑᯂᯉᯉᯱ dalam aksara Batak dibaca...", "ᯤᯉᯑᯂᯉᯉᯱ dibaca 'indahan' yang berarti nasi.", []Opt{{"Indahan", true}, {"Nasi", false}, {"Inang", false}, {"Indadang", false}}},
		{5, "ᯘᯐᯘᯉᯱ adalah penulisan aksara Batak untuk kata...", "ᯘᯐᯘᯉᯱ adalah kata saksang dalam aksara Batak.", []Opt{{"Saksang", true}, {"Sarang", false}, {"Saring", false}, {"Sanggar", false}}},
		{5, "Kata 'ulos' dalam aksara Batak adalah...", "Ulos dalam aksara Batak ditulis sebagai ᯗᯞᯮ.", []Opt{{"ᯖᯮᯞᯮ", false}, {"ᯎᯞᯬ", false}, {"ᯗᯞᯮ", true}, {"ᯂᯞᯬ", false}}},
		{5, "Kata 'mangulosi' dalam aksara Batak adalah...", "Mangulosi dalam aksara Batak ditulis sebagai ᯔᯉᯮᯞᯬᯘᯪ.", []Opt{{"ᯔᯉᯮᯞᯬᯘᯪ", true}, {"ᯔᯝᯮᯂᯬᯘᯪ", false}, {"ᯔᯝᯂᯬᯘ", false}, {"ᯔᯝᯞᯮᯘᯪ", false}}},
		{5, "Kata 'sortali' dalam aksara Batak ditulis sebagai...", "Sortali dalam aksara Batak ditulis sebagai ᯘᯬᯒᯖᯞᯪ.", []Opt{{"ᯘᯬᯒᯖᯞᯪ", true}, {"ᯘᯮᯒᯖᯂᯪ", false}, {"ᯘᯬᯒᯂᯪ", false}, {"ᯘᯬᯒᯖᯞ", false}}},
		{5, "Aksara Batak untuk kata 'parboru' adalah...", "Parboru dalam aksara Batak ditulis sebagai ᯇᯒᯅᯮᯒᯮ.", []Opt{{"ᯇᯒᯅᯮᯒᯮ", true}, {"ᯇᯒᯅᯮᯂᯮ", false}, {"ᯇᯒᯅᯂᯮᯮ", false}, {"ᯇᯒᯅᯜᯮ", false}}},
		{5, "Kata 'martumpol' dalam aksara Batak adalah...", "Martumpol dalam aksara Batak ditulis sebagai ᯔᯒᯖᯮᯔᯇᯞ᯲.", []Opt{{"ᯔᯒᯖᯮᯔᯇᯞ᯲", true}, {"ᯔᯒᯖᯮᯇᯞ᯲", false}, {"ᯔᯒᯖᯮᯔᯂᯞ᯲", false}, {"ᯔᯒᯖᯮᯇᯂᯞ᯲", false}}},
		{5, "ᯖᯒᯖᯒ᯲ dalam aksara Batak membentuk kata...", "ᯖᯒᯖᯒ᯲ adalah penulisan kata tortor yaitu tarian tradisional Batak.", []Opt{{"Tortor", true}, {"Tari", false}, {"Tarom", false}, {"Torap", false}}},
		{5, "Kata 'lappet' dalam aksara Batak adalah...", "Lappet dalam aksara Batak ditulis sebagai ᯖᯇᯇᯄ᯲.", []Opt{{"ᯂᯇᯇ᯲", false}, {"ᯂᯇᯇ᯲ᯖ", false}, {"ᯖᯇᯇᯄ᯲", true}, {"ᯂᯇᯇᯖ᯲", false}}},
		{5, "ᯎᯑᯂᯂ᯲ adalah penulisan aksara Batak untuk kata...", "ᯎᯑᯂᯂ᯲ adalah kata gomak dalam aksara Batak.", []Opt{{"Gomak", true}, {"Gomar", false}, {"Gomuk", false}, {"Gamok", false}}},
	}

	for _, q := range questions {
		var existing models.TriviaQuestion
		result := config.DB.Where("category_id = ? AND question = ?", q.CatID, q.Question).First(&existing)
		if result.Error == nil {
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek soal trivia: %v", result.Error)
			continue
		}

		newQ := models.TriviaQuestion{
			CategoryID:  q.CatID,
			Question:    q.Question,
			Explanation: q.Explanation,
			IsActive:    true,
		}
		if err := config.DB.Create(&newQ).Error; err != nil {
			log.Printf("❌ Gagal seed trivia: %v", err)
			continue
		}
		for _, opt := range q.Opts {
			config.DB.Create(&models.TriviaQuestionOption{
				QuestionID: newQ.ID,
				OptionText: opt.Text,
				IsCorrect:  opt.Correct,
			})
		}
	}
	log.Println("✅ Trivia Questions selesai (50 soal)")
}

// ============================================================
// BATTLE QUESTIONS (15 soal)
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
		{"Apa arti kata 'horas' dalam Bahasa Batak?", "'Horas' adalah salam khas Batak Toba yang berarti selamat, sehat, dan sejahtera.", []Opt{{"Selamat/Sehat", true}, {"Terima kasih", false}, {"Sampai jumpa", false}, {"Maaf", false}}},
		{"Danau Toba terletak di provinsi...", "Danau Toba adalah danau vulkanik terbesar di dunia yang terletak di Sumatera Utara.", []Opt{{"Sumatera Barat", false}, {"Sumatera Utara", true}, {"Riau", false}, {"Aceh", false}}},
		{"'Mauliate' dalam Bahasa Batak berarti...", "'Mauliate' adalah ungkapan terima kasih dalam Bahasa Batak Toba.", []Opt{{"Maaf", false}, {"Halo", false}, {"Terima kasih", true}, {"Selamat datang", false}}},
		{"Apa nama sistem kekerabatan khas Batak yang terdiri dari tiga unsur?", "Dalihan Na Tolu adalah sistem kekerabatan Batak yang terdiri dari hula-hula, dongan tubu, dan boru.", []Opt{{"Dalihan Na Tolu", true}, {"Gorga Batak", false}, {"Adat Sinamot", false}, {"Marga Tiga", false}}},
		{"Pulau yang berada di tengah Danau Toba disebut...", "Pulau Samosir adalah pulau vulkanik yang berada di tengah Danau Toba.", []Opt{{"Pulau Nias", false}, {"Pulau Samosir", true}, {"Pulau Berhala", false}, {"Pulau Sibolga", false}}},
		{"Apa yang dimaksud dengan 'marga' dalam adat Batak?", "Marga adalah nama klan atau kelompok kekerabatan patrilineal dalam masyarakat Batak.", []Opt{{"Nama desa", false}, {"Nama klan/garis keturunan", true}, {"Nama gelar bangsawan", false}, {"Nama upacara adat", false}}},
		{"Alat musik Batak yang terdiri dari 5 gendang adalah...", "Taganing adalah gendang khas Batak Toba yang terdiri dari 5 buah gendang berbeda ukuran.", []Opt{{"Gondang", false}, {"Taganing", true}, {"Ogung", false}, {"Sarune", false}}},
		{"Arsik adalah masakan Batak berbahan dasar...", "Arsik adalah masakan khas Batak Toba yang berbahan dasar ikan mas yang dimasak dengan bumbu kuning.", []Opt{{"Daging kerbau", false}, {"Ikan mas", true}, {"Ayam kampung", false}, {"Daging babi", false}}},
		{"Rumah adat Batak Toba disebut...", "Jabu atau Ruma Adat adalah rumah tradisional Batak Toba yang memiliki atap melengkung.", []Opt{{"Joglo", false}, {"Gadang", false}, {"Jabu/Ruma Batak", true}, {"Honai", false}}},
		{"Apa arti kata 'au' dalam Bahasa Batak?", "'Au' dalam Bahasa Batak berarti saya atau aku.", []Opt{{"Kamu", false}, {"Kita", false}, {"Saya/Aku", true}, {"Mereka", false}}},
		{"Ornamen ukiran khas yang menghiasi rumah adat Batak disebut...", "Gorga adalah ornamen atau ukiran tradisional khas Batak Toba yang menghiasi rumah adat.", []Opt{{"Batik", false}, {"Gorga", true}, {"Tenun", false}, {"Songket", false}}},
		{"Kata 'inang' dalam Bahasa Batak berarti...", "'Inang' dalam Bahasa Batak berarti ibu.", []Opt{{"Ayah", false}, {"Kakek", false}, {"Ibu", true}, {"Nenek", false}}},
		{"Apa nama festival budaya Batak yang dirayakan di Danau Toba?", "Pesta Danau Toba adalah festival budaya tahunan yang merayakan kekayaan budaya Batak Toba.", []Opt{{"Pesta Danau Toba", true}, {"Festival Nias", false}, {"Karnaval Batak", false}, {"Pekan Budaya Melayu", false}}},
		{"'Amang' dalam Bahasa Batak berarti...", "'Amang' dalam Bahasa Batak berarti ayah.", []Opt{{"Ibu", false}, {"Ayah", true}, {"Kakak", false}, {"Paman", false}}},
		{"Alat musik modern yang mirip dengan Garantung Batak adalah...", "Garantung mirip xylophone karena terdiri dari bilah kayu yang dipukul untuk menghasilkan nada.", []Opt{{"Drum", false}, {"Xylophone", true}, {"Gitar", false}, {"Piano", false}}},
	}

	for _, q := range questions {
		var existing models.BattleQuestion
		result := config.DB.Where("question = ?", q.Question).First(&existing)
		if result.Error == nil {
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek soal battle: %v", result.Error)
			continue
		}

		newQ := models.BattleQuestion{
			Question:    q.Question,
			Explanation: q.Explanation,
			IsActive:    true,
		}
		if err := config.DB.Create(&newQ).Error; err != nil {
			log.Printf("❌ Gagal seed battle: %v", err)
			continue
		}
		for _, opt := range q.Opts {
			config.DB.Create(&models.BattleQuestionOption{
				QuestionID: newQ.ID,
				OptionText: opt.Text,
				IsCorrect:  opt.Correct,
			})
		}
	}
	log.Println("✅ Battle Questions selesai (15 soal)")
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
			log.Printf("ℹ️  Achievement sudah ada: %s", a.Name)
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek achievement %s: %v", a.Name, result.Error)
			continue
		}
		if err := config.DB.Create(&a).Error; err != nil {
			log.Printf("❌ Gagal seed achievement %s: %v", a.Name, err)
			continue
		}
	}
	log.Println("✅ Achievements selesai (14 achievement)")
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
			DataJSON:    `{"words":[{"scrambled":"SOLU","answer":"ULOS","hint":"Kain tenun khas Batak"},{"scrambled":"SAROH","answer":"HORAS","hint":"Salam khas Batak"},{"scrambled":"NAGNOM","answer":"MANGAN","hint":"Kegiatan makan"}]}`,
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

	for _, c := range challenges {
		var existing models.WeeklyChallenge
		result := config.DB.Where("title = ? AND game_type = ?", c.Title, c.GameType).First(&existing)
		if result.Error == nil {
			log.Printf("ℹ️  Weekly Challenge sudah ada: %s", c.Title)
			continue
		}
		if !errors.Is(result.Error, gorm.ErrRecordNotFound) {
			log.Printf("❌ Gagal mengecek weekly challenge %s: %v", c.Title, result.Error)
			continue
		}
		if err := config.DB.Create(&c).Error; err != nil {
			log.Printf("❌ Gagal seed weekly challenge %s: %v", c.Title, err)
			continue
		}
	}
	log.Println("✅ Weekly Challenges selesai (3 contoh)")
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
