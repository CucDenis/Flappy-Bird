using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CrowSpawner : MonoBehaviour
{
    [Serializable]
    public sealed class DifficultyTier
    {
        [Min(0)]
        public int minimumScore;

        [Min(0.1f)]
        public float spawnInterval = 3f;

        [Min(0.1f)]
        public float crowSpeed = 3f;

        [Min(1)]
        public int minimumCrowsPerWave = 1;

        [Min(1)]
        public int maximumCrowsPerWave = 1;

        [Min(0f)]
        public float horizontalSpacing = 0.8f;

        [Min(0f)]
        public float minimumVerticalSpacing = 2f;
    }

    [Header("References")]
    [SerializeField] private EnemyCrow crowPrefab;
    [SerializeField] private Transform player;

    [Header("Spawn Area")]
    [SerializeField] private float spawnX = 8f;
    [SerializeField] private float minimumSpawnY = -3.5f;
    [SerializeField] private float maximumSpawnY = 3.5f;

    [Header("Timing")]
    [SerializeField] private float firstWaveDelay = 1.5f;

    [Header("Difficulty")]
    [SerializeField]
    private List<DifficultyTier> difficultyTiers = new();

    private Coroutine spawningCoroutine;
    private DifficultyTier currentTier;

    private void Awake()
    {
        ValidateDifficultyTiers();
        currentTier = GetTierForScore(0);
    }

    public void BeginSpawning()
    {
        StopSpawning();

        currentTier = GetTierForScore(
            GameManager.Instance != null
                ? GameManager.Instance.Score
                : 0
        );

        spawningCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawningCoroutine == null)
        {
            return;
        }

        StopCoroutine(spawningCoroutine);
        spawningCoroutine = null;
    }

    public void HandleScoreChanged(int score)
    {
        currentTier = GetTierForScore(score);
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        while (
            GameManager.Instance != null &&
            GameManager.Instance.IsPlaying
        )
        {
            SpawnWave();

            float interval = currentTier != null
                ? currentTier.spawnInterval
                : 3f;

            yield return new WaitForSeconds(interval);
        }

        spawningCoroutine = null;
    }

    private void SpawnWave()
    {
        if (crowPrefab == null || player == null || currentTier == null)
        {
            return;
        }

        int minimumCount = Mathf.Max(
            1,
            currentTier.minimumCrowsPerWave
        );

        int maximumCount = Mathf.Max(
            minimumCount,
            currentTier.maximumCrowsPerWave
        );

        int requestedCrowCount = UnityEngine.Random.Range(
            minimumCount,
            maximumCount + 1
        );

        List<float> usedYPositions = new();

        for (int index = 0; index < requestedCrowCount; index++)
        {
            if (
                !TryFindValidY(
                    usedYPositions,
                    currentTier.minimumVerticalSpacing,
                    out float spawnY
                )
            )
            {
                // Spawn fewer crows instead of an impossible formation.
                break;
            }

            usedYPositions.Add(spawnY);

            float crowX =
                spawnX +
                index * currentTier.horizontalSpacing;

            EnemyCrow crow = Instantiate(
                crowPrefab,
                new Vector3(crowX, spawnY, 0f),
                Quaternion.identity
            );

            crow.Initialize(
                currentTier.crowSpeed,
                player
            );
        }
    }

    private bool TryFindValidY(
        IReadOnlyList<float> usedPositions,
        float minimumSpacing,
        out float result
    )
    {
        const int maximumAttempts = 20;

        for (int attempt = 0; attempt < maximumAttempts; attempt++)
        {
            float candidate = UnityEngine.Random.Range(
                minimumSpawnY,
                maximumSpawnY
            );

            bool valid = true;

            for (int index = 0; index < usedPositions.Count; index++)
            {
                float distance = Mathf.Abs(
                    candidate - usedPositions[index]
                );

                if (distance < minimumSpacing)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                result = candidate;
                return true;
            }
        }

        result = 0f;
        return false;
    }

    private DifficultyTier GetTierForScore(int score)
    {
        if (difficultyTiers == null || difficultyTiers.Count == 0)
        {
            return null;
        }

        DifficultyTier selectedTier = difficultyTiers[0];

        for (int index = 0; index < difficultyTiers.Count; index++)
        {
            DifficultyTier tier = difficultyTiers[index];

            if (score >= tier.minimumScore)
            {
                selectedTier = tier;
            }
            else
            {
                break;
            }
        }

        return selectedTier;
    }

    private void ValidateDifficultyTiers()
    {
        if (difficultyTiers == null)
        {
            difficultyTiers = new List<DifficultyTier>();
        }

        difficultyTiers.Sort(
            (left, right) =>
                left.minimumScore.CompareTo(right.minimumScore)
        );

        for (int index = 0; index < difficultyTiers.Count; index++)
        {
            DifficultyTier tier = difficultyTiers[index];

            tier.minimumCrowsPerWave = Mathf.Max(
                1,
                tier.minimumCrowsPerWave
            );

            tier.maximumCrowsPerWave = Mathf.Max(
                tier.minimumCrowsPerWave,
                tier.maximumCrowsPerWave
            );
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateDifficultyTiers();
    }
#endif
}