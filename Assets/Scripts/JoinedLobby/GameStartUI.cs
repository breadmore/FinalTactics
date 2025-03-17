using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LobbyManager.Instance.GetJoinedLobby().HostId == AuthenticationService.Instance.PlayerId)
        {
            gameStartButton.gameObject.SetActive(true);
            gameStartButton.onClick.AddListener(OnGameStartClicked);
        }
        else
        {
            gameStartButton.gameObject.SetActive(false);
        }
    }

    private async void OnGameStartClicked()
    {
        await LobbyManager.Instance.StartGame();
    }

}
