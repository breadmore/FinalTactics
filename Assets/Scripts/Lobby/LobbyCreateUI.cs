using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyCreateUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_Dropdown maxPlayersDropdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        createLobbyButton.onClick.AddListener(OnCreateLobbyClicked);
    }

    private void OnCreateLobbyClicked()
    {
        string lobbyName = lobbyNameInput.text;
        int maxPlayers = int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

        Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
        {
            { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "DefaultGameMode") }
        };

        LobbyManager.Instance.CreateLobby(lobbyName, maxPlayers, lobbyData);
    }

}
