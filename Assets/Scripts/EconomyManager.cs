using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    private const int ScorePerCoin = 5;
    private const int CoinsPerDiamond = 10;

    public void AddScore(int scoreToAdd)
    {
        int newTotalScoreAmount = SaveManager.Instance.TotalScoreAmount + scoreToAdd;

        SaveManager.Instance.SetTotalScoreAmount(newTotalScoreAmount);

    }

    public void AddCoin(int coinToAdd)
    {
        int newTotalCoinAmount = SaveManager.Instance.TotalCoinAmount + coinToAdd;

        SaveManager.Instance.SetTotalCoinAmount(newTotalCoinAmount);
        
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
