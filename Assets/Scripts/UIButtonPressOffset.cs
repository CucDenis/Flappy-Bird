using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class UIButtonPressOffset :
    MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private Vector2 pressedOffset = new(0f, -2f);

    private Vector2 restingPosition;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (content == null)
        {
            Debug.LogError(
                $"{name}: Content RectTransform is not assigned.",
                this
            );

            return;
        }

        restingPosition = content.anchoredPosition;
    }

    private void OnEnable()
    {
        ResetContent();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (
            content == null ||
            button == null ||
            !button.interactable
        )
        {
            return;
        }

        content.anchoredPosition =
            restingPosition + pressedOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetContent();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetContent();
    }

    private void ResetContent()
    {
        if (content == null)
        {
            return;
        }

        content.anchoredPosition = restingPosition;
    }
}