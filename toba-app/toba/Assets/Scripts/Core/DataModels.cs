using System;
using System.Collections.Generic;

// ============================================================
// AUTH
// ============================================================
[Serializable]
public class RegisterRequest
{
    public string name;
    public string pin;
    public int character_id;
    public string device_id;
}

[Serializable]
public class LoginRequest
{
    public string username;
    public string pin;
    public string device_id;
}

[Serializable]
public class AuthResponse
{
    public string message;
    public string token;
    public int id;
    public string user_code;
    public string username;
    public string name;
    public int level;
    public int xp;
    public int stars;
    public string title_name;
    public PlayerData data; // fallback nested

    public int GetValidID() {
        if (id != 0) return id;
        if (data != null && data.id != 0) return data.id;
        return 0;
    }
    public string GetValidName() {
        if (!string.IsNullOrEmpty(name)) return name;
        if (data != null && !string.IsNullOrEmpty(data.name)) return data.name;
        return "Pemain";
    }
    public string GetValidUsername() {
        if (!string.IsNullOrEmpty(username)) return username;
        if (data != null && !string.IsNullOrEmpty(data.username)) return data.username;
        return "user_" + GetValidID();
    }
}

[Serializable]
public class LoginErrorResponse
{
    public string message;
}

[Serializable]
public class PlayerData
{
    public int id;
    public string user_code;
    public string username;
    public string name;
    public int level;
    public int xp;
    public int stars;
    public string title_name;
    public int character_id;
}

// ============================================================
// PROFILE
// ============================================================
[Serializable]
public class ProfileResponse
{
    public int id;
    public string user_code;
    public string username;
    public string name;
    public int level;
    public int xp;
    public int stars;
    public int character_id;
    public string title_name;
    public int location_done;
    public int achievement_count;
    public int gallery_count;
    public int xp_for_next;
    public int xp_progress;
}

// ============================================================
// LOCATION & QUIZ
// ============================================================
[Serializable]
public class LocationData
{
    public int id;
    public string name;
    public string description;
    public string image;
    public int reward_xp;
    public int reward_stars;
    public int question_count;
    public int minimum_correct;
    public int level_order;
    public bool is_active;
}

[Serializable]
public class LocationListResponse
{
    public List<LocationData> locations;
}

[Serializable]
public class LocationFactData
{
    public int id;
    public string fact;
}

[Serializable]
public class LocationFactResponse
{
    public LocationFactData fact;
}

[Serializable]
public class QuizOptionData
{
    public int id;
    public string option_text;
    public bool is_correct;
}

[Serializable]
public class QuizQuestionData
{
    public int id;
    public string question;
    public string explanation;
    public List<QuizOptionData> options;
}

[Serializable]
public class QuizResponse
{
    public LocationData location;
    public List<QuizQuestionData> questions;
}

[Serializable]
public class SubmitQuizRequest
{
    public int player_id;
    public int location_id;
    public int correct_count;
}

[Serializable]
public class SubmitQuizResponse
{
    public int location_id;
    public int correct_count;
    public int total_questions;
    public int minimum_correct;

    public bool passed;

    public int reward_xp;
    public int reward_stars;

    public bool is_new_pass;

    public int level;
    public int total_xp;
    public int total_stars;
}

// ============================================================
// PROGRESS
// ============================================================
[Serializable]
public class PlayerProgressData
{
    public int location_id;
    public bool is_unlocked;
    public bool is_completed;
    public int stars_earned;
}

[Serializable]
public class PlayerProgressResponse
{
    public List<PlayerProgressData> progress;
}

// ============================================================
// TRIVIA
// ============================================================
[Serializable]
public class TriviaCategoryData
{
    public int id;
    public string name;
    public string description;
    public int question_count;
    public int display_order;
}

[Serializable]
public class TriviaCategoryListResponse
{
    public List<TriviaCategoryData> categories;
}

[Serializable]
public class TriviaQuizResponse
{
    public TriviaCategoryData category;
    public List<QuizQuestionData> questions;
}

[Serializable]
public class SubmitTriviaRequest
{
    public int player_id;
    public int category_id;
    public int correct_count;
    public int wrong_count;
}

[Serializable]
public class SubmitTriviaResponse
{
    public int xp;
    public int level;
    public int total_xp;
    public int total_stars;
}

// ============================================================
// BATTLE
// ============================================================
[Serializable]
public class CreateRoomRequest   { public int player_id; }

[Serializable]
public class CreateRoomResponse
{
    public int room_id;
    public string room_code;
    public string status;
}

[Serializable]
public class JoinRoomRequest
{
    public int player_id;
    public string room_code;
}

[Serializable]
public class BattleRoomStatusResponse
{
    public int room_id;
    public string room_code;
    public string status;
    public int host_player_id;
    public int guest_player_id;
    public int winner_id;
    public string host_name;
    public string guest_name;
    public List<QuizQuestionData> questions;
}

[Serializable]
public class SubmitBattleAnswerRequest
{
    public int room_id;
    public int player_id;
    public int question_id;
    public int selected_option_id;
    public long answer_time_ms; 
}

[Serializable]
public class FinishBattleRequest  { public int room_id; public int player_id; }

[Serializable]
public class FinishBattleResponse
{
    public string status;
    public int host_score;
    public int guest_score;
    public int winner_id;
}

[Serializable]
public class DisconnectBattleRequest { public int room_id; public int player_id; }

// ============================================================
// GALLERY
// ============================================================
[Serializable]
public class GalleryCardData
{
    public int id;
    public string title;
    public string image;
    public string description;
    public int location_id;
}

[Serializable]
public class PlayerGalleryCardData
{
    public int id;
    public int gallery_card_id;
    public string obtained_at;
    public GalleryCardData card;
}

[Serializable]
public class GalleryResponse
{
    public int total;
    public List<PlayerGalleryCardData> cards;
}

// ============================================================
// ACHIEVEMENT
// ============================================================
[Serializable]
public class AchievementData
{
    public int id;
    public string name;
    public string description;
    public string icon;
    public int reward_xp;
    public int reward_stars;
    public string condition_type;
    public int condition_value;
}

[Serializable]
public class PlayerAchievementData
{
    public int id;
    public int achievement_id;
    public bool claimed;
    public string obtained_at;
    public AchievementData achievement;
}

[Serializable]
public class AchievementResponse
{
    public int total;
    public List<PlayerAchievementData> achievements;
}

[Serializable]
public class ClaimAchievementRequest
{
    public int player_id;
    public int achievement_id;
}

[Serializable]
public class ClaimAchievementResponse
{
    public string message;
    public int reward_xp;
    public int reward_stars;
    public int level;
    public int total_xp;
    public int total_stars;
}

[Serializable]
public class NewAchievementResponse
{
    public List<AchievementData> newly_unlocked;
    public int count;
}

// ============================================================
// LEADERBOARD
// ============================================================
[Serializable]
public class LeaderboardEntryData
{
    public int rank;
    public int id;
    public string name;
    public string username;
    public int level;
    public int stars;
    public int xp;
    public string title_name;
    public int character_id;
}

[Serializable]
public class LeaderboardListResponse
{
    public List<LeaderboardEntryData> entries;
}

// ── FIREBASE ──────────────────────────────────────────────
[System.Serializable]
public class FCMTokenRequest
{
    public int    player_id;
    public string fcm_token;
}


// ── MATCHMAKING ───────────────────────────────────────────
[System.Serializable]
public class MatchmakeRequest
{
    public int    player_id;
    public int    level;
    public string player_name;
}

[System.Serializable]
public class MatchmakeResponse
{
    public string status;
    public string room_code;
    public int    room_id;
    public string opponent_name;
    public bool   is_host;
    public bool   is_bot;        // ← TAMBAH
    public string bot_name;      // ← TAMBAH
    public int    bot_level;     // ← TAMBAH
    public int    waited;        // ← TAMBAH (detik sudah menunggu)
    public int    bot_in;        // ← TAMBAH (detik lagi vs bot)
    public string message;
}

// ── GAME SETTINGS ─────────────────────────────────────────
[System.Serializable]
public class GameSettingData
{
    public int    id;
    public string setting_key;
    public string setting_value;
    public string description;
}


[System.Serializable]
public class ChallengeData
{
    public int    id;
    public string title;
    public string description;
    public string game_type;
    public int    reward_xp;
    public int    reward_stars;
    public string data_json;
}

[System.Serializable]
public class ActiveChallengeResponse
{
    public bool          has_challenge;
    public bool          is_completed;
    public ChallengeData challenge;
}

[System.Serializable]
public class SubmitChallengeRequest
{
    public int player_id;
    public int challenge_id;
    public int score;
}

[System.Serializable]
public class MatchPair { public string batak; public string indo; }
[System.Serializable]   
public class MatchingData { public List<MatchPair> pairs; }

[System.Serializable]
public class VersionResponse
{
    public string latest_version;
    public string min_version;
    public bool   force_update;
    public string update_message;
}