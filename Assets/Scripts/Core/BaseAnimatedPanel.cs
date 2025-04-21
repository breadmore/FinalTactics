using UnityEngine;
using DG.Tweening;

public abstract class BaseAnimatedPanel : MonoBehaviour
{
    public float slideDuration = 0.5f;
    public float fadeDuration = 0.3f;
    public Vector2 onScreenPosition; // ȭ�鿡�� ������ ��ġ
    public Vector2 offScreenPosition; // ȭ�� �� ��� ��ġ

    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;
    protected bool isVisible = false;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        offScreenPosition = new Vector2(-Screen.width, 0);

        rectTransform.anchoredPosition = offScreenPosition;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public virtual void Show()
    {
        isVisible = true;

        rectTransform.DOAnchorPos(onScreenPosition, slideDuration).SetEase(Ease.OutExpo).SetUpdate(true);
        canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public virtual void Hide()
    {
        isVisible = false;

        rectTransform.DOAnchorPos(offScreenPosition, slideDuration).SetEase(Ease.InExpo).SetUpdate(true);
        canvasGroup.DOFade(0f, fadeDuration).SetUpdate(true);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsVisible() => isVisible;
}
