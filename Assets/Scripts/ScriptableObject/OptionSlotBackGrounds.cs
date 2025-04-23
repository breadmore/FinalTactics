using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "OptionSlotBackgrounds", menuName = "Game Data/OptionSlot Backgrounds")]
public class OptionSlotBackgrounds : ScriptableObject
{
    [SerializeField] private OptionSlot[] optionSlots; // 배경 이미지 목록

    // 특정 옵션 ID의 배경 이미지 가져오기
    public Sprite GetBackground(int optionId)
    {
        // null 체크
        if (optionSlots == null || optionSlots.Length == 0)
        {
            Debug.LogError("OptionSlots array is not initialized!");
            return null;
        }

        // LINQ를 사용해 옵션 ID로 검색
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

    // 모든 옵션 슬롯의 ID 배열 반환 (에디터에서 사용 가능)
    public int[] GetAllOptionIds()
    {
        return optionSlots?
            .Where(slot => slot != null)
            .Select(slot => slot.OptionId)
            .ToArray() ?? new int[0];
    }

    // 에디터에서 데이터 유효성 검사
    public void ValidateData()
    {
        if (optionSlots == null) return;

        // 중복 ID 검사
        var duplicateIds = optionSlots
            .Where(slot => slot != null)
            .GroupBy(slot => slot.OptionId)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var id in duplicateIds)
        {
            Debug.LogError($"Duplicate option ID found: {id}");
        }

        // null 이미지 검사
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

    // 프로퍼티로 안전한 접근 제공
    public int OptionId => optionId;
    public Sprite BackgroundImage => backgroundImage;

    // 에디터에서 사용할 수 있는 생성자
    public OptionSlot(int id, Sprite image)
    {
        optionId = id;
        backgroundImage = image;
    }
}