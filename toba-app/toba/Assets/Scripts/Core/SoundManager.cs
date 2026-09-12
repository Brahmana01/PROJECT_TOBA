using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private float bgmVolume  = 0.55f;
    private float sfxVolume  = 0.85f;
    private bool  isMuted    = false;

    private Dictionary<string, AudioClip> _cache =
        new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupSources();
            LoadSettings();
        }
        else Destroy(gameObject);
    }

    void SetupSources()
    {
        var bgmGO = new GameObject("BGM"); bgmGO.transform.SetParent(transform);
        bgmSource = bgmGO.AddComponent<AudioSource>();
        bgmSource.loop = true; bgmSource.playOnAwake = false;

        var sfxGO = new GameObject("SFX"); sfxGO.transform.SetParent(transform);
        sfxSource = sfxGO.AddComponent<AudioSource>();
        sfxSource.loop = false; sfxSource.playOnAwake = false;
    }

    void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVol", 0.55f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVol", 0.85f);
        isMuted   = PlayerPrefs.GetInt("Muted", 0) == 1;
        ApplyVolume();
    }

    void ApplyVolume()
    {
        if (bgmSource) bgmSource.volume = isMuted ? 0 : bgmVolume;
        if (sfxSource) sfxSource.volume = isMuted ? 0 : sfxVolume;
    }

    // ── BGM ───────────────────────────────────────────────
    public void PlayBGM(string name)
    {
        AudioClip clip = Resources.Load<AudioClip>(name)
                      ?? GetOrGenerate(name, true);
        if (bgmSource == null || clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip   = clip;
        bgmSource.volume = isMuted ? 0 : bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()  => bgmSource?.Stop();
    public void PauseBGM() => bgmSource?.Pause();
    public void ResumeBGM()=> bgmSource?.UnPause();

    // ── SFX ───────────────────────────────────────────────
    public void PlaySFX(string name)
    {
        if (isMuted) return;
        AudioClip clip = Resources.Load<AudioClip>(name)
                      ?? GetOrGenerate(name, false);
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ── VOLUME ────────────────────────────────────────────
    public void SetBGMVolume(float v)
    {
        bgmVolume = Mathf.Clamp01(v);
        if (bgmSource) bgmSource.volume = isMuted ? 0 : bgmVolume;
        PlayerPrefs.SetFloat("BGMVol", bgmVolume);
    }

    public void SetSFXVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        if (sfxSource) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVol", sfxVolume);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        ApplyVolume();
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
    }

    public bool IsMuted()       => isMuted;
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;

    // ── PROCEDURAL AUDIO ──────────────────────────────────
    AudioClip GetOrGenerate(string name, bool isLong)
    {
        if (_cache.TryGetValue(name, out var cached)) return cached;
        var clip = isLong ? GenBGM(name) : GenSFX(name);
        _cache[name] = clip;
        return clip;
    }

    AudioClip GenSFX(string name)
    {
        int sr = 44100;
        // Definisi SFX: (freq, durationSec, type)
        // type: 0=sine, 1=square, 2=sweep
        var sfxMap = new Dictionary<string, (float freq, float dur, int type)>
        {
            ["sfx_click"]         = (1200f, 0.08f, 0),
            ["sfx_correct"]       = (1450f, 0.22f, 0),
            ["sfx_wrong"]         = (280f,  0.30f, 0),
            ["sfx_win"]           = (1600f, 0.45f, 0),
            ["sfx_lose"]          = (250f,  0.40f, 0),
            ["sfx_star"]          = (1800f, 0.18f, 0),
            ["sfx_levelup"]       = (2000f, 0.55f, 2),
            ["sfx_achievement"]   = (1700f, 0.38f, 0),
            ["sfx_unlock"]        = (1000f, 0.28f, 2),
            ["sfx_timer_warning"] = (600f,  0.12f, 1),
            ["sfx_swipe"]         = (800f,  0.10f, 2),
            ["sfx_pop"]           = (1100f, 0.09f, 0),
        };

        sfxMap.TryGetValue(name, out var p);
        float freq = p.freq > 0 ? p.freq : 900f;
        float dur  = p.dur  > 0 ? p.dur  : 0.12f;
        int   type = p.type;

        int samples = Mathf.RoundToInt(sr * dur);
        var clip = AudioClip.Create(name, samples, 1, sr, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t    = (float)i / sr;
            float env  = Mathf.Clamp01(1f - t / dur);
            float wave = type switch
            {
                1 => Mathf.Sign(Mathf.Sin(2 * Mathf.PI * freq * t)),
                2 => Mathf.Sin(2 * Mathf.PI * (freq + (freq * 0.5f * t / dur)) * t),
                _ => Mathf.Sin(2 * Mathf.PI * freq * t)
            };
            data[i] = wave * env * 0.5f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenBGM(string name)
    {
        int sr = 44100;
        float dur = 3f;
        int samples = Mathf.RoundToInt(sr * dur);
        var clip = AudioClip.Create(name, samples, 1, sr, false);
        var data = new float[samples];

        float[] freqs = { 261f, 329f, 392f, 523f };
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sr;
            float s = 0f;
            foreach (float f in freqs)
                s += Mathf.Sin(2 * Mathf.PI * f * t) * 0.08f;
            data[i] = s;
        }
        clip.SetData(data, 0);
        return clip;
    }
}