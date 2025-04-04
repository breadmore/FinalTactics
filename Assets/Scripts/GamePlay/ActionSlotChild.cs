using UnityEngine;
using UnityEngine.UI;

public class ActionSlotChild : BaseLayoutGroupChild<ActionSlotChild>
{
    private Image thisImage;
    private Button button;
    private ActionSlotParent slotParent;
    public int ActionId { get; private set; }

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

        button.onClick.AddListener(OnActionSelected);
    }

    public void SetAction(int actionId)
    {
        ActionId = actionId;
    }

    private void OnActionSelected()
    {
        // Action ID를 기반으로 ActionData 검색 후 설정
        var actionData = LoadDataManager.Instance.actionDataReader.GetActionDataById(ActionId);

        if (actionData != null)
        {
            GameManager.Instance.OnActionSelected(actionData);

            ActionPreviewManager.Instance.HighlightTilesForAction(actionData.action, GameManager.Instance.SelectedPlayerCharacter);
            Debug.Log($"Selected Action: {actionData.action}");
        }
        else
        {
            Debug.LogWarning($"ActionData not found for ID: {ActionId}");
        }
    }


    public void SetActionSprite(Sprite sprite)
    {
        thisImage.sprite = sprite;
    }

}
