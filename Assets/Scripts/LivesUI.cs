using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class LivesUI : MonoBehaviour
{
    [SerializeField] private BirdController bird;
    [SerializeField] private Sprite heartSprite;
    [SerializeField] private Transform heartsContainer;
    [SerializeField] private Vector2 heartSize = new(100f, 100f);

    private const int MaxLives = 3;

    private readonly List<Image> hearts = new();

    private void Awake()
    {
        CreateHearts();

        if (bird != null)
        {
            OnLivesChanged(bird.CurrentLives);
        }
    }

    private void OnEnable()
    {
        if (bird != null)
        {
            bird.LivesChanged += OnLivesChanged;
            OnLivesChanged(bird.CurrentLives);
        }
    }

    private void OnDisable()
    {
        if (bird != null)
        {
            bird.LivesChanged -= OnLivesChanged;
        }
    }

    private void CreateHearts()
    {
        for (int i = 0; i < MaxLives; i++)
        {
            Image heart = CreateHeart();
            hearts.Add(heart);
        }
    }

    private Image CreateHeart()
    {
        GameObject heartObject = new GameObject(
            "Heart",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(LayoutElement)
        );

        heartObject.transform.SetParent(
            heartsContainer,
            false
        );

        Image image = heartObject.GetComponent<Image>();

        image.sprite = heartSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        LayoutElement layoutElement =
            heartObject.GetComponent<LayoutElement>();

        layoutElement.minWidth = heartSize.x;
        layoutElement.minHeight = heartSize.y;
        layoutElement.preferredWidth = heartSize.x;
        layoutElement.preferredHeight = heartSize.y;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;

        return image;
    }

    private void OnLivesChanged(int lives)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].gameObject.SetActive(i < lives);
        }
    }
}