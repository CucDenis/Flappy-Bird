using System;
using System.Collections;
using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    private sealed class MusicLayer
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Min(0f)] public float fadeDuration = 2f;
        [HideInInspector] public AudioSource source;
        [HideInInspector] public float currentVolume;
    }

    [Header("Music")]
    [SerializeField] private MusicLayer baseLayer;
    [SerializeField] private MusicLayer beachLayer;
    [SerializeField] private MusicLayer cityLayer;
    [SerializeField] private MusicLayer hillsLayer;
    [SerializeField] private MusicLayer mountainsLayer;
    [SerializeField] private MusicLayer lowSkyLayer;
    [SerializeField] private MusicLayer midSkyLayer;
    [SerializeField] private MusicLayer highSkyLayer;
    [SerializeField] private MusicLayer spaceLayer;

    // FIX 2: Separate your music and SFX variables so they don't overwrite each other
    private float masterMusicVolume = 100f;
    private float masterSfxVolume = 100f;

    private MusicLayer[] layers;
    private bool musicStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        layers = new[]
        {
            baseLayer, beachLayer, cityLayer, hillsLayer, mountainsLayer,
            lowSkyLayer, midSkyLayer, highSkyLayer, spaceLayer
        };

        CreateAudioSources();
    }

    private void CreateAudioSources()
    {
        foreach (MusicLayer layer in layers)
        {
            if (layer == null || layer.clip == null)
            {
                continue;
            }

            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.clip = layer.clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f; // Audio tracks deliberately start completely silent
            source.spatialBlend = 0f;

            layer.source = source;
            layer.currentVolume = 0f;
        }

        // 1. Fetch the persisted numerical values safely from your SaveManager
        masterMusicVolume = SaveManager.Instance != null ? SaveManager.Instance.MusicVolume : 100f;
        masterSfxVolume = SaveManager.Instance != null ? SaveManager.Instance.SfxVolume : 100f;

        // FIX: Force the system to actively push the loaded volume into your engine registers right now!
        UpdateMusicVolume(masterMusicVolume);
    }


    public void UpdateMusicVolume(float value)
    {
        masterMusicVolume = Mathf.Clamp(value, 0, 100);

        // FIX 3: Instantly update all working audio tracks so changes are heard live
        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null)
            {
                // FIX 1: Divide by 100f to convert 0-100 down to Unity's 0.0 - 1.0 standard
                layer.source.volume = layer.currentVolume * (masterMusicVolume / 100f);
            }
        }

        SaveManager.Instance?.SaveMusicVolume(masterMusicVolume);
    }

    public void UpdateSfxVolume(float value)
    {
        masterSfxVolume = Mathf.Clamp(value, 0, 100);

        // NOTE: Apply this volume variable to your Sound Effects AudioSources when you play sounds!
        SaveManager.Instance?.SaveSfxVolume(masterSfxVolume);
    }

    public void StartMusic()
    {
        if (musicStarted) return;
        musicStarted = true;

        double startTime = AudioSettings.dspTime + 0.1;

        foreach (MusicLayer layer in layers)
        {
            if (layer == null || layer.source == null) continue;

            layer.source.volume = 0f;
            layer.currentVolume = 0f;
            layer.source.PlayScheduled(startTime);
        }

        FadeIn(baseLayer);
    }

    public void OnStageChanged(BackgroundStageManager.StageType stage)
    {
        if (!musicStarted) return;

        switch (stage)
        {
            case BackgroundStageManager.StageType.Sea: break;
            case BackgroundStageManager.StageType.Beach: FadeIn(beachLayer); break;
            case BackgroundStageManager.StageType.Hills: FadeIn(hillsLayer); break;
            case BackgroundStageManager.StageType.Mountains: FadeIn(mountainsLayer); break;
            case BackgroundStageManager.StageType.LowSky: FadeIn(lowSkyLayer); break;
            case BackgroundStageManager.StageType.MidSky: FadeIn(midSkyLayer); break;
            case BackgroundStageManager.StageType.HighSky: FadeIn(highSkyLayer); break;
            case BackgroundStageManager.StageType.DeepSpace: FadeIn(spaceLayer); break;
        }
    }

    private void FadeIn(MusicLayer layer)
    {
        if (layer == null || layer.source == null) return;
        StartCoroutine(FadeLayerIn(layer));
    }

    private IEnumerator FadeLayerIn(MusicLayer layer)
    {
        float startVolume = layer.currentVolume;
        float duration = layer.fadeDuration;

        if (duration <= 0f)
        {
            layer.currentVolume = layer.volume;
            // FIX 1: Divide by 100f
            layer.source.volume = layer.volume * (masterMusicVolume / 100f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = progress * progress * (3f - 2f * progress);

            float volume = Mathf.Lerp(startVolume, layer.volume, eased);
            layer.currentVolume = volume;

            // FIX 1: Divide by 100f
            layer.source.volume = volume * (masterMusicVolume / 100f);
            yield return null;
        }

        layer.currentVolume = layer.volume;
        // FIX 1: Divide by 100f
        layer.source.volume = layer.volume * (masterMusicVolume / 100f);
    }

    public void PauseMusic()
    {
        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null) layer.source.Pause();
        }
    }

    public void ResumeMusic()
    {
        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null) layer.source.UnPause();
        }
    }

    public void StopMusic()
    {
        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null)
            {
                layer.source.Stop();
                layer.source.volume = 0f;
                layer.currentVolume = 0f;
            }
        }
        musicStarted = false;
    }

    public void SetMasterVolume(float volume)
    {
        // Converts a 0-1 input value directly up to a 0-100 volume standard
        UpdateMusicVolume(volume * 100f);
    }
}
