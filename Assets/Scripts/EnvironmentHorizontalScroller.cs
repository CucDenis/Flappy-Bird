using UnityEngine;

public sealed class EnvironmentHorizontalScroller : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 2f;

    [Header("Entry")]
    [SerializeField]
    private float entryX = 3.5f;

    [Header("Lifecycle")]
    [SerializeField]
    private bool disableWhenOffscreen = true;

    private bool scrolling;

    private Camera gameplayCamera;
    private SpriteRenderer[] spriteRenderers;

    private void Awake()
    {
        gameplayCamera = Camera.main;

        spriteRenderers =
            GetComponentsInChildren<SpriteRenderer>(
                true
            );
    }

    public void StartScrolling()
    {
        gameObject.SetActive(true);
        scrolling = true;
    }

    public void EnterFromRight()
    {
        gameObject.SetActive(true);

        Vector3 position =
            transform.localPosition;

        position.x = entryX;

        transform.localPosition =
            position;

        scrolling = true;
    }

    public void StopScrolling()
    {
        scrolling = false;
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

        if (IsFullyOutsideLeft())
        {
            FinishScrolling();
        }
    }

    private bool IsFullyOutsideLeft()
    {
        if (
            gameplayCamera == null ||
            spriteRenderers == null ||
            spriteRenderers.Length == 0
        )
        {
            return false;
        }

        float cameraLeft =
            gameplayCamera.transform.position.x -
            gameplayCamera.orthographicSize *
            gameplayCamera.aspect;

        float rightMostEdge =
            float.NegativeInfinity;

        foreach (
            SpriteRenderer spriteRenderer
            in spriteRenderers
        )
        {
            if (
                spriteRenderer == null ||
                !spriteRenderer.enabled
            )
            {
                continue;
            }

            rightMostEdge =
                Mathf.Max(
                    rightMostEdge,
                    spriteRenderer.bounds.max.x
                );
        }

        return rightMostEdge <
               cameraLeft;
    }

    private void FinishScrolling()
    {
        scrolling = false;

        if (disableWhenOffscreen)
        {
            gameObject.SetActive(false);
        }
    }
}