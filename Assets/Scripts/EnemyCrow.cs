using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyCrow : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float destructionX = -8f;

    private Rigidbody2D body;
    private Transform player;
    private float movementSpeed;
    private bool initialized;
    private bool scoreAwarded;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(float speed, Transform playerTransform)
    {
        movementSpeed = speed;
        player = playerTransform;
        initialized = true;
        scoreAwarded = false;

        SetVelocity(Vector2.left * movementSpeed);
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        CheckIfPlayerPassed();

        if (transform.position.x <= destructionX)
        {
            Destroy(gameObject);
        }
    }

    private void CheckIfPlayerPassed()
    {
        if (scoreAwarded || player == null)
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

        // The crow has moved behind the player.
        if (transform.position.x < player.position.x)
        {
            scoreAwarded = true;
            GameManager.Instance.AddScore(1);
        }
    }

    private void OnDisable()
    {
        SetVelocity(Vector2.zero);
    }

    private void SetVelocity(Vector2 velocity)
    {
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = velocity;
#else
        body.velocity = velocity;
#endif
    }
}