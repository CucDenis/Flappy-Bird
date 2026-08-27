using UnityEngine;

public class GameStorage : MonoBehaviour
{
    public static GameStorage Instance { get; private set; }
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
    } 

    public void UpdateBestScore(int value)
    {
        if ( value <= BestScore)
            return;

        PlayerPrefs.SetInt(
            BestScoreKey,
            value
        );

        PlayerPrefs.Save();

    }

    public void UpdateMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(
            MusicVolumKey,
            value
        );

        PlayerPrefs.Save();

    }

    public void UpdateSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(
            SfxVolumeKey,
            value
        );

        PlayerPrefs.Save();

    }
}
