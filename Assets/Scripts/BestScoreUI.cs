using TMPro;
using UnityEngine;

public class BestScoreUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text BestScoreLabel;

    private void OnEnable()
    {
        // 1. Listen for data updates instantly whenever a score changes or saves
        SaveManager.OnSettingsChanged += RefreshScoreDisplay;
        RefreshScoreDisplay();
    }

    private void OnDisable()
    {
        // 2. Unsubscribe to prevent memory leaks when panels turn off
        SaveManager.OnSettingsChanged -= RefreshScoreDisplay;
    }
    
    private void RefreshScoreDisplay()
    {
        if (BestScoreLabel == null || SaveManager.Instance == null)
            return;

        BestScoreLabel.SetText(SaveManager.Instance.BestScore.ToString());
    }

}
