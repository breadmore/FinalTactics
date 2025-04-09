using UnityEngine;

public class ShootOptionSlotParent : BaseLayoutGroupParent<ShootOptionSlotChild>
{
    public Sprite[] shootOptionSprites = new Sprite[3];
    private void Start()
    {
        CreateChild(3);
        for (int i = 0; i < 3; i++)
        {
            InitChild(i);
        }

    }

    private void InitChild(int index)
    {
        if (shootOptionSprites == null) return;

        childList[index].SetOption((ShootOption)index);
        childList[index].SetOptionSprite(shootOptionSprites[index]);

    }
}
