using System;
using UnityEngine;

public sealed class BackgroundStageManager : MonoBehaviour
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

    [Serializable]
    public sealed class Stage
    {
        public StageType stageType;

        [Min(0)]
        public int minimumScore;
    }

    [Header("Clouds")]
    [SerializeField]
    private BackgroundCloudSpawner cloudSpawner;

    [Header("Terrestrial")]
    [SerializeField]
    private EnvironmentHorizontalScroller terrestrialStripScroller;

    [Header("Sky")]
    [SerializeField]
    private SkyBackdropMover lowSkyMover;

    [SerializeField]
    private SkyBackdropMover midSkyMover;

    [SerializeField]
    private SkyBackdropMover highSkyMover;

    [SerializeField]
    private SkyBackdropMover deepSpaceMover;

    [Header("Stages")]
    [SerializeField]
    private Stage[] stages;

    [Header("Sky Positions")]
    [SerializeField]
    private float hiddenSkyY = 10f;

    [SerializeField]
    private float activeSkyY = 0f;

    [SerializeField]
    private float exitedSkyY = -10f;

    private int activeStageIndex = -1;

    private void Start()
    {
        InitializeSkyPositions();

        HandleScoreChanged(0);
    }

    public void HandleScoreChanged(int score)
    {
        if (
            stages == null ||
            stages.Length == 0
        )
        {
            return;
        }

        int targetIndex =
            FindStageIndex(score);

        if (targetIndex == activeStageIndex)
        {
            return;
        }

        activeStageIndex = targetIndex;

        ApplyStage(
            stages[targetIndex].stageType
        );
    }

    private int FindStageIndex(int score)
    {
        int result = 0;

        for (
            int index = 0;
            index < stages.Length;
            index++
        )
        {
            if (
                score >=
                stages[index].minimumScore
            )
            {
                result = index;
            }
        }

        return result;
    }

    private void InitializeSkyPositions()
    {
        SetLocalY(
            lowSkyMover,
            hiddenSkyY
        );

        SetLocalY(
            midSkyMover,
            hiddenSkyY * 2f
        );

        SetLocalY(
            highSkyMover,
            hiddenSkyY * 3f
        );

        SetLocalY(
            deepSpaceMover,
            hiddenSkyY * 4f
        );
    }

    private void ApplyStage(StageType stageType)
    {
        switch (stageType)
        {
            case StageType.Sea:
                terrestrialStripScroller?.StartScrolling();

                cloudSpawner?.SetCloudsEnabled(true);
                cloudSpawner?.SetVerticalRange(
                    0.5f,
                    4.2f
                );
                break;

            case StageType.Beach:
                cloudSpawner?.SetCloudsEnabled(true);
                cloudSpawner?.SetVerticalRange(
                    0.8f,
                    4.2f
                );
                break;

            case StageType.Hills:
                cloudSpawner?.SetVerticalRange(
                    1.4f,
                    4.2f
                );
                break;

            case StageType.Mountains:
                cloudSpawner?.SetVerticalRange(
                    2.2f,
                    4.2f
                );
                break;

            case StageType.LowSky:
                cloudSpawner?.SetCloudsEnabled(true);
                cloudSpawner?.SetVerticalRange(
                    -3.5f,
                    4.2f
                );
                break;

            case StageType.MidSky:
                EnterMidSky();
                cloudSpawner?.SetCloudsEnabled(true);
                cloudSpawner?.SetVerticalRange(
                    -3.5f,
                    4.2f
                );
                break;

            case StageType.HighSky:
                EnterHighSky();
                cloudSpawner?.SetCloudsEnabled(true);
                cloudSpawner?.SetVerticalRange(
                    -1.5f,
                    4.2f
                );
                break;

            case StageType.DeepSpace:
                EnterDeepSpace();
                cloudSpawner?.SetCloudsEnabled(false);
                cloudSpawner?.SetVerticalRange(
                    -1.5f,
                    4.2f
                );
                break;
        }
    }

    private void EnterMidSky()
    {
        lowSkyMover?.MoveToY(
            exitedSkyY
        );

        midSkyMover?.MoveToY(
            activeSkyY
        );
    }

    private void EnterHighSky()
    {
        midSkyMover?.MoveToY(
            exitedSkyY
        );

        highSkyMover?.MoveToY(
            activeSkyY
        );
    }

    private void EnterDeepSpace()
    {
        highSkyMover?.MoveToY(
            exitedSkyY
        );

        deepSpaceMover?.MoveToY(
            activeSkyY
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

        Transform target =
            mover.transform;

        Vector3 position =
            target.localPosition;

        position.y = y;

        target.localPosition =
            position;
    }
}