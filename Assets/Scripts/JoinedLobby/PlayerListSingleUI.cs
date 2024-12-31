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

    public bool ready = false;
    public void SetPlayerInfo(string playerName, bool ready)
    {
        playerNameText.text = playerName;
        SetPlayerReadyCheck(ready);
    }

    public void SetPlayerReadyCheck(bool ready)
    {
        readyImage.sprite = ready ? readyAgreeSprite : readyDisagreeSprite;
    }
}
