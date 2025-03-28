using UnityEngine;

[CreateAssetMenu(fileName = "ActionSlotBackgrounds", menuName = "Game Data/ActionSlotBackgrounds")]
public class ActionSlotBackgrounds : ScriptableObject
{
    [SerializeField] private Sprite[] backgroundImages; // 배경 이미지 목록

    // 특정 인덱스의 배경 이미지 가져오기
    public Sprite GetBackground(int index)
    {
        if (index >= 0 && index < backgroundImages.Length)
        {
            return backgroundImages[index];
        }

        Debug.LogWarning("Invalid background index: " + index);
        return null;
    }

    // 배경 이미지 개수 반환
    public int GetBackgroundCount()
    {
        return backgroundImages.Length;
    }
}