using UnityEngine;

public class GameDataPanel : BaseAnimatedPanel
{
    public void OpenWithCharacterData(CharacterData data)
    {
        // ĳ���� ���� ����
        Show();
    }

    public void Close()
    {
        Hide();
    }
}
