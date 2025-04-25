using TMPro;
using UnityEngine;
public class ResultPanel : BaseAnimatedPanel
{
    [SerializeField]
    private TextMeshProUGUI scoreText;
    public void ShowResult()
    {
        // 결과 텍스트 세팅
        scoreText.text = GameManager.Instance.teamRed.score + " - " + GameManager.Instance.teamBlue.score;
        Show();
    }
}

