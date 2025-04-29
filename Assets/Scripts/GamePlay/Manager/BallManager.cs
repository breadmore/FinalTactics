using Cysharp.Threading.Tasks;
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
    public Vector2Int CurrentTileGridPosition { get; private set; }
    public ulong BallOwnerNetworkId { get; private set; } = 0;

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
        Debug.Log("Ball Spawn : " + newValue);
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
        Debug.Log("Team : " + GameManager.Instance.AttackingTeam);
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
            spawnBallButton.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            spawnBallButton.GetComponent<CanvasGroup>().interactable = false;
            spawnBallButton.GetComponent<CanvasGroup>().alpha = 0;
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

        SetBallOwnerClientRpc(0);
        SetBallPositionClientRpc(gridTile.gridPosition);
    }

    public async UniTask DespawnBallAsync()
    {
        if (IsServer && spawnedBall != null)
        {
            spawnedBall.Despawn(false);
            spawnedBall.gameObject.SetActive(false);
            isBallSpawned.Value = false;
        }

        SetBallOwnerClientRpc(0);
        await UniTask.Yield(); // 네트워크 작업 후 프레임 yield
    }



    // 서버 전용 메서드: 공 소유자 설정
    [ClientRpc]
    public void SetBallOwnerClientRpc(ulong networkId)
    {
        if(networkId == 0)
        {
            BallOwnerNetworkId = 0;
            spawnedBall.transform.position = Vector3.zero;
            return;
        }

        BallOwnerNetworkId = networkId;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject obj))
        {
            spawnedBall.transform.position = obj.GetComponent<PlayerCharacter>().ballPosition.transform.position;

        }
    }

    [ClientRpc]
    public void SetBallPositionClientRpc(Vector2Int tilePosition)
    {
        CurrentTileGridPosition = tilePosition;
    }

    // 서버 전용 메서드: 공 이동
    public async void MoveBall(GridTile targetTile)
    {
        await SmoothMoveAsync(GridManager.Instance.GetNearestGridCenter(targetTile.transform.position));
        SetBallPositionClientRpc(targetTile.gridPosition);
    }    


    public void DribbleBall(GridTile targetTile, PlayerCharacter dribbler)
    {
        if (dribbler.NetworkObjectId != BallOwnerNetworkId)
        {
            return;
        }

        this.dribbler = dribbler;
        SetBallPositionClientRpc(targetTile.gridPosition);
    }

    // 클라이언트 전용 확인 함수들 (읽기만 허용)
    public bool IsBallAtTile(GridTile tile)
    {
        return CurrentTileGridPosition == tile.gridPosition;
    }

    public bool IsBallOwnedBy(PlayerCharacter player)
    {
        if(player == null) return false;
        return BallOwnerNetworkId == player.NetworkObjectId;
    }


    private async UniTask SmoothMoveAsync(Vector3 targetPosition)
    {
        float duration = 0.8f; // 이동 시간
        float elapsed = 0f;

        Vector3 startPos = spawnedBall.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            spawnedBall.transform.position = Vector3.Lerp(startPos, targetPosition, t);
            await UniTask.Yield();
        }

        spawnedBall.transform.position = targetPosition;
    }

    public void SetBallOwner()
    {
        if (!IsServer) return;
        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(CurrentTileGridPosition);

        if(targetTile == null || targetTile.occupyingCharacter == null) return;
        SetBallOwnerClientRpc(targetTile.occupyingCharacter.NetworkObjectId);
    }

    [Command]
    public void BallOwn()
    {
        Debug.Log(BallOwnerNetworkId);
    }

    [Command]
    public void BallInfo()
    {
        Debug.Log("Owner : " + BallOwnerNetworkId);
        Debug.Log("Position : " + CurrentTileGridPosition);
    }

    [Command]
    public void TestBall()
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(BallOwnerNetworkId, out NetworkObject obj))
        {
            spawnedBall.transform.position = obj.GetComponent<PlayerCharacter>().ballPosition.transform.position;
            Debug.Log("obj position" + obj.GetComponent<PlayerCharacter>().ballPosition.transform.position);
            Debug.Log("ball position" + spawnedBall.transform.position);
        }
    }
}
