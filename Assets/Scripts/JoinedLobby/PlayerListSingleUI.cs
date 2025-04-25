using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
public class PlayerListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image readyImage;
    [SerializeField] private TextMeshProUGUI gameTeamText;

    [SerializeField] private Sprite readyAgreeSprite;
    [SerializeField] private Sprite readyDisagreeSprite;
    private Player player;
    private string playerId;

    public void SetPlayerInfo(Player _player)
    {
        player = _player;
        playerNameText.text = player.Data["PlayerName"].Value;
        SetPlayerReadyCheck(ReadyCheck());
        SetPlayerTeamCheck(TeamCheck());
    }

    public void SetPlayerTeamCheck(int team)
    {
        if (team == 1)
        {
            gameTeamText.text = TeamName.Red.ToString();
        }
        else if (team == 2) 
        {
            gameTeamText.text = TeamName.Blue.ToString();
        }
    }

    public void SetPlayerReadyCheck(bool ready)
    {
        readyImage.sprite = ready ? readyAgreeSprite : readyDisagreeSprite;
    }

    public int TeamCheck()
    {
        int _team = 0;
        if (player.Data.TryGetValue("PlayerTeam", out PlayerDataObject teamData))
        {
            int.TryParse(teamData.Value, out _team);
        }

        return _team;
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
