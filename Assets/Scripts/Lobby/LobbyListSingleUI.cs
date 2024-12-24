using UnityEngine;
using TMPro;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI gameModeText;


    public void SetLobbyInfo(string lobbyName, int player, string gameMode)
    {
        player = Mathf.Min(player, 4);

        lobbyNameText.text = lobbyName;
        playersText.text = player + "/4";
        gameModeText.text = gameMode;
    }
}
