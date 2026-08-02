using UnityEngine;

public sealed class BackgroundCloudMover : MonoBehaviour
{
    private float movementSpeed;
    private float destructionX;
    private bool initialized;

    public void Initialize(
        float speed,
        float destroyAtX
    )
    {
        movementSpeed = speed;
        destructionX = destroyAtX;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        transform.Translate(
            Vector3.left *
            movementSpeed *
            Time.deltaTime,
            Space.World
        );

        if (transform.position.x <= destructionX)
        {
            Destroy(gameObject);
        }
    }
}