using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class BirdController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference flapAction;

    [Header("Movement")]
    [SerializeField] private float flapVelocity = 7.5f;
    [SerializeField] private float maximumFallSpeed = 10f;

    [Header("Playable Area")]
    [SerializeField] private float minimumY = -5.5f;
    [SerializeField] private float maximumY = 5.5f;

    [Header("Rotation")]
    [SerializeField] private bool rotateBird = true;
    [SerializeField] private float upwardRotation = 25f;
    [SerializeField] private float downwardRotation = -60f;
    [SerializeField] private float rotationSpeed = 8f;

    private readonly List<RaycastResult> uiRaycastResults = new();

    private Rigidbody2D body;
    private bool inputEnabled;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        inputEnabled = false;
        body.simulated = false;
    }

    private void OnEnable()
    {
        if (flapAction == null || flapAction.action == null)
        {
            Debug.LogError(
                "BirdController requires a Flap InputActionReference.",
                this
            );

            return;
        }

        flapAction.action.performed += OnFlapPerformed;
    }

    private void OnDisable()
    {
        if (flapAction == null || flapAction.action == null)
        {
            return;
        }

        flapAction.action.performed -= OnFlapPerformed;
        flapAction.action.Disable();
    }

    private void Update()
    {
        if (!inputEnabled)
        {
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        CheckVerticalLimits();
        UpdateBirdRotation();
    }

    private void FixedUpdate()
    {
        if (!body.simulated)
        {
            return;
        }

        Vector2 velocity = GetVelocity();

        if (velocity.y >= -maximumFallSpeed)
        {
            return;
        }

        velocity.y = -maximumFallSpeed;
        SetVelocity(velocity);
    }

    public void BeginGame()
    {
        inputEnabled = true;
        body.simulated = true;

        SetVelocity(Vector2.zero);

        if (flapAction != null && flapAction.action != null)
        {
            flapAction.action.Enable();
        }
    }

    public void PauseInput()
    {
        if (flapAction != null && flapAction.action != null)
        {
            flapAction.action.Disable();
        }
    }

    public void ResumeInput()
    {
        if (!inputEnabled)
        {
            return;
        }

        if (flapAction != null && flapAction.action != null)
        {
            flapAction.action.Enable();
        }
    }

    public void StopBird()
    {
        inputEnabled = false;

        if (flapAction != null && flapAction.action != null)
        {
            flapAction.action.Disable();
        }

        SetVelocity(Vector2.zero);
        body.simulated = false;
    }

    private void OnFlapPerformed(
        InputAction.CallbackContext context
    )
    {
        if (!inputEnabled)
        {
            return;
        }

        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        // Do not flap when the player presses a UI button.
        if (IsPointerOverUI(context))
        {
            return;
        }

        Flap();
    }

    private void Flap()
    {
        Vector2 velocity = GetVelocity();

        // Replace downward velocity instead of accumulating force.
        velocity.y = flapVelocity;

        SetVelocity(velocity);
    }

    private bool IsPointerOverUI(
        InputAction.CallbackContext context
    )
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        // Keyboard and gamepad controls are not pointer controls.
        if (context.control?.device is not Pointer pointer)
        {
            return false;
        }

        PointerEventData pointerEventData =
            new PointerEventData(EventSystem.current)
            {
                position = pointer.position.ReadValue()
            };

        uiRaycastResults.Clear();

        EventSystem.current.RaycastAll(
            pointerEventData,
            uiRaycastResults
        );

        return uiRaycastResults.Count > 0;
    }

    private void CheckVerticalLimits()
    {
        float currentY = transform.position.y;

        if (currentY < minimumY || currentY > maximumY)
        {
            GameManager.Instance.GameOver();
        }
    }

    private void UpdateBirdRotation()
    {
        if (!rotateBird)
        {
            return;
        }

        float verticalVelocity = GetVelocity().y;

        float targetAngle = verticalVelocity >= 0f
            ? upwardRotation
            : downwardRotation;

        Quaternion targetRotation = Quaternion.Euler(
            0f,
            0f,
            targetAngle
        );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!inputEnabled)
        {
            return;
        }

        if (
            other.CompareTag("Enemy") ||
            other.CompareTag("Boundary")
        )
        {
            GameManager.Instance.GameOver();
        }
    }

    private Vector2 GetVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return body.linearVelocity;
#else
        return body.velocity;
#endif
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