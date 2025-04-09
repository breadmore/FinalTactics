using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : Singleton<InGameUIManager>
{
    public Button readyButton;
    public GameObject CharacterSlot;
    public GameObject ActionSlot;
    public GameObject ShootOptionSlot;

    private bool isOption = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyButton.onClick.AddListener(OnReadyButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnReadyButtonClick()
    {
        GameManager.Instance.SetPlayerReady();
    }

    public void ToggleOption()
    {
        Debug.Log("Toggle!!");
        isOption = !isOption;
        ActionSlot.SetActive(!isOption);
        ShootOptionSlot.SetActive(isOption);
    }
}
