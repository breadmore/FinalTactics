using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyRefreshUI : MonoBehaviour
{
    [SerializeField] private Button refreshLobbyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refreshLobbyButton.onClick.AddListener(OnRefreshLobbyClicked);
    }

    private void OnRefreshLobbyClicked()
    {
        LobbyManager.Instance.ListLobbies();
    }
}
