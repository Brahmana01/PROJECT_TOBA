using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    // Volume settings
    private float bgmVolume = 0.5f;
    private float sfxVolume = 0.8f;
    private bool isMuted = false;

    // Cache audio clips yang sudah di-generate
    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupAudioSources()
    {
        // BGM Source
        GameObject bgmObj = new GameObject("BGM_Source");
        bgmObj.transform.SetParent(transform);
        bgmSource = bgmObj.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // SFX Source
        GameObject sfxObj = new GameObject("SFX_Source");
        sfxObj.transform.SetParent(transform);
        sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    void LoadVolumeSettings()
    {
        bgmVolume  = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxVolume  = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        isMuted    = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        if (bgmSource != null) bgmSource.volume = isMuted ? 0 : bgmVolume;
        if (sfxSource != null) sfxSource.volume = isMuted ? 0 : sfxVolume;
    }

    // ── BGM ──────────────────────────────────────────────────

    public void PlayBGM(string clipName)
    {
        // Coba load dari Resources dulu
        AudioClip clip = Resources.Load<AudioClip>(clipName);

        // Kalau tidak ada, generate sintetis
        if (clip == null)
            clip = GetOrGenerateClip(clipName, true);

        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.volume = isMuted ? 0 : bgmVolume;
            bgmSource.Play();
            Debug.Log("[Sound] BGM: " + clipName);
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // ── SFX ──────────────────────────────────────────────────

    public void PlaySFX(string clipName)
    {
        if (isMuted) return;

        // Coba load dari Resources dulu
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip == null)
            clip = GetOrGenerateClip(clipName, false);

        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ── VOLUME CONTROL ────────────────────────────────────────

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = isMuted ? 0 : bgmVolume;
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        if (bgmSource != null) bgmSource.volume = isMuted ? 0 : bgmVolume;
        if (sfxSource != null) sfxSource.volume = isMuted ? 0 : sfxVolume;
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
    }

    public bool IsMuted() => isMuted;
    public float GetBGMVolume() => bgmVolume;
    public float GetSFXVolume() => sfxVolume;

    // ── PROCEDURAL AUDIO (tanpa file audio asset) ─────────────
    // Generate clip sintetis agar tidak error saat file tidak ada

    AudioClip GetOrGenerateClip(string name, bool isLong)
    {
        if (clipCache.ContainsKey(name)) return clipCache[name];

        AudioClip clip = isLong
            ? GenerateBGMClip(name)
            : GenerateSFXClip(name);

        clipCache[name] = clip;
        return clip;
    }

    AudioClip GenerateSFXClip(string name)
    {
        int sampleRate = 44100;
        float duration = 0.15f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);

        float[] data = new float[samples];
        float freq = 880f; // Default frequency

        switch (name)
        {
            case "sfx_click":    freq = 1200f; duration = 0.08f; break;
            case "sfx_correct":  freq = 1400f; duration = 0.2f;  break;
            case "sfx_wrong":    freq = 300f;  duration = 0.3f;  break;
            case "sfx_win":      freq = 1600f; duration = 0.4f;  break;
            case "sfx_unlock":   freq = 1000f; duration = 0.25f; break;
            case "sfx_achieve":  freq = 1800f; duration = 0.35f; break;
            case "sfx_levelup":  freq = 2000f; duration = 0.5f;  break;
        }

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            envelope = Mathf.Clamp01(envelope);
            data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope * 0.5f;
        }

        clip.SetData(data, 0);
        return clip;
    }

    AudioClip GenerateBGMClip(string name)
    {
        // BGM sintetis sederhana (loop pendek)
        int sampleRate = 44100;
        float duration = 2f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);

        float[] data = new float[samples];
        float[] freqs = { 261f, 329f, 392f, 523f }; // C, E, G, C (chord natural)

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float sample = 0f;
            foreach (float f in freqs)
                sample += Mathf.Sin(2 * Mathf.PI * f * t) * 0.1f;
            data[i] = sample;
        }

        clip.SetData(data, 0);
        return clip;
    }
}