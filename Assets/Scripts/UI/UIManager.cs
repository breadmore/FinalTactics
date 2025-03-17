using System.Collections.Generic;
using UnityEngine;

public enum UIState
{
    Authentication,
    Lobby,
    JoinedLobby,
    MainMenu
}

public class UIManager : Singleton<UIManager>
{
    private UIState currentState;
    [SerializeField] private GameObject loadingUI;

    [SerializeField] private GameObject authenticateUI;
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject joinedLobbyUI;
    [SerializeField] private GameObject mainMenu;

    private Dictionary<UIState, GameObject> uiStateMap;

    private void Start()
    {
        // UIState와 GameObject 매핑
        uiStateMap = new Dictionary<UIState, GameObject>
        {
            { UIState.MainMenu, mainMenu },
            { UIState.Authentication, authenticateUI },
            { UIState.Lobby, lobbyUI },
            { UIState.JoinedLobby, joinedLobbyUI },
        };

    }

    public void SetState(UIState nowState, UIState newState)
    {
        // 현재 UI 상태 모두 비활성화
        //foreach (var ui in uiStateMap.Values)
        //{
        //    ui.SetActive(false);
        //}

        // 새 상태 활성화
        if (uiStateMap.TryGetValue(nowState, out GameObject nowUI))
        {
            nowUI.SetActive(false);
        }
        if (uiStateMap.TryGetValue(newState, out GameObject newUI))
        {
            newUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"UIState {newState} does not exist in UIManager.");
        }

        currentState = newState;
    }

}
