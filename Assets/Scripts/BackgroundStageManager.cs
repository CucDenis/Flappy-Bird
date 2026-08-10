using System;
using UnityEngine;

public sealed class BackgroundStageManager : MonoBehaviour
{
    [Serializable]
    public sealed class Stage
    {
        public string stageName;

        [Min(0)]
        public int minimumScore;

        public GameObject root;

        public bool allowClouds = true;
    }

    [Header("References")]
    [SerializeField]
    private BackgroundCloudSpawner cloudSpawner;

    [Header("Stages")]
    [SerializeField]
    private Stage[] stages;

    private int activeStageIndex = -1;

    private void Start()
    {
        ApplyStageForScore(0);
    }

    public void HandleScoreChanged(int score)
    {
        ApplyStageForScore(score);
    }

    private void ApplyStageForScore(int score)
    {
        if (stages == null || stages.Length == 0)
        {
            return;
        }

        int targetStageIndex = 0;

        for (int i = 0; i < stages.Length; i++)
        {
            if (score >= stages[i].minimumScore)
            {
                targetStageIndex = i;
            }
        }

        if (targetStageIndex == activeStageIndex)
        {
            return;
        }

        ApplyStage(targetStageIndex);
    }

    private void ApplyStage(int index)
    {
        if (index < 0 || index >= stages.Length)
        {
            return;
        }

        activeStageIndex = index;

        for (int i = 0; i < stages.Length; i++)
        {
            Stage stage = stages[i];

            if (stage.root != null)
            {
                stage.root.SetActive(i == activeStageIndex);
            }
        }

        if (cloudSpawner != null)
        {
            cloudSpawner.SetCloudsEnabled(
                stages[activeStageIndex].allowClouds
            );
        }
    }
}