using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameReadyUI : MonoBehaviour
{
    [SerializeField] private Button gameReadyButton;
    private bool gameReady;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameReadyButton.onClick.AddListener(OnGameReadyClicked);
    }

    private void OnGameReadyClicked()
    {
        if (LobbyManager.Instance.GetPlayer().Data.TryGetValue("PlayerReady", out PlayerDataObject readyData))
        {
            bool.TryParse(readyData.Value, out gameReady);
        }
        gameReady = !gameReady;
        LobbyManager.Instance.UpdatePlayerReady(gameReady);
    }
}