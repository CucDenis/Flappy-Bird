using System;
using UnityEngine;

public sealed class EnvironmentHorizontalScroller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 0.1f;

    [Header("Trigger")]
    [SerializeField]
    private Transform stageTriggerLine;

    [Header("Stage Markers")]
    [SerializeField]
    private Transform beachStageMarker;

    [SerializeField]
    private Transform hillsStageMarker;

    [SerializeField]
    private Transform mountainsStageMarker;

    [SerializeField]
    private Transform mountainExitMarker;

    private bool scrolling;

    private bool beachTriggered;
    private bool hillsTriggered;
    private bool mountainsTriggered;
    private bool mountainExitTriggered;

    public bool IsScrolling => scrolling;

    public event Action BeachReached;
    public event Action HillsReached;
    public event Action MountainsReached;
    public event Action MountainExitReached;

    public void StartScrolling()
    {
        scrolling = true;
    }

    public void StopScrolling()
    {
        scrolling = false;
    }

    public void ResetProgression()
    {
        scrolling = false;

        beachTriggered = false;
        hillsTriggered = false;
        mountainsTriggered = false;
        mountainExitTriggered = false;
    }

    private void Update()
    {
        if (!scrolling)
        {
            return;
        }

        transform.position +=
            Vector3.left *
            speed *
            Time.deltaTime;

        CheckMarkers();
    }

    private void CheckMarkers()
    {
        if (stageTriggerLine == null)
        {
            return;
        }

        float triggerX =
            stageTriggerLine.position.x;

        if (
            !beachTriggered &&
            HasReached(
                beachStageMarker,
                triggerX
            )
        )
        {
            beachTriggered = true;
            BeachReached?.Invoke();
        }

        if (
            !hillsTriggered &&
            HasReached(
                hillsStageMarker,
                triggerX
            )
        )
        {
            hillsTriggered = true;
            HillsReached?.Invoke();
        }

        if (
            !mountainsTriggered &&
            HasReached(
                mountainsStageMarker,
                triggerX
            )
        )
        {
            mountainsTriggered = true;
            MountainsReached?.Invoke();
        }

        if (
            !mountainExitTriggered &&
            HasReached(
                mountainExitMarker,
                triggerX
            )
        )
        {
            mountainExitTriggered = true;

            StopScrolling();

            MountainExitReached?.Invoke();
        }
    }

    private static bool HasReached(
        Transform marker,
        float triggerX
    )
    {
        return
            marker != null &&
            marker.position.x <= triggerX;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (stageTriggerLine == null)
        {
            return;
        }

        float x =
            stageTriggerLine.position.x;

        Vector3 bottom =
            new Vector3(
                x,
                -20f,
                0f
            );

        Vector3 top =
            new Vector3(
                x,
                20f,
                0f
            );

        Gizmos.DrawLine(
            bottom,
            top
        );
    }
#endif
}