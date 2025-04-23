using QFSW.QC;
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

    private NetworkVariable<bool> isBallSpawned = new NetworkVariable<bool>();
    public bool IsBallSpawned => isBallSpawned.Value;

    //[HideInInspector]
    public PlayerCharacter dribbler = null;

    private void Awake()
    {
        spawnBallButton.onClick.AddListener(OnClickSpawnBallButton);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isBallSpawned.OnValueChanged += OnIsBallSpawnedChanged;

        if (IsServer)
        {
            isBallSpawned.Value = false;
        }
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isBallSpawned.OnValueChanged -= OnIsBallSpawnedChanged;
    }


    private void Update()
    {
        HandleBallSpawnInput();

        if (dribbler != null)
        {
            spawnedBall.transform.position = dribbler.ballPosition.position;
        }
    }

    public void OnIsBallSpawnedChanged(bool oldValue, bool newValue)
    {
        // 같은 팀에서 생성 했을 경우
        if (IsBallSpawned)
        {
            ActiveButton(false);

            // 강제 상태 변경
            if(GameManager.Instance._currentState is CharacterPlacementCompleteState)
            {
                GameManager.Instance.ChangeState<ReadyCheckState>();
            }
        }
    }
    public void UpdateSpawnBallButtonState()
    {
        if (IsBallSpawned) return;

        Debug.Log("Att : " + GameManager.Instance.AttackingTeam);
        Debug.Log("My : " + GameManager.Instance.thisPlayerBrain.GetMyTeam());
        if (GameManager.Instance.AttackingTeam == GameManager.Instance.thisPlayerBrain.GetMyTeam())
        {
            ActiveButton(true);
        }
        else
        {
            ActiveButton(false);
        }
    }

    public void ActiveButton(bool active)
    {
        if (active)
        {
            spawnBallButton.GetComponent<CanvasGroup>().interactable = true;
        }
        else
        {
            spawnBallButton.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    private void HandleBallSpawnInput()
    {
        if (!IsWaitingForSpawnBallState()) return;
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit) || !hit.collider.CompareTag("Grid")) return;

        TryBallSpawn(hit.collider.GetComponent<GridTile>());
    }

    private void TryBallSpawn(GridTile gridTile)
    {
        GameManager.Instance.OnGridTileSelected(gridTile);

        SpawnBall(gridTile);

    }
    private bool IsWaitingForSpawnBallState()
    {
        return GameManager.Instance._currentState is BallPlacementState;
    }

    public void OnClickSpawnBallButton()
    {
        if(GameManager.Instance._currentState is CharacterPlacementCompleteState)
        {
            GameManager.Instance.ChangeState<BallPlacementState>();
        }
    }

    public void PreSpawnBall()
    {
        GameObject obj = Instantiate(ballObjectPrefab);
        spawnedBall = obj.GetComponent<NetworkObject>();
        spawnedBall.gameObject.SetActive(false);
    }


    public void SpawnBall(GridTile gridTile)
    {
        if(gridTile == null) return;

        Vector3 tilePosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);

        RequestBallSpawnServerRpc(tilePosition, gridTile.gridPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestBallSpawnServerRpc(Vector3 centerPosition, Vector2Int gridPosition)
    {
        if (!IsServer) return;

        GridTile gridTile = GridManager.Instance.GetGridTileAtPosition(gridPosition);
        spawnedBall.gameObject.SetActive(true);
        spawnedBall.Spawn(true);
        spawnedBall.transform.position = centerPosition;

        isBallSpawned.Value = true;

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
        isBallSpawned.Value = false;
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

    public void TurnStartSetting()
    {
        if(!IsServer) return;
        SetBallOwner(CurrentTile.occupyingCharacter);
    }

    [Command]
    public void BallOwn()
    {
        Debug.Log(BallOwnerNetworkId.Value);
    }
}
