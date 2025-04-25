using Ricimi;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomCleanButton : Button
{
    private CleanButtonConfig config;
    private CanvasGroup canvasGroup;

    protected override void Awake()
    {
        config = GetComponent<CleanButtonConfig>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        StopAllCoroutines();
        StartCoroutine(Utils.FadeOut(canvasGroup, config.onHoverAlpha, config.fadeTime));
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        StopAllCoroutines();
        StartCoroutine(Utils.FadeIn(canvasGroup, 1.0f, config.fadeTime));
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        canvasGroup.alpha = config.onClickAlpha;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        canvasGroup.alpha = 1.0f;
    }
}
