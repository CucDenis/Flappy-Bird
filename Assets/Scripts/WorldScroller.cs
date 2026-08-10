using UnityEngine;

public sealed class WorldScroller : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2f;

    [SerializeField]
    private float destroyX = -7f;

    [SerializeField]
    private bool destroyWhenOffscreen = true;

    private void Update()
    {
        if (
            GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying
        )
        {
            return;
        }

        transform.Translate(
            Vector3.left *
            moveSpeed *
            Time.deltaTime,
            Space.World
        );

        if (
            destroyWhenOffscreen &&
            transform.position.x <= destroyX
        )
        {
            Destroy(gameObject);
        }
    }
}