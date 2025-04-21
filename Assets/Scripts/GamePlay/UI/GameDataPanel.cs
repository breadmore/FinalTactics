using TMPro;
using UnityEngine;

public class GameDataPanel : BaseAnimatedPanel
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI passText;
    [SerializeField] private TextMeshProUGUI shootText;
    [SerializeField] private TextMeshProUGUI dribbleText;
    [SerializeField] private TextMeshProUGUI tackleText;
    [SerializeField] private TextMeshProUGUI staminaText;
    public void OpenWithCharacterData(PlayerCharacter character)
    {
        nameText.text = "Name";
             
        // 캐릭터 정보 세팅
        speedText.text = character.CharacterStat.speed.ToString();
        passText.text = character.CharacterStat.pass.ToString();
        shootText.text = character.CharacterStat.shoot.ToString();
        dribbleText.text = character.CharacterStat.dribble.ToString();
        tackleText.text = character.CharacterStat.tackle.ToString();
        staminaText.text = character.CharacterStat.stamina.ToString();
        Show();
    }

    public void Close()
    {
        Hide();
    }
}
