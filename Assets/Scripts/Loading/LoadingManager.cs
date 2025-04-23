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
    [SerializeField] private Slider loadingBar;  // 로딩 진행 표시를 위한 슬라이더
    [SerializeField] private TextMeshProUGUI loadingText;   // 로딩 상태 표시를 위한 텍스트
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
            loadingBar.DOValue(targetProgress, 0.2f); // DOTween을 사용해 부드럽게 변화

            await Task.Yield();
        }

        loadingBar.DOValue(asyncLoad.progress, 0.2f); // 마지막 0.9까지 반영
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
        // Main 씬의 초기화 작업 (예: 네트워크 연결, UI 설정 등)
        await GameManager.Instance.LoadPlayers();
        //GameManager.Instance.AssignTeams();
        Debug.Log("플레이어 로드 작업 완료");

        // Relay 연결이 완료될 때까지 기다리기
        await RelayManager.Instance.WaitForRelayConnection();

        Debug.Log("씬이 활성화됨! UI를 활성화합니다.");
        GameManager.Instance.ChangeState<CharacterSelectionState>();
    }



    public void SetLoadSceneType(LoadSceneType loadSceneType)
    {
        this.loadSceneType = loadSceneType;
    }
}
