using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : Singleton<InGameUIManager>
{
    public TextMeshProUGUI goalText;
    public TextMeshProUGUI turnText;

    public TextMeshProUGUI teamAScoreText;
    public TextMeshProUGUI teamBScoreText;

    public Button readyButton;
    public GameObject CharacterSlot;
    public GameObject ActionSlot;
    public GameObject ShootOptionSlot;

    [Header("패널 관리")]
    public ResultPanel resultPanel;
    public PausePanel pausePanel;
    public GameDataPanel gameDataPanel;

    private bool isOption = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CloseAllSlot();
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
        GameManager.Instance.SetPlayerReady();
    }

    public void ToggleOption()
    {
        Debug.Log("Toggle!!");
        isOption = !isOption;
        ActionSlot.SetActive(!isOption);
        ShootOptionSlot.SetActive(isOption);
    }

    public void ShowGoalMessage(TeamName scoringTeam)
    {
        Debug.Log("Goal!!!!!!!! [" + scoringTeam + "]");

        goalText.text = "GOAL!";
        goalText.transform.localScale = Vector3.zero;
        goalText.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(goalText.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack));
        seq.AppendInterval(1.2f); // 유지 시간
        seq.Append(goalText.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));
        seq.OnComplete(() => goalText.gameObject.SetActive(false));
    }


    public void UpdateScoreUI(int teamAScore, int teamBScore)
    {
        teamAScoreText.text = teamAScore.ToString();
        teamBScoreText.text = teamBScore.ToString();
    }

    public void CloseAllSlot()
    {
        isOption = false;
        CharacterSlot.SetActive(false);
        ActionSlot.SetActive(false);
        ShootOptionSlot.SetActive(false);
    }
}
