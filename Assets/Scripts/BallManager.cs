using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BallManager : NetworkSingleton<BallManager>
{
    public GameObject ballObjectPrefab;
    public GameObject spawnedBall;
    public Button spawnBallButton;
    public GridTile CurrentTile { get; private set; }

    // ✅ 네트워크 동기화 변수 (공 소유자 ID)
    private NetworkVariable<ulong> BallOwnerNetworkId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,  // 모든 클라이언트가 읽기 가능
        NetworkVariableWritePermission.Server    // 서버만 값 변경 가능
    );

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
        ball.GetComponent<NetworkObject>().Spawn(true);

        spawnedBall = ball;
        CurrentTile = gridTile;
        Debug.Log(ball.name + " Object Spawn! : " + gridTile.gridPosition);

        if (gridTile.occupyingCharacter != null)
        {
            RequestSetBallOwnerServerRpc(gridTile.occupyingCharacter.NetworkObjectId);
        }
        else
        {
            Debug.Log("No Ball Owner Character!");
        }
    }

    // ✅ 공 소유자 변경 요청 (클라이언트 → 서버)
    [ServerRpc(RequireOwnership = false)]
    private void RequestSetBallOwnerServerRpc(ulong playerNetworkId)
    {
        BallOwnerNetworkId.Value = playerNetworkId;
        Debug.Log($"Ball is now owned by player {playerNetworkId}");
    }

    public void MoveBall(GridTile targetTile)
    {
        RequestMoveBallServerRpc(targetTile.gridPosition);
    }

    // ✅ 공 이동 요청 (클라이언트 → 서버)
    [ServerRpc(RequireOwnership = false)]
    private void RequestMoveBallServerRpc(Vector2Int targetGridPosition)
    {
        CurrentTile = GridManager.Instance.GetGridTileAtPosition(targetGridPosition);
        BallOwnerNetworkId.Value = 0;  // ✅ 소유권 해제
        spawnedBall.transform.position = GridManager.Instance.GetNearestGridCenter(CurrentTile.transform.position);

        if (CurrentTile.occupyingCharacter != null)
        {
            BallOwnerNetworkId.Value = CurrentTile.occupyingCharacter.NetworkObjectId;
        }
    }

    public void PassBall(PlayerCharacter passer, GridTile targetTile)
    {
        if (BallOwnerNetworkId.Value != passer.NetworkObjectId)
        {
            Debug.LogWarning("Player does not have the ball!");
            return;
        }

        MoveBall(targetTile);
    }

    public void StealBall(PlayerCharacter stealer)
    {
        RequestStealBallServerRpc(stealer.NetworkObjectId);
    }

    // ✅ 공 탈취 요청 (클라이언트 → 서버)
    [ServerRpc(RequireOwnership = false)]
    private void RequestStealBallServerRpc(ulong stealerNetworkId)
    {
        BallOwnerNetworkId.Value = stealerNetworkId;
        //CurrentTile = GridManager.Instance.GetGridTileAtPosition(GridManager.Instance.GetCharacterByNetworkId(stealerNetworkId).GridPosition);
        Debug.Log($"{stealerNetworkId} stole the ball!");
    }

    public bool IsBallAtTile(GridTile tile)
    {
        return CurrentTile == tile;
    }

    public bool IsBallOwnedBy(PlayerCharacter player)
    {
        Debug.Log("내 ID : " + player.NetworkObjectId);
        Debug.Log("볼 주인 ID : " + BallOwnerNetworkId.Value);
        return BallOwnerNetworkId.Value == player.NetworkObjectId;
    }
}
