using TMPro;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private int totalScoreAmount;
    private int totalCoinAmount;
    private int totalDimondsAmount;
    
    [SerializeField] private TMP_Text totalScoreAmountText; 
    [SerializeField] private TMP_Text totalCoinAmountText;
    [SerializeField] private TMP_Text totalDimondsAmountText;
    [SerializeField] private TMP_InputField amountToBeConverted;
    [SerializeField] private TMP_Dropdown baseTypeConvertion;

    [Header("Managers")]
    [SerializeField] private EconomyManager economyManager;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[StoreManager] SaveManager is not initialized.");
            return;
        }

        totalScoreAmount = SaveManager.Instance.TotalScoreAmount;
        totalCoinAmount = SaveManager.Instance.TotalCoinAmount;
        totalDimondsAmount = SaveManager.Instance.TotalDimondsAmount;

        UpdateUI();
    }

    public void Convert()
    {
        if (!int.TryParse(amountToBeConverted.text, out int amount) || amount <= 0)
        {
            Debug.LogWarning("Invalid conversion amount entered.");
            return;
        }

        string selectedOption = baseTypeConvertion.options[baseTypeConvertion.value].text.ToLower();

        bool conversionSuccessful = false;

        switch (selectedOption)
        {
            case "coin":
                conversionSuccessful = economyManager.ConvertScoreIntoCoins(amount);
                break;
            case "diamond":
                conversionSuccessful = economyManager.ConvertCoinsIntoDiamonds(amount);
                break;
            default:
                Debug.LogWarning($"Unknown conversion type: {selectedOption}");
                break;
        }

        if (conversionSuccessful)
        {
            Refresh();
            amountToBeConverted.text = "";
        }
    }

    private void UpdateUI()
    {
        if (totalScoreAmountText != null) totalScoreAmountText.text = totalScoreAmount.ToString();
        if (totalCoinAmountText != null) totalCoinAmountText.text = totalCoinAmount.ToString();
        if (totalDimondsAmountText != null) totalDimondsAmountText.text = totalDimondsAmount.ToString();
    }

}
