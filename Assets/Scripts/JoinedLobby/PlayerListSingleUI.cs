using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image readyImage;
    [SerializeField] private TextMeshProUGUI gameModeText;

    [SerializeField] private Sprite readyAgreeSprite;
    [SerializeField] private Sprite readyDisagreeSprite;

    public void SetPlayerInfo(string playerName, bool ready, string gameMode)
    {
        playerNameText.text = playerName;
        SetPlayerReadyCheck(ready);
        gameModeText.text = gameMode;
    }

    public void SetPlayerReadyCheck(bool ready)
    {
        readyImage.sprite = ready ? readyAgreeSprite : readyDisagreeSprite;
    }
}
