using UnityEngine;
using UnityEngine.UI;

public class LobbyLeaveUI : MonoBehaviour
{
    [SerializeField] private Button leaveLobbyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
    }

    private async void OnLeaveLobbyClicked()
    {
        await LobbyManager.Instance.LeaveLobby();
        LobbyManager.Instance.playerListUI.DestroyAllPlayerList();
        UIManager.Instance.SetState(UIState.JoinedLobby, UIState.Lobby);
    }
}