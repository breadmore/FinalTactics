using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingManager : DontDestroySingleton<LoadingManager>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider loadingBar;  // �ε� ���� ǥ�ø� ���� �����̴�
    [SerializeField] private TextMeshProUGUI loadingText;   // �ε� ���� ǥ�ø� ���� �ؽ�Ʈ
    private LoadSceneType loadSceneType;
    public bool isLoading = false;

    public async Task DecideNextScene()
    {
        SceneManager.LoadScene("Loading");
        isLoading = true;
        switch (loadSceneType)
        {
            case LoadSceneType.Intro:
                break;
            case LoadSceneType.InGame:
                await LoadMainSceneAsync();
                break;
            case LoadSceneType.Loading:
                break;
        }
    }

    private async Task LoadMainSceneAsync()
    {
        canvas.gameObject.SetActive(true);
        loadingBar.value = 0;
        loadingBar.maxValue = 1;
        loadingText.text = "Loading...";

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("InGame");
        asyncLoad.allowSceneActivation = false;
        float targetProgress = 0f;

        while (asyncLoad.progress < 0.9f)
        {
            targetProgress = asyncLoad.progress;
            loadingBar.DOValue(targetProgress, 0.2f); // DOTween�� ����� �ε巴�� ��ȭ

            await Task.Yield();
        }

        loadingBar.DOValue(asyncLoad.progress, 0.2f); // ������ 0.9���� �ݿ�
        loadingText.text = "Loading player data ....";
        await InitializeMainScene();

        loadingText.text = "Press Any Key";

        while (!Input.GetMouseButtonDown(0))
        {
            await Task.Yield();
        }

        canvas.gameObject.SetActive(false);
        isLoading = false;
        asyncLoad.allowSceneActivation = true;
    }


    private async Task InitializeMainScene()
    {
        // Main ���� �ʱ�ȭ �۾� (��: ��Ʈ��ũ ����, UI ���� ��)
        await GameManager.Instance.LoadPlayers();
        //GameManager.Instance.AssignTeams();
        Debug.Log("�÷��̾� �ε� �۾� �Ϸ�");

        // Relay ������ �Ϸ�� ������ ��ٸ���
        await RelayManager.Instance.WaitForRelayConnection();

        Debug.Log("���� Ȱ��ȭ��! UI�� Ȱ��ȭ�մϴ�.");
        GameManager.Instance.ChangeState<CharacterSelectionState>();
    }



    public void SetLoadSceneType(LoadSceneType loadSceneType)
    {
        this.loadSceneType = loadSceneType;
    }
}
