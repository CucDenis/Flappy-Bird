using UnityEngine;

public sealed class TerrestrialVerticalExitMover
    : MonoBehaviour
{
    [Header("Vertical Exit")]
    [SerializeField]
    private float exitDistance = 10f;

    [SerializeField]
    private float exitDuration = 4f;

    private bool moving;

    private float elapsed;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public bool IsMoving => moving;

    public void BeginExit()
    {
        if (moving)
        {
            return;
        }

        startPosition =
            transform.localPosition;

        targetPosition =
            startPosition +
            Vector3.down *
            exitDistance;

        elapsed = 0f;
        moving = true;
    }

    private void Update()
    {
        if (!moving)
        {
            return;
        }

        elapsed += Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                elapsed /
                exitDuration
            );

        float smoothProgress =
            Mathf.SmoothStep(
                0f,
                1f,
                progress
            );

        transform.localPosition =
            Vector3.Lerp(
                startPosition,
                targetPosition,
                smoothProgress
            );

        if (progress >= 1f)
        {
            transform.localPosition =
                targetPosition;

            moving = false;
        }
    }
}