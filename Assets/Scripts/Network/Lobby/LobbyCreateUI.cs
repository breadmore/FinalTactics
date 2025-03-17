using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button createLobbyConfirmButton;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_Dropdown maxPlayersDropdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        createLobbyButton.onClick.AddListener(OpenPopUp);
        createLobbyConfirmButton.onClick.AddListener(OnCreateLobbyClicked);
    }

    private async void OnCreateLobbyClicked()
    {
        string lobbyName = lobbyNameInput.text;
        int maxPlayers = int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "DefaultGameMode") }
        };

        // 끝날때까지 대기
        await LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers, lobbyData);

        // 플레이어 리스트 UI 출력
        UIManager.Instance.SetState(UIState.Lobby, UIState.JoinedLobby);
        LobbyManager.Instance.playerListUI.CreatePlayer(LobbyManager.Instance.GetHostLobby().Players[0]);
        PopUpGroup.Instance.CloseTopPopUp();
    }

    private void OpenPopUp()
    {
        PopUpGroup.Instance.PushPopUp(PopUpGroup.Instance.createLobbyPopUp);
    }

}
