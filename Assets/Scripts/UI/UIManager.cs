using UnityEngine;

public enum UIState
{
    Authentication,
    Lobby,
    MainMenu
}

public class UIManager : Singleton<UIManager>
{
    private UIState currentState;
    [SerializeField] private GameObject loadingUI;

    [SerializeField] private GameObject authenticateUI;
    [SerializeField] private GameObject mainMenu;

    public void SetState(UIState newState)
    {
        currentState = newState;

        // 상태에 따라 UI 관리
        switch (currentState)
        {
            case UIState.Authentication:
                authenticateUI.SetActive(true);
                mainMenu.SetActive(false);
                break;
            case UIState.Lobby:
                authenticateUI.SetActive(false);
                mainMenu.SetActive(true);
                break;
        }
    }
}




