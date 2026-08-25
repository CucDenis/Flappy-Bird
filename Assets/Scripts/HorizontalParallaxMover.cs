using UnityEngine;

public sealed class HorizontalParallaxMover
    : MonoBehaviour
{
    [SerializeField]
    private float speed = 0.1f;

    private bool moving;

    public void StartMoving()
    {
        moving = true;
    }

    public void StopMoving()
    {
        moving = false;
    }

    private void Update()
    {
        if (!moving)
        {
            return;
        }

        transform.position +=
            Vector3.left *
            speed *
            Time.deltaTime;
    }
}