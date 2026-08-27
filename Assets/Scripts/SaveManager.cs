using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public static event Action OnSettingsChanged;
    private string BestScoreKey = "best_score";
    private string MusicVolumKey = "music_volume";
    private string SfxVolumeKey = "sfx_volume";

    private const float DefaultMusicVolume = 100f;
    private const float DefaultSfxVolume = 100f;

    public int BestScore => 
        PlayerPrefs.GetInt(
            BestScoreKey,
            0
        );

    public float MusicVolume =>
        PlayerPrefs.GetFloat(
            MusicVolumKey,
            DefaultMusicVolume
        );

    public float SfxVolume =>
        PlayerPrefs.GetFloat(
            SfxVolumeKey,
            DefaultSfxVolume
        );

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    } 

    public void SaveBestScore(int value)
    {
        if ( value <= BestScore)
            return;

        PlayerPrefs.SetInt(
            BestScoreKey,
            value
        );

        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();

    }

    public void SaveMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(
            MusicVolumKey,
            value
        );

        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();

    }

    public void SaveSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(
            SfxVolumeKey,
            value
        );

        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();

    }
}
