using System.Collections.Generic;
using UnityEngine;

public class PopUpGroup : Singleton<PopUpGroup>
{
    private Stack<GameObject> popUpStack = new Stack<GameObject>();

    public GameObject createLobbyPopUp;
    public GameObject createPlayerPopUp;

    // �˾� UI�� Ǫ���Ͽ� ���ÿ� �߰�
    public void PushPopUp(GameObject popUp)
    {
        // ���� �ֻ��� �˾��� ��ȣ�ۿ� ��Ȱ��ȭ
        if (popUpStack.Count > 0)
        {
            var topPopUp = popUpStack.Peek();
            SetPopUpInteractable(topPopUp, false);
        }

        // �� �˾� Ȱ��ȭ �� ���ÿ� �߰�
        popUp.SetActive(true);
        popUpStack.Push(popUp);
    }

    // �˾� UI�� ���ÿ��� ����
    public void PopPopUp()
    {
        if (popUpStack.Count > 0)
        {
            // ���� �˾� �ݱ�
            var topPopUp = popUpStack.Pop();
            topPopUp.SetActive(false);

            // ���� �˾� ��ȣ�ۿ� Ȱ��ȭ
            if (popUpStack.Count > 0)
            {
                var previousPopUp = popUpStack.Peek();
                SetPopUpInteractable(previousPopUp, true);
            }
        }
    }

    // ���� �˾� �ݱ�(�ٱ� Ŭ�� ����)
    public void CloseTopPopUp()
    {
        if (popUpStack.Count > 0)
        {
            PopPopUp();
        }
    }

    // �˾��� ��ȣ�ۿ� ����
    private void SetPopUpInteractable(GameObject popUp, bool isInteractable)
    {
        var canvasGroup = popUp.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = popUp.AddComponent<CanvasGroup>();
        }
        canvasGroup.interactable = isInteractable;
        canvasGroup.blocksRaycasts = isInteractable;
    }

    // �˾� ���� �� Ŭ�� ����
    private void Update()
    {
        if (popUpStack.Count > 0 && Input.GetMouseButtonDown(0)) // ���� Ŭ��
        {
            var topPopUp = popUpStack.Peek();
            if (!IsPointerInsidePopUp(topPopUp))
            {
                CloseTopPopUp();
            }
        }
    }

    // ���콺�� �˾� ���ο� �ִ��� Ȯ��
    private bool IsPointerInsidePopUp(GameObject popUp)
    {
        RectTransform rectTransform = popUp.GetComponent<RectTransform>();
        if (rectTransform == null) return false;

        Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        return rectTransform.rect.Contains(localMousePosition);
    }
}
