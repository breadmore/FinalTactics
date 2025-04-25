using UnityEngine;
using UnityEngine.UI;

public class OptionSlotChild : BaseLayoutGroupChild<ActionSlotChild>
{
    private Image thisImage;
    private Button button;
    private ActionSlotParent slotParent;
    public ActionOptionData ActionOptionData { get; private set; }
    public override void SetParent<T>(ILayoutGroupParent<T> newParent)
    {
        base.SetParent(newParent);

        if (newParent is ActionSlotParent parent)
        {
            slotParent = parent;
        }
    }

    private void Awake()
    {
        thisImage = GetComponent<Image>();
        button = GetComponent<Button>();

        button.onClick.AddListener(OnOptionSelected);
    }

    public void OnOptionSelected() 
    {
        GameManager.Instance.OnActionOptionSelected(ActionOptionData);
    }
    public void SetOption(ActionOptionData actionOptionData)
    {
        ActionOptionData = actionOptionData;
    }
    public void SetOptionSprite(Sprite sprite)
    {
        thisImage.sprite = sprite;
    }

}
