using TMPro;
using UnityEngine;
public class ResultPanel : BaseAnimatedPanel
{
    [SerializeField]
    private TextMeshProUGUI scoreText;
    public void ShowResult()
    {
        // ��� �ؽ�Ʈ ����
        scoreText.text = GameManager.Instance.teamA.score + " - " + GameManager.Instance.teamB.score;
        Show();
    }
}

