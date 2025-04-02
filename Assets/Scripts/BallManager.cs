using UnityEngine;
using UnityEngine.UI;

public class BallManager : Singleton<BallManager>
{
    public GameObject ballObjectPrefab;
    public Button spawnBallButton;
    private GridTile currentTile;  // 공의 현재 위치
    private PlayerCharacter ballOwner;  // 공을 가진 플레이어

    private void Start()
    {
        spawnBallButton.onClick.AddListener(OnClickSpawnBallButton);
    }
    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.WaitingForSpawnBall && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                        GameManager.Instance.OnGridTileSelected(hit.collider.GetComponent<GridTile>());
                        SpawnBall(GameManager.Instance.SelectedGridTile);
                        // spawn 성공시 아래 작업
                }
                else
                {
                    Debug.LogError("No Grid Selected");
                }
            }
        }
    }
    public void OnClickSpawnBallButton()
    {
        GameManager.Instance.OnWaitingForSpawnBall();
    }
    
    public void SpawnBall(GridTile gridTile)
    {
        Vector3 tilePosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        GameObject ball = Instantiate(ballObjectPrefab, tilePosition, Quaternion.identity);
        Debug.Log(ball.name + " Object Spawn! : " + gridTile.gridPosition);
        if(gridTile.occupyingCharacter != null)
        {
            SetBallOwner(gridTile.occupyingCharacter);
            Debug.Log("Ball Owner Exist!");
        }
        else
        {
            Debug.Log("No Ball Owner Character!");
        }
    }
    public void SetBallOwner(PlayerCharacter player)
    {
        ballOwner = player;
        currentTile = GridManager.Instance.GetGridTileAtPosition(player.GridPosition); // 캐릭터 위치로 공 이동
        Debug.Log($"Ball is now owned by {player.CharacterData.id}");
    }

    public void MoveBall(GridTile targetTile)
    {
        currentTile = targetTile;
        ballOwner = null; // 소유자가 없음 (공만 이동)

        Debug.Log($"Ball moved to {targetTile.gridPosition}");
    }

    public void PassBall(PlayerCharacter passer, GridTile targetTile)
    {
        if (ballOwner != passer)
        {
            Debug.LogWarning("Player does not have the ball!");
            return;
        }

        MoveBall(targetTile);
        Debug.Log($"{passer.CharacterData.id} passed the ball to {targetTile.gridPosition}");
    }

    public void StealBall(PlayerCharacter stealer)
    {
        ballOwner = stealer;
        currentTile = GridManager.Instance.GetGridTileAtPosition(stealer.GridPosition);
        Debug.Log($"{stealer.CharacterData.id} stole the ball!");
    }

    public bool IsBallAtTile(GridTile tile)
    {
        return currentTile == tile;
    }

    public bool IsBallOwnedBy(PlayerCharacter player)
    {
        return ballOwner == player;
    }

    //public bool IsBallHeadingTowardsGoal()
    //{
    //    return currentTile.IsGoalTile();
    //}
}
