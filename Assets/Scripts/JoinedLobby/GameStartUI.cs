using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviour
{
    [SerializeField] private Button gameStartButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStartButton.onClick.AddListener(OnGameStartClicked);
    }

    private void OnGameStartClicked()
    {

    }
}
