using TMPro;
using UnityEngine;

public class ResultPanel : BaseAnimatedPanel
{
    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Red Team Stats")]
    [SerializeField] private TextMeshProUGUI redShootText;
    [SerializeField] private TextMeshProUGUI redPassText;
    [SerializeField] private TextMeshProUGUI redDribbleText;
    [SerializeField] private TextMeshProUGUI redBlockText;
    [SerializeField] private TextMeshProUGUI redTackleText;

    [Header("Blue Team Stats")]
    [SerializeField] private TextMeshProUGUI blueShootText;
    [SerializeField] private TextMeshProUGUI bluePassText;
    [SerializeField] private TextMeshProUGUI blueDribbleText;
    [SerializeField] private TextMeshProUGUI blueBlockText;
    [SerializeField] private TextMeshProUGUI blueTackleText;

    public void ShowResult()
    {
        // ���� ǥ��
        scoreText.text = $"{GameManager.Instance.teamRed.score} - {GameManager.Instance.teamBlue.score}";

        // ���� �� �׼� ���
        var redStats = GameManager.Instance.teamActionCounters[TeamName.Red];
        redShootText.text = redStats.shootCount.ToString();
        redPassText.text = redStats.passCount.ToString();
        redDribbleText.text = redStats.dribbleCount.ToString();
        redBlockText.text = redStats.blockCount.ToString();
        redTackleText.text = redStats.tackleCount.ToString();

        // ��� �� �׼� ���
        var blueStats = GameManager.Instance.teamActionCounters[TeamName.Blue];
        blueShootText.text = blueStats.shootCount.ToString();
        bluePassText.text = blueStats.passCount.ToString();
        blueDribbleText.text = blueStats.dribbleCount.ToString();
        blueBlockText.text = blueStats.blockCount.ToString();
        blueTackleText.text = blueStats.tackleCount.ToString();

        Show();
    }
}