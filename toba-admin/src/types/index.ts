export interface AdminStats {
  total_players: number;
  total_battle:  number;
  total_trivia:  number;
  total_location: number;
}

export interface Player {
  id: number; user_code: string; username: string; name: string;
  level: number; xp: number; stars: number; is_active: boolean; created_at: string;
}

export interface Location {
  id: number; name: string; description: string; image: string;
  reward_xp: number; reward_stars: number; question_count: number;
  minimum_correct: number; level_order: number; is_active: boolean; created_at: string;
}

export interface LocationFact {
  id: number; location_id: number; fact: string;
}

export interface QuestionOption {
  id?: number; option_text: string; is_correct: boolean;
}

export interface LocationQuestion {
  id: number; location_id: number; question: string;
  explanation: string; is_active: boolean; options: QuestionOption[];
}

export interface TriviaCategory {
  id: number; name: string; description: string;
  question_count: number; display_order: number; is_active: boolean;
}

export interface TriviaQuestion {
  id: number; category_id: number; question: string;
  explanation: string; is_active: boolean; options: QuestionOption[];
}

export interface BattleQuestion {
  id: number; question: string; explanation: string;
  is_active: boolean; options: QuestionOption[];
}

export interface Achievement {
  id: number; name: string; description: string; icon: string;
  condition_type: string; condition_value: number;
  reward_xp: number; reward_stars: number; is_active: boolean;
}

export interface GameSetting {
  id: number; setting_key: string; setting_value: string; description: string;
}

export interface MusicFile {
  filename: string; url: string; size: number; type: string;
}

export interface GalleryCard {
  id: number; location_id: number; title: string; image: string; description: string;
}