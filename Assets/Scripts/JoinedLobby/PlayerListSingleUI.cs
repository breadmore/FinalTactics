using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
public class PlayerListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image readyImage;
    [SerializeField] private TextMeshProUGUI gameModeText;

    [SerializeField] private Sprite readyAgreeSprite;
    [SerializeField] private Sprite readyDisagreeSprite;
    private Player player;
    private string playerId;

    public void SetPlayerInfo(Player _player)
    {
        player = _player;
        playerNameText.text = player.Data["PlayerName"].Value;
        SetPlayerReadyCheck(ReadyCheck());
    }

    public void SetPlayerReadyCheck(bool ready)
    {
        readyImage.sprite = ready ? readyAgreeSprite : readyDisagreeSprite;
    }

    public bool ReadyCheck()
    {
        bool _ready = false;
        if (player.Data.TryGetValue("PlayerReady", out PlayerDataObject readyData))
        {
            bool.TryParse(readyData.Value, out _ready);
        }

        return _ready;
    }
    public Player GetPlayer()
    {
        return player;
    }

    public string GetPlayerId()
    {
        return playerId;
    }
}
