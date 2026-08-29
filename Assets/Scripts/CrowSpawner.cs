using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CrowSpawner : MonoBehaviour
{
    public enum DifficultyStyle
    {
        Easy,
        Fast,
        Technical,
        BurstTechnical,
        Alternating,
        DeepSpace
    }

    private enum TechnicalFormation
    {
        HighLow,
        LowHigh,
        WideGate,
        OffsetPair,
        RisingSteps,
        FallingSteps
    }

    [Serializable]
    public sealed class DifficultyTier
    {
        public BackgroundStageManager.StageType stage;

        public DifficultyStyle style;

        [Header("Base")]
        [Min(0.1f)]
        public float spawnInterval = 2.5f;

        [Min(0.1f)]
        public float crowSpeed = 3f;

        [Min(1)]
        public int minimumCrowsPerWave = 1;

        [Min(1)]
        public int maximumCrowsPerWave = 1;

        [Min(0f)]
        public float horizontalSpacing = 1.2f;

        [Min(0f)]
        public float minimumVerticalSpacing = 1.8f;

        [Header("Burst")]
        [Range(0f, 1f)]
        public float burstChance = 0f;

        [Min(0.1f)]
        public float burstSpeed = 4f;

        [Min(0f)]
        public float burstDelay = 0.7f;

        [Min(0f)]
        public float burstDuration = 0.6f;

        [Header("Alternating")]
        [Min(1)]
        public int alternatingWavesPerPhase = 2;

        [Min(0.1f)]
        public float fastPhaseSpeed = 4.2f;

        [Min(0.1f)]
        public float fastPhaseSpawnInterval = 1.5f;

        [Min(0.1f)]
        public float technicalPhaseSpawnInterval = 2f;
    }

    [Header("References")]
    [SerializeField]
    private EnemyCrow crowPrefab;

    [SerializeField]
    private Transform player;

    [Header("Spawn Area")]
    [SerializeField]
    private float spawnX = 8f;

    [SerializeField]
    private float minimumSpawnY = -3.5f;

    [SerializeField]
    private float maximumSpawnY = 3.5f;

    [Header("Timing")]
    [SerializeField]
    private float firstWaveDelay = 1.5f;

    [Header("Difficulty")]
    [SerializeField]
    private List<DifficultyTier> difficultyTiers = new();

    private Coroutine spawningCoroutine;

    private DifficultyTier currentTier;

    private bool alternatingFastPhase = true;

    private int alternatingWavesRemaining = 2;

    private int deepSpacePatternIndex;

    private void Awake()
    {
        ValidateDifficultyTiers();

        currentTier =
            GetTierForStage(
                BackgroundStageManager.StageType.Sea
            );
    }

    public void BeginSpawning()
    {
        StopSpawning();

        if (currentTier == null)
        {
            currentTier =
                GetTierForStage(
                    BackgroundStageManager.StageType.Sea
                );
        }

        spawningCoroutine =
            StartCoroutine(
                SpawnLoop()
            );
    }

    public void StopSpawning()
    {
        if (spawningCoroutine == null)
        {
            return;
        }

        StopCoroutine(
            spawningCoroutine
        );

        spawningCoroutine = null;
    }

    public void SetStage(
        BackgroundStageManager.StageType stage
    )
    {
        DifficultyTier tier =
            GetTierForStage(stage);

        if (tier == null)
        {
            Debug.LogWarning(
                $"No CrowSpawner tier configured for {stage}."
            );

            return;
        }

        currentTier = tier;

        if (
            currentTier.style ==
            DifficultyStyle.Alternating
        )
        {
            alternatingFastPhase = true;

            alternatingWavesRemaining =
                Mathf.Max(
                    1,
                    currentTier.alternatingWavesPerPhase
                );
        }

        if (
            stage ==
            BackgroundStageManager.StageType.DeepSpace
        )
        {
            deepSpacePatternIndex = 0;
        }

        Debug.Log(
            $"CROW DIFFICULTY -> {stage} / {currentTier.style}"
        );
    }

    private float GetRandomVerticalSpacing()
    {
        float minimum =
            Mathf.Max(
                0.5f,
                currentTier.minimumVerticalSpacing
            );

        float maximum =
            minimum * 1.5f;

        return UnityEngine.Random.Range(
            minimum,
            maximum
        );
    }

    private IEnumerator SpawnLoop()
    {
        yield return
            new WaitForSeconds(
                firstWaveDelay
            );

        while (
            GameManager.Instance != null &&
            GameManager.Instance.IsPlaying
        )
        {
            float interval =
                GetCurrentSpawnInterval();

            SpawnWave();

            yield return
                new WaitForSeconds(
                    interval
                );
        }

        spawningCoroutine = null;
    }

    private float GetCurrentSpawnInterval()
    {
        if (currentTier == null)
        {
            return 3f;
        }

        if (
            currentTier.style ==
            DifficultyStyle.Alternating
        )
        {
            return alternatingFastPhase
                ? currentTier.fastPhaseSpawnInterval
                : currentTier.technicalPhaseSpawnInterval;
        }

        if (
            currentTier.style ==
            DifficultyStyle.DeepSpace
        )
        {
            return GetDeepSpaceSpawnInterval();
        }

        return currentTier.spawnInterval;
    }

    private float GetDeepSpaceSpawnInterval()
    {
        int pattern =
            deepSpacePatternIndex % 6;

        switch (pattern)
        {
            case 0:
                return 1.6f;

            case 1:
                return 2.1f;

            case 2:
                return 1.8f;

            case 3:
                return 1.6f;

            case 4:
                return 2.4f;

            case 5:
                return 2.6f;

            default:
                return 2f;
        }
    }

    private void SpawnWave()
    {
        if (
            crowPrefab == null ||
            player == null ||
            currentTier == null
        )
        {
            return;
        }

        switch (currentTier.style)
        {
            case DifficultyStyle.Easy:
                SpawnEasyWave();
                break;

            case DifficultyStyle.Fast:
                SpawnFastWave();
                break;

            case DifficultyStyle.Technical:
                SpawnTechnicalWave();
                break;

            case DifficultyStyle.BurstTechnical:
                SpawnBurstTechnicalWave();
                break;

            case DifficultyStyle.Alternating:
                SpawnAlternatingWave();
                break;

            case DifficultyStyle.DeepSpace:
                SpawnDeepSpaceWave();
                break;
        }
    }

    private EnemyCrow SpawnCrow(
        float x,
        float y,
        float speed,
        bool allowBurst = false
    )
    {
        EnemyCrow crow =
            Instantiate(
                crowPrefab,
                new Vector3(
                    x,
                    y,
                    0f
                ),
                Quaternion.identity
            );

        crow.Initialize(
            speed,
            player
        );

        if (allowBurst)
        {
            TryConfigureBurst(
                crow
            );
        }

        return crow;
    }

    private void SpawnEasyWave()
    {
        float y =
            UnityEngine.Random.Range(
                minimumSpawnY + 0.5f,
                maximumSpawnY - 0.5f
            );

        SpawnCrow(
            spawnX,
            y,
            currentTier.crowSpeed
        );
    }

    private void SpawnFastWave()
    {
        float y =
            UnityEngine.Random.Range(
                minimumSpawnY + 0.35f,
                maximumSpawnY - 0.35f
            );

        SpawnCrow(
            spawnX,
            y,
            currentTier.crowSpeed
        );
    }

    private void SpawnTechnicalWave()
    {
        TechnicalFormation formation =
            GetRandomTechnicalFormation();

        SpawnTechnicalFormation(
            formation,
            currentTier.crowSpeed,
            false
        );
    }

    private void SpawnBurstTechnicalWave()
    {
        TechnicalFormation formation =
            GetRandomMountainFormation();

        SpawnTechnicalFormation(
            formation,
            currentTier.crowSpeed,
            true
        );
    }

    private TechnicalFormation
        GetRandomTechnicalFormation()
    {
        if (currentTier == null)
        {
            return TechnicalFormation.HighLow;
        }

        switch (currentTier.stage)
        {
            case BackgroundStageManager.StageType.Hills:
                return GetRandomHillsFormation();

            case BackgroundStageManager.StageType.Mountains:
                return GetRandomMountainFormation();

            case BackgroundStageManager.StageType.MidSky:
                return GetRandomMidSkyFormation();

            case BackgroundStageManager.StageType.HighSky:
                return GetRandomMidSkyFormation();

            default:
                return GetRandomBasicTechnicalFormation();
        }
    }

    private TechnicalFormation
        GetRandomBasicTechnicalFormation()
    {
        int index =
            UnityEngine.Random.Range(
                0,
                3
            );

        switch (index)
        {
            case 0:
                return TechnicalFormation.HighLow;

            case 1:
                return TechnicalFormation.LowHigh;

            default:
                return TechnicalFormation.WideGate;
        }
    }

    private TechnicalFormation
        GetRandomHillsFormation()
    {
        int index =
            UnityEngine.Random.Range(
                0,
                4
            );

        switch (index)
        {
            case 0:
                return TechnicalFormation.HighLow;

            case 1:
                return TechnicalFormation.LowHigh;

            case 2:
                return TechnicalFormation.WideGate;

            default:
                return TechnicalFormation.OffsetPair;
        }
    }

    private TechnicalFormation
        GetRandomMountainFormation()
    {
        int index =
            UnityEngine.Random.Range(
                0,
                5
            );

        switch (index)
        {
            case 0:
                return TechnicalFormation.HighLow;

            case 1:
                return TechnicalFormation.LowHigh;

            case 2:
                return TechnicalFormation.OffsetPair;

            case 3:
                return TechnicalFormation.RisingSteps;

            default:
                return TechnicalFormation.FallingSteps;
        }
    }

    private TechnicalFormation
        GetRandomMidSkyFormation()
    {
        int index =
            UnityEngine.Random.Range(
                0,
                6
            );

        switch (index)
        {
            case 0:
                return TechnicalFormation.HighLow;

            case 1:
                return TechnicalFormation.LowHigh;

            case 2:
                return TechnicalFormation.WideGate;

            case 3:
                return TechnicalFormation.OffsetPair;

            case 4:
                return TechnicalFormation.RisingSteps;

            default:
                return TechnicalFormation.FallingSteps;
        }
    }

    private void SpawnTechnicalFormation(
        TechnicalFormation formation,
        float speed,
        bool allowBurst
    )
    {

        float spacing = Mathf.Max(
            1.2f,
            currentTier.horizontalSpacing
        );

        switch (formation)
        {
            case TechnicalFormation.HighLow:
            {
                float verticalSpacing = GetRandomVerticalSpacing();

                float centerY = UnityEngine.Random.Range(
                    minimumSpawnY + verticalSpacing / 2f,
                    maximumSpawnY - verticalSpacing / 2f
                );

                SpawnCrow(
                    spawnX,
                    centerY + verticalSpacing / 2f,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing,
                    centerY - verticalSpacing / 2f,
                    speed,
                    allowBurst
                );

                break;
            }

            case TechnicalFormation.LowHigh:
            {
                float verticalSpacing = GetRandomVerticalSpacing();

                float centerY = UnityEngine.Random.Range(
                    minimumSpawnY + verticalSpacing / 2f,
                    maximumSpawnY - verticalSpacing / 2f
                );

                SpawnCrow(
                    spawnX,
                    centerY - verticalSpacing / 2f,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing,
                    centerY + verticalSpacing / 2f,
                    speed,
                    allowBurst
                );

                break;
            }

            case TechnicalFormation.WideGate:
            {
                SpawnCrow(
                    spawnX,
                    maximumSpawnY,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing,
                    minimumSpawnY,
                    speed,
                    allowBurst
                );

                break;
            }

            case TechnicalFormation.RisingSteps:
            {
                float spacing1 = GetRandomVerticalSpacing();
                float spacing2 = GetRandomVerticalSpacing();

                float totalHeight = spacing1 + spacing2;

                float startY = UnityEngine.Random.Range(
                    minimumSpawnY + totalHeight,
                    maximumSpawnY
                );

                SpawnCrow(
                    spawnX,
                    startY,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing,
                    startY - spacing1,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing * 2f,
                    startY - totalHeight,
                    speed,
                    allowBurst
                );

                break;
            }

            case TechnicalFormation.FallingSteps:
            {
                float spacing1 = GetRandomVerticalSpacing();
                float spacing2 = GetRandomVerticalSpacing();

                float totalHeight = spacing1 + spacing2;

                float startY = UnityEngine.Random.Range(
                    minimumSpawnY,
                    maximumSpawnY - totalHeight
                );

                SpawnCrow(
                    spawnX,
                    startY,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing,
                    startY + spacing1,
                    speed,
                    allowBurst
                );

                SpawnCrow(
                    spawnX + spacing * 2f,
                    startY + totalHeight,
                    speed,
                    allowBurst
                );

                break;
            }
        }
    }

    private void TryConfigureBurst(
        EnemyCrow crow
    )
    {
        if (
            crow == null ||
            currentTier == null
        )
        {
            return;
        }

        if (
            currentTier.burstChance <= 0f
        )
        {
            return;
        }

        if (
            UnityEngine.Random.value >
            currentTier.burstChance
        )
        {
            return;
        }

        crow.ConfigureBurst(
            currentTier.burstSpeed,
            currentTier.burstDelay,
            currentTier.burstDuration
        );
    }

    private void SpawnAlternatingWave()
    {
        if (alternatingFastPhase)
        {
            SpawnAlternatingFastWave();
        }
        else
        {
            SpawnAlternatingTechnicalWave();
        }

        alternatingWavesRemaining--;

        if (
            alternatingWavesRemaining <= 0
        )
        {
            alternatingFastPhase =
                !alternatingFastPhase;

            alternatingWavesRemaining =
                Mathf.Max(
                    1,
                    currentTier.alternatingWavesPerPhase
                );
        }
    }

    private void SpawnAlternatingFastWave()
    {
        float y =
            UnityEngine.Random.Range(
                minimumSpawnY + 0.35f,
                maximumSpawnY - 0.35f
            );

        SpawnCrow(
            spawnX,
            y,
            currentTier.fastPhaseSpeed
        );
    }

    private void SpawnAlternatingTechnicalWave()
    {
        TechnicalFormation formation =
            GetRandomMidSkyFormation();

        SpawnTechnicalFormation(
            formation,
            currentTier.crowSpeed,
            false
        );
    }

    private void SpawnDeepSpaceWave()
    {
        switch (
            deepSpacePatternIndex % 6
        )
        {
            case 0:
                SpawnDeepSpaceFast();
                break;

            case 1:
                SpawnDeepSpaceTechnical();
                break;

            case 2:
                SpawnDeepSpaceBurst();
                break;

            case 3:
                SpawnDeepSpaceFast();
                break;

            case 4:
                SpawnDeepSpaceHardTechnical();
                break;

            case 5:
                SpawnDeepSpaceRecovery();
                break;
        }

        deepSpacePatternIndex++;
    }

    private void SpawnDeepSpaceFast()
    {
        float y =
            UnityEngine.Random.Range(
                minimumSpawnY + 0.5f,
                maximumSpawnY - 0.5f
            );

        SpawnCrow(
            spawnX,
            y,
            4.2f
        );
    }

    private void SpawnDeepSpaceTechnical()
    {
        TechnicalFormation formation =
            UnityEngine.Random.value > 0.5f
                ? TechnicalFormation.HighLow
                : TechnicalFormation.LowHigh;

        SpawnTechnicalFormation(
            formation,
            3.4f,
            false
        );
    }

    private void SpawnDeepSpaceBurst()
    {
        float y =
            UnityEngine.Random.Range(
                minimumSpawnY + 0.6f,
                maximumSpawnY - 0.6f
            );

        EnemyCrow crow =
            SpawnCrow(
                spawnX,
                y,
                currentTier.crowSpeed
            );

        if (crow != null)
        {
            crow.ConfigureBurst(
                currentTier.burstSpeed,
                currentTier.burstDelay,
                currentTier.burstDuration
            );
        }
    }

    private void SpawnDeepSpaceHardTechnical()
    {
        TechnicalFormation formation =
            UnityEngine.Random.value > 0.5f
                ? TechnicalFormation.RisingSteps
                : TechnicalFormation.FallingSteps;

        SpawnTechnicalFormation(
            formation,
            3.3f,
            false
        );
    }

    private void SpawnDeepSpaceRecovery()
    {
        float safeY =
            UnityEngine.Random.Range(
                -1.5f,
                1.5f
            );

        SpawnCrow(
            spawnX,
            safeY,
            3.0f
        );
    }

    private DifficultyTier GetTierForStage(
        BackgroundStageManager.StageType stage
    )
    {
        if (
            difficultyTiers == null ||
            difficultyTiers.Count == 0
        )
        {
            return null;
        }

        for (
            int index = 0;
            index < difficultyTiers.Count;
            index++
        )
        {
            DifficultyTier tier =
                difficultyTiers[index];

            if (tier.stage == stage)
            {
                return tier;
            }
        }

        return null;
    }

    private void ValidateDifficultyTiers()
    {
        if (difficultyTiers == null)
        {
            difficultyTiers =
                new List<DifficultyTier>();
        }

        for (
            int index = 0;
            index < difficultyTiers.Count;
            index++
        )
        {
            DifficultyTier tier =
                difficultyTiers[index];

            if (tier == null)
            {
                continue;
            }

            tier.spawnInterval =
                Mathf.Max(
                    0.1f,
                    tier.spawnInterval
                );

            tier.crowSpeed =
                Mathf.Max(
                    0.1f,
                    tier.crowSpeed
                );

            tier.minimumCrowsPerWave =
                Mathf.Max(
                    1,
                    tier.minimumCrowsPerWave
                );

            tier.maximumCrowsPerWave =
                Mathf.Max(
                    tier.minimumCrowsPerWave,
                    tier.maximumCrowsPerWave
                );

            tier.horizontalSpacing =
                Mathf.Max(
                    0f,
                    tier.horizontalSpacing
                );

            tier.minimumVerticalSpacing =
                Mathf.Max(
                    0f,
                    tier.minimumVerticalSpacing
                );

            tier.burstChance =
                Mathf.Clamp01(
                    tier.burstChance
                );

            tier.burstSpeed =
                Mathf.Max(
                    0.1f,
                    tier.burstSpeed
                );

            tier.burstDelay =
                Mathf.Max(
                    0f,
                    tier.burstDelay
                );

            tier.burstDuration =
                Mathf.Max(
                    0f,
                    tier.burstDuration
                );

            tier.alternatingWavesPerPhase =
                Mathf.Max(
                    1,
                    tier.alternatingWavesPerPhase
                );

            tier.fastPhaseSpeed =
                Mathf.Max(
                    0.1f,
                    tier.fastPhaseSpeed
                );

            tier.fastPhaseSpawnInterval =
                Mathf.Max(
                    0.1f,
                    tier.fastPhaseSpawnInterval
                );

            tier.technicalPhaseSpawnInterval =
                Mathf.Max(
                    0.1f,
                    tier.technicalPhaseSpawnInterval
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