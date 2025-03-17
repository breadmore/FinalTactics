using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ChangeTeamUI : MonoBehaviour
{
    [SerializeField] private Button changeTeamButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        changeTeamButton.onClick.AddListener(OnChangeTeamClicked);
    }

    private async void OnChangeTeamClicked()
    {
        Debug.Log("Team change");
        await LobbyManager.Instance.UpdatePlayerTeam();
    }
}
