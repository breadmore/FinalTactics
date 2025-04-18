using UnityEngine;

public class GameDataPanel : BaseAnimatedPanel
{
    public void OpenWithCharacterData(CharacterData data)
    {
        // 캐릭터 정보 세팅
        Show();
    }

    public void Close()
    {
        Hide();
    }
}
