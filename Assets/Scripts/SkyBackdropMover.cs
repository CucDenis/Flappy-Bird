using UnityEngine;

public sealed class SkyBackdropMover : MonoBehaviour
{
    [SerializeField]
    private float transitionDuration = 4f;

    private bool transitioning;
    private float timer;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    public void MoveToY(float targetY)
    {
        startPosition = transform.localPosition;

        targetPosition = new Vector3(
            startPosition.x,
            targetY,
            startPosition.z
        );

        timer = 0f;
        transitioning = true;
    }

    private void Update()
    {
        if (!transitioning)
        {
            return;
        }

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(
            timer / transitionDuration
        );

        t = Mathf.SmoothStep(
            0f,
            1f,
            t
        );

        transform.localPosition =
            Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

        if (t >= 1f)
        {
            transform.localPosition =
                targetPosition;

            transitioning = false;
        }
    }
}