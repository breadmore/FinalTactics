using UnityEngine;

[CreateAssetMenu(fileName = "ActionSlotBackgrounds", menuName = "Game Data/ActionSlotBackgrounds")]
public class ActionSlotBackgrounds : ScriptableObject
{
    [SerializeField] private Sprite[] backgroundImages; // ��� �̹��� ���

    // Ư�� �ε����� ��� �̹��� ��������
    public Sprite GetBackground(int index)
    {
        if (index >= 0 && index < backgroundImages.Length)
        {
            return backgroundImages[index];
        }

        Debug.LogWarning("Invalid background index: " + index);
        return null;
    }

    // ��� �̹��� ���� ��ȯ
    public int GetBackgroundCount()
    {
        return backgroundImages.Length;
    }
}