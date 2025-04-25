using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : Singleton<InGameUIManager>
{
    public TextMeshProUGUI AlertText;
    public TextMeshProUGUI turnText;

    public TextMeshProUGUI teamAScoreText;
    public TextMeshProUGUI teamBScoreText;

    public Button readyButton;
    public CharacterSlotParent CharacterSlot;
    public ActionSlotParent ActionSlot;
    public OptionSlotParent OptionSlot;

    [Header("패널 관리")]
    public ResultPanel resultPanel;
    public PausePanel pausePanel;
    public GameDataPanel gameDataPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyButton.onClick.AddListener(OnReadyButtonClick);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!pausePanel.IsVisible())
                pausePanel.OnPause();
            else
                pausePanel.OnResume();
        }

    }

    public void OnReadyButtonClick()
    {
        Debug.Log("Button Click");
        GameManager.Instance.SetPlayerReady();
    }

    public void ShowGoalMessage(TeamName scoringTeam)
    {
        Debug.Log("Goal!!!!!!!! [" + scoringTeam + "]");

        AlertText.text = "GOAL!";
        AlertText.transform.localScale = Vector3.zero;
        AlertText.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(AlertText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack));
        seq.AppendInterval(1.2f); // 유지 시간
        seq.Append(AlertText.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        seq.OnComplete(() => AlertText.gameObject.SetActive(false));
    }


    public void UpdateScoreUI(int teamAScore, int teamBScore)
    {
        teamAScoreText.text = teamAScore.ToString();
        teamBScoreText.text = teamBScore.ToString();
    }

    public void OpenSlotUI()
    {
        CloseAllSlot();

        if (GameManager.Instance._currentState is CharacterDataSelectionState)
        {
            CharacterSlot.gameObject.SetActive(true);
        }
        else if(GameManager.Instance._currentState is CharacterControlState)
        {
            ActionSlot.gameObject.SetActive(true);
        }
        else if(GameManager.Instance._currentState is ActionOptionSelecteState)
        {
            OptionSlot.gameObject.SetActive(true);
        }
    }
    public void CloseAllSlot()
    {
        CharacterSlot.gameObject.SetActive(false);
        ActionSlot.gameObject.SetActive(false);
        OptionSlot.gameObject.SetActive(false);
    }
}
