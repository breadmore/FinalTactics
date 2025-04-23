using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "OptionSlotBackgrounds", menuName = "Game Data/OptionSlot Backgrounds")]
public class OptionSlotBackgrounds : ScriptableObject
{
    [SerializeField] private OptionSlot[] optionSlots; // ��� �̹��� ���

    // Ư�� �ɼ� ID�� ��� �̹��� ��������
    public Sprite GetBackground(int optionId)
    {
        // null üũ
        if (optionSlots == null || optionSlots.Length == 0)
        {
            Debug.LogError("OptionSlots array is not initialized!");
            return null;
        }

        // LINQ�� ����� �ɼ� ID�� �˻�
        var slot = optionSlots.FirstOrDefault(s => s != null && s.OptionId == optionId);

        if (slot == null)
        {
            Debug.LogWarning($"No background found for option ID: {optionId}");
            return null;
        }

        if (slot.BackgroundImage == null)
        {
            Debug.LogWarning($"Background image is null for option ID: {optionId}");
        }

        return slot.BackgroundImage;
    }

    // ��� �ɼ� ������ ID �迭 ��ȯ (�����Ϳ��� ��� ����)
    public int[] GetAllOptionIds()
    {
        return optionSlots?
            .Where(slot => slot != null)
            .Select(slot => slot.OptionId)
            .ToArray() ?? new int[0];
    }

    // �����Ϳ��� ������ ��ȿ�� �˻�
    public void ValidateData()
    {
        if (optionSlots == null) return;

        // �ߺ� ID �˻�
        var duplicateIds = optionSlots
            .Where(slot => slot != null)
            .GroupBy(slot => slot.OptionId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var id in duplicateIds)
        {
            Debug.LogError($"Duplicate option ID found: {id}");
        }

        // null �̹��� �˻�
        foreach (var slot in optionSlots)
        {
            if (slot != null && slot.BackgroundImage == null)
            {
                Debug.LogWarning($"Null background image for option ID: {slot.OptionId}");
            }
        }
    }
}

[System.Serializable]
public class OptionSlot
{
    [SerializeField] private int optionId;
    [SerializeField] private Sprite backgroundImage;

    // ������Ƽ�� ������ ���� ����
    public int OptionId => optionId;
    public Sprite BackgroundImage => backgroundImage;

    // �����Ϳ��� ����� �� �ִ� ������
    public OptionSlot(int id, Sprite image)
    {
        optionId = id;
        backgroundImage = image;
    }
}