using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BallManager : NetworkSingleton<BallManager>
{
    public GameObject ballObjectPrefab;
    public NetworkObject spawnedBall;
    public Button spawnBallButton;
    public GridTile CurrentTile { get; private set; }

    private NetworkVariable<ulong> BallOwnerNetworkId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    //[HideInInspector]
    public PlayerCharacter dribbler = null;


    private void Start()
    {
        spawnBallButton.onClick.AddListener(OnClickSpawnBallButton);
        if (IsServer)
        {
            TurnManager.Instance.OnTurnEnd += TurnStartSetting;
        }
    }

    private void Update()
    {
        if (!IsServer) return;  // 👈 서버에서만 실행

        if (GameManager.Instance.CurrentState == GameState.WaitingForSpawnBall
            && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Grid"))
            {
                var tile = hit.collider.GetComponent<GridTile>();
                GameManager.Instance.OnGridTileSelected(tile);
                SpawnBall(tile);
            }
        }

        if(dribbler != null)
        {
            spawnedBall.transform.position = dribbler.ballPosition.position;
        }
    }

    public void OnClickSpawnBallButton()
    {
        if (!IsServer) return;
        GameManager.Instance.OnWaitingForSpawnBall();
    }

    public void PreSpawnBall()
    {
        Debug.Log("Server");
        GameObject obj = Instantiate(ballObjectPrefab);
        spawnedBall = obj.GetComponent<NetworkObject>();
        spawnedBall.gameObject.SetActive(false);
    }

    public void SpawnBall(GridTile gridTile)
    {
        if (!IsServer) return;

        Vector3 tilePosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        spawnedBall.gameObject.SetActive(true);
        spawnedBall.Spawn(true);
        spawnedBall.transform.position = tilePosition;

        CurrentTile = gridTile;

        if (gridTile.occupyingCharacter != null)
        {
            SetBallOwner(gridTile.occupyingCharacter);
        }
    }

    public void DespawnBall() 
    {
        if (!IsServer) return;
        BallOwnerNetworkId.Value = 0;
        CurrentTile = null;

        spawnedBall.Despawn(false);
        spawnedBall.gameObject.SetActive(false);
    }

    // 서버 전용 메서드: 공 소유자 설정
    public void SetBallOwner(PlayerCharacter playerCharacter)
    {
        if (!IsServer || playerCharacter == null) return;

        BallOwnerNetworkId.Value = playerCharacter.NetworkObjectId;
        spawnedBall.transform.position = playerCharacter.ballPosition.position;
    }

    // 서버 전용 메서드: 공 이동
    public void MoveBall(GridTile targetTile)
    {
        if (!IsServer) return;

        CurrentTile = targetTile;
        BallOwnerNetworkId.Value = 0;  // 소유권 해제
        StartCoroutine(SmoothMoveCoroutine(GridManager.Instance.GetNearestGridCenter(targetTile.transform.position)));

        if (targetTile.occupyingCharacter != null)
        {
            SetBallOwner(targetTile.occupyingCharacter);
        }
    }

    public void DribbleBall(GridTile targetTile, PlayerCharacter dribbler)
    {
        if (!IsServer) return;

        if (dribbler.NetworkObjectId != BallOwnerNetworkId.Value)
        {
            return;
        }

        this.dribbler = dribbler;
        SetBallOwner(dribbler);
        CurrentTile = targetTile;
    }

    // 서버 전용 메서드: 패스
    public void PassBall(PlayerCharacter passer, GridTile targetTile)
    {
        if (!IsServer) return;
        if (BallOwnerNetworkId.Value != passer.NetworkObjectId)
        {
            Debug.LogWarning("Cannot pass: player doesn't have the ball.");
            return;
        }

        passer.PlayAnimationPass();

        MoveBall(targetTile);

    }

    // 서버 전용 메서드: 탈취
    public void StealBall(PlayerCharacter stealer)
    {
        if (!IsServer) return;

        SetBallOwner(stealer);
        //MoveBall(GridManager.Instance.GetGridTileAtPosition(stealer.GridPosition));
    }

    // 클라이언트 전용 확인 함수들 (읽기만 허용)
    public bool IsBallAtTile(GridTile tile)
    {
        return CurrentTile == tile;
    }

    public bool IsBallOwnedBy(PlayerCharacter player)
    {
        return BallOwnerNetworkId.Value == player.NetworkObjectId;
    }


    public void ResetBallPosition()
    {
        Vector2Int gridPosition = new Vector2Int(7, 4);
        // 볼 위치 중앙 초기화

        MoveBall(GridManager.Instance.GetGridTileAtPosition(gridPosition));
    }

    private IEnumerator SmoothMoveCoroutine(Vector3 targetPosition)
    {
        float duration = 0.8f; // 이동 시간
        float elapsed = 0f;

        Vector3 startPos = spawnedBall.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            spawnedBall.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }

        spawnedBall.transform.position = targetPosition;
    }

    private void TurnStartSetting()
    {
        if(!IsServer) return;
        SetBallOwner(CurrentTile.occupyingCharacter);
    }
}
