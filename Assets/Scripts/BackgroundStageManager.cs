using System.Collections;
using UnityEngine;

public sealed class BackgroundStageManager
    : MonoBehaviour
{
    public enum StageType
    {
        Sea,
        Beach,
        Hills,
        Mountains,
        LowSky,
        MidSky,
        HighSky,
        DeepSpace
    }

    [Header("Music")]
    [SerializeField]
    private MusicManager musicManager;

    [Header("Terrestrial")]
    [SerializeField]
    private EnvironmentHorizontalScroller
        terrestrialScroller;

    [SerializeField]
    private TerrestrialVerticalExitMover
        terrestrialVerticalExitMover;

    [Header("Clouds")]
    [SerializeField]
    private BackgroundCloudSpawner
        cloudSpawner;

    [Header("Crow Difficulty")]
    [SerializeField]
    private CrowSpawner
        crowSpawner;

    [Header("Sky Backdrops")]
    [SerializeField]
    private SkyBackdropMover lowSkyMover;

    [SerializeField]
    private SkyBackdropMover midSkyMover;

    [SerializeField]
    private SkyBackdropMover highSkyMover;

    [SerializeField]
    private SkyBackdropMover deepSpaceMover;

    [Header("Sky Positions")]
    [SerializeField]
    private float hiddenSkyY = 10f;

    [SerializeField]
    private float activeSkyY = 0f;

    [SerializeField]
    private float exitedSkyY = -10f;

    [Header("Sky Level Durations")]
    [SerializeField]
    private float lowSkyDuration = 20f;

    [SerializeField]
    private float midSkyDuration = 20f;

    [SerializeField]
    private float highSkyDuration = 20f;

    [SerializeField]
    private HorizontalParallaxMover[]
        deepSpaceParallaxLayers;

    [SerializeField]
    private float deepSpaceDuration = 35f;

    private bool hasActiveStage;

    private bool progressionStarted;

    private StageType currentStage;

    private Coroutine skyProgressionCoroutine;

    private Coroutine deepSpaceCoroutine;

    public StageType CurrentStage =>
        currentStage;

    private void Awake()
    {
        SubscribeToTerrestrialEvents();
    }

    private void Start()
    {
        progressionStarted = false;

        InitializeSkyPositions();

        SetStage(
            StageType.Sea
        );

        cloudSpawner?
            .SetCloudsEnabled(false);
    }

    private void OnDestroy()
    {
        UnsubscribeFromTerrestrialEvents();
    }

    public void BeginProgression()
    {
        if (progressionStarted)
        {
            return;
        }

        progressionStarted = true;

        ConfigureCloudRange(
            currentStage
        );

        cloudSpawner?
            .BeginSpawning();

        terrestrialScroller?
            .StartScrolling();
    }

    public void StopProgression()
    {
        progressionStarted = false;

        terrestrialScroller?
            .StopScrolling();

        cloudSpawner?
            .StopSpawning(
                clearExistingClouds: true
            );

        if (
            skyProgressionCoroutine != null
        )
        {
            StopCoroutine(
                skyProgressionCoroutine
            );

            skyProgressionCoroutine = null;
        }

        if (
            deepSpaceCoroutine != null
        )
        {
            StopCoroutine(
                deepSpaceCoroutine
            );

            deepSpaceCoroutine = null;
        }

        StopDeepSpaceParallax();
    }

    private void SubscribeToTerrestrialEvents()
    {
        if (terrestrialScroller == null)
        {
            return;
        }

        terrestrialScroller.BeachReached +=
            HandleBeachReached;

        terrestrialScroller.HillsReached +=
            HandleHillsReached;

        terrestrialScroller.MountainsReached +=
            HandleMountainsReached;

        terrestrialScroller.MountainExitReached +=
            HandleMountainExitReached;
    }

    private void ConfigureCloudRange(
        StageType stage
    )
    {
        switch (stage)
        {
            case StageType.Sea:
                cloudSpawner?
                    .SetVerticalRange(
                        0.5f,
                        4.2f
                    );
                break;

            case StageType.Beach:
                cloudSpawner?
                    .SetVerticalRange(
                        0.8f,
                        4.2f
                    );
                break;

            case StageType.Hills:
                cloudSpawner?
                    .SetVerticalRange(
                        1.4f,
                        4.2f
                    );
                break;

            case StageType.Mountains:
                cloudSpawner?
                    .SetVerticalRange(
                        2.2f,
                        4.2f
                    );
                break;

            case StageType.LowSky:
            case StageType.MidSky:
                cloudSpawner?
                    .SetVerticalRange(
                        -3.5f,
                        4.2f
                    );
                break;

            case StageType.HighSky:
                cloudSpawner?
                    .SetVerticalRange(
                        -1.5f,
                        4.2f
                    );
                break;
        }
    }

    private void UnsubscribeFromTerrestrialEvents()
    {
        if (terrestrialScroller == null)
        {
            return;
        }

        terrestrialScroller.BeachReached -=
            HandleBeachReached;

        terrestrialScroller.HillsReached -=
            HandleHillsReached;

        terrestrialScroller.MountainsReached -=
            HandleMountainsReached;

        terrestrialScroller.MountainExitReached -=
            HandleMountainExitReached;
    }

    private void HandleBeachReached()
    {
        SetStage(
            StageType.Beach
        );
    }

    private void HandleHillsReached()
    {
        SetStage(
            StageType.Hills
        );
    }

    private void HandleMountainsReached()
    {
        SetStage(
            StageType.Mountains
        );
    }

    private void HandleMountainExitReached()
    {
        terrestrialVerticalExitMover?
            .BeginExit();

        EnterLowSky();

        if (
            skyProgressionCoroutine != null
        )
        {
            StopCoroutine(
                skyProgressionCoroutine
            );
        }

        skyProgressionCoroutine =
            StartCoroutine(
                RunSkyProgression()
            );
    }

    private IEnumerator RunSkyProgression()
    {
        yield return
            new WaitForSeconds(
                lowSkyDuration
            );

        EnterMidSky();

        yield return
            new WaitForSeconds(
                midSkyDuration
            );

        EnterHighSky();

        yield return
            new WaitForSeconds(
                highSkyDuration
            );

        EnterDeepSpace();

        skyProgressionCoroutine = null;
    }

    private void EnterLowSky()
    {
        SetStage(
            StageType.LowSky
        );

        lowSkyMover?
            .MoveToY(
                activeSkyY
            );
    }

    private void EnterMidSky()
    {
        SetStage(
            StageType.MidSky
        );

        lowSkyMover?
            .MoveToY(
                exitedSkyY
            );

        midSkyMover?
            .MoveToY(
                activeSkyY
            );
    }

    private void EnterHighSky()
    {
        SetStage(
            StageType.HighSky
        );

        midSkyMover?
            .MoveToY(
                exitedSkyY
            );

        highSkyMover?
            .MoveToY(
                activeSkyY
            );
    }

    private void EnterDeepSpace()
    {
        SetStage(
            StageType.DeepSpace
        );

        highSkyMover?
            .MoveToY(
                exitedSkyY
            );

        deepSpaceMover?
            .MoveToY(
                activeSkyY
            );

        StartDeepSpaceParallax();
        StartDeepSpaceChallenge();
    }

    private void StartDeepSpaceParallax()
    {
        if (deepSpaceParallaxLayers == null)
        {
            return;
        }

        foreach (
            HorizontalParallaxMover layer
            in deepSpaceParallaxLayers
        )
        {
            if (layer != null)
            {
                layer.StartMoving();
            }
        }
    }

    private void StopDeepSpaceParallax()
    {
        if (deepSpaceParallaxLayers == null)
        {
            return;
        }

        foreach (
            HorizontalParallaxMover layer
            in deepSpaceParallaxLayers
        )
        {
            if (layer != null)
            {
                layer.StopMoving();
            }
        }
    }

    private void StartDeepSpaceChallenge()
    {
        if (deepSpaceCoroutine != null)
        {
            StopCoroutine(
                deepSpaceCoroutine
            );
        }

        deepSpaceCoroutine =
            StartCoroutine(
                RunDeepSpaceChallenge()
            );
    }

    private IEnumerator RunDeepSpaceChallenge()
    {
        yield return
            new WaitForSeconds(
                deepSpaceDuration
            );

        deepSpaceCoroutine = null;

        if (
            GameManager.Instance != null &&
            GameManager.Instance.IsPlaying
        )
        {
            GameManager.Instance
                .CompleteGame();
        }
    }

    private void SetStage(
       StageType stage
    )
    {
        if (
           hasActiveStage &&
           currentStage == stage
        )
        {
           return;
        }

        currentStage = stage;
        hasActiveStage = true;

        ConfigureClouds(stage);

        crowSpawner?
        .SetStage(stage);

        musicManager?
        .OnStageChanged(stage);

        Debug.Log(
           $"ENVIRONMENT STAGE -> {stage}"
        );
    }

    private void ConfigureClouds(
        StageType stage
    )
    {
        ConfigureCloudRange(stage);

        if (
            stage ==
            StageType.DeepSpace
        )
        {
            cloudSpawner?
                .SetCloudsEnabled(false);

            cloudSpawner?
                .ClearAllClouds();

            return;
        }

        if (!progressionStarted)
        {
            cloudSpawner?
                .SetCloudsEnabled(false);

            return;
        }

        cloudSpawner?
            .SetCloudsEnabled(true);
    }

    private void InitializeSkyPositions()
    {
        SetLocalY(
            lowSkyMover,
            hiddenSkyY
        );

        SetLocalY(
            midSkyMover,
            hiddenSkyY
        );

        SetLocalY(
            highSkyMover,
            hiddenSkyY
        );

        SetLocalY(
            deepSpaceMover,
            hiddenSkyY
        );
    }

    private static void SetLocalY(
        SkyBackdropMover mover,
        float y
    )
    {
        if (mover == null)
        {
            return;
        }

        Vector3 position =
            mover.transform.localPosition;

        position.y = y;

        mover.transform.localPosition =
            position;
    }
}