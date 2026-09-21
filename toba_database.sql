--
-- PostgreSQL database dump
--

\restrict LiSsp7Es4j0KE7rG7apzRmfg2eC4TtfvAWEYcHJfUG5qDDUc8lSyashbyo3gjeu

-- Dumped from database version 16.10
-- Dumped by pg_dump version 16.10

-- Started on 2026-09-16 14:41:35

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 903 (class 1247 OID 69341)
-- Name: admin_role; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.admin_role AS ENUM (
    'SUPER_ADMIN',
    'ADMIN'
);


ALTER TYPE public.admin_role OWNER TO postgres;

--
-- TOC entry 909 (class 1247 OID 69360)
-- Name: battle_status; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.battle_status AS ENUM (
    'WAITING',
    'PLAYING',
    'FINISHED',
    'EXPIRED'
);


ALTER TYPE public.battle_status OWNER TO postgres;

--
-- TOC entry 906 (class 1247 OID 69346)
-- Name: condition_type; Type: TYPE; Schema: public; Owner: postgres
--

CREATE TYPE public.condition_type AS ENUM (
    'LEVEL',
    'STAR',
    'XP',
    'LOCATION',
    'TRIVIA',
    'BATTLE'
);


ALTER TYPE public.condition_type OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 234 (class 1259 OID 69528)
-- Name: achievements; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.achievements (
    id integer NOT NULL,
    name text,
    description text,
    icon text,
    condition_type text,
    condition_value bigint,
    reward_xp bigint DEFAULT 0,
    reward_stars bigint DEFAULT 0,
    is_active boolean,
    created_by_admin_id integer,
    updated_by_admin_id integer,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.achievements OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 69527)
-- Name: achievements_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.achievements_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.achievements_id_seq OWNER TO postgres;

--
-- TOC entry 5361 (class 0 OID 0)
-- Dependencies: 233
-- Name: achievements_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.achievements_id_seq OWNED BY public.achievements.id;


--
-- TOC entry 226 (class 1259 OID 69451)
-- Name: admin_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.admin_logs (
    id bigint NOT NULL,
    admin_id integer NOT NULL,
    activity character varying(255) NOT NULL,
    table_name character varying(100) NOT NULL,
    record_id bigint,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.admin_logs OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 69450)
-- Name: admin_logs_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.admin_logs_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.admin_logs_id_seq OWNER TO postgres;

--
-- TOC entry 5362 (class 0 OID 0)
-- Dependencies: 225
-- Name: admin_logs_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.admin_logs_id_seq OWNED BY public.admin_logs.id;


--
-- TOC entry 274 (class 1259 OID 70659)
-- Name: admin_sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.admin_sessions (
    id bigint NOT NULL,
    admin_id bigint,
    token text,
    is_active boolean,
    last_login timestamp with time zone,
    created_at timestamp with time zone
);


ALTER TABLE public.admin_sessions OWNER TO postgres;

--
-- TOC entry 273 (class 1259 OID 70658)
-- Name: admin_sessions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.admin_sessions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.admin_sessions_id_seq OWNER TO postgres;

--
-- TOC entry 5363 (class 0 OID 0)
-- Dependencies: 273
-- Name: admin_sessions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.admin_sessions_id_seq OWNED BY public.admin_sessions.id;


--
-- TOC entry 220 (class 1259 OID 69395)
-- Name: admins; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.admins (
    id integer NOT NULL,
    fullname text,
    username text,
    password_hash text,
    role text DEFAULT 'ADMIN'::public.admin_role,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.admins OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 69394)
-- Name: admins_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.admins_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.admins_id_seq OWNER TO postgres;

--
-- TOC entry 5364 (class 0 OID 0)
-- Dependencies: 219
-- Name: admins_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.admins_id_seq OWNED BY public.admins.id;


--
-- TOC entry 256 (class 1259 OID 69764)
-- Name: battle_answers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battle_answers (
    id bigint NOT NULL,
    room_id bigint NOT NULL,
    player_id bigint,
    question_id bigint,
    selected_option_id bigint,
    is_correct boolean,
    answered_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.battle_answers OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 69763)
-- Name: battle_answers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.battle_answers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battle_answers_id_seq OWNER TO postgres;

--
-- TOC entry 5365 (class 0 OID 0)
-- Dependencies: 255
-- Name: battle_answers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.battle_answers_id_seq OWNED BY public.battle_answers.id;


--
-- TOC entry 252 (class 1259 OID 69725)
-- Name: battle_question_options; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battle_question_options (
    id integer NOT NULL,
    question_id bigint,
    option_text text,
    is_correct boolean
);


ALTER TABLE public.battle_question_options OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 69724)
-- Name: battle_question_options_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.battle_question_options_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battle_question_options_id_seq OWNER TO postgres;

--
-- TOC entry 5366 (class 0 OID 0)
-- Dependencies: 251
-- Name: battle_question_options_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.battle_question_options_id_seq OWNED BY public.battle_question_options.id;


--
-- TOC entry 250 (class 1259 OID 69703)
-- Name: battle_questions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battle_questions (
    id integer NOT NULL,
    question text NOT NULL,
    explanation text NOT NULL,
    is_active boolean,
    created_by_admin_id integer,
    updated_by_admin_id integer,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.battle_questions OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 69702)
-- Name: battle_questions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.battle_questions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battle_questions_id_seq OWNER TO postgres;

--
-- TOC entry 5367 (class 0 OID 0)
-- Dependencies: 249
-- Name: battle_questions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.battle_questions_id_seq OWNED BY public.battle_questions.id;


--
-- TOC entry 268 (class 1259 OID 69905)
-- Name: battle_room_questions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battle_room_questions (
    id bigint NOT NULL,
    room_id bigint NOT NULL,
    question_id bigint,
    question_order bigint
);


ALTER TABLE public.battle_room_questions OWNER TO postgres;

--
-- TOC entry 267 (class 1259 OID 69904)
-- Name: battle_room_questions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.battle_room_questions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battle_room_questions_id_seq OWNER TO postgres;

--
-- TOC entry 5368 (class 0 OID 0)
-- Dependencies: 267
-- Name: battle_room_questions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.battle_room_questions_id_seq OWNED BY public.battle_room_questions.id;


--
-- TOC entry 254 (class 1259 OID 69738)
-- Name: battle_rooms; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.battle_rooms (
    id bigint NOT NULL,
    room_code text,
    host_player_id bigint,
    guest_player_id bigint,
    winner_player_id bigint,
    status text DEFAULT 'WAITING'::public.battle_status,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    started_at timestamp without time zone,
    finished_at timestamp without time zone,
    expired_at timestamp without time zone
);


ALTER TABLE public.battle_rooms OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 69737)
-- Name: battle_rooms_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.battle_rooms_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.battle_rooms_id_seq OWNER TO postgres;

--
-- TOC entry 5369 (class 0 OID 0)
-- Dependencies: 253
-- Name: battle_rooms_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.battle_rooms_id_seq OWNED BY public.battle_rooms.id;


--
-- TOC entry 216 (class 1259 OID 69370)
-- Name: characters; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.characters (
    id integer NOT NULL,
    name text,
    image text,
    description text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.characters OWNER TO postgres;

--
-- TOC entry 215 (class 1259 OID 69369)
-- Name: characters_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.characters_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.characters_id_seq OWNER TO postgres;

--
-- TOC entry 5370 (class 0 OID 0)
-- Dependencies: 215
-- Name: characters_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.characters_id_seq OWNED BY public.characters.id;


--
-- TOC entry 232 (class 1259 OID 69509)
-- Name: gallery_cards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.gallery_cards (
    id integer NOT NULL,
    location_id bigint,
    title text,
    image text,
    description text,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.gallery_cards OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 69508)
-- Name: gallery_cards_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.gallery_cards_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.gallery_cards_id_seq OWNER TO postgres;

--
-- TOC entry 5371 (class 0 OID 0)
-- Dependencies: 231
-- Name: gallery_cards_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.gallery_cards_id_seq OWNED BY public.gallery_cards.id;


--
-- TOC entry 238 (class 1259 OID 69582)
-- Name: game_settings; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.game_settings (
    id integer NOT NULL,
    setting_key text,
    setting_value text,
    description text,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.game_settings OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 69581)
-- Name: game_settings_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.game_settings_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.game_settings_id_seq OWNER TO postgres;

--
-- TOC entry 5372 (class 0 OID 0)
-- Dependencies: 237
-- Name: game_settings_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.game_settings_id_seq OWNED BY public.game_settings.id;


--
-- TOC entry 230 (class 1259 OID 69494)
-- Name: location_facts; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.location_facts (
    id integer NOT NULL,
    location_id bigint,
    fact text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.location_facts OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 69493)
-- Name: location_facts_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.location_facts_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.location_facts_id_seq OWNER TO postgres;

--
-- TOC entry 5373 (class 0 OID 0)
-- Dependencies: 229
-- Name: location_facts_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.location_facts_id_seq OWNED BY public.location_facts.id;


--
-- TOC entry 242 (class 1259 OID 69621)
-- Name: location_question_options; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.location_question_options (
    id integer NOT NULL,
    question_id bigint,
    option_text text,
    is_correct boolean
);


ALTER TABLE public.location_question_options OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 69620)
-- Name: location_question_options_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.location_question_options_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.location_question_options_id_seq OWNER TO postgres;

--
-- TOC entry 5374 (class 0 OID 0)
-- Dependencies: 241
-- Name: location_question_options_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.location_question_options_id_seq OWNED BY public.location_question_options.id;


--
-- TOC entry 240 (class 1259 OID 69594)
-- Name: location_questions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.location_questions (
    id integer NOT NULL,
    location_id bigint,
    question text NOT NULL,
    explanation text NOT NULL,
    is_active boolean,
    created_by_admin_id integer,
    updated_by_admin_id integer,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.location_questions OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 69593)
-- Name: location_questions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.location_questions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.location_questions_id_seq OWNER TO postgres;

--
-- TOC entry 5375 (class 0 OID 0)
-- Dependencies: 239
-- Name: location_questions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.location_questions_id_seq OWNED BY public.location_questions.id;


--
-- TOC entry 228 (class 1259 OID 69466)
-- Name: locations; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.locations (
    id integer NOT NULL,
    name text,
    description text,
    image text,
    reward_xp bigint DEFAULT 100,
    reward_stars bigint DEFAULT 3,
    question_count bigint DEFAULT 5,
    minimum_correct bigint DEFAULT 3,
    level_order bigint,
    is_active boolean,
    created_by_admin_id integer,
    updated_by_admin_id integer,
    deleted_at timestamp without time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.locations OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 69465)
-- Name: locations_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.locations_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.locations_id_seq OWNER TO postgres;

--
-- TOC entry 5376 (class 0 OID 0)
-- Dependencies: 227
-- Name: locations_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.locations_id_seq OWNED BY public.locations.id;


--
-- TOC entry 260 (class 1259 OID 69813)
-- Name: player_achievements; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_achievements (
    id bigint NOT NULL,
    player_id bigint,
    achievement_id bigint,
    claimed boolean,
    obtained_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    claimed_at timestamp without time zone
);


ALTER TABLE public.player_achievements OWNER TO postgres;

--
-- TOC entry 259 (class 1259 OID 69812)
-- Name: player_achievements_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_achievements_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_achievements_id_seq OWNER TO postgres;

--
-- TOC entry 5377 (class 0 OID 0)
-- Dependencies: 259
-- Name: player_achievements_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_achievements_id_seq OWNED BY public.player_achievements.id;


--
-- TOC entry 278 (class 1259 OID 70678)
-- Name: player_challenge_progresses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_challenge_progresses (
    id bigint NOT NULL,
    player_id bigint,
    challenge_id bigint,
    is_completed boolean,
    score bigint,
    completed_at timestamp with time zone,
    created_at timestamp with time zone
);


ALTER TABLE public.player_challenge_progresses OWNER TO postgres;

--
-- TOC entry 277 (class 1259 OID 70677)
-- Name: player_challenge_progresses_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_challenge_progresses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_challenge_progresses_id_seq OWNER TO postgres;

--
-- TOC entry 5378 (class 0 OID 0)
-- Dependencies: 277
-- Name: player_challenge_progresses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_challenge_progresses_id_seq OWNED BY public.player_challenge_progresses.id;


--
-- TOC entry 258 (class 1259 OID 69793)
-- Name: player_gallery_cards; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_gallery_cards (
    id bigint NOT NULL,
    player_id bigint,
    gallery_card_id bigint,
    obtained_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.player_gallery_cards OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 69792)
-- Name: player_gallery_cards_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_gallery_cards_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_gallery_cards_id_seq OWNER TO postgres;

--
-- TOC entry 5379 (class 0 OID 0)
-- Dependencies: 257
-- Name: player_gallery_cards_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_gallery_cards_id_seq OWNED BY public.player_gallery_cards.id;


--
-- TOC entry 262 (class 1259 OID 69834)
-- Name: player_location_answers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_location_answers (
    id bigint NOT NULL,
    progress_id bigint NOT NULL,
    question_id integer NOT NULL,
    selected_option_id integer NOT NULL,
    is_correct boolean DEFAULT false NOT NULL,
    answered_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.player_location_answers OWNER TO postgres;

--
-- TOC entry 261 (class 1259 OID 69833)
-- Name: player_location_answers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_location_answers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_location_answers_id_seq OWNER TO postgres;

--
-- TOC entry 5380 (class 0 OID 0)
-- Dependencies: 261
-- Name: player_location_answers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_location_answers_id_seq OWNED BY public.player_location_answers.id;


--
-- TOC entry 244 (class 1259 OID 69634)
-- Name: player_location_progress; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_location_progress (
    id bigint NOT NULL,
    player_id integer NOT NULL,
    location_id integer NOT NULL,
    attempt_count integer DEFAULT 0 NOT NULL,
    score integer DEFAULT 0 NOT NULL,
    correct_answer integer DEFAULT 0 NOT NULL,
    wrong_answer integer DEFAULT 0 NOT NULL,
    xp_earned integer DEFAULT 0 NOT NULL,
    stars_earned integer DEFAULT 0 NOT NULL,
    is_unlocked boolean DEFAULT false NOT NULL,
    is_completed boolean DEFAULT false NOT NULL,
    completed_at timestamp without time zone,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.player_location_progress OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 69633)
-- Name: player_location_progress_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_location_progress_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_location_progress_id_seq OWNER TO postgres;

--
-- TOC entry 5381 (class 0 OID 0)
-- Dependencies: 243
-- Name: player_location_progress_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_location_progress_id_seq OWNED BY public.player_location_progress.id;


--
-- TOC entry 270 (class 1259 OID 70238)
-- Name: player_location_progresses; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_location_progresses (
    id bigint NOT NULL,
    player_id bigint,
    location_id bigint,
    attempt_count bigint,
    score bigint,
    correct_answer bigint,
    wrong_answer bigint,
    xp_earned bigint,
    stars_earned bigint,
    is_unlocked boolean,
    is_completed boolean,
    completed_at timestamp with time zone,
    created_at timestamp with time zone,
    updated_at timestamp with time zone
);


ALTER TABLE public.player_location_progresses OWNER TO postgres;

--
-- TOC entry 269 (class 1259 OID 70237)
-- Name: player_location_progresses_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_location_progresses_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_location_progresses_id_seq OWNER TO postgres;

--
-- TOC entry 5382 (class 0 OID 0)
-- Dependencies: 269
-- Name: player_location_progresses_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_location_progresses_id_seq OWNED BY public.player_location_progresses.id;


--
-- TOC entry 224 (class 1259 OID 69434)
-- Name: player_sessions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_sessions (
    id bigint NOT NULL,
    player_id bigint,
    token text,
    device_name text,
    device_id text,
    is_active boolean,
    last_login timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    expired_at timestamp without time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.player_sessions OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 69433)
-- Name: player_sessions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_sessions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_sessions_id_seq OWNER TO postgres;

--
-- TOC entry 5383 (class 0 OID 0)
-- Dependencies: 223
-- Name: player_sessions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_sessions_id_seq OWNED BY public.player_sessions.id;


--
-- TOC entry 266 (class 1259 OID 69881)
-- Name: player_trivia_answers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_trivia_answers (
    id bigint NOT NULL,
    history_id bigint NOT NULL,
    question_id bigint,
    selected_option_id bigint,
    is_correct boolean,
    answered_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.player_trivia_answers OWNER TO postgres;

--
-- TOC entry 265 (class 1259 OID 69880)
-- Name: player_trivia_answers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_trivia_answers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_trivia_answers_id_seq OWNER TO postgres;

--
-- TOC entry 5384 (class 0 OID 0)
-- Dependencies: 265
-- Name: player_trivia_answers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_trivia_answers_id_seq OWNED BY public.player_trivia_answers.id;


--
-- TOC entry 272 (class 1259 OID 70334)
-- Name: player_trivia_histories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_trivia_histories (
    id bigint NOT NULL,
    player_id bigint,
    category_id bigint,
    score bigint,
    correct_answer bigint,
    wrong_answer bigint,
    xp_earned bigint,
    stars_earned bigint,
    played_at timestamp with time zone
);


ALTER TABLE public.player_trivia_histories OWNER TO postgres;

--
-- TOC entry 271 (class 1259 OID 70333)
-- Name: player_trivia_histories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_trivia_histories_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_trivia_histories_id_seq OWNER TO postgres;

--
-- TOC entry 5385 (class 0 OID 0)
-- Dependencies: 271
-- Name: player_trivia_histories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_trivia_histories_id_seq OWNED BY public.player_trivia_histories.id;


--
-- TOC entry 264 (class 1259 OID 69858)
-- Name: player_trivia_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.player_trivia_history (
    id bigint NOT NULL,
    player_id integer NOT NULL,
    category_id integer NOT NULL,
    score integer DEFAULT 0 NOT NULL,
    correct_answer integer DEFAULT 0 NOT NULL,
    wrong_answer integer DEFAULT 0 NOT NULL,
    xp_earned integer DEFAULT 0 NOT NULL,
    stars_earned integer DEFAULT 0 NOT NULL,
    played_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.player_trivia_history OWNER TO postgres;

--
-- TOC entry 263 (class 1259 OID 69857)
-- Name: player_trivia_history_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.player_trivia_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.player_trivia_history_id_seq OWNER TO postgres;

--
-- TOC entry 5386 (class 0 OID 0)
-- Dependencies: 263
-- Name: player_trivia_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.player_trivia_history_id_seq OWNED BY public.player_trivia_history.id;


--
-- TOC entry 222 (class 1259 OID 69407)
-- Name: players; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.players (
    id integer NOT NULL,
    user_code text,
    username text,
    name text,
    pin_hash text,
    character_id bigint,
    title_id bigint,
    level bigint DEFAULT 1,
    xp bigint DEFAULT 0,
    stars bigint DEFAULT 0,
    is_active boolean,
    last_login timestamp without time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    fcm_token text
);


ALTER TABLE public.players OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 69406)
-- Name: players_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.players_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.players_id_seq OWNER TO postgres;

--
-- TOC entry 5387 (class 0 OID 0)
-- Dependencies: 221
-- Name: players_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.players_id_seq OWNED BY public.players.id;


--
-- TOC entry 218 (class 1259 OID 69382)
-- Name: titles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.titles (
    id integer NOT NULL,
    title_name text,
    minimum_level bigint,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.titles OWNER TO postgres;

--
-- TOC entry 217 (class 1259 OID 69381)
-- Name: titles_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.titles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.titles_id_seq OWNER TO postgres;

--
-- TOC entry 5388 (class 0 OID 0)
-- Dependencies: 217
-- Name: titles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.titles_id_seq OWNED BY public.titles.id;


--
-- TOC entry 236 (class 1259 OID 69554)
-- Name: trivia_categories; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.trivia_categories (
    id integer NOT NULL,
    name text,
    description text,
    question_count bigint DEFAULT 10,
    minimum_question bigint DEFAULT 10,
    display_order bigint,
    is_active boolean,
    created_by_admin_id integer,
    updated_by_admin_id integer,
    deleted_at timestamp without time zone,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.trivia_categories OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 69553)
-- Name: trivia_categories_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.trivia_categories_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.trivia_categories_id_seq OWNER TO postgres;

--
-- TOC entry 5389 (class 0 OID 0)
-- Dependencies: 235
-- Name: trivia_categories_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.trivia_categories_id_seq OWNED BY public.trivia_categories.id;


--
-- TOC entry 248 (class 1259 OID 69690)
-- Name: trivia_question_options; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.trivia_question_options (
    id integer NOT NULL,
    question_id bigint,
    option_text text,
    is_correct boolean
);


ALTER TABLE public.trivia_question_options OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 69689)
-- Name: trivia_question_options_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.trivia_question_options_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.trivia_question_options_id_seq OWNER TO postgres;

--
-- TOC entry 5390 (class 0 OID 0)
-- Dependencies: 247
-- Name: trivia_question_options_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.trivia_question_options_id_seq OWNED BY public.trivia_question_options.id;


--
-- TOC entry 246 (class 1259 OID 69663)
-- Name: trivia_questions; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.trivia_questions (
    id integer NOT NULL,
    category_id bigint,
    question text NOT NULL,
    explanation text NOT NULL,
    is_active boolean,
    created_by_admin_id integer,
    updated_by_admin_id integer,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.trivia_questions OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 69662)
-- Name: trivia_questions_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.trivia_questions_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.trivia_questions_id_seq OWNER TO postgres;

--
-- TOC entry 5391 (class 0 OID 0)
-- Dependencies: 245
-- Name: trivia_questions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.trivia_questions_id_seq OWNED BY public.trivia_questions.id;


--
-- TOC entry 276 (class 1259 OID 70669)
-- Name: weekly_challenges; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.weekly_challenges (
    id bigint NOT NULL,
    title text,
    description text,
    game_type text,
    reward_xp bigint,
    reward_stars bigint,
    is_active boolean,
    start_date timestamp with time zone,
    end_date timestamp with time zone,
    data_json text,
    created_at timestamp with time zone
);


ALTER TABLE public.weekly_challenges OWNER TO postgres;

--
-- TOC entry 275 (class 1259 OID 70668)
-- Name: weekly_challenges_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.weekly_challenges_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.weekly_challenges_id_seq OWNER TO postgres;

--
-- TOC entry 5392 (class 0 OID 0)
-- Dependencies: 275
-- Name: weekly_challenges_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.weekly_challenges_id_seq OWNED BY public.weekly_challenges.id;


--
-- TOC entry 4930 (class 2604 OID 69531)
-- Name: achievements id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.achievements ALTER COLUMN id SET DEFAULT nextval('public.achievements_id_seq'::regclass);


--
-- TOC entry 4917 (class 2604 OID 69454)
-- Name: admin_logs id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_logs ALTER COLUMN id SET DEFAULT nextval('public.admin_logs_id_seq'::regclass);


--
-- TOC entry 4989 (class 2604 OID 70662)
-- Name: admin_sessions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_sessions ALTER COLUMN id SET DEFAULT nextval('public.admin_sessions_id_seq'::regclass);


--
-- TOC entry 4904 (class 2604 OID 69398)
-- Name: admins id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admins ALTER COLUMN id SET DEFAULT nextval('public.admins_id_seq'::regclass);


--
-- TOC entry 4968 (class 2604 OID 69767)
-- Name: battle_answers id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_answers ALTER COLUMN id SET DEFAULT nextval('public.battle_answers_id_seq'::regclass);


--
-- TOC entry 4964 (class 2604 OID 69728)
-- Name: battle_question_options id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_question_options ALTER COLUMN id SET DEFAULT nextval('public.battle_question_options_id_seq'::regclass);


--
-- TOC entry 4961 (class 2604 OID 69706)
-- Name: battle_questions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_questions ALTER COLUMN id SET DEFAULT nextval('public.battle_questions_id_seq'::regclass);


--
-- TOC entry 4986 (class 2604 OID 69908)
-- Name: battle_room_questions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_room_questions ALTER COLUMN id SET DEFAULT nextval('public.battle_room_questions_id_seq'::regclass);


--
-- TOC entry 4965 (class 2604 OID 69741)
-- Name: battle_rooms id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_rooms ALTER COLUMN id SET DEFAULT nextval('public.battle_rooms_id_seq'::regclass);


--
-- TOC entry 4899 (class 2604 OID 69373)
-- Name: characters id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.characters ALTER COLUMN id SET DEFAULT nextval('public.characters_id_seq'::regclass);


--
-- TOC entry 4928 (class 2604 OID 69512)
-- Name: gallery_cards id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gallery_cards ALTER COLUMN id SET DEFAULT nextval('public.gallery_cards_id_seq'::regclass);


--
-- TOC entry 4940 (class 2604 OID 69585)
-- Name: game_settings id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.game_settings ALTER COLUMN id SET DEFAULT nextval('public.game_settings_id_seq'::regclass);


--
-- TOC entry 4926 (class 2604 OID 69497)
-- Name: location_facts id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_facts ALTER COLUMN id SET DEFAULT nextval('public.location_facts_id_seq'::regclass);


--
-- TOC entry 4945 (class 2604 OID 69624)
-- Name: location_question_options id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_question_options ALTER COLUMN id SET DEFAULT nextval('public.location_question_options_id_seq'::regclass);


--
-- TOC entry 4942 (class 2604 OID 69597)
-- Name: location_questions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_questions ALTER COLUMN id SET DEFAULT nextval('public.location_questions_id_seq'::regclass);


--
-- TOC entry 4919 (class 2604 OID 69469)
-- Name: locations id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.locations ALTER COLUMN id SET DEFAULT nextval('public.locations_id_seq'::regclass);


--
-- TOC entry 4972 (class 2604 OID 69816)
-- Name: player_achievements id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_achievements ALTER COLUMN id SET DEFAULT nextval('public.player_achievements_id_seq'::regclass);


--
-- TOC entry 4991 (class 2604 OID 70681)
-- Name: player_challenge_progresses id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_challenge_progresses ALTER COLUMN id SET DEFAULT nextval('public.player_challenge_progresses_id_seq'::regclass);


--
-- TOC entry 4970 (class 2604 OID 69796)
-- Name: player_gallery_cards id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_gallery_cards ALTER COLUMN id SET DEFAULT nextval('public.player_gallery_cards_id_seq'::regclass);


--
-- TOC entry 4974 (class 2604 OID 69837)
-- Name: player_location_answers id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_answers ALTER COLUMN id SET DEFAULT nextval('public.player_location_answers_id_seq'::regclass);


--
-- TOC entry 4946 (class 2604 OID 69637)
-- Name: player_location_progress id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progress ALTER COLUMN id SET DEFAULT nextval('public.player_location_progress_id_seq'::regclass);


--
-- TOC entry 4987 (class 2604 OID 70241)
-- Name: player_location_progresses id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progresses ALTER COLUMN id SET DEFAULT nextval('public.player_location_progresses_id_seq'::regclass);


--
-- TOC entry 4914 (class 2604 OID 69437)
-- Name: player_sessions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_sessions ALTER COLUMN id SET DEFAULT nextval('public.player_sessions_id_seq'::regclass);


--
-- TOC entry 4984 (class 2604 OID 69884)
-- Name: player_trivia_answers id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_answers ALTER COLUMN id SET DEFAULT nextval('public.player_trivia_answers_id_seq'::regclass);


--
-- TOC entry 4988 (class 2604 OID 70337)
-- Name: player_trivia_histories id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_histories ALTER COLUMN id SET DEFAULT nextval('public.player_trivia_histories_id_seq'::regclass);


--
-- TOC entry 4977 (class 2604 OID 69861)
-- Name: player_trivia_history id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_history ALTER COLUMN id SET DEFAULT nextval('public.player_trivia_history_id_seq'::regclass);


--
-- TOC entry 4908 (class 2604 OID 69410)
-- Name: players id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.players ALTER COLUMN id SET DEFAULT nextval('public.players_id_seq'::regclass);


--
-- TOC entry 4901 (class 2604 OID 69385)
-- Name: titles id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.titles ALTER COLUMN id SET DEFAULT nextval('public.titles_id_seq'::regclass);


--
-- TOC entry 4935 (class 2604 OID 69557)
-- Name: trivia_categories id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_categories ALTER COLUMN id SET DEFAULT nextval('public.trivia_categories_id_seq'::regclass);


--
-- TOC entry 4960 (class 2604 OID 69693)
-- Name: trivia_question_options id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_question_options ALTER COLUMN id SET DEFAULT nextval('public.trivia_question_options_id_seq'::regclass);


--
-- TOC entry 4957 (class 2604 OID 69666)
-- Name: trivia_questions id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_questions ALTER COLUMN id SET DEFAULT nextval('public.trivia_questions_id_seq'::regclass);


--
-- TOC entry 4990 (class 2604 OID 70672)
-- Name: weekly_challenges id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.weekly_challenges ALTER COLUMN id SET DEFAULT nextval('public.weekly_challenges_id_seq'::regclass);


--
-- TOC entry 5311 (class 0 OID 69528)
-- Dependencies: 234
-- Data for Name: achievements; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.achievements (id, name, description, icon, condition_type, condition_value, reward_xp, reward_stars, is_active, created_by_admin_id, updated_by_admin_id, created_at, updated_at) FROM stdin;
1	Penjelajah Pemula	Selesaikan 1 lokasi Jelajah Budaya	\N	LOCATION	1	100	5	t	\N	\N	2026-07-24 19:39:13.803057+07	2026-07-24 19:39:13.803057
2	Sahabat Budaya	Mencapai Level 5	\N	LEVEL	5	200	10	t	\N	\N	2026-07-24 19:39:13.803057+07	2026-07-24 19:39:13.803057
3	Penjelajah Sejati	Selesaikan 5 lokasi Jelajah Budaya	🧭	LOCATION	5	300	5	t	\N	\N	2026-08-08 14:53:09.8799+07	2026-08-08 14:53:09.879852
4	Master Penjelajah	Selesaikan semua 10 lokasi	🏆	LOCATION	10	500	10	t	\N	\N	2026-08-08 14:53:09.882537+07	2026-08-08 14:53:09.882535
5	Kuiz Perdana	Mainkan Trivia Budaya pertama kali	❓	TRIVIA	1	50	0	t	\N	\N	2026-08-08 14:53:09.883802+07	2026-08-08 14:53:09.88403
6	Pecinta Trivia	Mainkan Trivia sebanyak 10 kali	📚	TRIVIA	10	200	3	t	\N	\N	2026-08-08 14:53:09.88553+07	2026-08-08 14:53:09.885815
7	Pendekar Battle	Menangkan 1 Battle Quiz	⚔	BATTLE	1	150	2	t	\N	\N	2026-08-08 14:53:09.887195+07	2026-08-08 14:53:09.887455
8	Raja Battle	Menangkan 5 Battle Quiz	👑	BATTLE	5	400	8	t	\N	\N	2026-08-08 14:53:09.888492+07	2026-08-08 14:53:09.888796
9	Kolektor Bintang	Kumpulkan 20 bintang	⭐	STAR	20	200	0	t	\N	\N	2026-08-08 14:53:09.889641+07	2026-08-08 14:53:09.890031
10	Bintang Sejati	Kumpulkan 50 bintang	🌟	STAR	50	500	0	t	\N	\N	2026-08-08 14:53:09.89175+07	2026-08-08 14:53:09.891727
11	Naik Level	Capai Level 5	📈	LEVEL	5	100	5	t	\N	\N	2026-08-08 14:53:09.892361+07	2026-08-08 14:53:09.892866
12	Duta Budaya	Capai Level 15	🎖	LEVEL	15	500	10	t	\N	\N	2026-08-08 14:53:09.893505+07	2026-08-08 14:53:09.893919
13	Maestro Toba	Capai Level 20	🎓	LEVEL	20	1000	20	t	\N	\N	2026-08-08 14:53:09.894594+07	2026-08-08 14:53:09.894839
14	XP Pertama	Kumpulkan 500 XP	✨	XP	500	0	2	t	\N	\N	2026-08-08 14:53:09.895667+07	2026-08-08 14:53:09.896016
15	Pencari Ilmu	Kumpulkan 2000 XP	💡	XP	2000	0	5	t	\N	\N	2026-08-08 14:53:09.896883+07	2026-08-08 14:53:09.897084
\.


--
-- TOC entry 5303 (class 0 OID 69451)
-- Dependencies: 226
-- Data for Name: admin_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.admin_logs (id, admin_id, activity, table_name, record_id, description, created_at) FROM stdin;
\.


--
-- TOC entry 5351 (class 0 OID 70659)
-- Dependencies: 274
-- Data for Name: admin_sessions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.admin_sessions (id, admin_id, token, is_active, last_login, created_at) FROM stdin;
1	1	adm_490055c8148c3d325509dbbd73abe90af2f6566b90549ae4cdb3f3dd60cc4913	t	2026-09-08 22:55:07.239737+07	2026-09-08 22:55:07.239737+07
\.


--
-- TOC entry 5297 (class 0 OID 69395)
-- Dependencies: 220
-- Data for Name: admins; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.admins (id, fullname, username, password_hash, role, created_at, updated_at) FROM stdin;
1	Super Admin TOBA	admin	$2a$10$LS1HwA3dNfmfgSIBKyjAnulTNgxL/VRA95EfCKXgVUYVyq3OAj6mq	SUPER_ADMIN	2026-08-08 14:53:10.014119+07	2026-09-16 10:12:28.998167+07
\.


--
-- TOC entry 5333 (class 0 OID 69764)
-- Dependencies: 256
-- Data for Name: battle_answers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battle_answers (id, room_id, player_id, question_id, selected_option_id, is_correct, answered_at) FROM stdin;
\.


--
-- TOC entry 5329 (class 0 OID 69725)
-- Dependencies: 252
-- Data for Name: battle_question_options; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battle_question_options (id, question_id, option_text, is_correct) FROM stdin;
1	1	Sarune	t
2	1	Seruling	f
3	1	Pianika	f
4	1	Gitar	f
5	2	3	t
6	2	2	f
7	2	5	f
8	2	4	f
9	3	Danau Toba	t
10	3	Danau Singkarak	f
11	3	Danau Maninjau	f
12	3	Danau Poso	f
13	4	Samosir	t
14	4	Nias	f
15	4	Batam	f
16	4	Mentawai	f
17	5	Ulos	t
18	5	Batik	f
19	5	Songket	f
20	5	Tapis	f
21	6	Dalihan Na Tolu	t
22	6	Marga System	f
23	6	Adat Minang	f
24	6	Suku Adat	f
25	7	Tortor	t
26	7	Saman	f
27	7	Kecak	f
28	7	Pendet	f
29	8	Horas	t
30	8	Marhaban	f
31	8	Selamat	f
32	8	Mejuah-juah	f
33	9	Arsik	t
34	9	Pindang	f
35	9	Saksang	f
36	9	Gulai	f
37	10	Hasapi	t
38	10	Sasando	f
39	10	Kecapi	f
40	10	Siter	f
41	11	Rumah Bolon	t
42	11	Rumah Gadang	f
43	11	Joglo	f
44	11	Tongkonan	f
45	12	Andaliman	t
46	12	Kemiri	f
47	12	Lada	f
48	12	Ketumbar	f
49	13	Sigale-gale	t
50	13	Wayang	f
51	13	Ondel-ondel	f
52	13	Barong	f
53	14	Festival Danau Toba	t
54	14	Pesta Rakyat	f
55	14	Karnaval Toba	f
56	14	Pekan Budaya	f
57	15	Global Geopark	t
58	15	Warisan Dunia	f
59	15	Biosfer	f
60	15	Taman Nasional	f
61	16	Desa Tomok	t
62	16	Parapat	f
63	16	Balige	f
64	16	Tarutung	f
65	17	Mangongkal Holi	t
66	17	Saur Matua	f
67	17	Mardebata	f
68	17	Tortor	f
\.


--
-- TOC entry 5327 (class 0 OID 69703)
-- Dependencies: 250
-- Data for Name: battle_questions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battle_questions (id, question, explanation, is_active, created_by_admin_id, updated_by_admin_id, created_at, updated_at) FROM stdin;
1	Apa nama alat musik tiup khas Batak Toba?	Sarune Bolon adalah alat musik tiup utama.	t	\N	\N	2026-07-24 18:55:17.858914+07	2026-07-24 18:55:17.858914+07
2	Berapa jumlah warna pada bendera Sisingamangaraja?	Ada 3 warna: Merah, Putih, dan Hitam.	t	\N	\N	2026-07-24 18:55:17.858914+07	2026-07-24 18:55:17.858914+07
3	Apa nama danau terbesar di Indonesia?	Danau Toba adalah danau terbesar di Indonesia dan danau vulkanik terbesar di dunia.	t	\N	\N	2026-08-08 14:53:09.803713+07	2026-08-08 14:53:09.803713+07
4	Pulau apa yang berada di tengah Danau Toba?	Pulau Samosir adalah pulau terbesar yang berada di tengah Danau Toba.	t	\N	\N	2026-08-08 14:53:09.811517+07	2026-08-08 14:53:09.811517+07
5	Kain tradisional Batak Toba yang terkenal disebut?	Ulos adalah kain tenun tradisional Batak Toba yang memiliki makna budaya mendalam.	t	\N	\N	2026-08-08 14:53:09.81747+07	2026-08-08 14:53:09.81747+07
6	Sistem kekerabatan Batak yang mengatur tiga pilar hubungan disebut?	Dalihan Na Tolu adalah sistem adat Batak yang mengatur hubungan hula-hula, dongan tubu, dan boru.	t	\N	\N	2026-08-08 14:53:09.823421+07	2026-08-08 14:53:09.823421+07
7	Tari tradisional Batak Toba yang diiringi gondang disebut?	Tortor adalah tari tradisional Batak Toba yang memiliki makna ritual dan spiritual.	t	\N	\N	2026-08-08 14:53:09.827538+07	2026-08-08 14:53:09.827538+07
8	Ucapan salam khas masyarakat Batak Toba adalah?	Horas adalah salam khas Batak Toba yang berarti sehat dan sejahtera.	t	\N	\N	2026-08-08 14:53:09.833477+07	2026-08-08 14:53:09.833477+07
9	Masakan khas Batak dari ikan mas dengan bumbu kuning disebut?	Arsik adalah masakan ikan mas khas Batak Toba yang dimasak dengan bumbu andaliman.	t	\N	\N	2026-08-08 14:53:09.837605+07	2026-08-08 14:53:09.837605+07
10	Alat musik petik tradisional Batak Toba disebut?	Hasapi adalah alat musik petik tradisional Batak Toba menyerupai kecapi.	t	\N	\N	2026-08-08 14:53:09.842528+07	2026-08-08 14:53:09.842528+07
11	Rumah adat Batak Toba disebut?	Rumah Bolon adalah rumah adat tradisional masyarakat Batak Toba.	t	\N	\N	2026-08-08 14:53:09.847077+07	2026-08-08 14:53:09.847077+07
12	Rempah khas Batak dengan rasa getir yang tumbuh di kawasan Toba?	Andaliman adalah rempah khas Batak Toba yang sering digunakan dalam masakan arsik dan saksang.	t	\N	\N	2026-08-08 14:53:09.852059+07	2026-08-08 14:53:09.852059+07
13	Boneka kayu yang bisa bergerak dalam upacara adat Batak disebut?	Sigale-gale adalah patung boneka kayu yang digerakkan dalam upacara adat Batak Toba.	t	\N	\N	2026-08-08 14:53:09.856703+07	2026-08-08 14:53:09.856703+07
14	Festival budaya tahunan di kawasan Danau Toba disebut?	Festival Danau Toba adalah event budaya tahunan yang mempromosikan kekayaan budaya Batak.	t	\N	\N	2026-08-08 14:53:09.860812+07	2026-08-08 14:53:09.860812+07
15	UNESCO mengakui Danau Toba sebagai?	Danau Toba diakui UNESCO sebagai Global Geopark pada tahun 2020.	t	\N	\N	2026-08-08 14:53:09.865223+07	2026-08-08 14:53:09.865223+07
16	Makam Raja Sidabutar yang bersejarah terletak di?	Makam Raja Sidabutar yang berusia 500 tahun terletak di Desa Tomok, Samosir.	t	\N	\N	2026-08-08 14:53:09.869956+07	2026-08-08 14:53:09.869956+07
17	Upacara pemindahan tulang leluhur Batak disebut?	Mangongkal Holi adalah ritual sakral pemindahan tulang leluhur ke tempat yang lebih mulia.	t	\N	\N	2026-08-08 14:53:09.874749+07	2026-08-08 14:53:09.874749+07
\.


--
-- TOC entry 5345 (class 0 OID 69905)
-- Dependencies: 268
-- Data for Name: battle_room_questions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battle_room_questions (id, room_id, question_id, question_order) FROM stdin;
1	4	7	0
6	6	9	0
\.


--
-- TOC entry 5331 (class 0 OID 69738)
-- Dependencies: 254
-- Data for Name: battle_rooms; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.battle_rooms (id, room_code, host_player_id, guest_player_id, winner_player_id, status, created_at, started_at, finished_at, expired_at) FROM stdin;
4	KM3AXS	41	\N	\N	FINISHED	2026-09-08 23:51:16.765104+07	\N	2026-09-08 23:51:27.399193	\N
5	BE27QF	42	\N	\N	WAITING	2026-09-09 23:27:38.660008+07	\N	\N	2026-09-09 23:32:38.659301
6	W9XN9Z	42	\N	\N	FINISHED	2026-09-09 23:28:12.959512+07	\N	2026-09-09 23:28:31.285206	\N
\.


--
-- TOC entry 5293 (class 0 OID 69370)
-- Dependencies: 216
-- Data for Name: characters; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.characters (id, name, image, description, created_at) FROM stdin;
1	Ucok	characters/ucok.png	Karakter anak laki-laki Batak	2026-07-08 02:39:51.382328+07
2	Butet	characters/butet.png	Karakter anak perempuan Batak	2026-07-08 02:39:51.382328+07
\.


--
-- TOC entry 5309 (class 0 OID 69509)
-- Dependencies: 232
-- Data for Name: gallery_cards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.gallery_cards (id, location_id, title, image, description, created_at) FROM stdin;
1	1	Kartu IT Del	cards/itdel.png	Kampus teknologi kebanggaan di tepian Danau Toba.	2026-07-24 19:39:13.803057+07
\.


--
-- TOC entry 5315 (class 0 OID 69582)
-- Dependencies: 238
-- Data for Name: game_settings; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.game_settings (id, setting_key, setting_value, description, updated_at) FROM stdin;
1	battle_question_count	5	Jumlah soal per Battle	2026-08-08 14:53:09.901411+07
2	battle_wait_timeout	300	Waktu tunggu room battle (detik)	2026-08-08 14:53:09.907058+07
3	trivia_question_count	10	Jumlah soal per sesi Trivia	2026-08-08 14:53:09.908812+07
4	location_question_count	5	Jumlah soal per lokasi Jelajah	2026-08-08 14:53:09.90994+07
5	xp_per_level	500	XP yang dibutuhkan per level	2026-08-08 14:53:09.911639+07
6	character_change_level	5	Level minimum ganti karakter	2026-08-08 14:53:09.913988+07
7	initial_xp	0	XP awal pemain baru	2026-08-08 14:53:09.916053+07
8	initial_stars	0	Bintang awal pemain baru	2026-08-08 14:53:09.91775+07
9	battle_reward_xp	50	XP reward untuk pemenang battle	2026-08-08 14:53:09.918294+07
10	battle_reward_stars	2	Bintang reward pemenang battle	2026-08-08 14:53:09.919403+07
\.


--
-- TOC entry 5307 (class 0 OID 69494)
-- Dependencies: 230
-- Data for Name: location_facts; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.location_facts (id, location_id, fact, created_at) FROM stdin;
1	1	IT Del merupakan singkatan dari Inpres Toba Development.	2026-08-08 14:53:09.538871+07
2	1	IT Del didirikan oleh Luhut Binsar Pandjaitan pada tahun 2001.	2026-08-08 14:53:09.5461+07
3	1	IT Del terletak di tepi Danau Toba, Laguboti, Sumatera Utara.	2026-08-08 14:53:09.549394+07
4	1	IT Del menjadi salah satu kampus teknologi terbaik di Sumatera.	2026-08-08 14:53:09.551753+07
5	2	Pantai Bulbul memiliki pasir putih yang halus khas Danau Toba.	2026-08-08 14:53:09.554174+07
6	2	Pantai Bulbul berlokasi di Balige, ibu kota Kabupaten Toba.	2026-08-08 14:53:09.556417+07
7	2	Danau Toba adalah danau vulkanik terbesar di dunia.	2026-08-08 14:53:09.558414+07
8	2	Danau Toba terbentuk akibat letusan supervulkan sekitar 74.000 tahun lalu.	2026-08-08 14:53:09.559644+07
9	3	Museum Batak menyimpan lebih dari 1.000 koleksi benda budaya Batak.	2026-08-08 14:53:09.562206+07
10	3	TB Silalahi adalah tokoh militer dan politisi asal Batak Toba.	2026-08-08 14:53:09.564364+07
11	3	Ulos adalah kain tradisional Batak yang memiliki makna filosofis mendalam.	2026-08-08 14:53:09.566549+07
12	3	Gorga adalah ukiran khas Batak yang biasa menghiasi rumah adat.	2026-08-08 14:53:09.569332+07
13	4	Huta Siallagan berasal dari nama marga Siallagan yang mendiami desa ini.	2026-08-08 14:53:09.572064+07
14	4	Batu kursi di Huta Siallagan digunakan sebagai tempat pengadilan adat.	2026-08-08 14:53:09.573914+07
15	4	Pulau Samosir adalah pulau terbesar di dalam danau di dunia.	2026-08-08 14:53:09.575044+07
16	4	Samosir memiliki luas sekitar 630 kilometer persegi.	2026-08-08 14:53:09.577402+07
17	5	Bukit Holbung mendapat julukan Bukit Teletubbies karena bentuknya yang bergelombang hijau.	2026-08-08 14:53:09.579704+07
18	5	Dari Bukit Holbung terlihat panorama Danau Toba 360 derajat.	2026-08-08 14:53:09.581551+07
19	5	Ketinggian Bukit Holbung sekitar 1.000 meter di atas permukaan laut.	2026-08-08 14:53:09.583894+07
20	5	Bukit Holbung ramai dikunjungi saat matahari terbit dan terbenam.	2026-08-08 14:53:09.586813+07
21	6	Makam Raja Sidabutar di Desa Tomok berusia lebih dari 500 tahun.	2026-08-08 14:53:09.590809+07
22	6	Sigale-gale pertama kali dibuat di Desa Tomok.	2026-08-08 14:53:09.593516+07
23	6	Desa Tomok terkenal sebagai pusat kerajinan tangan Batak Toba.	2026-08-08 14:53:09.595404+07
24	6	Kain ulos dari Tomok dikenal sebagai salah satu yang terbaik.	2026-08-08 14:53:09.598104+07
25	7	Batu Gantung terletak di Parapat, Kabupaten Simalungun.	2026-08-08 14:53:09.601056+07
26	7	Legenda Batu Gantung bercerita tentang seorang gadis bernama Seruni.	2026-08-08 14:53:09.603879+07
27	7	Batu Gantung dapat dilihat dari kapal feri yang melintasi Danau Toba.	2026-08-08 14:53:09.606236+07
28	7	Lokasi Batu Gantung menjadi salah satu ikon wisata Parapat.	2026-08-08 14:53:09.607977+07
29	8	Menara Tele berdiri di ketinggian sekitar 1.300 meter di atas permukaan laut.	2026-08-08 14:53:09.611111+07
30	8	Dari Menara Tele terlihat hamparan sawah dan Danau Toba sekaligus.	2026-08-08 14:53:09.61318+07
31	8	Tele adalah nama desa yang menjadi gerbang masuk Pulau Samosir dari darat.	2026-08-08 14:53:09.614969+07
32	8	Jalan Tele dikenal sebagai salah satu jalur pegunungan terindah di Sumatera Utara.	2026-08-08 14:53:09.618016+07
33	9	Air Terjun Situmurun memiliki ketinggian sekitar 70 meter.	2026-08-08 14:53:09.620903+07
34	9	Situmurun berarti turun bertahap dalam bahasa Batak Toba.	2026-08-08 14:53:09.62257+07
35	9	Air terjun ini langsung jatuh ke perairan Danau Toba.	2026-08-08 14:53:09.624729+07
36	9	Wisatawan bisa berenang di kolam alami di bawah air terjun ini.	2026-08-08 14:53:09.626369+07
37	10	Sigale-gale dibuat sebagai pengganti anak bagi raja yang tidak memiliki keturunan.	2026-08-08 14:53:09.62911+07
38	10	Sigale-gale dapat bergerak dengan mekanisme tali yang dikendalikan manusia.	2026-08-08 14:53:09.631757+07
39	10	Upacara Sigale-gale biasanya disertai musik gondang.	2026-08-08 14:53:09.633449+07
40	10	Gondang adalah alat musik perkusi tradisional Batak Toba.	2026-08-08 14:53:09.636113+07
\.


--
-- TOC entry 5319 (class 0 OID 69621)
-- Dependencies: 242
-- Data for Name: location_question_options; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.location_question_options (id, question_id, option_text, is_correct) FROM stdin;
1	1	Laguboti	t
2	1	Medan	f
3	1	Balige	f
4	1	Tarutung	f
5	2	Luhut B. Pandjaitan	t
6	2	Jokowi	f
7	2	Ahok	f
8	2	Anies	f
9	3	Biru dan Putih	t
10	3	Merah	f
11	3	Hijau	f
12	3	Kuning	f
13	4	Danau Toba	t
14	4	Danau Singkarak	f
15	4	Danau Maninjau	f
16	4	Danau Poso	f
17	5	Sitoluama	t
18	5	Balige	f
19	5	Laguboti	f
20	5	Porsea	f
21	6	Institut Teknologi Del	t
22	6	Inpres Toba Development	f
23	6	Institut Toba Deliserdang	f
24	6	Inisiatif Teknologi Delta	f
25	7	Laguboti	t
26	7	Balige	f
27	7	Parapat	f
28	7	Tarutung	f
29	8	Luhut Binsar Pandjaitan	t
30	8	B.J. Habibie	f
31	8	Susilo Bambang Yudhoyono	f
32	8	Joko Widodo	f
33	9	2012	t
34	9	2001	f
35	9	2005	f
36	9	2010	f
37	10	Danau Toba	t
38	10	Danau Singkarak	f
39	10	Danau Maninjau	f
40	10	Danau Poso	f
41	11	Terletak di tepi Danau Toba	t
42	11	Kampus terbesar di Indonesia	f
43	11	Didirikan oleh presiden	f
44	11	Gratis untuk semua mahasiswa	f
45	12	Teknik Informatika	t
46	12	Kedokteran	f
47	12	Hukum	f
48	12	Manajemen Bisnis	f
49	13	Sumatera Utara	t
50	13	Sumatera Barat	f
51	13	Riau	f
52	13	Aceh	f
53	14	Menjadi kampus teknologi terkemuka	t
54	14	Mencetak atlet nasional	f
55	14	Mengembangkan pariwisata	f
56	14	Menjadi kampus kedokteran terbaik	f
57	15	Letusan supervulkan	t
58	15	Gempa bumi besar	f
59	15	Erosi tanah	f
60	15	Hujan deras ribuan tahun	f
61	16	Kabupaten Toba	t
62	16	Kabupaten Samosir	f
63	16	Kabupaten Dairi	f
64	16	Kabupaten Humbang	f
65	17	Susilo Bambang Yudhoyono	t
66	17	Megawati	f
67	17	Joko Widodo	f
68	17	Habibie	f
69	18	Teknologi	t
70	18	Seni dan Budaya	f
71	18	Kedokteran	f
72	18	Pertanian	f
73	19	Danau Toba	t
74	19	Danau Ranau	f
75	19	Danau Kerinci	f
76	19	Danau Sentani	f
77	20	Kampus di tepi danau	t
78	20	Kampus di atas bukit	f
79	20	Kampus dalam hutan	f
80	20	Kampus pulau terpencil	f
81	21	Balige	t
82	21	Parapat	f
83	21	Samosir	f
84	21	Laguboti	f
85	22	Pasir putih dan pemandangan Danau Toba	t
86	22	Terumbu karang yang berwarna-warni	f
87	22	Ombak besar untuk surfing	f
88	22	Hutan mangrove yang lebat	f
89	23	Di dunia	t
90	23	Di Asia	f
91	23	Di Indonesia	f
92	23	Di Asia Tenggara	f
93	24	Berenang dan menikmati pemandangan	t
94	24	Selancar ombak	f
95	24	Menyelam laut dalam	f
96	24	Memanjat tebing	f
97	25	Kabupaten Toba	t
98	25	Kabupaten Samosir	f
99	25	Kabupaten Taput	f
100	25	Kabupaten Dairi	f
101	26	Putih	t
102	26	Hitam	f
103	26	Coklat	f
104	26	Kuning keemasan	f
105	27	74.000 tahun lalu	t
106	27	10.000 tahun lalu	f
107	27	1.000 tahun lalu	f
108	27	500.000 tahun lalu	f
109	28	Festival Danau Toba	t
110	28	Pesta Batak	f
111	28	Karnaval Samosir	f
112	28	Pekan Budaya Toba	f
113	29	Airnya tenang dan pantainya bersih	t
114	29	Ombaknya besar dan menantang	f
115	29	Ada wahana permainan modern	f
116	29	Tempatnya sangat sepi	f
117	30	Pulau Samosir	t
118	30	Pulau Nias	f
119	30	Pulau Batam	f
120	30	Pulau Berhala	f
121	31	Sumatera Utara	t
122	31	Sumatera Barat	f
123	31	Riau	f
124	31	Aceh	f
125	32	Jernih dan dalam memantulkan langit	t
126	32	Banyak ganggang biru	f
127	32	Pengaruh mineral khusus	f
128	32	Efek cahaya matahari	f
129	33	505 meter	t
130	33	200 meter	f
131	33	1.000 meter	f
132	33	100 meter	f
133	34	Kapal feri	t
134	34	Jembatan	f
135	34	Helikopter	f
136	34	Perahu dayung saja	f
137	35	Batak Toba	t
138	35	Batak Karo	f
139	35	Minangkabau	f
140	35	Melayu	f
141	36	TB Silalahi Center	t
142	36	Museum Nasional Batak	f
143	36	Galeri Budaya Toba	f
144	36	Rumah Adat Batak	f
145	37	TB Silalahi	t
146	37	Luhut Binsar Pandjaitan	f
147	37	Raja Siallagan	f
148	37	B.J. Habibie	f
149	38	Koleksi benda budaya Batak Toba	t
150	38	Koleksi senjata perang dunia	f
151	38	Koleksi batik nusantara	f
152	38	Koleksi benda prasejarah	f
153	39	Ulos	t
154	39	Batik	f
155	39	Songket	f
156	39	Tapis	f
157	40	Gorga	t
158	40	Batik	f
159	40	Tenun	f
160	40	Wayang	f
161	41	Balige	t
162	41	Parapat	f
163	41	Samosir	f
164	41	Tarutung	f
165	42	Piso Gaja Dompak	t
166	42	Keris	f
167	42	Rencong	f
168	42	Mandau	f
169	43	Pelestarian dan edukasi budaya	t
170	43	Tempat hiburan semata	f
171	43	Pusat perbelanjaan budaya	f
172	43	Tempat ibadah	f
173	44	Lebih dari 1.000	t
174	44	Kurang dari 100	f
175	44	Tepat 500	f
176	44	Lebih dari 10.000	f
177	45	Hasapi	t
178	45	Gitar	f
179	45	Kecapi	f
180	45	Sitar	f
181	46	Ulos	t
182	46	Kebaya	f
183	46	Baju Bodo	f
184	46	Baju Kurung	f
185	47	Koleksi terlengkap budaya Batak Toba	t
186	47	Museum terbesar di dunia	f
187	47	Gratis untuk semua pengunjung	f
188	47	Hanya buka malam hari	f
189	48	Koleksi pakaian, senjata, dan alat musik	t
190	48	Film dokumenter saja	f
191	48	Pertunjukan seni langsung	f
192	48	Pameran lukisan modern	f
193	49	Tokoh militer dan politisi Batak	t
194	49	Raja Batak terakhir	f
195	49	Penulis buku terkenal	f
196	49	Penemu batik Batak	f
197	50	Danau Toba	t
198	50	Pantai laut	f
199	50	Sungai besar	f
200	50	Pegunungan	f
201	51	Pulau Samosir	t
202	51	Pulau Nias	f
203	51	Pulau Batam	f
204	51	Daratan Sumatera	f
205	52	Batu Kursi Raja untuk pengadilan adat	t
206	52	Pantai pasir putih	f
207	52	Air terjun tertinggi	f
208	52	Hutan bambu yang lebat	f
209	53	Raja adat Siallagan	t
210	53	Tamu kerajaan	f
211	53	Para pedagang	f
212	53	Wisatawan	f
213	54	Batak Toba	t
214	54	Batak Karo	f
215	54	Minangkabau	f
216	54	Dayak	f
217	55	Desa atau kampung	t
218	55	Sungai	f
219	55	Hutan	f
220	55	Danau	f
221	56	Di dunia	t
222	56	Di Asia	f
223	56	Di Indonesia	f
224	56	Di Asia Tenggara	f
225	57	Pengadilan adat	t
226	57	Tempat bertapa	f
227	57	Meja makan raja	f
228	57	Tempat berdagang	f
229	58	Siallagan	t
230	58	Sinaga	f
231	58	Sitorus	f
232	58	Simanjuntak	f
233	59	630 km persegi	t
234	59	100 km persegi	f
235	59	1.000 km persegi	f
236	59	50 km persegi	f
237	60	Sistem aturan dan tradisi masyarakat Batak	t
238	60	Bahasa resmi Batak	f
239	60	Jenis tarian Batak	f
240	60	Makanan khas Batak	f
241	61	Naik kapal feri	t
242	61	Melewati jembatan	f
243	61	Naik kereta api	f
244	61	Terbang dengan pesawat	f
245	62	Dalihan Na Tolu	t
246	62	Marga Sistem	f
247	62	Adat Minang	f
248	62	Suku Adat	f
249	63	Rumah Bolon	t
250	63	Rumah Gadang	f
251	63	Joglo	f
252	63	Honai	f
253	64	Pihak ayah	t
254	64	Pihak ibu	f
255	64	Kedua orang tua	f
256	64	Pilihan sendiri	f
257	65	Horas	t
258	65	Mejuah-juah	f
259	65	Yaahowu	f
260	65	Selamat	f
261	66	Bukit Teletubbies	t
262	66	Bukit Pelangi	f
263	66	Bukit Indah	f
264	66	Bukit Hijau	f
265	67	Samosir	t
266	67	Toba	f
267	67	Dairi	f
268	67	Humbang Hasundutan	f
269	68	1.000 meter	t
270	68	500 meter	f
271	68	2.000 meter	f
272	68	200 meter	f
273	69	Panorama Danau Toba 360 derajat	t
274	69	Lautan luas	f
275	69	Kota besar	f
276	69	Gunung berapi aktif	f
277	70	Saat matahari terbit dan terbenam	t
278	70	Tengah malam	f
279	70	Siang hari terik	f
280	70	Musim hujan	f
281	71	Pulau Samosir	t
282	71	Pulau Nias	f
283	71	Pulau Batam	f
284	71	Daratan Sumatera	f
285	72	Bentuknya bergelombang hijau	t
286	72	Ada batu besar ajaib	f
287	72	Tanahnya berwarna merah	f
288	72	Selalu berkabut sepanjang tahun	f
289	73	Posisinya tinggi dengan pemandangan terbuka	t
290	73	Ada teleskop raksasa di sana	f
291	73	Danau Toba sangat besar	f
292	73	Bukit ini tertinggi di Indonesia	f
293	74	Danau Toba	t
294	74	Danau Singkarak	f
295	74	Danau Maninjau	f
296	74	Danau Kerinci	f
297	75	Hiking dan fotografi	t
298	75	Selancar air	f
299	75	Menyelam	f
300	75	Berperahu	f
301	76	Di dunia	t
302	76	Di Asia	f
303	76	Di Indonesia	f
304	76	Di Sumatera	f
305	77	Hijau	t
306	77	Merah	f
307	77	Kuning	f
308	77	Biru	f
309	78	Pangururan	t
310	78	Parapat	f
311	78	Balige	f
312	78	Tarutung	f
313	79	Sekitar 1.000 MDPL	t
314	79	Di bawah 100 MDPL	f
315	79	Lebih dari 3.000 MDPL	f
316	79	Tepat 500 MDPL	f
317	80	Pemandangan indah dan unik untuk foto	t
318	80	Ada pertunjukan seni setiap hari	f
319	80	Tiket masuknya gratis selalu	f
320	80	Ada pusat perbelanjaan di sana	f
321	81	Makam Raja Sidabutar	t
322	81	Pantai indah	f
323	81	Air terjun	f
324	81	Hutan pinus	f
325	82	Lebih dari 500 tahun	t
326	82	100 tahun	f
327	82	1.000 tahun	f
328	82	50 tahun	f
329	83	Pulau Samosir	t
330	83	Pulau Nias	f
331	83	Pulau Batam	f
332	83	Daratan Sumatera	f
333	84	Kerajinan tangan Batak seperti ulos	t
334	84	Elektronik modern	f
335	84	Senjata tradisional	f
336	84	Makanan impor	f
337	85	Raja Batak Toba di Tomok	t
338	85	Raja Majapahit	f
339	85	Raja Sriwijaya	f
340	85	Sultan Batak pertama	f
341	86	Desa Tomok	t
342	86	Huta Siallagan	f
343	86	Balige	f
344	86	Parapat	f
345	87	Salah satu tertua	t
346	87	Desa termuda	f
347	87	Desa terbesar	f
348	87	Desa terkecil	f
349	88	Salah satu ulos terbaik	t
350	88	Ulos paling murah	f
351	88	Ulos untuk pemula	f
352	88	Ulos hanya untuk pria	f
353	89	Bahasa Batak Toba	t
354	89	Bahasa Melayu	f
355	89	Bahasa Jawa	f
356	89	Bahasa Minang	f
357	90	Batu andesit	t
358	90	Kayu jati	f
359	90	Emas murni	f
360	90	Beton modern	f
361	91	Parapat	t
362	91	Balige	f
363	91	Laguboti	f
364	91	Tarutung	f
365	92	Tari Tortor	t
366	92	Tari Saman	f
367	92	Tari Kecak	f
368	92	Tari Piring	f
369	93	Patung boneka kayu yang bisa bergerak	t
370	93	Jenis masakan Batak	f
371	93	Alat musik Batak	f
372	93	Nama senjata Batak	f
373	94	Gorga	t
374	94	Batik	f
375	94	Wayang	f
376	94	Kaligrafi	f
377	95	30-45 menit	t
378	95	5 menit	f
379	95	3 jam	f
380	95	1 hari	f
381	96	Parapat	t
382	96	Balige	f
383	96	Samosir	f
384	96	Laguboti	f
385	97	Gadis bernama Seruni yang bersedih	t
386	97	Pertempuran raja Batak	f
387	97	Kisah cinta terlarang	f
388	97	Asal usul Danau Toba	f
389	98	Menggantung di tepi Danau Toba	t
390	98	Terapung di tengah danau	f
391	98	Berada di dasar danau	f
392	98	Berbentuk seperti rumah	f
393	99	Dari kapal feri di Danau Toba	t
394	99	Hanya dari pesawat	f
395	99	Hanya dari bawah air	f
396	99	Dari puncak gunung saja	f
397	100	Kota wisata pintu masuk Danau Toba	t
398	100	Kota industri	f
399	100	Kota pendidikan	f
400	100	Ibukota provinsi	f
401	101	Simalungun	t
402	101	Toba	f
403	101	Samosir	f
404	101	Dairi	f
405	102	Seruni	t
406	102	Butet	f
407	102	Sari	f
408	102	Toba	f
409	103	Parapat	t
410	103	Balige	f
411	103	Samosir	f
412	103	Tarutung	f
413	104	Formasi batuan vulkanik yang unik	t
414	104	Mengandung emas	f
415	104	Berubah warna setiap hari	f
416	104	Selalu bergerak	f
417	105	Keindahan alam dan wisata utama	t
418	105	Airnya bisa diminum langsung	f
419	105	Ada monster danau	f
420	105	Sering ada tsunami	f
421	106	Batak Toba	t
422	106	Melayu	f
423	106	Minangkabau	f
424	106	Jawa	f
425	107	Perairan Danau Toba	t
426	107	Laut lepas	f
427	107	Sungai besar	f
428	107	Terowongan bawah air	f
429	108	Danau Toba	t
430	108	Danau Singkarak	f
431	108	Danau Ranau	f
432	108	Danau Poso	f
433	109	Danau Toba	t
434	109	Danau Ranau	f
435	109	Selat Malaka	f
436	109	Danau Kerinci	f
437	110	Melihat batu unik dan mendengar legenda	t
438	110	Berenang di sekitarnya	f
439	110	Mendaki ke puncaknya	f
440	110	Berkemah di bawahnya	f
441	111	Menara pandang	t
442	111	Menara komunikasi	f
443	111	Mercusuar	f
444	111	Menara air	f
445	112	1.300 meter	t
446	112	500 meter	f
447	112	200 meter	f
448	112	3.000 meter	f
449	113	Pulau Samosir	t
450	113	Pulau Nias	f
451	113	Pulau Batam	f
452	113	Pulau Belitung	f
453	114	Sawah, perbukitan, dan Danau Toba	t
454	114	Lautan luas	f
455	114	Kota metropolitan	f
456	114	Gunung berapi aktif	f
457	115	Jalur pegunungan terindah di Sumatera Utara	t
458	115	Jalan tol terpanjang	f
459	115	Jalan berbahaya	f
460	115	Jalan buntu	f
461	116	Desa Tele	t
462	116	Desa Tomok	f
463	116	Desa Balige	f
464	116	Desa Siallagan	f
465	117	Sumatera Utara	t
466	117	Sumatera Barat	f
467	117	Riau	f
468	117	Aceh	f
469	118	Pintu gerbang darat ke Samosir	t
470	118	Tempat belanja oleh-oleh	f
471	118	Bandara kecil	f
472	118	Terminal bus utama	f
473	119	Ketinggian 1.300 MDPL	t
474	119	Ada kamera canggih	f
475	119	Lokasinya di tepi danau	f
476	119	Ada teleskop di menara	f
477	120	Danau Toba	t
478	120	Danau Singkarak	f
479	120	Danau Maninjau	f
480	120	Danau Ranau	f
481	121	Cuaca cerah	t
482	121	Malam hari	f
483	121	Musim hujan	f
484	121	Tengah malam	f
485	122	5-6 jam	t
486	122	30 menit	f
487	122	1 hari penuh	f
488	122	2 jam	f
489	123	Makanan khas Batak dan oleh-oleh	t
490	123	Elektronik	f
491	123	Kendaraan bermotor	f
492	123	Buku pelajaran	f
493	124	Nama tempat dalam bahasa Batak	t
494	124	Berarti danau	f
495	124	Berarti menara	f
496	124	Berarti puncak gunung	f
497	125	Jalan darat	t
498	125	Kapal feri	f
499	125	Perahu dayung	f
500	125	Jembatan gantung	f
501	126	Kabupaten Toba	t
502	126	Kabupaten Samosir	f
503	126	Kabupaten Simalungun	f
504	126	Kabupaten Dairi	f
505	127	Airnya jatuh langsung ke Danau Toba	t
506	127	Berwarna merah	f
507	127	Mengalir ke atas	f
508	127	Berbentuk spiral	f
509	128	Turun bertahap	t
510	128	Air biru	f
511	128	Jatuh deras	f
512	128	Sungai indah	f
513	129	70 meter	t
514	129	10 meter	f
515	129	200 meter	f
516	129	500 meter	f
517	130	Binangalom	t
518	130	Sipisopiso	f
519	130	Sigura-gura	f
520	130	Ponot	f
521	131	Berenang di kolam alami	t
522	131	Mendaki ke puncak	f
523	131	Berselancar	f
524	131	Memancing di laut	f
525	132	Dengan perahu dari tepi danau	t
526	132	Dengan kereta api	f
527	132	Dengan pesawat helikopter	f
528	132	Dengan jalan kaki dari Medan	f
529	133	Danau Toba	t
530	133	Laut Sumatera	f
531	133	Sungai Asahan	f
532	133	Sawah di bawahnya	f
533	134	Danau Toba	t
534	134	Danau Singkarak	f
535	134	Danau Ranau	f
536	134	Danau Poso	f
537	135	Sumatera Utara	t
538	135	Sumatera Barat	f
539	135	Riau	f
540	135	Jambi	f
541	136	Hutan hijau dan Danau Toba	t
542	136	Gedung-gedung tinggi	f
543	136	Padang pasir	f
544	136	Salju abadi	f
545	137	Wisata alam air terjun dan danau	t
546	137	Wisata belanja	f
547	137	Wisata kuliner	f
548	137	Wisata sejarah	f
549	138	Pertemuan air terjun dan Danau Toba	t
550	138	Penggalian manusia	f
551	138	Kawah gunung berapi	f
552	138	Banjir tahunan	f
553	139	Air Terjun Situmurun	t
554	139	Danau Toba	f
555	139	Pulau Samosir	f
556	139	Bukit Holbung	f
557	140	Langsung jatuh ke danau	t
558	140	Airnya berwarna biru	f
559	140	Bisa diminum langsung	f
560	140	Mengalir tanpa henti sepanjang tahun	f
561	141	Patung boneka kayu yang bisa bergerak	t
562	141	Masakan khas Batak	f
563	141	Alat musik Batak	f
564	141	Tarian Batak	f
565	142	Pengganti anak bagi raja tanpa keturunan	t
566	142	Hiasan istana	f
567	142	Senjata perang	f
568	142	Media pertanian	f
569	143	Mekanisme tali yang dikendalikan manusia	t
570	143	Tenaga listrik	f
571	143	Angin	f
572	143	Kekuatan gaib saja	f
573	144	Gondang	t
574	144	Gamelan	f
575	144	Dangdut	f
576	144	Kolintang	f
577	145	Alat musik perkusi tradisional Batak	t
578	145	Jenis makanan Batak	f
579	145	Nama suku Batak	f
580	145	Pakaian adat Batak	f
581	146	Desa Tomok	t
582	146	Huta Siallagan	f
583	146	Balige	f
584	146	Parapat	f
585	147	Ritual pemakaman adat Batak	t
586	147	Ritual pernikahan	f
587	147	Ritual panen	f
588	147	Ritual kelahiran	f
589	148	Tari Tortor	t
590	148	Tari Saman	f
591	148	Tari Kecak	f
592	148	Tari Jaipong	f
593	149	Kayu	t
594	149	Batu	f
595	149	Tanah liat	f
596	149	Logam	f
597	150	Penghormatan kepada leluhur	t
598	150	Lambang kekayaan	f
599	150	Simbol keberanian	f
600	150	Tanda kesuburan tanah	f
601	151	Desa Tomok, Pulau Samosir	t
602	151	Kota Medan	f
603	151	Jakarta	f
604	151	Museum Nasional	f
605	152	Gondang Sabangunan	t
606	152	Gamelan Jawa	f
607	152	Jazz modern	f
608	152	Pop Indonesia	f
609	153	Sigale-gale	t
610	153	Ondel-ondel	f
611	153	Barong	f
612	153	Wayang	f
613	154	Keunikan boneka kayu yang bisa menari	t
614	154	Harganya sangat mahal	f
615	154	Bisa berbicara sendiri	f
616	154	Dibuat dari emas	f
617	155	Pulau Samosir	t
618	155	Pulau Nias	f
619	155	Pulau Batam	f
620	155	Pulau Jawa	f
\.


--
-- TOC entry 5317 (class 0 OID 69594)
-- Dependencies: 240
-- Data for Name: location_questions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.location_questions (id, location_id, question, explanation, is_active, created_by_admin_id, updated_by_admin_id, created_at, updated_at) FROM stdin;
1	1	Di kota manakah kampus IT Del berada?	IT Del berada di Sitoluama, Laguboti.	t	\N	\N	2026-07-24 17:34:25.765259	2026-07-24 17:34:25.765259
2	1	Siapakah pendiri yayasan Del?	Luhut Binsar Pandjaitan.	t	\N	\N	2026-07-24 17:34:25.765259	2026-07-24 17:34:25.765259
3	1	Apa warna dominan logo IT Del?	Biru dan Putih.	t	\N	\N	2026-07-24 17:34:25.765259	2026-07-24 17:34:25.765259
4	1	IT Del terletak di dekat danau apa?	Danau Toba.	t	\N	\N	2026-07-24 17:34:25.765259	2026-07-24 17:34:25.765259
5	1	Apa nama desa tempat IT Del berada?	Sitoluama.	t	\N	\N	2026-07-24 17:34:25.765259	2026-07-24 17:34:25.765259
6	1	Apa kepanjangan dari IT Del?	IT Del merupakan singkatan dari Institut Teknologi Del.	t	\N	\N	2026-08-10 12:19:17.088335	2026-08-10 12:19:17.088335
7	1	Di kota mana IT Del berada?	IT Del berlokasi di Laguboti, Kabupaten Toba, Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.108053	2026-08-10 12:19:17.108053
8	1	Siapa pendiri IT Del?	IT Del didirikan oleh Luhut Binsar Pandjaitan pada tahun 2001.	t	\N	\N	2026-08-10 12:19:17.114473	2026-08-10 12:19:17.114473
9	1	Tahun berapa IT Del diresmikan?	IT Del diresmikan oleh Presiden Susilo Bambang Yudhoyono pada tahun 2012.	t	\N	\N	2026-08-10 12:19:17.122861	2026-08-10 12:19:17.122861
10	1	IT Del terletak di tepi danau apa?	IT Del terletak di tepi Danau Toba yang merupakan danau vulkanik terbesar di dunia.	t	\N	\N	2026-08-10 12:19:17.128531	2026-08-10 12:19:17.128531
11	1	Apa yang membuat IT Del unik dibanding kampus lain?	IT Del terletak di kawasan wisata Danau Toba dan fokus pada teknologi untuk daerah.	t	\N	\N	2026-08-10 12:19:17.1359	2026-08-10 12:19:17.1359
12	1	Program studi apa yang pertama ada di IT Del?	IT Del awalnya membuka program Diploma III Teknik Informatika.	t	\N	\N	2026-08-10 12:19:17.141278	2026-08-10 12:19:17.141278
13	1	IT Del berada di provinsi mana?	IT Del berada di Provinsi Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.146959	2026-08-10 12:19:17.146959
14	1	Apa visi utama IT Del?	IT Del bertujuan menjadi perguruan tinggi teknologi terkemuka yang berkontribusi pada kemajuan bangsa.	t	\N	\N	2026-08-10 12:19:17.156096	2026-08-10 12:19:17.156096
15	1	Danau Toba terbentuk akibat apa?	Danau Toba terbentuk akibat letusan supervulkan sekitar 74.000 tahun yang lalu.	t	\N	\N	2026-08-10 12:19:17.162541	2026-08-10 12:19:17.162541
16	1	Apa nama kabupaten tempat IT Del berada?	IT Del berada di Kabupaten Toba, Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.176994	2026-08-10 12:19:17.176994
17	1	Siapa presiden yang meresmikan IT Del?	IT Del diresmikan oleh Presiden Susilo Bambang Yudhoyono pada tahun 2012.	t	\N	\N	2026-08-10 12:19:17.185856	2026-08-10 12:19:17.185856
18	1	IT Del merupakan kampus berbasis apa?	IT Del merupakan kampus berbasis teknologi yang fokus pada inovasi dan pengembangan SDM.	t	\N	\N	2026-08-10 12:19:17.191534	2026-08-10 12:19:17.191534
19	1	Apa nama danau yang ada di dekat IT Del?	Danau Toba adalah danau vulkanik terbesar di dunia yang berada dekat IT Del.	t	\N	\N	2026-08-10 12:19:17.198095	2026-08-10 12:19:17.198095
20	1	IT Del sering disebut sebagai apa oleh masyarakat sekitar?	IT Del sering disebut sebagai kampus di tepi danau karena lokasinya yang indah di tepi Danau Toba.	t	\N	\N	2026-08-10 12:19:17.204772	2026-08-10 12:19:17.204772
21	2	Di kota mana Pantai Bulbul berada?	Pantai Bulbul terletak di Balige, ibu kota Kabupaten Toba.	t	\N	\N	2026-08-10 12:19:17.210674	2026-08-10 12:19:17.210674
22	2	Apa yang terkenal dari Pantai Bulbul?	Pantai Bulbul terkenal dengan pasir putih halus dan pemandangan Danau Toba yang indah.	t	\N	\N	2026-08-10 12:19:17.216924	2026-08-10 12:19:17.216924
23	2	Danau Toba merupakan danau vulkanik terbesar di mana?	Danau Toba adalah danau vulkanik terbesar di dunia.	t	\N	\N	2026-08-10 12:19:17.223717	2026-08-10 12:19:17.223717
24	2	Kegiatan apa yang populer di Pantai Bulbul?	Berenang dan menikmati pemandangan Danau Toba adalah aktivitas paling populer di Pantai Bulbul.	t	\N	\N	2026-08-10 12:19:17.230399	2026-08-10 12:19:17.230399
25	2	Balige merupakan ibu kota kabupaten apa?	Balige adalah ibu kota Kabupaten Toba di Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.23661	2026-08-10 12:19:17.23661
26	2	Apa warna pasir di Pantai Bulbul?	Pantai Bulbul memiliki pasir berwarna putih yang halus dan bersih.	t	\N	\N	2026-08-10 12:19:17.242389	2026-08-10 12:19:17.242389
27	2	Danau Toba terbentuk berapa tahun lalu?	Danau Toba terbentuk sekitar 74.000 tahun lalu akibat letusan supervulkan.	t	\N	\N	2026-08-10 12:19:17.247823	2026-08-10 12:19:17.247823
28	2	Apa nama festival yang sering diadakan di kawasan Danau Toba?	Festival Danau Toba adalah festival budaya tahunan yang diadakan di kawasan Danau Toba.	t	\N	\N	2026-08-10 12:19:17.25537	2026-08-10 12:19:17.25537
29	2	Pantai Bulbul cocok untuk wisata keluarga karena?	Pantai Bulbul cocok untuk keluarga karena airnya tenang dan pantainya bersih.	t	\N	\N	2026-08-10 12:19:17.261263	2026-08-10 12:19:17.261263
30	2	Apa nama pulau yang berada di tengah Danau Toba?	Pulau Samosir adalah pulau vulkanik yang berada di tengah Danau Toba.	t	\N	\N	2026-08-10 12:19:17.269445	2026-08-10 12:19:17.269445
31	2	Balige terletak di provinsi mana?	Balige berada di Provinsi Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.289365	2026-08-10 12:19:17.289365
32	2	Apa yang membuat air Danau Toba berwarna biru?	Air Danau Toba berwarna biru karena sangat jernih dan dalam, memantulkan warna langit.	t	\N	\N	2026-08-10 12:19:17.308792	2026-08-10 12:19:17.308792
33	2	Berapa kedalaman maksimum Danau Toba?	Danau Toba memiliki kedalaman maksimum sekitar 505 meter.	t	\N	\N	2026-08-10 12:19:17.32374	2026-08-10 12:19:17.32374
34	2	Transportasi apa yang digunakan untuk ke Pulau Samosir dari Balige?	Kapal feri digunakan untuk menyeberang dari daratan menuju Pulau Samosir.	t	\N	\N	2026-08-10 12:19:17.330884	2026-08-10 12:19:17.330884
35	2	Apa nama suku yang mendiami kawasan Danau Toba?	Suku Batak Toba adalah suku utama yang mendiami kawasan sekitar Danau Toba.	t	\N	\N	2026-08-10 12:19:17.342001	2026-08-10 12:19:17.342001
36	3	Apa nama lengkap Museum Batak di Balige?	Museum Batak TB Silalahi Center adalah museum budaya Batak yang terletak di Balige.	t	\N	\N	2026-08-10 12:19:17.358129	2026-08-10 12:19:17.358129
37	3	Siapa yang mendirikan Museum Batak TB Silalahi Center?	Museum ini didirikan oleh TB Silalahi, seorang tokoh militer dan politisi asal Batak.	t	\N	\N	2026-08-10 12:19:17.367664	2026-08-10 12:19:17.367664
38	3	Apa yang disimpan di Museum Batak?	Museum Batak menyimpan lebih dari 1.000 koleksi benda budaya Batak Toba.	t	\N	\N	2026-08-10 12:19:17.376967	2026-08-10 12:19:17.376967
39	3	Kain tradisional Batak yang disimpan di museum disebut?	Ulos adalah kain tenun tradisional Batak Toba yang menjadi koleksi utama museum.	t	\N	\N	2026-08-10 12:19:17.390385	2026-08-10 12:19:17.390385
40	3	Ukiran khas yang menghiasi rumah adat Batak disebut?	Gorga adalah ukiran tradisional khas Batak yang biasa menghiasi rumah adat.	t	\N	\N	2026-08-10 12:19:17.402909	2026-08-10 12:19:17.402909
41	3	Museum Batak berada di kota mana?	Museum Batak TB Silalahi Center berada di Balige, Kabupaten Toba.	t	\N	\N	2026-08-10 12:19:17.411364	2026-08-10 12:19:17.411364
42	3	Senjata tradisional Batak apa yang ada di museum?	Piso Gaja Dompak adalah pedang pusaka khas Batak Toba yang tersimpan di museum.	t	\N	\N	2026-08-10 12:19:17.422348	2026-08-10 12:19:17.422348
43	3	Apa fungsi museum bagi masyarakat?	Museum berfungsi sebagai tempat pelestarian dan edukasi budaya bagi generasi penerus.	t	\N	\N	2026-08-10 12:19:17.431722	2026-08-10 12:19:17.431722
44	3	Berapa jumlah koleksi di Museum Batak?	Museum Batak TB Silalahi Center menyimpan lebih dari 1.000 koleksi benda budaya.	t	\N	\N	2026-08-10 12:19:17.440413	2026-08-10 12:19:17.440413
87	6	Desa Tomok merupakan desa tertua ke berapa di Samosir?	Desa Tomok merupakan salah satu desa tertua di Pulau Samosir.	t	\N	\N	2026-08-10 12:19:17.751603	2026-08-10 12:19:17.751603
45	3	Apa nama alat musik tradisional Batak yang ada di museum?	Hasapi adalah alat musik petik tradisional Batak Toba yang tersimpan di Museum Batak.	t	\N	\N	2026-08-10 12:19:17.44672	2026-08-10 12:19:17.44672
46	3	Pakaian adat Batak yang paling dikenal adalah?	Ulos adalah pakaian adat Batak Toba yang paling dikenal dan sering dikenakan dalam upacara.	t	\N	\N	2026-08-10 12:19:17.457346	2026-08-10 12:19:17.457346
47	3	Apa keistimewaan Museum Batak TB Silalahi?	Museum ini memiliki koleksi terlengkap tentang budaya Batak Toba di Indonesia.	t	\N	\N	2026-08-10 12:19:17.463517	2026-08-10 12:19:17.463517
48	3	Di museum, pengunjung bisa melihat apa?	Pengunjung bisa melihat koleksi pakaian adat, senjata, alat musik, dan peralatan rumah tangga Batak.	t	\N	\N	2026-08-10 12:19:17.470728	2026-08-10 12:19:17.470728
49	3	Siapa TB Silalahi?	TB Silalahi adalah tokoh militer, politisi, dan pengusaha asal Batak Toba yang mendirikan museum.	t	\N	\N	2026-08-10 12:19:17.476402	2026-08-10 12:19:17.476402
50	3	Museum Batak terletak di tepi apa?	Museum Batak TB Silalahi Center terletak di tepi Danau Toba dengan pemandangan yang indah.	t	\N	\N	2026-08-10 12:19:17.482206	2026-08-10 12:19:17.482206
51	4	Huta Siallagan terletak di pulau mana?	Huta Siallagan terletak di Pulau Samosir, di tengah Danau Toba.	t	\N	\N	2026-08-10 12:19:17.488754	2026-08-10 12:19:17.488754
52	4	Apa yang terkenal dari Huta Siallagan?	Huta Siallagan terkenal dengan Batu Kursi Raja yang digunakan untuk pengadilan adat.	t	\N	\N	2026-08-10 12:19:17.494318	2026-08-10 12:19:17.494318
53	4	Siapa yang biasa duduk di Batu Kursi Siallagan?	Raja adat Siallagan yang duduk di batu kursi untuk mengadili perkara adat.	t	\N	\N	2026-08-10 12:19:17.502849	2026-08-10 12:19:17.502849
54	4	Huta Siallagan merupakan desa adat suku apa?	Huta Siallagan adalah desa adat suku Batak Toba yang masih terjaga keasliannya.	t	\N	\N	2026-08-10 12:19:17.508796	2026-08-10 12:19:17.508796
55	4	Apa arti kata Huta dalam bahasa Batak?	Huta berarti desa atau kampung dalam bahasa Batak Toba.	t	\N	\N	2026-08-10 12:19:17.514784	2026-08-10 12:19:17.514784
56	4	Pulau Samosir merupakan pulau terbesar di dalam danau di mana?	Pulau Samosir adalah pulau terbesar yang berada di dalam danau di dunia.	t	\N	\N	2026-08-10 12:19:17.5214	2026-08-10 12:19:17.5214
57	4	Batu Kursi di Huta Siallagan digunakan untuk?	Batu Kursi digunakan sebagai tempat pengadilan adat dan permusyawaratan raja.	t	\N	\N	2026-08-10 12:19:17.527469	2026-08-10 12:19:17.527469
58	4	Apa nama marga yang mendiami Huta Siallagan?	Marga Siallagan adalah marga yang secara turun-temurun mendiami desa ini.	t	\N	\N	2026-08-10 12:19:17.534374	2026-08-10 12:19:17.534374
59	4	Luas Pulau Samosir sekitar berapa km persegi?	Pulau Samosir memiliki luas sekitar 630 kilometer persegi.	t	\N	\N	2026-08-10 12:19:17.539963	2026-08-10 12:19:17.539963
60	4	Apa yang dimaksud dengan adat Batak?	Adat Batak adalah sistem aturan dan tradisi yang mengatur kehidupan masyarakat Batak Toba.	t	\N	\N	2026-08-10 12:19:17.54644	2026-08-10 12:19:17.54644
61	4	Huta Siallagan dapat dicapai dengan cara apa dari Parapat?	Huta Siallagan di Pulau Samosir dapat dicapai dengan naik kapal feri dari Parapat.	t	\N	\N	2026-08-10 12:19:17.555657	2026-08-10 12:19:17.555657
62	4	Sistem kekerabatan Batak disebut?	Dalihan Na Tolu adalah sistem kekerabatan Batak yang mengatur hubungan antar keluarga.	t	\N	\N	2026-08-10 12:19:17.564611	2026-08-10 12:19:17.564611
63	4	Rumah adat di Huta Siallagan disebut?	Rumah Bolon adalah rumah adat tradisional Batak Toba yang ada di Huta Siallagan.	t	\N	\N	2026-08-10 12:19:17.572306	2026-08-10 12:19:17.572306
64	4	Marga dalam adat Batak diturunkan dari?	Marga Batak diturunkan dari garis keturunan ayah (patrilineal).	t	\N	\N	2026-08-10 12:19:17.579134	2026-08-10 12:19:17.579134
65	4	Apa salam khas masyarakat Batak?	Horas adalah ucapan salam khas masyarakat Batak yang berarti sehat dan sejahtera.	t	\N	\N	2026-08-10 12:19:17.586493	2026-08-10 12:19:17.586493
66	5	Bukit Holbung mendapat julukan apa?	Bukit Holbung dijuluki Bukit Teletubbies karena bentuknya yang bergelombang hijau.	t	\N	\N	2026-08-10 12:19:17.59309	2026-08-10 12:19:17.59309
67	5	Bukit Holbung terletak di kabupaten mana?	Bukit Holbung terletak di Kabupaten Samosir, Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.600932	2026-08-10 12:19:17.600932
68	5	Ketinggian Bukit Holbung sekitar berapa meter?	Bukit Holbung berada pada ketinggian sekitar 1.000 meter di atas permukaan laut.	t	\N	\N	2026-08-10 12:19:17.608045	2026-08-10 12:19:17.608045
69	5	Apa yang bisa dilihat dari puncak Bukit Holbung?	Dari Bukit Holbung terlihat panorama Danau Toba 360 derajat yang menakjubkan.	t	\N	\N	2026-08-10 12:19:17.614203	2026-08-10 12:19:17.614203
70	5	Waktu terbaik mengunjungi Bukit Holbung adalah?	Bukit Holbung paling indah dikunjungi saat matahari terbit dan terbenam.	t	\N	\N	2026-08-10 12:19:17.621902	2026-08-10 12:19:17.621902
71	5	Bukit Holbung berada di Pulau apa?	Bukit Holbung berada di Pulau Samosir yang terletak di tengah Danau Toba.	t	\N	\N	2026-08-10 12:19:17.63685	2026-08-10 12:19:17.63685
72	5	Apa yang membuat Bukit Holbung unik?	Bukit Holbung unik karena bentuknya yang bergelombang seperti bukit dalam serial Teletubbies.	t	\N	\N	2026-08-10 12:19:17.642585	2026-08-10 12:19:17.642585
73	5	Danau Toba terlihat dari Bukit Holbung karena?	Danau Toba terlihat jelas dari Bukit Holbung karena posisinya yang tinggi dan pemandangan terbuka.	t	\N	\N	2026-08-10 12:19:17.647493	2026-08-10 12:19:17.647493
74	5	Apa nama danau yang terlihat dari Bukit Holbung?	Danau Toba adalah danau vulkanik terbesar di dunia yang terlihat dari Bukit Holbung.	t	\N	\N	2026-08-10 12:19:17.653473	2026-08-10 12:19:17.653473
75	5	Bukit Holbung cocok untuk kegiatan apa?	Bukit Holbung cocok untuk hiking, fotografi, dan menikmati pemandangan alam.	t	\N	\N	2026-08-10 12:19:17.661059	2026-08-10 12:19:17.661059
76	5	Samosir merupakan pulau di dalam danau terbesar di mana?	Samosir adalah pulau terbesar yang berada di dalam danau di dunia.	t	\N	\N	2026-08-10 12:19:17.669886	2026-08-10 12:19:17.669886
77	5	Apa warna dominan pemandangan di Bukit Holbung?	Pemandangan Bukit Holbung didominasi warna hijau dari rumput dan perbukitan.	t	\N	\N	2026-08-10 12:19:17.680882	2026-08-10 12:19:17.680882
78	5	Jalur menuju Bukit Holbung bisa ditempuh dari kota mana?	Bukit Holbung bisa dicapai dari Pangururan, ibu kota Kabupaten Samosir.	t	\N	\N	2026-08-10 12:19:17.694305	2026-08-10 12:19:17.694305
79	5	Bukit Holbung terletak di ketinggian berapa MDPL?	Bukit Holbung berada di sekitar 1.000 meter di atas permukaan laut.	t	\N	\N	2026-08-10 12:19:17.701982	2026-08-10 12:19:17.701982
80	5	Kenapa Bukit Holbung ramai dikunjungi?	Bukit Holbung ramai karena pemandangannya yang indah dan unik, cocok untuk foto.	t	\N	\N	2026-08-10 12:19:17.708361	2026-08-10 12:19:17.708361
81	6	Desa Tomok terkenal dengan apa?	Desa Tomok terkenal dengan Makam Raja Sidabutar yang bersejarah dan berusia lebih dari 500 tahun.	t	\N	\N	2026-08-10 12:19:17.714121	2026-08-10 12:19:17.714121
82	6	Makam Raja Sidabutar berusia sekitar berapa tahun?	Makam Raja Sidabutar diperkirakan berusia lebih dari 500 tahun.	t	\N	\N	2026-08-10 12:19:17.721015	2026-08-10 12:19:17.721015
83	6	Desa Tomok terletak di pulau mana?	Desa Tomok berada di Pulau Samosir, di tengah Danau Toba.	t	\N	\N	2026-08-10 12:19:17.726834	2026-08-10 12:19:17.726834
84	6	Apa yang bisa dibeli di Desa Tomok?	Desa Tomok terkenal sebagai pusat kerajinan tangan Batak seperti ulos dan ukiran kayu.	t	\N	\N	2026-08-10 12:19:17.733787	2026-08-10 12:19:17.733787
85	6	Siapa Raja Sidabutar?	Raja Sidabutar adalah raja Batak Toba yang memerintah di wilayah Tomok, Samosir.	t	\N	\N	2026-08-10 12:19:17.739442	2026-08-10 12:19:17.739442
86	6	Sigale-gale pertama kali dibuat di mana?	Sigale-gale, boneka kayu yang bisa bergerak, pertama kali dibuat di Desa Tomok.	t	\N	\N	2026-08-10 12:19:17.744887	2026-08-10 12:19:17.744887
88	6	Ulos dari Tomok dikenal sebagai?	Ulos dari Tomok dikenal sebagai salah satu ulos terbaik dengan kualitas tenun yang tinggi.	t	\N	\N	2026-08-10 12:19:17.756971	2026-08-10 12:19:17.756971
89	6	Apa bahasa yang digunakan masyarakat Tomok sehari-hari?	Masyarakat Tomok menggunakan Bahasa Batak Toba dalam kehidupan sehari-hari.	t	\N	\N	2026-08-10 12:19:17.762591	2026-08-10 12:19:17.762591
90	6	Makam Raja Sidabutar terbuat dari apa?	Makam Raja Sidabutar terbuat dari batu andesit yang diukir dengan motif Batak.	t	\N	\N	2026-08-10 12:19:17.76806	2026-08-10 12:19:17.76806
91	6	Desa Tomok merupakan gerbang masuk utama ke Samosir dari mana?	Desa Tomok menjadi gerbang masuk utama ke Samosir dari Parapat via feri.	t	\N	\N	2026-08-10 12:19:17.773633	2026-08-10 12:19:17.773633
92	6	Apa nama tarian yang dipentaskan di Desa Tomok?	Tari Tortor adalah tarian tradisional Batak Toba yang sering dipentaskan di Tomok.	t	\N	\N	2026-08-10 12:19:17.779743	2026-08-10 12:19:17.779743
93	6	Sigale-gale adalah?	Sigale-gale adalah patung boneka kayu yang dapat bergerak dalam upacara adat Batak.	t	\N	\N	2026-08-10 12:19:17.787566	2026-08-10 12:19:17.787566
94	6	Kerajinan ukiran kayu Batak di Tomok biasanya bermotif apa?	Ukiran kayu Batak di Tomok biasanya bermotif gorga, ukiran tradisional khas Batak.	t	\N	\N	2026-08-10 12:19:17.795984	2026-08-10 12:19:17.795984
95	6	Berapa lama perjalanan feri dari Parapat ke Tomok?	Perjalanan feri dari Parapat ke Tomok memakan waktu sekitar 30-45 menit.	t	\N	\N	2026-08-10 12:19:17.803579	2026-08-10 12:19:17.803579
96	7	Batu Gantung terletak di kota mana?	Batu Gantung terletak di Parapat, Kabupaten Simalungun.	t	\N	\N	2026-08-10 12:19:17.81051	2026-08-10 12:19:17.81051
97	7	Apa legenda di balik Batu Gantung?	Legenda Batu Gantung bercerita tentang seorang gadis bernama Seruni yang bersedih.	t	\N	\N	2026-08-10 12:19:17.818103	2026-08-10 12:19:17.818103
98	7	Batu Gantung adalah formasi batu yang?	Batu Gantung adalah formasi batu unik yang menggantung di tepi Danau Toba.	t	\N	\N	2026-08-10 12:19:17.824609	2026-08-10 12:19:17.824609
99	7	Dari mana Batu Gantung bisa dilihat?	Batu Gantung dapat dilihat dari kapal feri yang melintasi Danau Toba.	t	\N	\N	2026-08-10 12:19:17.830441	2026-08-10 12:19:17.830441
100	7	Parapat terkenal sebagai apa?	Parapat terkenal sebagai kota wisata dan pintu masuk utama ke Danau Toba.	t	\N	\N	2026-08-10 12:19:17.837551	2026-08-10 12:19:17.837551
101	7	Batu Gantung berada di kabupaten mana?	Batu Gantung berada di Kabupaten Simalungun, Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.84458	2026-08-10 12:19:17.84458
102	7	Nama gadis dalam legenda Batu Gantung adalah?	Nama gadis dalam legenda Batu Gantung adalah Seruni.	t	\N	\N	2026-08-10 12:19:17.851343	2026-08-10 12:19:17.851343
103	7	Batu Gantung menjadi ikon wisata kota?	Batu Gantung menjadi salah satu ikon wisata kota Parapat di tepi Danau Toba.	t	\N	\N	2026-08-10 12:19:17.859777	2026-08-10 12:19:17.859777
104	7	Apa yang membuat Batu Gantung menarik secara geologi?	Batu Gantung menarik secara geologi karena merupakan formasi batuan vulkanik yang unik.	t	\N	\N	2026-08-10 12:19:17.86597	2026-08-10 12:19:17.86597
105	7	Danau Toba di sekitar Parapat terkenal karena?	Danau Toba di Parapat terkenal karena keindahannya dan menjadi destinasi wisata utama.	t	\N	\N	2026-08-10 12:19:17.872784	2026-08-10 12:19:17.872784
106	7	Legenda Batu Gantung berasal dari suku apa?	Legenda Batu Gantung berasal dari tradisi lisan masyarakat Batak Toba.	t	\N	\N	2026-08-10 12:19:17.878643	2026-08-10 12:19:17.878643
107	7	Jalur feri dari Parapat menuju Samosir melewati?	Jalur feri dari Parapat melewati perairan Danau Toba dan melewati area Batu Gantung.	t	\N	\N	2026-08-10 12:19:17.885479	2026-08-10 12:19:17.885479
108	7	Apa nama danau tempat Batu Gantung berada?	Batu Gantung berada di tepi Danau Toba, danau vulkanik terbesar di dunia.	t	\N	\N	2026-08-10 12:19:17.891218	2026-08-10 12:19:17.891218
109	7	Kabupaten Simalungun berbatasan dengan danau apa?	Kabupaten Simalungun berbatasan langsung dengan Danau Toba.	t	\N	\N	2026-08-10 12:19:17.896752	2026-08-10 12:19:17.896752
110	7	Batu Gantung sering dikunjungi wisatawan untuk apa?	Wisatawan mengunjungi Batu Gantung untuk melihat formasi batu unik dan mendengar legendanya.	t	\N	\N	2026-08-10 12:19:17.905652	2026-08-10 12:19:17.905652
111	8	Menara Tele berfungsi sebagai apa?	Menara Tele berfungsi sebagai menara pandang untuk menikmati panorama Danau Toba.	t	\N	\N	2026-08-10 12:19:17.911336	2026-08-10 12:19:17.911336
112	8	Menara Tele berada di ketinggian sekitar berapa meter?	Menara Tele berada di ketinggian sekitar 1.300 meter di atas permukaan laut.	t	\N	\N	2026-08-10 12:19:17.920957	2026-08-10 12:19:17.920957
113	8	Tele merupakan gerbang masuk ke pulau mana?	Tele adalah gerbang masuk ke Pulau Samosir dari daratan Sumatera melalui jalan darat.	t	\N	\N	2026-08-10 12:19:17.926737	2026-08-10 12:19:17.926737
114	8	Dari Menara Tele apa yang bisa dilihat?	Dari Menara Tele terlihat hamparan sawah, perbukitan, dan panorama Danau Toba.	t	\N	\N	2026-08-10 12:19:17.932418	2026-08-10 12:19:17.932418
115	8	Jalan Tele dikenal sebagai?	Jalan Tele dikenal sebagai salah satu jalur pegunungan terindah di Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.93913	2026-08-10 12:19:17.93913
116	8	Menara Tele berada di desa bernama?	Menara Tele berada di Desa Tele, Kabupaten Samosir.	t	\N	\N	2026-08-10 12:19:17.94461	2026-08-10 12:19:17.94461
117	8	Kabupaten Samosir terletak di provinsi mana?	Kabupaten Samosir terletak di Provinsi Sumatera Utara.	t	\N	\N	2026-08-10 12:19:17.960908	2026-08-10 12:19:17.960908
118	8	Mengapa Tele penting bagi wisatawan ke Samosir?	Tele adalah pintu gerbang darat satu-satunya menuju Samosir tanpa harus naik feri.	t	\N	\N	2026-08-10 12:19:17.97089	2026-08-10 12:19:17.97089
119	8	Pemandangan sawah dari Menara Tele terlihat karena?	Ketinggian Menara Tele yang mencapai 1.300 MDPL memungkinkan melihat hamparan sawah.	t	\N	\N	2026-08-10 12:19:17.977367	2026-08-10 12:19:17.977367
120	8	Apa nama danau yang terlihat dari Menara Tele?	Danau Toba adalah danau vulkanik terbesar di dunia yang terlihat dari Menara Tele.	t	\N	\N	2026-08-10 12:19:17.983947	2026-08-10 12:19:17.983947
121	8	Menara Tele cocok dikunjungi saat?	Menara Tele paling indah dikunjungi saat cuaca cerah agar pemandangan terlihat jelas.	t	\N	\N	2026-08-10 12:19:17.98963	2026-08-10 12:19:17.98963
122	8	Berapa lama perjalanan dari Medan ke Menara Tele?	Perjalanan dari Medan ke Menara Tele sekitar 5-6 jam dengan kendaraan darat.	t	\N	\N	2026-08-10 12:19:17.995646	2026-08-10 12:19:17.995646
123	8	Apa yang dijual di sekitar Menara Tele?	Di sekitar Menara Tele biasa dijual makanan khas Batak dan oleh-oleh Samosir.	t	\N	\N	2026-08-10 12:19:18.001657	2026-08-10 12:19:18.001657
124	8	Tele berarti apa dalam bahasa setempat?	Tele adalah nama tempat yang berasal dari bahasa Batak lokal.	t	\N	\N	2026-08-10 12:19:18.008585	2026-08-10 12:19:18.008585
125	8	Pulau Samosir bisa dicapai dari Tele dengan?	Dari Tele, Pulau Samosir bisa dicapai melalui jalan darat yang melintasi perbatasan.	t	\N	\N	2026-08-10 12:19:18.014597	2026-08-10 12:19:18.014597
126	9	Air Terjun Situmurun terletak di kabupaten mana?	Air Terjun Situmurun terletak di Kabupaten Toba, Sumatera Utara.	t	\N	\N	2026-08-10 12:19:18.021357	2026-08-10 12:19:18.021357
127	9	Apa keunikan Air Terjun Situmurun?	Air Terjun Situmurun unik karena airnya langsung jatuh ke perairan Danau Toba.	t	\N	\N	2026-08-10 12:19:18.028698	2026-08-10 12:19:18.028698
128	9	Situmurun berarti apa dalam bahasa Batak?	Situmurun berarti turun bertahap dalam bahasa Batak Toba.	t	\N	\N	2026-08-10 12:19:18.03735	2026-08-10 12:19:18.03735
129	9	Ketinggian Air Terjun Situmurun sekitar berapa meter?	Air Terjun Situmurun memiliki ketinggian sekitar 70 meter.	t	\N	\N	2026-08-10 12:19:18.044049	2026-08-10 12:19:18.044049
130	9	Nama lain Air Terjun Situmurun adalah?	Air Terjun Situmurun juga dikenal dengan nama Binangalom.	t	\N	\N	2026-08-10 12:19:18.050554	2026-08-10 12:19:18.050554
131	9	Wisatawan bisa melakukan apa di Air Terjun Situmurun?	Wisatawan bisa berenang di kolam alami dan menikmati kesegaran air terjun.	t	\N	\N	2026-08-10 12:19:18.060981	2026-08-10 12:19:18.060981
132	9	Cara terbaik mencapai Air Terjun Situmurun adalah?	Air Terjun Situmurun paling mudah dicapai dengan perahu dari tepi Danau Toba.	t	\N	\N	2026-08-10 12:19:18.070738	2026-08-10 12:19:18.070738
133	9	Air Terjun Situmurun mengalir ke mana?	Air Terjun Situmurun mengalir langsung ke perairan Danau Toba.	t	\N	\N	2026-08-10 12:19:18.077941	2026-08-10 12:19:18.077941
134	9	Apa nama danau yang menjadi tujuan aliran air terjun ini?	Danau Toba adalah danau vulkanik terbesar di dunia yang menjadi tujuan aliran Situmurun.	t	\N	\N	2026-08-10 12:19:18.085288	2026-08-10 12:19:18.085288
135	9	Air Terjun Situmurun berada di provinsi mana?	Air Terjun Situmurun berada di Provinsi Sumatera Utara.	t	\N	\N	2026-08-10 12:19:18.092045	2026-08-10 12:19:18.092045
136	9	Pemandangan di sekitar Air Terjun Situmurun didominasi oleh?	Pemandangan sekitar Air Terjun Situmurun didominasi oleh hutan hijau dan perairan Danau Toba.	t	\N	\N	2026-08-10 12:19:18.097671	2026-08-10 12:19:18.097671
137	9	Air Terjun Situmurun termasuk kategori wisata apa?	Situmurun termasuk wisata alam yang menggabungkan keindahan air terjun dan danau.	t	\N	\N	2026-08-10 12:19:18.104839	2026-08-10 12:19:18.104839
138	9	Kolam alami di bawah Air Terjun Situmurun terbentuk dari?	Kolam alami terbentuk dari pertemuan air terjun dengan permukaan Danau Toba.	t	\N	\N	2026-08-10 12:19:18.110447	2026-08-10 12:19:18.110447
139	9	Binangalom adalah nama lain dari?	Binangalom adalah nama lain dari Air Terjun Situmurun di Kabupaten Toba.	t	\N	\N	2026-08-10 12:19:18.117484	2026-08-10 12:19:18.117484
140	9	Apa yang membuat Air Terjun Situmurun berbeda dari air terjun lain?	Keunikannya adalah langsung jatuh ke danau, bukan ke sungai atau kolam biasa.	t	\N	\N	2026-08-10 12:19:18.124043	2026-08-10 12:19:18.124043
141	10	Sigale-gale adalah?	Sigale-gale adalah patung boneka kayu yang dapat bergerak dalam upacara adat Batak Toba.	t	\N	\N	2026-08-10 12:19:18.130953	2026-08-10 12:19:18.130953
142	10	Untuk apa Sigale-gale dibuat?	Sigale-gale dibuat sebagai pengganti anak bagi raja yang tidak memiliki keturunan.	t	\N	\N	2026-08-10 12:19:18.137978	2026-08-10 12:19:18.137978
143	10	Bagaimana Sigale-gale bisa bergerak?	Sigale-gale digerakkan dengan mekanisme tali yang dikendalikan oleh manusia.	t	\N	\N	2026-08-10 12:19:18.143986	2026-08-10 12:19:18.143986
144	10	Upacara Sigale-gale biasanya diiringi musik apa?	Upacara Sigale-gale biasanya diiringi musik gondang Batak Toba.	t	\N	\N	2026-08-10 12:19:18.155978	2026-08-10 12:19:18.155978
145	10	Gondang adalah?	Gondang adalah alat musik perkusi tradisional Batak Toba yang digunakan dalam upacara.	t	\N	\N	2026-08-10 12:19:18.172126	2026-08-10 12:19:18.172126
146	10	Sigale-gale pertama kali ada di desa mana?	Sigale-gale pertama kali dibuat dan digunakan di Desa Tomok, Pulau Samosir.	t	\N	\N	2026-08-10 12:19:18.178301	2026-08-10 12:19:18.178301
147	10	Upacara Sigale-gale termasuk dalam ritual apa?	Upacara Sigale-gale termasuk dalam ritual pemakaman adat Batak Toba.	t	\N	\N	2026-08-10 12:19:18.187682	2026-08-10 12:19:18.187682
148	10	Tari apa yang dilakukan bersama Sigale-gale?	Tari Tortor adalah tarian yang dilakukan bersama Sigale-gale dalam upacara adat.	t	\N	\N	2026-08-10 12:19:18.194358	2026-08-10 12:19:18.194358
149	10	Sigale-gale terbuat dari bahan apa?	Sigale-gale terbuat dari kayu yang diukir menyerupai manusia.	t	\N	\N	2026-08-10 12:19:18.202653	2026-08-10 12:19:18.202653
150	10	Apa makna Sigale-gale bagi masyarakat Batak?	Sigale-gale merupakan simbol penghormatan kepada leluhur dan arwah yang telah meninggal.	t	\N	\N	2026-08-10 12:19:18.209456	2026-08-10 12:19:18.209456
151	10	Di mana wisatawan bisa melihat pertunjukan Sigale-gale?	Wisatawan bisa melihat pertunjukan Sigale-gale di Desa Tomok, Pulau Samosir.	t	\N	\N	2026-08-10 12:19:18.21542	2026-08-10 12:19:18.21542
152	10	Sigale-gale menari mengikuti irama musik apa?	Sigale-gale menari mengikuti irama gondang Sabangunan yang khas.	t	\N	\N	2026-08-10 12:19:18.223544	2026-08-10 12:19:18.223544
153	10	Apa nama boneka kayu yang bisa menari dalam adat Batak?	Sigale-gale adalah nama boneka kayu yang bisa bergerak dan menari dalam upacara Batak.	t	\N	\N	2026-08-10 12:19:18.22937	2026-08-10 12:19:18.22937
154	10	Sigale-gale menjadi daya tarik wisata karena?	Sigale-gale menarik wisatawan karena keunikannya sebagai boneka kayu yang bisa menari.	t	\N	\N	2026-08-10 12:19:18.236176	2026-08-10 12:19:18.236176
155	10	Pulau tempat Sigale-gale berasal adalah?	Sigale-gale berasal dari Pulau Samosir yang terletak di tengah Danau Toba.	t	\N	\N	2026-08-10 12:19:18.242907	2026-08-10 12:19:18.242907
\.


--
-- TOC entry 5305 (class 0 OID 69466)
-- Dependencies: 228
-- Data for Name: locations; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.locations (id, name, description, image, reward_xp, reward_stars, question_count, minimum_correct, level_order, is_active, created_by_admin_id, updated_by_admin_id, deleted_at, created_at, updated_at) FROM stdin;
1	IT Del	Kampus teknologi di pinggir Danau Toba	itdel.png	100	3	5	3	1	t	\N	\N	\N	2026-07-24 17:34:25.765259+07	2026-07-24 17:34:25.765259+07
2	Pantai Bulbul	Pantai Bulbul adalah salah satu pantai indah di Danau Toba yang terletak di Balige. Terkenal dengan pasir putihnya dan pemandangan Danau Toba yang menakjubkan.	pantaibulbul	120	3	5	3	2	t	\N	\N	\N	2026-08-08 14:53:09.521654+07	2026-08-08 14:53:09.521654+07
3	Museum Batak	Museum Batak TB Silalahi Center terletak di Balige, menyimpan ribuan koleksi benda budaya Batak Toba mulai dari pakaian adat, senjata tradisional, hingga peralatan rumah tangga.	museumbatak	140	3	5	3	3	t	\N	\N	\N	2026-08-08 14:53:09.524669+07	2026-08-08 14:53:09.524669+07
4	Huta Siallagan	Huta Siallagan adalah desa tradisional Batak Toba di Pulau Samosir. Terkenal dengan Batu Kursi Raja Siallagan yang digunakan untuk mengadili penjahat pada zaman dahulu.	hutasiallagan	160	4	5	3	4	t	\N	\N	\N	2026-08-08 14:53:09.525902+07	2026-08-08 14:53:09.525902+07
5	Bukit Holbung	Bukit Holbung atau Bukit Teletubbies adalah bukit hijau yang terletak di Kabupaten Samosir. Dari puncaknya, pengunjung dapat menikmati pemandangan Danau Toba yang spektakuler.	bukitholbung	180	4	5	3	5	t	\N	\N	\N	2026-08-08 14:53:09.527183+07	2026-08-08 14:53:09.527183+07
6	Desa Tomok	Desa Tomok adalah salah satu desa tertua di Pulau Samosir. Terkenal dengan Makam Raja Sidabutar yang merupakan situs bersejarah penting bagi masyarakat Batak Toba.	desatomok	200	4	5	3	6	t	\N	\N	\N	2026-08-08 14:53:09.528295+07	2026-08-08 14:53:09.528295+07
7	Batu Gantung	Batu Gantung adalah formasi batu unik yang menggantung di tepi Danau Toba, Parapat. Legenda masyarakat setempat menyebutkan batu ini berasal dari seorang putri yang bersedih.	batugantung	220	4	5	3	7	t	\N	\N	\N	2026-08-08 14:53:09.528895+07	2026-08-08 14:53:09.528895+07
8	Menara Tele	Menara Tele adalah menara pandang yang terletak di Desa Tele, Samosir. Dari menara ini pengunjung dapat melihat panorama Danau Toba dan hamparan sawah yang indah.	menaratele	240	5	5	3	8	t	\N	\N	\N	2026-08-08 14:53:09.529998+07	2026-08-08 14:53:09.529998+07
9	Air Terjun Situmurun	Air Terjun Situmurun atau Binangalom adalah air terjun yang langsung mengalir ke Danau Toba di Kabupaten Toba. Merupakan salah satu air terjun paling indah di kawasan Danau Toba.	situmurun	260	5	5	3	9	t	\N	\N	\N	2026-08-08 14:53:09.532463+07	2026-08-08 14:53:09.532463+07
10	Sigale-gale	Sigale-gale adalah patung boneka kayu yang dapat bergerak dan menari dalam upacara adat Batak Toba. Patung ini merupakan simbol penting dalam ritual pemakaman adat Batak.	sigalegale	300	5	5	3	10	t	\N	\N	\N	2026-08-08 14:53:09.535092+07	2026-08-08 14:53:09.535092+07
\.


--
-- TOC entry 5337 (class 0 OID 69813)
-- Dependencies: 260
-- Data for Name: player_achievements; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_achievements (id, player_id, achievement_id, claimed, obtained_at, claimed_at) FROM stdin;
4	42	1	t	2026-09-09 23:26:26.818143+07	2026-09-09 23:26:29.442425
5	42	5	t	2026-09-09 23:27:27.396474+07	2026-09-09 23:27:32.395787
\.


--
-- TOC entry 5355 (class 0 OID 70678)
-- Dependencies: 278
-- Data for Name: player_challenge_progresses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_challenge_progresses (id, player_id, challenge_id, is_completed, score, completed_at, created_at) FROM stdin;
\.


--
-- TOC entry 5335 (class 0 OID 69793)
-- Dependencies: 258
-- Data for Name: player_gallery_cards; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_gallery_cards (id, player_id, gallery_card_id, obtained_at) FROM stdin;
\.


--
-- TOC entry 5339 (class 0 OID 69834)
-- Dependencies: 262
-- Data for Name: player_location_answers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_location_answers (id, progress_id, question_id, selected_option_id, is_correct, answered_at) FROM stdin;
\.


--
-- TOC entry 5321 (class 0 OID 69634)
-- Dependencies: 244
-- Data for Name: player_location_progress; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_location_progress (id, player_id, location_id, attempt_count, score, correct_answer, wrong_answer, xp_earned, stars_earned, is_unlocked, is_completed, completed_at, created_at, updated_at) FROM stdin;
\.


--
-- TOC entry 5347 (class 0 OID 70238)
-- Dependencies: 270
-- Data for Name: player_location_progresses; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_location_progresses (id, player_id, location_id, attempt_count, score, correct_answer, wrong_answer, xp_earned, stars_earned, is_unlocked, is_completed, completed_at, created_at, updated_at) FROM stdin;
1	24	1	0	0	0	0	0	0	t	f	\N	2026-08-08 14:53:54.478379+07	2026-08-08 14:53:54.478379+07
2	25	1	1	0	3	2	100	3	t	t	2026-08-10 00:55:37.856549+07	2026-08-08 20:17:29.321768+07	2026-08-10 00:55:38.060746+07
3	25	2	1	0	2	3	0	0	t	f	\N	2026-08-10 00:55:37.915379+07	2026-08-10 12:45:38.260396+07
4	26	1	0	0	0	0	0	0	t	f	\N	2026-08-14 15:59:30.7708+07	2026-08-14 15:59:30.7708+07
5	27	1	0	0	0	0	0	0	t	f	\N	2026-08-14 16:00:15.934017+07	2026-08-14 16:00:15.934017+07
6	28	1	0	0	0	0	0	0	t	f	\N	2026-08-14 16:06:34.149741+07	2026-08-14 16:06:34.149741+07
7	29	1	0	0	0	0	0	0	t	f	\N	2026-08-14 16:07:22.40338+07	2026-08-14 16:07:22.40338+07
8	30	1	0	0	0	0	0	0	t	f	\N	2026-08-15 00:42:59.888783+07	2026-08-15 00:42:59.888783+07
9	31	1	0	0	0	0	0	0	t	f	\N	2026-08-15 01:21:24.334394+07	2026-08-15 01:21:24.334394+07
10	32	1	0	0	0	0	0	0	t	f	\N	2026-08-16 20:30:30.514353+07	2026-08-16 20:30:30.514353+07
11	33	1	0	0	0	0	0	0	t	f	\N	2026-08-17 01:59:59.009156+07	2026-08-17 01:59:59.009156+07
12	34	1	0	0	0	0	0	0	t	f	\N	2026-08-17 13:52:35.670917+07	2026-08-17 13:52:35.670917+07
13	35	1	0	0	0	0	0	0	t	f	\N	2026-08-17 14:10:22.291138+07	2026-08-17 14:10:22.291138+07
14	36	1	0	0	0	0	0	0	t	f	\N	2026-08-17 14:45:03.904122+07	2026-08-17 14:45:03.904122+07
15	37	1	0	0	0	0	0	0	t	f	\N	2026-08-17 14:58:03.699103+07	2026-08-17 14:58:03.699103+07
16	38	1	0	0	0	0	0	0	t	f	\N	2026-08-17 15:15:55.853455+07	2026-08-17 15:15:55.853455+07
17	39	1	0	0	0	0	0	0	t	f	\N	2026-08-17 15:22:12.821545+07	2026-08-17 15:22:12.821545+07
18	40	1	0	0	0	0	0	0	t	f	\N	2026-08-17 15:46:47.513804+07	2026-08-17 15:46:47.513804+07
19	41	1	0	0	0	0	0	0	t	f	\N	2026-09-08 23:48:34.6223+07	2026-09-08 23:48:34.6223+07
21	42	2	0	0	0	0	0	0	t	f	\N	2026-09-09 23:26:26.792168+07	2026-09-09 23:26:26.792168+07
20	42	1	3	0	5	0	100	3	t	t	2026-09-09 23:26:26.78804+07	2026-09-09 23:23:08.118311+07	2026-09-09 23:26:26.798276+07
22	43	1	0	0	0	0	0	0	t	f	\N	2026-09-15 09:47:10.398803+07	2026-09-15 09:47:10.398803+07
23	44	1	0	0	0	0	0	0	t	f	\N	2026-09-15 20:20:58.162494+07	2026-09-15 20:20:58.162494+07
\.


--
-- TOC entry 5301 (class 0 OID 69434)
-- Dependencies: 224
-- Data for Name: player_sessions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_sessions (id, player_id, token, device_name, device_id, is_active, last_login, expired_at, created_at) FROM stdin;
14	35	tok_35_1786950622_32688		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-08-17 14:10:22.294492+07
15	36	tok_36_1786952703_23431		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-08-17 14:45:03.910057+07
16	37	tok_37_1786953483_32834		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-08-17 14:58:03.703105+07
17	38	tok_38_1786954555_67990		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-08-17 15:15:55.856184+07
18	39	tok_39_1786954932_1952		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-08-17 15:22:12.823721+07
19	40	tok_40_1786956407_77331		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-08-17 15:46:47.531893+07
20	41	tok_41_8a795ef0dc90cad7d3c4e9bacd66743aabb8c6738dae5235		d43323ba74a1923a58690cb30974961c85a2eec6	f	0001-01-01 06:42:04+06:42:04	\N	2026-09-08 23:48:34.627595+07
21	42	tok_42_e4ef1cc30e03546c6d013d7013387be9ee7a1ccc7da21566		d43323ba74a1923a58690cb30974961c85a2eec6	f	0001-01-01 06:42:04+06:42:04	\N	2026-09-09 23:23:08.12397+07
22	43	tok_43_9b0f57d9fb335dd61ec3f740026196fa51bc6216ffb83da0		d43323ba74a1923a58690cb30974961c85a2eec6	f	0001-01-01 06:42:04+06:42:04	\N	2026-09-15 09:47:10.408002+07
23	44	tok_44_13e03216e78a36b3972d80fd0292f462ce4428613f987a80		d43323ba74a1923a58690cb30974961c85a2eec6	t	0001-01-01 06:42:04+06:42:04	\N	2026-09-15 20:20:58.169138+07
\.


--
-- TOC entry 5343 (class 0 OID 69881)
-- Dependencies: 266
-- Data for Name: player_trivia_answers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_trivia_answers (id, history_id, question_id, selected_option_id, is_correct, answered_at) FROM stdin;
\.


--
-- TOC entry 5349 (class 0 OID 70334)
-- Dependencies: 272
-- Data for Name: player_trivia_histories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_trivia_histories (id, player_id, category_id, score, correct_answer, wrong_answer, xp_earned, stars_earned, played_at) FROM stdin;
1	25	1	0	1	1	10	0	2026-08-10 01:13:47.856316+07
2	42	1	0	2	0	20	0	2026-09-09 23:27:18.967709+07
3	44	1	0	0	2	0	0	2026-09-15 21:31:09.142571+07
\.


--
-- TOC entry 5341 (class 0 OID 69858)
-- Dependencies: 264
-- Data for Name: player_trivia_history; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.player_trivia_history (id, player_id, category_id, score, correct_answer, wrong_answer, xp_earned, stars_earned, played_at) FROM stdin;
\.


--
-- TOC entry 5299 (class 0 OID 69407)
-- Dependencies: 222
-- Data for Name: players; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.players (id, user_code, username, name, pin_hash, character_id, title_id, level, xp, stars, is_active, last_login, created_at, updated_at, fcm_token) FROM stdin;
35	USR000001	santoso4871	SANTOSO	$2a$10$Az.qzRPgqWOqrIrKO.8a8uiYJD8kZhRIKRHoZPpKxZ8Qyk/zh9nRa	1	1	1	0	0	t	\N	2026-08-17 14:10:22.265419+07	2026-08-17 14:10:22.265419+07	\N
36	USR000036	nina5841	NINA	$2a$10$GRGJk0boEtSrtOalsJ8HyubzJvcD3GDV.89KFuwU1uSB/qY/Jur3e	2	1	1	0	0	t	\N	2026-08-17 14:45:03.893444+07	2026-08-17 14:45:03.893444+07	\N
37	USR000037	butet3112	BUTET	$2a$10$xJ9FCqK6Sotux//QUCkuKedQMs0XEVY/0E72rEVtEHRP6Q5khFKqG	2	1	1	0	0	t	\N	2026-08-17 14:58:03.692243+07	2026-08-17 14:58:03.692243+07	\N
38	USR000038	boas6038	BOAS	$2a$10$YbWVD0KtoDGNQ12c3oAbWuzSRahxTMvMaK1XhvOhyX4qVCG1kahlm	1	1	1	0	0	t	\N	2026-08-17 15:15:55.84708+07	2026-08-17 15:15:55.84708+07	\N
39	USR000039	langga4618	LANGGA	$2a$10$UMhtZQ.B9UylA2XZzjpLq.HsPWXvTeEYyflbPOeK56Frs6oijCalO	1	1	1	0	0	t	\N	2026-08-17 15:22:12.813689+07	2026-08-17 15:22:12.813689+07	\N
40	USR000040	sientokpisang2788	SIENTOK PISANG	$2a$10$FHgdP3UHyexkVDLy0raAHeeIlU5hQuMPgk8JcPq0bg3j8okFf1Ana	2	1	1	0	0	t	\N	2026-08-17 15:46:47.448567+07	2026-08-17 15:46:47.448567+07	\N
41	USR000041	brahmani6746	brahmani	$2a$10$dlq3ddMpkafPhAlX2jCNveeFMOqKPydStROKH8NG3e0ZUcvd2GR6a	1	1	1	0	0	t	\N	2026-09-08 23:48:34.60376+07	2026-09-08 23:48:34.60376+07	
42	USR000042	rina2649	Rina	$2a$10$yKN7BrL44WIXa.Px.NsS8ukbi1bIjtknQILOLZSZfymFjcTsX7xX2	2	1	1	270	5	t	\N	2026-09-09 23:23:08.108066+07	2026-09-09 23:27:32.391802+07	
43	USR000043	bram7981	Bram	$2a$10$DKNPAmWKNqxqOTKF5vWbsOAWstO8MMDS.S1WuShOfWBy7U3pJ7EzG	1	1	1	0	0	t	\N	2026-09-15 09:47:10.360278+07	2026-09-15 09:47:10.360278+07	
44	USR000044	brahmana5275	brahmana	$2a$10$vh3rUSPSBJz5qh9ctuh3H.FXpluga1/LF4FfnEz65NPa08wX1oOEi	1	1	1	0	0	t	\N	2026-09-15 20:20:58.150605+07	2026-09-15 21:31:09.134636+07	
\.


--
-- TOC entry 5295 (class 0 OID 69382)
-- Dependencies: 218
-- Data for Name: titles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.titles (id, title_name, minimum_level, created_at, updated_at) FROM stdin;
1	Wisatawan	1	2026-07-08 02:39:51.382328+07	2026-07-08 02:39:51.382328+07
2	Penjelajah Toba	5	2026-07-08 02:39:51.382328+07	2026-07-08 02:39:51.382328+07
3	Sahabat Budaya	10	2026-07-08 02:39:51.382328+07	2026-07-08 02:39:51.382328+07
4	Duta Budaya	15	2026-07-08 02:39:51.382328+07	2026-07-08 02:39:51.382328+07
5	Maestro Budaya Batak	20	2026-07-08 02:39:51.382328+07	2026-07-08 02:39:51.382328+07
\.


--
-- TOC entry 5313 (class 0 OID 69554)
-- Dependencies: 236
-- Data for Name: trivia_categories; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.trivia_categories (id, name, description, question_count, minimum_question, display_order, is_active, created_by_admin_id, updated_by_admin_id, deleted_at, created_at, updated_at) FROM stdin;
1	Makanan	Kuliner khas Batak Toba	10	10	1	t	\N	\N	\N	2026-07-24 18:19:58.659472+07	2026-07-24 18:19:58.659472+07
2	Musik	Alat musik dan lagu daerah	10	10	2	t	\N	\N	\N	2026-07-24 18:19:58.659472+07	2026-07-24 18:19:58.659472+07
3	Pakaian	Ulos dan perlengkapan adat	10	10	3	t	\N	\N	\N	2026-07-24 18:19:58.659472+07	2026-07-24 18:19:58.659472+07
7	Tempat Wisata	Destinasi wisata di kawasan Toba	10	10	4	t	\N	\N	\N	2026-08-08 14:53:09.657815+07	2026-08-08 14:53:09.657815+07
8	Adat Istiadat	Tradisi dan budaya Batak Toba	10	10	5	t	\N	\N	\N	2026-08-08 14:53:09.660016+07	2026-08-08 14:53:09.660016+07
\.


--
-- TOC entry 5325 (class 0 OID 69690)
-- Dependencies: 248
-- Data for Name: trivia_question_options; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.trivia_question_options (id, question_id, option_text, is_correct) FROM stdin;
1	1	Naniura	t
2	1	Rendang	f
3	1	Sate	f
4	1	Arsik	f
5	2	Andaliman	t
6	2	Merica	f
7	2	Ketumbar	f
8	2	Lengkuas	f
9	3	Sumatera Utara	t
10	3	Sumatera Barat	f
11	3	Riau	f
12	3	Aceh	f
13	4	Samosir	t
14	4	Nias	f
15	4	Batam	f
16	4	Bintan	f
17	5	74.000 tahun lalu	t
18	5	10.000 tahun lalu	f
19	5	1.000 tahun lalu	f
20	5	100.000 tahun lalu	f
21	6	Parapat	t
22	6	Medan	f
23	6	Pematangsiantar	f
24	6	Balige	f
25	7	Global Geopark	t
26	7	Warisan Budaya	f
27	7	Taman Nasional	f
28	7	Biosfer Dunia	f
29	8	Kapal feri	t
30	8	Jembatan	f
31	8	Helikopter	f
32	8	Speedboat saja	f
33	9	Kabupaten Toba	t
34	9	Kabupaten Dairi	f
35	9	Kabupaten Karo	f
36	9	Kabupaten Humbang	f
37	10	Festival Danau Toba	t
38	10	Pesta Rakyat Toba	f
39	10	Karnaval Samosir	f
40	10	Pekan Batak	f
41	11	505 meter	t
42	11	200 meter	f
43	11	100 meter	f
44	11	1.000 meter	f
45	12	1.145 km²	t
46	12	500 km²	f
47	12	2.000 km²	f
48	12	300 km²	f
49	13	Patrilineal	t
50	13	Matrilineal	f
51	13	Bilateral	f
52	13	Ambilineal	f
53	14	Dalihan Na Tolu	t
54	14	Suku Adat	f
55	14	Marga Sistem	f
56	14	Hula-hula	f
57	15	Hula-hula, Dongan Tubu, Boru	t
58	15	Raja, Rakyat, Pendeta	f
59	15	Ayah, Ibu, Anak	f
60	15	Kakek, Ayah, Cucu	f
61	16	Saur Matua	t
62	16	Mangongkal Holi	f
63	16	Marhata	f
64	16	Martumpol	f
65	17	Mangongkal Holi	t
66	17	Saur Matua	f
67	17	Mardebata	f
68	17	Tortor	f
69	18	Pihak ayah	t
70	18	Pihak ibu	f
71	18	Kedua orang tua	f
72	18	Pilihan sendiri	f
73	19	Pardalanan	t
74	19	Sidang Adat	f
75	19	Musyawarah Marga	f
76	19	Ronda Malam	f
77	20	Keluarga pihak istri	t
78	20	Keluarga pihak suami	f
79	20	Saudara kandung	f
80	20	Tetangga dekat	f
81	21	Horas	t
82	21	Mejuah-juah	f
83	21	Yaahowu	f
84	21	Njuah-juah	f
85	22	Rumah Bolon	t
86	22	Rumah Gadang	f
87	22	Joglo	f
88	22	Honai	f
\.


--
-- TOC entry 5323 (class 0 OID 69663)
-- Dependencies: 246
-- Data for Name: trivia_questions; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.trivia_questions (id, category_id, question, explanation, is_active, created_by_admin_id, updated_by_admin_id, created_at, updated_at) FROM stdin;
1	1	Apa nama makanan khas Batak yang berbahan dasar ikan mas?	Dengke Mas Na Niura adalah makanan tradisional khas Batak.	t	\N	\N	2026-07-24 18:19:58.659472+07	2026-07-24 18:19:58.659472+07
2	1	Bumbu khas Batak yang memberikan rasa getir di lidah adalah...	Andaliman adalah bumbu rahasia masakan Batak.	t	\N	\N	2026-07-24 18:19:58.659472+07	2026-07-24 18:19:58.659472+07
3	7	Danau Toba terletak di provinsi mana?	Danau Toba terletak di Provinsi Sumatera Utara, Indonesia.	t	\N	\N	2026-08-08 14:53:09.681633+07	2026-08-08 14:53:09.681633+07
4	7	Pulau yang terletak di tengah Danau Toba disebut?	Pulau Samosir adalah pulau vulkanik yang terletak di tengah Danau Toba.	t	\N	\N	2026-08-08 14:53:09.692466+07	2026-08-08 14:53:09.692466+07
5	7	Danau Toba terbentuk akibat letusan supervulkan pada berapa tahun lalu?	Danau Toba terbentuk sekitar 74.000 tahun lalu akibat letusan supervolcano terbesar.	t	\N	\N	2026-08-08 14:53:09.699751+07	2026-08-08 14:53:09.699751+07
6	7	Kota terdekat dari Danau Toba yang menjadi pintu masuk wisatawan adalah?	Parapat adalah kota di tepi Danau Toba yang menjadi pintu masuk utama wisatawan.	t	\N	\N	2026-08-08 14:53:09.707049+07	2026-08-08 14:53:09.707049+07
7	7	Danau Toba diakui UNESCO sebagai?	Danau Toba diakui UNESCO sebagai Global Geopark pada tahun 2020.	t	\N	\N	2026-08-08 14:53:09.712257+07	2026-08-08 14:53:09.712257+07
8	7	Penyeberangan dari Parapat ke Samosir menggunakan?	Kapal feri menjadi transportasi utama dari Parapat menuju Pulau Samosir.	t	\N	\N	2026-08-08 14:53:09.718198+07	2026-08-08 14:53:09.718198+07
9	7	Kabupaten yang melingkupi sebagian besar Danau Toba adalah?	Kabupaten Toba (dahulu Toba Samosir) melingkupi wilayah terbesar Danau Toba.	t	\N	\N	2026-08-08 14:53:09.723971+07	2026-08-08 14:53:09.723971+07
10	7	Festival budaya tahunan yang digelar di kawasan Danau Toba disebut?	Festival Danau Toba adalah event tahunan yang mempromosikan budaya Batak kepada dunia.	t	\N	\N	2026-08-08 14:53:09.729249+07	2026-08-08 14:53:09.729249+07
11	7	Kedalaman maksimum Danau Toba adalah sekitar?	Danau Toba memiliki kedalaman maksimum sekitar 505 meter, menjadikannya danau terdalam di Asia Tenggara.	t	\N	\N	2026-08-08 14:53:09.734592+07	2026-08-08 14:53:09.734592+07
12	7	Luas Danau Toba adalah sekitar?	Danau Toba memiliki luas sekitar 1.145 kilometer persegi.	t	\N	\N	2026-08-08 14:53:09.740234+07	2026-08-08 14:53:09.740234+07
13	8	Sistem kekerabatan masyarakat Batak Toba berdasarkan garis keturunan?	Masyarakat Batak Toba menganut sistem patrilineal, yaitu garis keturunan dari pihak ayah.	t	\N	\N	2026-08-08 14:53:09.745373+07	2026-08-08 14:53:09.745373+07
14	8	Sistem adat Batak Toba yang mengatur hubungan kekerabatan disebut?	Dalihan Na Tolu adalah sistem adat Batak yang mengatur tiga pilar hubungan kekerabatan.	t	\N	\N	2026-08-08 14:53:09.75256+07	2026-08-08 14:53:09.75256+07
15	8	Tiga pilar dalam Dalihan Na Tolu adalah?	Dalihan Na Tolu terdiri dari Hula-hula, Dongan Tubu, dan Boru.	t	\N	\N	2026-08-08 14:53:09.757774+07	2026-08-08 14:53:09.757774+07
16	8	Upacara kematian adat Batak Toba yang besar disebut?	Saur Matua adalah upacara adat kematian tertinggi dalam budaya Batak Toba.	t	\N	\N	2026-08-08 14:53:09.763172+07	2026-08-08 14:53:09.763172+07
17	8	Upacara pemindahan tulang leluhur dalam adat Batak disebut?	Mangongkal Holi adalah ritual pemindahan tulang leluhur ke tempat yang lebih terhormat.	t	\N	\N	2026-08-08 14:53:09.769028+07	2026-08-08 14:53:09.769028+07
18	8	Marga dalam suku Batak diturunkan dari?	Marga dalam adat Batak diturunkan dari pihak ayah (patrilineal).	t	\N	\N	2026-08-08 14:53:09.774262+07	2026-08-08 14:53:09.774262+07
19	8	Musyawarah adat dalam tradisi Batak Toba disebut?	Rapat Adat atau Pardalanan adalah forum musyawarah dalam masyarakat Batak Toba.	t	\N	\N	2026-08-08 14:53:09.78028+07	2026-08-08 14:53:09.78028+07
20	8	Posisi Hula-hula dalam Dalihan Na Tolu adalah?	Hula-hula adalah keluarga dari pihak istri yang memiliki posisi terhormat dalam adat Batak.	t	\N	\N	2026-08-08 14:53:09.786782+07	2026-08-08 14:53:09.786782+07
21	8	Ucapan salam khas Batak Toba adalah?	Horas adalah salam khas Batak Toba yang bermakna sehat, sejahtera, dan selamat.	t	\N	\N	2026-08-08 14:53:09.792442+07	2026-08-08 14:53:09.792442+07
22	8	Rumah adat Batak Toba disebut?	Rumah Bolon atau Jabu adalah rumah adat tradisional masyarakat Batak Toba.	t	\N	\N	2026-08-08 14:53:09.798338+07	2026-08-08 14:53:09.798338+07
\.


--
-- TOC entry 5353 (class 0 OID 70669)
-- Dependencies: 276
-- Data for Name: weekly_challenges; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.weekly_challenges (id, title, description, game_type, reward_xp, reward_stars, is_active, start_date, end_date, data_json, created_at) FROM stdin;
1	Bahasa Batak	Cocokkan kata batak dengan artinya	MATCHING	100	2	t	2026-09-11 00:05:13.200799+07	2026-09-18 00:05:13.200799+07	{\n  "pairs": [\n    {\n      "batak": "Horas",\n      "indo": "Halo/Selamat"\n    },\n    {\n      "batak": "Mauliate",\n      "indo": "Terima kasih"\n    },\n    {\n      "batak": "Bagas",\n      "indo": "Rumah"\n    }\n  ]\n}	2026-09-11 00:05:13.201915+07
\.


--
-- TOC entry 5393 (class 0 OID 0)
-- Dependencies: 233
-- Name: achievements_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.achievements_id_seq', 15, true);


--
-- TOC entry 5394 (class 0 OID 0)
-- Dependencies: 225
-- Name: admin_logs_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.admin_logs_id_seq', 1, false);


--
-- TOC entry 5395 (class 0 OID 0)
-- Dependencies: 273
-- Name: admin_sessions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.admin_sessions_id_seq', 1, true);


--
-- TOC entry 5396 (class 0 OID 0)
-- Dependencies: 219
-- Name: admins_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.admins_id_seq', 1, true);


--
-- TOC entry 5397 (class 0 OID 0)
-- Dependencies: 255
-- Name: battle_answers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.battle_answers_id_seq', 1, false);


--
-- TOC entry 5398 (class 0 OID 0)
-- Dependencies: 251
-- Name: battle_question_options_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.battle_question_options_id_seq', 72, true);


--
-- TOC entry 5399 (class 0 OID 0)
-- Dependencies: 249
-- Name: battle_questions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.battle_questions_id_seq', 18, true);


--
-- TOC entry 5400 (class 0 OID 0)
-- Dependencies: 267
-- Name: battle_room_questions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.battle_room_questions_id_seq', 10, true);


--
-- TOC entry 5401 (class 0 OID 0)
-- Dependencies: 253
-- Name: battle_rooms_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.battle_rooms_id_seq', 6, true);


--
-- TOC entry 5402 (class 0 OID 0)
-- Dependencies: 215
-- Name: characters_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.characters_id_seq', 2, true);


--
-- TOC entry 5403 (class 0 OID 0)
-- Dependencies: 231
-- Name: gallery_cards_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.gallery_cards_id_seq', 1, true);


--
-- TOC entry 5404 (class 0 OID 0)
-- Dependencies: 237
-- Name: game_settings_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.game_settings_id_seq', 10, true);


--
-- TOC entry 5405 (class 0 OID 0)
-- Dependencies: 229
-- Name: location_facts_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.location_facts_id_seq', 40, true);


--
-- TOC entry 5406 (class 0 OID 0)
-- Dependencies: 241
-- Name: location_question_options_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.location_question_options_id_seq', 620, true);


--
-- TOC entry 5407 (class 0 OID 0)
-- Dependencies: 239
-- Name: location_questions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.location_questions_id_seq', 155, true);


--
-- TOC entry 5408 (class 0 OID 0)
-- Dependencies: 227
-- Name: locations_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.locations_id_seq', 10, true);


--
-- TOC entry 5409 (class 0 OID 0)
-- Dependencies: 259
-- Name: player_achievements_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_achievements_id_seq', 5, true);


--
-- TOC entry 5410 (class 0 OID 0)
-- Dependencies: 277
-- Name: player_challenge_progresses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_challenge_progresses_id_seq', 1, false);


--
-- TOC entry 5411 (class 0 OID 0)
-- Dependencies: 257
-- Name: player_gallery_cards_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_gallery_cards_id_seq', 1, true);


--
-- TOC entry 5412 (class 0 OID 0)
-- Dependencies: 261
-- Name: player_location_answers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_location_answers_id_seq', 1, false);


--
-- TOC entry 5413 (class 0 OID 0)
-- Dependencies: 243
-- Name: player_location_progress_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_location_progress_id_seq', 1, false);


--
-- TOC entry 5414 (class 0 OID 0)
-- Dependencies: 269
-- Name: player_location_progresses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_location_progresses_id_seq', 23, true);


--
-- TOC entry 5415 (class 0 OID 0)
-- Dependencies: 223
-- Name: player_sessions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_sessions_id_seq', 23, true);


--
-- TOC entry 5416 (class 0 OID 0)
-- Dependencies: 265
-- Name: player_trivia_answers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_trivia_answers_id_seq', 1, false);


--
-- TOC entry 5417 (class 0 OID 0)
-- Dependencies: 271
-- Name: player_trivia_histories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_trivia_histories_id_seq', 3, true);


--
-- TOC entry 5418 (class 0 OID 0)
-- Dependencies: 263
-- Name: player_trivia_history_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.player_trivia_history_id_seq', 1, false);


--
-- TOC entry 5419 (class 0 OID 0)
-- Dependencies: 221
-- Name: players_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.players_id_seq', 44, true);


--
-- TOC entry 5420 (class 0 OID 0)
-- Dependencies: 217
-- Name: titles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.titles_id_seq', 5, true);


--
-- TOC entry 5421 (class 0 OID 0)
-- Dependencies: 235
-- Name: trivia_categories_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.trivia_categories_id_seq', 80, true);


--
-- TOC entry 5422 (class 0 OID 0)
-- Dependencies: 247
-- Name: trivia_question_options_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.trivia_question_options_id_seq', 88, true);


--
-- TOC entry 5423 (class 0 OID 0)
-- Dependencies: 245
-- Name: trivia_questions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.trivia_questions_id_seq', 22, true);


--
-- TOC entry 5424 (class 0 OID 0)
-- Dependencies: 275
-- Name: weekly_challenges_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.weekly_challenges_id_seq', 1, true);


--
-- TOC entry 5031 (class 2606 OID 70570)
-- Name: achievements achievements_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.achievements
    ADD CONSTRAINT achievements_name_key UNIQUE (name);


--
-- TOC entry 5033 (class 2606 OID 69540)
-- Name: achievements achievements_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.achievements
    ADD CONSTRAINT achievements_pkey PRIMARY KEY (id);


--
-- TOC entry 5017 (class 2606 OID 69459)
-- Name: admin_logs admin_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_logs
    ADD CONSTRAINT admin_logs_pkey PRIMARY KEY (id);


--
-- TOC entry 5092 (class 2606 OID 70666)
-- Name: admin_sessions admin_sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_sessions
    ADD CONSTRAINT admin_sessions_pkey PRIMARY KEY (id);


--
-- TOC entry 5003 (class 2606 OID 69403)
-- Name: admins admins_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admins
    ADD CONSTRAINT admins_pkey PRIMARY KEY (id);


--
-- TOC entry 5005 (class 2606 OID 69991)
-- Name: admins admins_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admins
    ADD CONSTRAINT admins_username_key UNIQUE (username);


--
-- TOC entry 5066 (class 2606 OID 69771)
-- Name: battle_answers battle_answers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_answers
    ADD CONSTRAINT battle_answers_pkey PRIMARY KEY (id);


--
-- TOC entry 5059 (class 2606 OID 69731)
-- Name: battle_question_options battle_question_options_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_question_options
    ADD CONSTRAINT battle_question_options_pkey PRIMARY KEY (id);


--
-- TOC entry 5057 (class 2606 OID 69713)
-- Name: battle_questions battle_questions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_questions
    ADD CONSTRAINT battle_questions_pkey PRIMARY KEY (id);


--
-- TOC entry 5082 (class 2606 OID 69910)
-- Name: battle_room_questions battle_room_questions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_room_questions
    ADD CONSTRAINT battle_room_questions_pkey PRIMARY KEY (id);


--
-- TOC entry 5084 (class 2606 OID 70456)
-- Name: battle_room_questions battle_room_questions_room_id_question_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_room_questions
    ADD CONSTRAINT battle_room_questions_room_id_question_id_key UNIQUE (room_id, question_id);


--
-- TOC entry 5086 (class 2606 OID 70469)
-- Name: battle_room_questions battle_room_questions_room_id_question_order_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_room_questions
    ADD CONSTRAINT battle_room_questions_room_id_question_order_key UNIQUE (room_id, question_order);


--
-- TOC entry 5061 (class 2606 OID 69745)
-- Name: battle_rooms battle_rooms_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_rooms
    ADD CONSTRAINT battle_rooms_pkey PRIMARY KEY (id);


--
-- TOC entry 5063 (class 2606 OID 70394)
-- Name: battle_rooms battle_rooms_room_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_rooms
    ADD CONSTRAINT battle_rooms_room_code_key UNIQUE (room_code);


--
-- TOC entry 4993 (class 2606 OID 69947)
-- Name: characters characters_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.characters
    ADD CONSTRAINT characters_name_key UNIQUE (name);


--
-- TOC entry 4995 (class 2606 OID 69378)
-- Name: characters characters_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.characters
    ADD CONSTRAINT characters_pkey PRIMARY KEY (id);


--
-- TOC entry 5025 (class 2606 OID 70509)
-- Name: gallery_cards gallery_cards_location_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gallery_cards
    ADD CONSTRAINT gallery_cards_location_id_key UNIQUE (location_id);


--
-- TOC entry 5027 (class 2606 OID 69517)
-- Name: gallery_cards gallery_cards_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gallery_cards
    ADD CONSTRAINT gallery_cards_pkey PRIMARY KEY (id);


--
-- TOC entry 5029 (class 2606 OID 70524)
-- Name: gallery_cards gallery_cards_title_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gallery_cards
    ADD CONSTRAINT gallery_cards_title_key UNIQUE (title);


--
-- TOC entry 5041 (class 2606 OID 69590)
-- Name: game_settings game_settings_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.game_settings
    ADD CONSTRAINT game_settings_pkey PRIMARY KEY (id);


--
-- TOC entry 5043 (class 2606 OID 70645)
-- Name: game_settings game_settings_setting_key_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.game_settings
    ADD CONSTRAINT game_settings_setting_key_key UNIQUE (setting_key);


--
-- TOC entry 5023 (class 2606 OID 69502)
-- Name: location_facts location_facts_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_facts
    ADD CONSTRAINT location_facts_pkey PRIMARY KEY (id);


--
-- TOC entry 5047 (class 2606 OID 69627)
-- Name: location_question_options location_question_options_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_question_options
    ADD CONSTRAINT location_question_options_pkey PRIMARY KEY (id);


--
-- TOC entry 5045 (class 2606 OID 69604)
-- Name: location_questions location_questions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_questions
    ADD CONSTRAINT location_questions_pkey PRIMARY KEY (id);


--
-- TOC entry 5019 (class 2606 OID 70168)
-- Name: locations locations_level_order_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.locations
    ADD CONSTRAINT locations_level_order_key UNIQUE (level_order);


--
-- TOC entry 5021 (class 2606 OID 69480)
-- Name: locations locations_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.locations
    ADD CONSTRAINT locations_pkey PRIMARY KEY (id);


--
-- TOC entry 5072 (class 2606 OID 69820)
-- Name: player_achievements player_achievements_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_achievements
    ADD CONSTRAINT player_achievements_pkey PRIMARY KEY (id);


--
-- TOC entry 5074 (class 2606 OID 70622)
-- Name: player_achievements player_achievements_player_id_achievement_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_achievements
    ADD CONSTRAINT player_achievements_player_id_achievement_id_key UNIQUE (player_id, achievement_id);


--
-- TOC entry 5097 (class 2606 OID 70683)
-- Name: player_challenge_progresses player_challenge_progresses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_challenge_progresses
    ADD CONSTRAINT player_challenge_progresses_pkey PRIMARY KEY (id);


--
-- TOC entry 5068 (class 2606 OID 69799)
-- Name: player_gallery_cards player_gallery_cards_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_gallery_cards
    ADD CONSTRAINT player_gallery_cards_pkey PRIMARY KEY (id);


--
-- TOC entry 5070 (class 2606 OID 70547)
-- Name: player_gallery_cards player_gallery_cards_player_id_gallery_card_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_gallery_cards
    ADD CONSTRAINT player_gallery_cards_player_id_gallery_card_id_key UNIQUE (player_id, gallery_card_id);


--
-- TOC entry 5076 (class 2606 OID 69841)
-- Name: player_location_answers player_location_answers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_answers
    ADD CONSTRAINT player_location_answers_pkey PRIMARY KEY (id);


--
-- TOC entry 5049 (class 2606 OID 69649)
-- Name: player_location_progress player_location_progress_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progress
    ADD CONSTRAINT player_location_progress_pkey PRIMARY KEY (id);


--
-- TOC entry 5051 (class 2606 OID 69651)
-- Name: player_location_progress player_location_progress_player_id_location_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progress
    ADD CONSTRAINT player_location_progress_player_id_location_id_key UNIQUE (player_id, location_id);


--
-- TOC entry 5088 (class 2606 OID 70243)
-- Name: player_location_progresses player_location_progresses_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progresses
    ADD CONSTRAINT player_location_progresses_pkey PRIMARY KEY (id);


--
-- TOC entry 5015 (class 2606 OID 69444)
-- Name: player_sessions player_sessions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_sessions
    ADD CONSTRAINT player_sessions_pkey PRIMARY KEY (id);


--
-- TOC entry 5080 (class 2606 OID 69888)
-- Name: player_trivia_answers player_trivia_answers_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_answers
    ADD CONSTRAINT player_trivia_answers_pkey PRIMARY KEY (id);


--
-- TOC entry 5090 (class 2606 OID 70339)
-- Name: player_trivia_histories player_trivia_histories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_histories
    ADD CONSTRAINT player_trivia_histories_pkey PRIMARY KEY (id);


--
-- TOC entry 5078 (class 2606 OID 69869)
-- Name: player_trivia_history player_trivia_history_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_history
    ADD CONSTRAINT player_trivia_history_pkey PRIMARY KEY (id);


--
-- TOC entry 5009 (class 2606 OID 69418)
-- Name: players players_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_pkey PRIMARY KEY (id);


--
-- TOC entry 5011 (class 2606 OID 70017)
-- Name: players players_user_code_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_user_code_key UNIQUE (user_code);


--
-- TOC entry 5013 (class 2606 OID 70021)
-- Name: players players_username_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_username_key UNIQUE (username);


--
-- TOC entry 4997 (class 2606 OID 69961)
-- Name: titles titles_minimum_level_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.titles
    ADD CONSTRAINT titles_minimum_level_key UNIQUE (minimum_level);


--
-- TOC entry 4999 (class 2606 OID 69389)
-- Name: titles titles_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.titles
    ADD CONSTRAINT titles_pkey PRIMARY KEY (id);


--
-- TOC entry 5001 (class 2606 OID 69957)
-- Name: titles titles_title_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.titles
    ADD CONSTRAINT titles_title_name_key UNIQUE (title_name);


--
-- TOC entry 5035 (class 2606 OID 70265)
-- Name: trivia_categories trivia_categories_display_order_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_categories
    ADD CONSTRAINT trivia_categories_display_order_key UNIQUE (display_order);


--
-- TOC entry 5037 (class 2606 OID 70245)
-- Name: trivia_categories trivia_categories_name_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_categories
    ADD CONSTRAINT trivia_categories_name_key UNIQUE (name);


--
-- TOC entry 5039 (class 2606 OID 69566)
-- Name: trivia_categories trivia_categories_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_categories
    ADD CONSTRAINT trivia_categories_pkey PRIMARY KEY (id);


--
-- TOC entry 5055 (class 2606 OID 69696)
-- Name: trivia_question_options trivia_question_options_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_question_options
    ADD CONSTRAINT trivia_question_options_pkey PRIMARY KEY (id);


--
-- TOC entry 5053 (class 2606 OID 69673)
-- Name: trivia_questions trivia_questions_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_questions
    ADD CONSTRAINT trivia_questions_pkey PRIMARY KEY (id);


--
-- TOC entry 5095 (class 2606 OID 70676)
-- Name: weekly_challenges weekly_challenges_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.weekly_challenges
    ADD CONSTRAINT weekly_challenges_pkey PRIMARY KEY (id);


--
-- TOC entry 5093 (class 1259 OID 70667)
-- Name: idx_admin_sessions_token; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_admin_sessions_token ON public.admin_sessions USING btree (token);


--
-- TOC entry 5064 (class 1259 OID 70395)
-- Name: idx_battle_room_code; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_battle_room_code ON public.battle_rooms USING btree (room_code);


--
-- TOC entry 5006 (class 1259 OID 70077)
-- Name: idx_player_stars_xp; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_player_stars_xp ON public.players USING btree (stars, xp);


--
-- TOC entry 5007 (class 1259 OID 70022)
-- Name: idx_player_username; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_player_username ON public.players USING btree (username);


--
-- TOC entry 5106 (class 2606 OID 69543)
-- Name: achievements achievements_created_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.achievements
    ADD CONSTRAINT achievements_created_by_admin_id_fkey FOREIGN KEY (created_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5107 (class 2606 OID 69548)
-- Name: achievements achievements_updated_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.achievements
    ADD CONSTRAINT achievements_updated_by_admin_id_fkey FOREIGN KEY (updated_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5101 (class 2606 OID 69460)
-- Name: admin_logs admin_logs_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.admin_logs
    ADD CONSTRAINT admin_logs_admin_id_fkey FOREIGN KEY (admin_id) REFERENCES public.admins(id) ON DELETE CASCADE;


--
-- TOC entry 5129 (class 2606 OID 70476)
-- Name: battle_answers battle_answers_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_answers
    ADD CONSTRAINT battle_answers_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5130 (class 2606 OID 70485)
-- Name: battle_answers battle_answers_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_answers
    ADD CONSTRAINT battle_answers_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.battle_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5131 (class 2606 OID 69772)
-- Name: battle_answers battle_answers_room_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_answers
    ADD CONSTRAINT battle_answers_room_id_fkey FOREIGN KEY (room_id) REFERENCES public.battle_rooms(id) ON DELETE CASCADE;


--
-- TOC entry 5132 (class 2606 OID 70494)
-- Name: battle_answers battle_answers_selected_option_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_answers
    ADD CONSTRAINT battle_answers_selected_option_id_fkey FOREIGN KEY (selected_option_id) REFERENCES public.battle_question_options(id) ON DELETE CASCADE;


--
-- TOC entry 5124 (class 2606 OID 70377)
-- Name: battle_question_options battle_question_options_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_question_options
    ADD CONSTRAINT battle_question_options_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.battle_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5122 (class 2606 OID 69714)
-- Name: battle_questions battle_questions_created_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_questions
    ADD CONSTRAINT battle_questions_created_by_admin_id_fkey FOREIGN KEY (created_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5123 (class 2606 OID 69719)
-- Name: battle_questions battle_questions_updated_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_questions
    ADD CONSTRAINT battle_questions_updated_by_admin_id_fkey FOREIGN KEY (updated_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5147 (class 2606 OID 70457)
-- Name: battle_room_questions battle_room_questions_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_room_questions
    ADD CONSTRAINT battle_room_questions_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.battle_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5148 (class 2606 OID 69915)
-- Name: battle_room_questions battle_room_questions_room_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_room_questions
    ADD CONSTRAINT battle_room_questions_room_id_fkey FOREIGN KEY (room_id) REFERENCES public.battle_rooms(id) ON DELETE CASCADE;


--
-- TOC entry 5126 (class 2606 OID 70411)
-- Name: battle_rooms battle_rooms_guest_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_rooms
    ADD CONSTRAINT battle_rooms_guest_player_id_fkey FOREIGN KEY (guest_player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5127 (class 2606 OID 70398)
-- Name: battle_rooms battle_rooms_host_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_rooms
    ADD CONSTRAINT battle_rooms_host_player_id_fkey FOREIGN KEY (host_player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5128 (class 2606 OID 70424)
-- Name: battle_rooms battle_rooms_winner_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_rooms
    ADD CONSTRAINT battle_rooms_winner_player_id_fkey FOREIGN KEY (winner_player_id) REFERENCES public.players(id) ON DELETE SET NULL;


--
-- TOC entry 5125 (class 2606 OID 70388)
-- Name: battle_question_options fk_battle_questions_options; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.battle_question_options
    ADD CONSTRAINT fk_battle_questions_options FOREIGN KEY (question_id) REFERENCES public.battle_questions(id);


--
-- TOC entry 5113 (class 2606 OID 70232)
-- Name: location_question_options fk_location_questions_options; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_question_options
    ADD CONSTRAINT fk_location_questions_options FOREIGN KEY (question_id) REFERENCES public.location_questions(id);


--
-- TOC entry 5136 (class 2606 OID 70639)
-- Name: player_achievements fk_player_achievements_achievement; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_achievements
    ADD CONSTRAINT fk_player_achievements_achievement FOREIGN KEY (achievement_id) REFERENCES public.achievements(id);


--
-- TOC entry 5133 (class 2606 OID 70564)
-- Name: player_gallery_cards fk_player_gallery_cards_card; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_gallery_cards
    ADD CONSTRAINT fk_player_gallery_cards_card FOREIGN KEY (gallery_card_id) REFERENCES public.gallery_cards(id);


--
-- TOC entry 5120 (class 2606 OID 70328)
-- Name: trivia_question_options fk_trivia_questions_options; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_question_options
    ADD CONSTRAINT fk_trivia_questions_options FOREIGN KEY (question_id) REFERENCES public.trivia_questions(id);


--
-- TOC entry 5105 (class 2606 OID 70510)
-- Name: gallery_cards gallery_cards_location_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.gallery_cards
    ADD CONSTRAINT gallery_cards_location_id_fkey FOREIGN KEY (location_id) REFERENCES public.locations(id) ON DELETE CASCADE;


--
-- TOC entry 5104 (class 2606 OID 70192)
-- Name: location_facts location_facts_location_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_facts
    ADD CONSTRAINT location_facts_location_id_fkey FOREIGN KEY (location_id) REFERENCES public.locations(id) ON DELETE CASCADE;


--
-- TOC entry 5114 (class 2606 OID 70221)
-- Name: location_question_options location_question_options_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_question_options
    ADD CONSTRAINT location_question_options_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.location_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5110 (class 2606 OID 69610)
-- Name: location_questions location_questions_created_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_questions
    ADD CONSTRAINT location_questions_created_by_admin_id_fkey FOREIGN KEY (created_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5111 (class 2606 OID 70210)
-- Name: location_questions location_questions_location_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_questions
    ADD CONSTRAINT location_questions_location_id_fkey FOREIGN KEY (location_id) REFERENCES public.locations(id) ON DELETE CASCADE;


--
-- TOC entry 5112 (class 2606 OID 69615)
-- Name: location_questions location_questions_updated_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.location_questions
    ADD CONSTRAINT location_questions_updated_by_admin_id_fkey FOREIGN KEY (updated_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5102 (class 2606 OID 69483)
-- Name: locations locations_created_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.locations
    ADD CONSTRAINT locations_created_by_admin_id_fkey FOREIGN KEY (created_by_admin_id) REFERENCES public.admins(id);


--
-- TOC entry 5103 (class 2606 OID 69488)
-- Name: locations locations_updated_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.locations
    ADD CONSTRAINT locations_updated_by_admin_id_fkey FOREIGN KEY (updated_by_admin_id) REFERENCES public.admins(id);


--
-- TOC entry 5137 (class 2606 OID 70623)
-- Name: player_achievements player_achievements_achievement_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_achievements
    ADD CONSTRAINT player_achievements_achievement_id_fkey FOREIGN KEY (achievement_id) REFERENCES public.achievements(id) ON DELETE CASCADE;


--
-- TOC entry 5138 (class 2606 OID 70611)
-- Name: player_achievements player_achievements_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_achievements
    ADD CONSTRAINT player_achievements_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5134 (class 2606 OID 70548)
-- Name: player_gallery_cards player_gallery_cards_gallery_card_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_gallery_cards
    ADD CONSTRAINT player_gallery_cards_gallery_card_id_fkey FOREIGN KEY (gallery_card_id) REFERENCES public.gallery_cards(id) ON DELETE CASCADE;


--
-- TOC entry 5135 (class 2606 OID 70536)
-- Name: player_gallery_cards player_gallery_cards_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_gallery_cards
    ADD CONSTRAINT player_gallery_cards_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5139 (class 2606 OID 69842)
-- Name: player_location_answers player_location_answers_progress_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_answers
    ADD CONSTRAINT player_location_answers_progress_id_fkey FOREIGN KEY (progress_id) REFERENCES public.player_location_progress(id) ON DELETE CASCADE;


--
-- TOC entry 5140 (class 2606 OID 69847)
-- Name: player_location_answers player_location_answers_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_answers
    ADD CONSTRAINT player_location_answers_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.location_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5141 (class 2606 OID 69852)
-- Name: player_location_answers player_location_answers_selected_option_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_answers
    ADD CONSTRAINT player_location_answers_selected_option_id_fkey FOREIGN KEY (selected_option_id) REFERENCES public.location_question_options(id) ON DELETE CASCADE;


--
-- TOC entry 5115 (class 2606 OID 69657)
-- Name: player_location_progress player_location_progress_location_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progress
    ADD CONSTRAINT player_location_progress_location_id_fkey FOREIGN KEY (location_id) REFERENCES public.locations(id) ON DELETE CASCADE;


--
-- TOC entry 5116 (class 2606 OID 69652)
-- Name: player_location_progress player_location_progress_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_location_progress
    ADD CONSTRAINT player_location_progress_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5100 (class 2606 OID 70110)
-- Name: player_sessions player_sessions_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_sessions
    ADD CONSTRAINT player_sessions_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5144 (class 2606 OID 69889)
-- Name: player_trivia_answers player_trivia_answers_history_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_answers
    ADD CONSTRAINT player_trivia_answers_history_id_fkey FOREIGN KEY (history_id) REFERENCES public.player_trivia_history(id) ON DELETE CASCADE;


--
-- TOC entry 5145 (class 2606 OID 70340)
-- Name: player_trivia_answers player_trivia_answers_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_answers
    ADD CONSTRAINT player_trivia_answers_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.trivia_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5146 (class 2606 OID 70349)
-- Name: player_trivia_answers player_trivia_answers_selected_option_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_answers
    ADD CONSTRAINT player_trivia_answers_selected_option_id_fkey FOREIGN KEY (selected_option_id) REFERENCES public.trivia_question_options(id) ON DELETE CASCADE;


--
-- TOC entry 5142 (class 2606 OID 69875)
-- Name: player_trivia_history player_trivia_history_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_history
    ADD CONSTRAINT player_trivia_history_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.trivia_categories(id) ON DELETE CASCADE;


--
-- TOC entry 5143 (class 2606 OID 69870)
-- Name: player_trivia_history player_trivia_history_player_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.player_trivia_history
    ADD CONSTRAINT player_trivia_history_player_id_fkey FOREIGN KEY (player_id) REFERENCES public.players(id) ON DELETE CASCADE;


--
-- TOC entry 5098 (class 2606 OID 70023)
-- Name: players players_character_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_character_id_fkey FOREIGN KEY (character_id) REFERENCES public.characters(id);


--
-- TOC entry 5099 (class 2606 OID 70038)
-- Name: players players_title_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.players
    ADD CONSTRAINT players_title_id_fkey FOREIGN KEY (title_id) REFERENCES public.titles(id);


--
-- TOC entry 5108 (class 2606 OID 69571)
-- Name: trivia_categories trivia_categories_created_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_categories
    ADD CONSTRAINT trivia_categories_created_by_admin_id_fkey FOREIGN KEY (created_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5109 (class 2606 OID 69576)
-- Name: trivia_categories trivia_categories_updated_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_categories
    ADD CONSTRAINT trivia_categories_updated_by_admin_id_fkey FOREIGN KEY (updated_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5121 (class 2606 OID 70317)
-- Name: trivia_question_options trivia_question_options_question_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_question_options
    ADD CONSTRAINT trivia_question_options_question_id_fkey FOREIGN KEY (question_id) REFERENCES public.trivia_questions(id) ON DELETE CASCADE;


--
-- TOC entry 5117 (class 2606 OID 70292)
-- Name: trivia_questions trivia_questions_category_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_questions
    ADD CONSTRAINT trivia_questions_category_id_fkey FOREIGN KEY (category_id) REFERENCES public.trivia_categories(id) ON DELETE CASCADE;


--
-- TOC entry 5118 (class 2606 OID 69679)
-- Name: trivia_questions trivia_questions_created_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_questions
    ADD CONSTRAINT trivia_questions_created_by_admin_id_fkey FOREIGN KEY (created_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


--
-- TOC entry 5119 (class 2606 OID 69684)
-- Name: trivia_questions trivia_questions_updated_by_admin_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.trivia_questions
    ADD CONSTRAINT trivia_questions_updated_by_admin_id_fkey FOREIGN KEY (updated_by_admin_id) REFERENCES public.admins(id) ON DELETE SET NULL;


-- Completed on 2026-09-16 14:41:35

--
-- PostgreSQL database dump complete
--

\unrestrict LiSsp7Es4j0KE7rG7apzRmfg2eC4TtfvAWEYcHJfUG5qDDUc8lSyashbyo3gjeu

