using System.Collections.Generic;
using UnityEngine;

public class PopUpGroup : Singleton<PopUpGroup>
{
    private Stack<GameObject> popUpStack = new Stack<GameObject>();

    public GameObject createLobbyPopUp;
    public GameObject createPlayerPopUp;

    // 팝업 UI를 푸시하여 스택에 추가
    public void PushPopUp(GameObject popUp)
    {
        // 기존 최상위 팝업의 상호작용 비활성화
        if (popUpStack.Count > 0)
        {
            var topPopUp = popUpStack.Peek();
            SetPopUpInteractable(topPopUp, false);
        }

        // 새 팝업 활성화 및 스택에 추가
        popUp.SetActive(true);
        popUpStack.Push(popUp);
    }

    // 팝업 UI를 스택에서 제거
    public void PopPopUp()
    {
        if (popUpStack.Count > 0)
        {
            // 현재 팝업 닫기
            var topPopUp = popUpStack.Pop();
            topPopUp.SetActive(false);

            // 이전 팝업 상호작용 활성화
            if (popUpStack.Count > 0)
            {
                var previousPopUp = popUpStack.Peek();
                SetPopUpInteractable(previousPopUp, true);
            }
        }
    }

    // 현재 팝업 닫기(바깥 클릭 감지)
    public void CloseTopPopUp()
    {
        if (popUpStack.Count > 0)
        {
            PopPopUp();
        }
    }

    // 팝업의 상호작용 설정
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

    // 팝업 영역 밖 클릭 감지
    private void Update()
    {
        if (popUpStack.Count > 0 && Input.GetMouseButtonDown(0)) // 왼쪽 클릭
        {
            var topPopUp = popUpStack.Peek();
            if (!IsPointerInsidePopUp(topPopUp))
            {
                CloseTopPopUp();
            }
        }
    }

    // 마우스가 팝업 내부에 있는지 확인
    private bool IsPointerInsidePopUp(GameObject popUp)
    {
        RectTransform rectTransform = popUp.GetComponent<RectTransform>();
        if (rectTransform == null) return false;

        Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        return rectTransform.rect.Contains(localMousePosition);
    }
}
