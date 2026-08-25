using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyCrow : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField]
    private float destructionX = -8f;

    private Rigidbody2D body;

    private Transform player;

    private float movementSpeed;

    private float baseMovementSpeed;

    private bool initialized;

    private bool scoreAwarded;

    private Coroutine burstCoroutine;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        float speed,
        Transform playerTransform
    )
    {
        baseMovementSpeed = speed;
        movementSpeed = speed;

        player = playerTransform;

        initialized = true;
        scoreAwarded = false;

        SetVelocity(
            Vector2.left *
            movementSpeed
        );
    }

    public void ConfigureBurst(
        float burstSpeed,
        float delay,
        float duration
    )
    {
        if (!initialized)
        {
            return;
        }

        if (
            burstSpeed <=
            baseMovementSpeed
        )
        {
            return;
        }

        if (burstCoroutine != null)
        {
            StopCoroutine(
                burstCoroutine
            );
        }

        burstCoroutine =
            StartCoroutine(
                RunBurst(
                    burstSpeed,
                    delay,
                    duration
                )
            );
    }

    private IEnumerator RunBurst(
        float burstSpeed,
        float delay,
        float duration
    )
    {
        if (delay > 0f)
        {
            yield return
                new WaitForSeconds(
                    delay
                );
        }

        if (!initialized)
        {
            burstCoroutine = null;
            yield break;
        }

        movementSpeed =
            burstSpeed;

        SetVelocity(
            Vector2.left *
            movementSpeed
        );

        if (duration > 0f)
        {
            yield return
                new WaitForSeconds(
                    duration
                );
        }

        if (!initialized)
        {
            burstCoroutine = null;
            yield break;
        }

        movementSpeed =
            baseMovementSpeed;

        SetVelocity(
            Vector2.left *
            movementSpeed
        );

        burstCoroutine = null;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        CheckIfPlayerPassed();

        if (
            transform.position.x <=
            destructionX
        )
        {
            Destroy(
                gameObject
            );
        }
    }

    private void CheckIfPlayerPassed()
    {
        if (
            scoreAwarded ||
            player == null
        )
        {
            return;
        }

        if (
            GameManager.Instance == null ||
            !GameManager.Instance.IsPlaying
        )
        {
            return;
        }

        if (
            transform.position.x <
            player.position.x
        )
        {
            scoreAwarded = true;

            GameManager.Instance
                .AddScore(1);
        }
    }

    private void OnDisable()
    {
        initialized = false;

        if (burstCoroutine != null)
        {
            StopCoroutine(
                burstCoroutine
            );

            burstCoroutine = null;
        }

        SetVelocity(
            Vector2.zero
        );
    }

    private void SetVelocity(
        Vector2 velocity
    )
    {
        if (body == null)
        {
            return;
        }

#if UNITY_6000_0_OR_NEWER
        body.linearVelocity =
            velocity;
#else
        body.velocity =
            velocity;
#endif
    }
}