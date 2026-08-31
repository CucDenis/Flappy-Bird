using UnityEngine;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    public static event Action OnSettingsChanged;
    private const string BestScoreKey = "best_score";
    private const string TotalScoreAmountKey = "total_score_amount";
    private const string TotalCoinAmountKey = "total_coin_amount";
    private const string TotalDimondsAmountKey = "total_diamonds_amount";
    private const string MusicVolumKey = "music_volume";
    private const string SfxVolumeKey = "sfx_volume";

    private const float DefaultMusicVolume = 100f;
    private const float DefaultSfxVolume = 100f;

    public int BestScore => 
        PlayerPrefs.GetInt(
            BestScoreKey,
            0
        );

    public int TotalScoreAmount =>
        PlayerPrefs.GetInt(
            TotalScoreAmountKey,
            0
        );

    public int TotalCoinAmount =>
        PlayerPrefs.GetInt(
            TotalCoinAmountKey,
            0
        );

    public int TotalDimondsAmount =>
        PlayerPrefs.GetInt(
            TotalDimondsAmountKey,
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

    public void SetTotalScoreAmount(int value)
    {
        int sanitizedValue = Mathf.Max(0, value);

        PlayerPrefs.SetInt(
            TotalScoreAmountKey,
            sanitizedValue
        );

        PlayerPrefs.Save();

    }

    public void SetTotalCoinAmount(int value)
    {
        int sanitizedValue = Mathf.Max(0, value);

        PlayerPrefs.SetInt(
            TotalCoinAmountKey,
            sanitizedValue
        );

        PlayerPrefs.Save();

    }

    public void SetTotalDiamondsAmount(int value)
    {
        int sanitizedValue = Mathf.Max(0, value);

        PlayerPrefs.SetInt(
            TotalDimondsAmountKey,
            sanitizedValue
        );

        PlayerPrefs.Save();
        
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
