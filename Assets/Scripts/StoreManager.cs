using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    private int totalScoreAmount;
    private int totalCoinAmount;
    private int totalDimondsAmount;
    private const int ScorePerCoin = 5;
    private const int CoinsPerDiamond = 10;
    
    [SerializeField] private TMP_Text totalScoreAmountText; 
    [SerializeField] private TMP_Text totalCoinAmountText;
    [SerializeField] private TMP_Text totalDimondsAmountText;
    [SerializeField] private TMP_InputField amountToBeConverted;
    [SerializeField] private TMP_Dropdown baseTypeConvertion;

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

    private void UpdateUI()
    {
        if (totalScoreAmountText != null) totalScoreAmountText.text = totalScoreAmount.ToString();
        if (totalCoinAmountText != null) totalCoinAmountText.text = totalCoinAmount.ToString();
        if (totalDimondsAmountText != null) totalDimondsAmountText.text = totalDimondsAmount.ToString();
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
                conversionSuccessful = ConvertScoreIntoCoins(amount);
                break;
            case "diamond":
                conversionSuccessful = ConvertCoinsIntoDiamonds(amount);
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

    public bool ConvertScoreIntoCoins(int scoreAmount)
    {
        if (scoreAmount <= 0)
            return false;

        int coins = scoreAmount / ScorePerCoin;

        if (coins <= 0)
            return false;

        int scoreCost = coins * ScorePerCoin;

        if (SaveManager.Instance.TotalScoreAmount < scoreCost)
            return false;

        int remainingScore =
            SaveManager.Instance.TotalScoreAmount - scoreCost;

        int newCoins =
            SaveManager.Instance.TotalCoinAmount + coins;

        SaveManager.Instance.SetTotalScoreAmount(remainingScore);
        SaveManager.Instance.SetTotalCoinAmount(newCoins);

        return true;
    }

    public bool ConvertCoinsIntoDiamonds(int coinsAmount)
    {
        if (coinsAmount <= 0)
            return false;

        int diamonds = coinsAmount / CoinsPerDiamond;

        if (diamonds <= 0)
            return false;

        int coinCost = diamonds * CoinsPerDiamond;

        if (SaveManager.Instance.TotalCoinAmount < coinCost)
            return false;

        int remainingCoin =
            SaveManager.Instance.TotalCoinAmount - coinCost;

        int newDiamonds =
            SaveManager.Instance.TotalDimondsAmount + diamonds;

        SaveManager.Instance.SetTotalCoinAmount(remainingCoin);
        SaveManager.Instance.SetTotalDiamondsAmount(newDiamonds);

        return true;
    }

}
