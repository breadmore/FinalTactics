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
        // 로딩 진행 텍스트 업데이트
        loadingText.text = "Loading...";

        // 비동기 씬 로딩 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("InGame");

        // 씬이 로드되었지만 자동으로 활성화되지 않도록 설정
        asyncLoad.allowSceneActivation = false;  // 씬 활성화 지연
        float targetProgress = loadingBar.value;

        // 로딩 중 진행 상태를 업데이트
        while (asyncLoad.progress < 0.9f)  // progress가 0.9 미만일 때까지 대기
        {
            // 슬라이더의 값 업데이트
            loadingBar.value = asyncLoad.progress;

            // 프레임마다 기다리며 계속 진행
            await Task.Yield();
        }

        // 슬라이더의 값 업데이트
        loadingBar.value = asyncLoad.progress;

        // 로딩이 90% 이상일 때, 초기화 작업 수행
        loadingText.text = "Initializing...";
        await InitializeMainScene();

        // 마우스 클릭을 기다리며, 클릭이 있으면 씬을 전환
        while (!Input.GetMouseButtonDown(0))  // 클릭 대기
        {
            await Task.Yield();  // 계속 기다림 (매 프레임마다 확인)
        }
        canvas.gameObject.SetActive(false);


        isLoading = false;
        asyncLoad.allowSceneActivation = true;  // 씬 활성화


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
        InGameUIManager.Instance.CharacterSlot.SetActive(true);
    }



    public void SetLoadSceneType(LoadSceneType loadSceneType)
    {
        this.loadSceneType = loadSceneType;
    }
}
