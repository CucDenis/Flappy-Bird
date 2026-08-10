using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public sealed class BirdController : MonoBehaviour
{
    private enum FlightState
    {
        Waiting,
        Rising,
        Hovering,
        Descending,
        Diving,
        Recovering,
        Stopped
    }

    [Header("Start Flight Transition")]
    [SerializeField] private float gameplayX = -2f;
    [SerializeField] private float startTransitionDuration = 1.2f;
    [SerializeField] private AnimationCurve startTransitionCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Input")]
    [SerializeField]
    private InputActionReference primaryContactAction;

    [SerializeField]
    private InputActionReference pointerPositionAction;

    [SerializeField]
    private InputActionReference keyboardFlapAction;

    [Header("Single Tap")]
    [SerializeField] private float flapVelocity = 5.2f;
    [SerializeField] private float singleHoverDuration = 0.2f;

    [Header("Double Tap")]
    [SerializeField] private float doubleTapWindow = 0.28f;
    [SerializeField] private float burstVelocity = 7.8f;
    [SerializeField] private float burstHoverDuration = 0.5f;

    [Header("Rise")]
    [SerializeField] private float riseDeceleration = 8f;
    [SerializeField] private float maximumRiseSpeed = 8.5f;

    [Header("Altitude")]
    [SerializeField] private float minimumY = -4.8f;
    [SerializeField] private float maximumY = 4.8f;

    [Header("High Altitude Descent")]
    [SerializeField] private float highDescentAcceleration = 1.1f;
    [SerializeField] private float highMaximumFallSpeed = 2.2f;
    [SerializeField] private float highHoverBonus = 0.15f;

    [Header("Middle Altitude Descent")]
    [SerializeField] private float middleDescentAcceleration = 2.2f;
    [SerializeField] private float middleMaximumFallSpeed = 4.2f;
    [SerializeField] private float middleHoverBonus = 0.05f;

    [Header("Low Altitude Descent")]
    [SerializeField] private float lowDescentAcceleration = 3.8f;
    [SerializeField] private float lowMaximumFallSpeed = 7f;

    [Header("Downward Swipe")]
    [SerializeField] private float minimumSwipeDistancePixels = 80f;
    [SerializeField] private float maximumSwipeDuration = 0.45f;
    [SerializeField] private float diveRowHeight = 1f;
    [SerializeField] private float diveSpeed = 9f;

    [Header("Animation Speeds")]
    [SerializeField] private float hoverAnimationSpeed = 1f;
    [SerializeField] private float flapAnimationSpeed = 1.2f;
    [SerializeField] private float burstAnimationSpeed = 1.5f;
    [SerializeField] private float descentAnimationSpeed = 0.8f;

    [Header("Rotation")]
    [SerializeField] private float risingAngle = 18f;
    [SerializeField] private float hoverAngle = 0f;
    [SerializeField] private float descendingAngle = -22f;
    [SerializeField] private float diveAngle = -60f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [Header("Dive Recovery")]
    [SerializeField] private float recoveryDuration = 0.15f;

    private bool startTransitionActive;
    private float startTransitionTimer;
    private float startTransitionInitialX;

    private static readonly int FlightSpeedId =
        Animator.StringToHash("FlightSpeed");

    private static readonly int IsDivingId =
        Animator.StringToHash("IsDiving");

    private static readonly int RecoverId =
        Animator.StringToHash("Recover");

    private readonly List<RaycastResult> uiResults = new();

    private Rigidbody2D body;
    private Animator animator;

    private FlightState currentState = FlightState.Waiting;

    private bool gameplayInputEnabled;
    private bool pointerGestureActive;

    private Vector2 pointerStartPosition;
    private float pointerStartTime;
    private float previousTapTime = float.NegativeInfinity;

    private float hoverTimer;
    private float diveTargetY;
    private float recoveryTimer;
    public bool IsStartTransitionActive =>
    startTransitionActive;
    
    public void BeginStartTransition()
    {
        startTransitionInitialX = transform.position.x;
        startTransitionTimer = 0f;
        startTransitionActive = true;
    }

    private void UpdateStartTransition()
    {
        startTransitionTimer += Time.fixedDeltaTime;

        float normalizedTime = Mathf.Clamp01(
            startTransitionTimer / startTransitionDuration
        );

        float curvedTime =
            startTransitionCurve.Evaluate(normalizedTime);

        Vector2 position = body.position;

        position.x = Mathf.Lerp(
            startTransitionInitialX,
            gameplayX,
            curvedTime
        );

        body.MovePosition(position);

        if (normalizedTime >= 1f)
        {
            Vector2 finalPosition = body.position;
            finalPosition.x = gameplayX;

            body.position = finalPosition;

            startTransitionActive = false;
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        body.gravityScale = 0f;
        body.simulated = false;

        animator.SetFloat(FlightSpeedId, hoverAnimationSpeed);
        animator.SetBool(IsDivingId, false);
    }

    private void OnEnable()
    {
        SubscribeToInput();
    }

    private void OnDisable()
    {
        UnsubscribeFromInput();
        DisableInputActions();
    }

    private void FixedUpdate()
    {
        if (!gameplayInputEnabled || !body.simulated)
        {
            return;
        }

        if (startTransitionActive)
        {
            UpdateStartTransition();
        }

        switch (currentState)
        {
            case FlightState.Rising:
                UpdateRising();
                break;

            case FlightState.Hovering:
                UpdateHovering();
                break;

            case FlightState.Descending:
                UpdateDescending();
                break;

            case FlightState.Diving:
                UpdateDiving();
                break;

            case FlightState.Recovering:
                UpdateRecovering();
                break;
        }

        ClampRiseSpeed();
        CheckVerticalLimits();
        UpdateRotation();
    }

    public void BeginGame()
    {
        gameplayInputEnabled = true;
        body.simulated = true;

        SetVelocity(Vector2.zero);

        EnableInputActions();

        EnterHover(0.15f);

        BeginStartTransition();
    }

    public void PauseInput()
    {
        DisableInputActions();
    }

    public void ResumeInput()
    {
        if (!gameplayInputEnabled)
        {
            return;
        }

        EnableInputActions();
    }

    public void StopBird()
    {
        gameplayInputEnabled = false;
        pointerGestureActive = false;

        DisableInputActions();

        SetVelocity(Vector2.zero);
        body.simulated = false;

        currentState = FlightState.Stopped;
    }

    private void SubscribeToInput()
    {
        if (primaryContactAction?.action != null)
        {
            primaryContactAction.action.started += OnPointerPressed;
            primaryContactAction.action.canceled += OnPointerReleased;
        }

        if (keyboardFlapAction?.action != null)
        {
            keyboardFlapAction.action.performed += OnKeyboardFlap;
        }
    }

    private void UnsubscribeFromInput()
    {
        if (primaryContactAction?.action != null)
        {
            primaryContactAction.action.started -= OnPointerPressed;
            primaryContactAction.action.canceled -= OnPointerReleased;
        }

        if (keyboardFlapAction?.action != null)
        {
            keyboardFlapAction.action.performed -= OnKeyboardFlap;
        }
    }

    private void EnableInputActions()
    {
        primaryContactAction?.action?.Enable();
        pointerPositionAction?.action?.Enable();
        keyboardFlapAction?.action?.Enable();
    }

    private void DisableInputActions()
    {
        primaryContactAction?.action?.Disable();
        pointerPositionAction?.action?.Disable();
        keyboardFlapAction?.action?.Disable();
    }

    private void OnPointerPressed(
        InputAction.CallbackContext context
    )
    {
        // Discard any stale gesture left by an interrupted click.
        pointerGestureActive = false;

        if (!CanAcceptInput())
        {
            return;
        }

        Vector2 pointerPosition = ReadPointerPosition();

        if (IsPointerOverUI(pointerPosition))
        {
            return;
        }

        pointerGestureActive = true;
        pointerStartPosition = pointerPosition;
        pointerStartTime = Time.unscaledTime;
    }

    private void OnPointerReleased(InputAction.CallbackContext context)
    {
        if (!pointerGestureActive)
        {
            return;
        }

        pointerGestureActive = false;

        if (!CanAcceptInput())
        {
            return;
        }

        Vector2 endPosition = ReadPointerPosition();
        Vector2 delta = endPosition - pointerStartPosition;

        float duration = Time.unscaledTime - pointerStartTime;

        bool isDownwardSwipe =
            duration <= maximumSwipeDuration &&
            delta.y <= -minimumSwipeDistancePixels &&
            Mathf.Abs(delta.y) > Mathf.Abs(delta.x);

        if (isDownwardSwipe)
        {
            BeginDive();
            return;
        }

        ProcessTap();
    }

    private void OnKeyboardFlap(InputAction.CallbackContext context)
    {
        if (CanAcceptInput())
        {
            ProcessTap();
        }
    }

    private void ProcessTap()
    {
        float now = Time.unscaledTime;
        float interval = now - previousTapTime;

        bool isDoubleTap =
            interval > 0f &&
            interval <= doubleTapWindow;

        previousTapTime = now;

        if (isDoubleTap)
        {
            previousTapTime = float.NegativeInfinity;
            BeginBurst();
            return;
        }

        BeginSingleFlap();
    }

    private void BeginSingleFlap()
    {
        currentState = FlightState.Rising;

        animator.SetBool(IsDivingId, false);
        animator.SetFloat(FlightSpeedId, flapAnimationSpeed);

        Vector2 velocity = GetVelocity();
        velocity.y = Mathf.Max(velocity.y, flapVelocity);
        SetVelocity(velocity);

        hoverTimer =
            singleHoverDuration +
            GetAltitudeHoverBonus();
    }

    private void BeginBurst()
    {
        currentState = FlightState.Rising;

        animator.SetBool(IsDivingId, false);
        animator.SetFloat(FlightSpeedId, burstAnimationSpeed);

        Vector2 velocity = GetVelocity();
        velocity.y = burstVelocity;
        SetVelocity(velocity);

        hoverTimer =
            burstHoverDuration +
            GetAltitudeHoverBonus();
    }

    private void UpdateRising()
    {
        Vector2 velocity = GetVelocity();

        velocity.y = Mathf.MoveTowards(
            velocity.y,
            0f,
            riseDeceleration * Time.fixedDeltaTime
        );

        SetVelocity(velocity);

        if (velocity.y <= 0.05f)
        {
            EnterHover(hoverTimer);
        }
    }

    private void EnterHover(float duration)
    {
        currentState = FlightState.Hovering;
        hoverTimer = Mathf.Max(0f, duration);

        animator.SetBool(IsDivingId, false);
        animator.SetFloat(FlightSpeedId, hoverAnimationSpeed);

        Vector2 velocity = GetVelocity();
        velocity.y = 0f;
        SetVelocity(velocity);
    }

    private void UpdateHovering()
    {
        hoverTimer -= Time.fixedDeltaTime;

        if (hoverTimer > 0f)
        {
            return;
        }

        currentState = FlightState.Descending;
        animator.SetFloat(
            FlightSpeedId,
            descentAnimationSpeed
        );
    }

    private void UpdateDescending()
    {
        GetDescentPhysics(
            out float acceleration,
            out float maximumFallSpeed
        );

        Vector2 velocity = GetVelocity();

        velocity.y -= acceleration * Time.fixedDeltaTime;
        velocity.y = Mathf.Max(
            velocity.y,
            -maximumFallSpeed
        );

        SetVelocity(velocity);
    }

    private void BeginDive()
    {
        if (startTransitionActive)
        {
            return;
        }
        
        diveTargetY = Mathf.Max(
            minimumY,
            transform.position.y - diveRowHeight
        );

        if (Mathf.Approximately(
            diveTargetY,
            transform.position.y
        ))
        {
            return;
        }

        pointerGestureActive = false;
        currentState = FlightState.Diving;

        // Clear an old recovery trigger before starting another dive.
        animator.ResetTrigger(RecoverId);
        animator.SetBool(IsDivingId, true);

        Vector2 velocity = GetVelocity();
        velocity.y = -diveSpeed;
        SetVelocity(velocity);
    }

    private void UpdateDiving()
    {
        if (transform.position.y > diveTargetY)
        {
            return;
        }

        Vector3 position = transform.position;
        position.y = diveTargetY;
        transform.position = position;

        SetVelocity(Vector2.zero);

        currentState = FlightState.Recovering;
        recoveryTimer = recoveryDuration;

        animator.SetBool(IsDivingId, false);
        animator.ResetTrigger(RecoverId);
        animator.SetTrigger(RecoverId);
    }

    private void UpdateRecovering()
    {
        SetVelocity(Vector2.zero);

        recoveryTimer -= Time.fixedDeltaTime;

        if (recoveryTimer > 0f)
        {
            return;
        }

        EnterHover(0.1f);
    }

    private void GetDescentPhysics(
        out float acceleration,
        out float maximumFallSpeed
    )
    {
        float altitude = GetNormalizedAltitude();

        if (altitude >= 0.66f)
        {
            acceleration = highDescentAcceleration;
            maximumFallSpeed = highMaximumFallSpeed;
            return;
        }

        if (altitude >= 0.33f)
        {
            acceleration = middleDescentAcceleration;
            maximumFallSpeed = middleMaximumFallSpeed;
            return;
        }

        acceleration = lowDescentAcceleration;
        maximumFallSpeed = lowMaximumFallSpeed;
    }

    private float GetAltitudeHoverBonus()
    {
        float altitude = GetNormalizedAltitude();

        if (altitude >= 0.66f)
        {
            return highHoverBonus;
        }

        if (altitude >= 0.33f)
        {
            return middleHoverBonus;
        }

        return 0f;
    }

    private float GetNormalizedAltitude()
    {
        return Mathf.InverseLerp(
            minimumY,
            maximumY,
            transform.position.y
        );
    }

    private void ClampRiseSpeed()
    {
        Vector2 velocity = GetVelocity();

        if (velocity.y <= maximumRiseSpeed)
        {
            return;
        }

        velocity.y = maximumRiseSpeed;
        SetVelocity(velocity);
    }

    private void CheckVerticalLimits()
    {
        float currentY = transform.position.y;

        if (currentY < minimumY || currentY > maximumY)
        {
            GameManager.Instance?.GameOver();
        }
    }

    private void UpdateRotation()
    {
        float targetAngle = currentState switch
        {
            FlightState.Rising => risingAngle,
            FlightState.Hovering => hoverAngle,
            FlightState.Descending => descendingAngle,
            FlightState.Diving => diveAngle,
            FlightState.Recovering => hoverAngle,
            _ => hoverAngle
        };

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(0f, 0f, targetAngle),
            rotationSpeed * Time.fixedDeltaTime
        );
    }

    private bool CanAcceptInput()
    {
        if (
            currentState == FlightState.Diving ||
            currentState == FlightState.Recovering
        )
        {
            return false;
        }

        return gameplayInputEnabled &&
            GameManager.Instance != null &&
            GameManager.Instance.IsPlaying;
    }

    private Vector2 ReadPointerPosition()
    {
        return pointerPositionAction?.action != null
            ? pointerPositionAction.action.ReadValue<Vector2>()
            : Vector2.zero;
    }

    private bool IsPointerOverUI(Vector2 pointerPosition)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData eventData =
            new PointerEventData(EventSystem.current)
            {
                position = pointerPosition
            };

        uiResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiResults);

        return uiResults.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameplayInputEnabled)
        {
            return;
        }

        if (
            other.CompareTag("Enemy") ||
            other.CompareTag("Boundary")
        )
        {
            GameManager.Instance?.GameOver();
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