using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestLoadManager : MonoBehaviour
{
    [SerializeField] private PlayerCharacter spawnPlayerPrefab;
    public Button toggleUIButton;
    public Button ballSpawnButton;
    private bool toggleUI= true;
    private bool toggleBallSpawn = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.SetState(GameState.TestState);
        InGameUIManager.Instance.CharacterSlot.SetActive(true);
        toggleUIButton.onClick.AddListener(ToggleButtonUI);
        ballSpawnButton.onClick.AddListener(ToggleBallSpawn);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.SelectedCharacterData != null && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                    if (GameManager.Instance.SelectedCharacterData != null)
                    {
                        GameManager.Instance.OnGridTileSelected(hit.collider.GetComponent<GridTile>());

                        SpawnPlayer(GameManager.Instance.SelectedGridTile);

                        GameManager.Instance.OnCharacterDataSelected(null);
                        GameManager.Instance.OnGridTileSelected(null);
                    }
                    else
                    {
                        Debug.LogError("No character selected!");
                    }
                }
                else
                {
                    Debug.LogError("No Grid Selected");
                }
            }
        }
        if (GameManager.Instance.SelectedPlayerCharacter != null 
            && GameManager.Instance.SelectedActionData != 0
            && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                        GameManager.Instance.OnGridTileSelected(hit.collider.GetComponent<GridTile>());
                        GameManager.Instance.ExecuteSelectedAction(GameManager.Instance.SelectedGridTile.gridPosition);
                        GameManager.Instance.OnPlayerCharacterSelected(null);
                        GameManager.Instance.OnGridTileSelected(null);

                }
                else
                {
                    Debug.LogError("No Grid Selected");
                }
            }
        }
        if (toggleBallSpawn && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                    GameManager.Instance.OnGridTileSelected(hit.collider.GetComponent<GridTile>());
                    SpawnBall(GameManager.Instance.SelectedGridTile);

                    ToggleBallSpawn();
                    GameManager.Instance.OnGridTileSelected(null);

                }
                else
                {
                    Debug.LogError("No Grid Selected");
                }
            }
        }
    }

    public void SpawnPlayer(GridTile gridTile)
    {
        if (gridTile == null) return;

        Vector3 centerPosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        Vector2Int gridPosition = gridTile.gridPosition;  // 서버에서 사용할 위치 값 넘기기

        int characterId = GameManager.Instance.SelectedCharacterData.id;
        PlayerCharacter playerCharacter = Instantiate(spawnPlayerPrefab, centerPosition, Quaternion.identity);

        // 서버에 생성 요청 (소유권을 클라이언트로 설정)
    }

    public void ToggleButtonUI()
    {
        toggleUI = !toggleUI;
        InGameUIManager.Instance.CharacterSlot.SetActive(toggleUI);
        InGameUIManager.Instance.ActionSlot.SetActive(!toggleUI);
    }
    public void ToggleBallSpawn()
    {
        toggleBallSpawn = !toggleBallSpawn;
    }
    public void SpawnBall(GridTile gridTile)
    {
        Vector3 tilePosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        GameObject ball = Instantiate(BallManager.Instance.ballObjectPrefab, tilePosition, Quaternion.identity);

        //BallManager.Instance.spawnedBall = ball;
Debug.Log(ball.name + " Object Spawn! : " + gridTile.gridPosition);
    }
}
