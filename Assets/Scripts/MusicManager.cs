using System.Collections;
using UnityEngine;

public sealed class MusicManager : MonoBehaviour
{
    [System.Serializable]
    private sealed class MusicLayer
    {
        public string name;

        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Min(0f)]
        public float fadeDuration = 2f;

        [HideInInspector]
        public AudioSource source;

        [HideInInspector]
        public float currentVolume;
    }

    [Header("Music")]
    [SerializeField]
    private MusicLayer baseLayer;

    [SerializeField]
    private MusicLayer beachLayer;

    [SerializeField]
    private MusicLayer cityLayer;

    [SerializeField]
    private MusicLayer hillsLayer;

    [SerializeField]
    private MusicLayer mountainsLayer;

    [SerializeField]
    private MusicLayer lowSkyLayer;

    [SerializeField]
    private MusicLayer midSkyLayer;

    [SerializeField]
    private MusicLayer highSkyLayer;

    [SerializeField]
    private MusicLayer spaceLayer;

    [Header("Master")]
    [SerializeField]
    [Range(0f, 1f)]
    private float masterVolume = 1f;

    private MusicLayer[] layers;

    private bool musicStarted;

    private void Awake()
    {
        layers = new[]
        {
            baseLayer,
            beachLayer,
            cityLayer,
            hillsLayer,
            mountainsLayer,
            lowSkyLayer,
            midSkyLayer,
            highSkyLayer,
            spaceLayer
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

            AudioSource source =
                gameObject.AddComponent<AudioSource>();

            source.clip = layer.clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            source.spatialBlend = 0f;

            layer.source = source;
            layer.currentVolume = 0f;
        }
    }

    public void StartMusic()
    {
        if (musicStarted)
        {
            return;
        }

        musicStarted = true;

        /*
         * Start every layer at exactly the same DSP time.
         *
         * This is important because all of your WAV files
         * are exactly 8 seconds long.
         */
        double startTime =
            AudioSettings.dspTime + 0.1;

        foreach (MusicLayer layer in layers)
        {
            if (layer == null ||
                layer.source == null)
            {
                continue;
            }

            layer.source.volume = 0f;
            layer.currentVolume = 0f;

            layer.source.PlayScheduled(startTime);
        }

        /*
         * Ocean is always present from the beginning.
         */
        FadeIn(baseLayer);
    }

    public void OnStageChanged(
        BackgroundStageManager.StageType stage
    )
    {
        if (!musicStarted)
        {
            return;
        }

        switch (stage)
        {
            case BackgroundStageManager.StageType.Sea:
                break;

            case BackgroundStageManager.StageType.Beach:
                FadeIn(beachLayer);
                break;

            case BackgroundStageManager.StageType.Hills:
                FadeIn(hillsLayer);
                break;

            case BackgroundStageManager.StageType.Mountains:
                FadeIn(mountainsLayer);
                break;

            case BackgroundStageManager.StageType.LowSky:
                FadeIn(lowSkyLayer);
                break;

            case BackgroundStageManager.StageType.MidSky:
                FadeIn(midSkyLayer);
                break;

            case BackgroundStageManager.StageType.HighSky:
                FadeIn(highSkyLayer);
                break;

            case BackgroundStageManager.StageType.DeepSpace:
                FadeIn(spaceLayer);
                break;
        }
    }

    private void FadeIn(MusicLayer layer)
    {
        if (layer == null ||
            layer.source == null)
        {
            return;
        }

        StartCoroutine(
            FadeLayerIn(layer)
        );
    }

    private IEnumerator FadeLayerIn(
        MusicLayer layer
    )
    {
        float startVolume =
            layer.currentVolume;

        float duration =
            layer.fadeDuration;

        if (duration <= 0f)
        {
            layer.currentVolume =
                layer.volume;

            layer.source.volume =
                layer.volume *
                masterVolume;

            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration
                );

            /*
             * SmoothStep creates a softer musical fade.
             */
            float eased =
                progress *
                progress *
                (3f - 2f * progress);

            float volume =
                Mathf.Lerp(
                    startVolume,
                    layer.volume,
                    eased
                );

            layer.currentVolume =
                volume;

            layer.source.volume =
                volume *
                masterVolume;

            yield return null;
        }

        layer.currentVolume =
            layer.volume;

        layer.source.volume =
            layer.volume *
            masterVolume;
    }

    public void PauseMusic()
    {
        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null)
            {
                layer.source.Pause();
            }
        }
    }

    public void ResumeMusic()
    {
        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null)
            {
                layer.source.UnPause();
            }
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
        masterVolume =
            Mathf.Clamp01(volume);

        foreach (MusicLayer layer in layers)
        {
            if (layer?.source != null)
            {
                layer.source.volume =
                    layer.currentVolume *
                    masterVolume;
            }
        }
    }
}