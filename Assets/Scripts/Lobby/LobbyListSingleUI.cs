using UnityEngine;
using TMPro;
public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI gameModeText;


    public void SetLobbyInfo(string lobbyName, int nowPlayer,int maxPlayer, string gameMode)
    {
        maxPlayer = Mathf.Min(maxPlayer, 4);

        lobbyNameText.text = lobbyName;
        playersText.text = nowPlayer + "/" + maxPlayer;
        gameModeText.text = gameMode;
    }
}
