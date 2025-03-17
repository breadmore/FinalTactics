using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;

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
            case LoadSceneType.Main:
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
        // �ε� ���� �ؽ�Ʈ ������Ʈ
        loadingText.text = "Loading...";

        // �񵿱� �� �ε� ����
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Main");

        // ���� �ε�Ǿ����� �ڵ����� Ȱ��ȭ���� �ʵ��� ����
        asyncLoad.allowSceneActivation = false;  // �� Ȱ��ȭ ����
        float targetProgress = loadingBar.value;

        // �ε� �� ���� ���¸� ������Ʈ
        while (asyncLoad.progress < 0.9f)  // progress�� 0.9 �̸��� ������ ���
        {
            // �����̴��� �� ������Ʈ
            loadingBar.value = asyncLoad.progress;

            // �����Ӹ��� ��ٸ��� ��� ����
            await Task.Yield();
        }

        // �����̴��� �� ������Ʈ
        loadingBar.value = asyncLoad.progress;

        // �ε��� 90% �̻��� ��, �ʱ�ȭ �۾� ����
        loadingText.text = "Initializing...";
        await InitializeMainScene();

        // ���콺 Ŭ���� ��ٸ���, Ŭ���� ������ ���� ��ȯ
        while (!Input.GetMouseButtonDown(0))  // Ŭ�� ���
        {
            await Task.Yield();  // ��� ��ٸ� (�� �����Ӹ��� Ȯ��)
        }
        canvas.gameObject.SetActive(false);
        // Ŭ���� ������ ���� Ȱ��ȭ
        asyncLoad.allowSceneActivation = true;  // �� Ȱ��ȭ
        isLoading = false;
        // �� ��ȯ ��, ĵ������ �����

    }

    private async Task InitializeMainScene()
    {
        // Main ���� �ʱ�ȭ �۾� (��: ��Ʈ��ũ ����, UI ���� ��)
        await GameManager.Instance.LoadPlayers();
        GameManager.Instance.InitPlayerData();
        //GameManager.Instance.AssignTeams();
        Debug.Log("Main �� �ʱ�ȭ �۾� �Ϸ�");

        // Relay ������ �Ϸ�� ������ ��ٸ���
        await RelayManager.Instance.WaitForRelayConnection();
    }



    public void SetLoadSceneType(LoadSceneType loadSceneType)
    {
        this.loadSceneType = loadSceneType;
    }
}
