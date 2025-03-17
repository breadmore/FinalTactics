using UnityEngine;
using UnityEngine.UI;

public class LayoutGroupChild : BaseLayoutGroupChild<LayoutGroupChild>
{
    private Button button;

    protected override void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log($"{gameObject.name} ¹öÆ° Å¬¸¯µÊ!");
    }
}
