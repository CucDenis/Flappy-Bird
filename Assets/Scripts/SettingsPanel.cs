using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text MusicVolumeLable;
    [SerializeField] private TMP_Text SfxVolumeLable;
    [SerializeField] private Slider MusicVolumeSlider;
    [SerializeField] private Slider SfxVolumeSlider;

    private void OnEnable()
    {
        SaveManager.OnSettingsChanged += LoadSettingsValues;
        LoadSettingsValues();
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks!
        SaveManager.OnSettingsChanged -= LoadSettingsValues;
    }
    
    private void LoadSettingsValues()
    {
        // Guard clause: Prevents crashing if singletons are building on Frame 1
        if (SaveManager.Instance == null || AudioManager.Instance == null)
            return;

        // Cast to (int) so loaded text matches the whole-number formatting
        MusicVolumeLable.text = ((int)SaveManager.Instance.MusicVolume).ToString();
        SfxVolumeLable.text = ((int)SaveManager.Instance.SfxVolume).ToString();

        // Assign slider values directly without re-triggering events loops
        MusicVolumeSlider.SetValueWithoutNotify(SaveManager.Instance.MusicVolume);
        SfxVolumeSlider.SetValueWithoutNotify(SaveManager.Instance.SfxVolume);

    }

    public void OnMusicSliderChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.UpdateMusicVolume(value);

    }

    public void OnSfxSliderChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.UpdateSfxVolume(value);

    }
}
