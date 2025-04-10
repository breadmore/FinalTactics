﻿using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BallManager : NetworkSingleton<BallManager>
{
    public GameObject ballObjectPrefab;
    public GameObject spawnedBall;
    public Button spawnBallButton;
    public GridTile CurrentTile { get; private set; }

    private NetworkVariable<ulong> BallOwnerNetworkId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        spawnBallButton.onClick.AddListener(OnClickSpawnBallButton);
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
    }

    public void OnClickSpawnBallButton()
    {
        if (!IsServer) return;
        GameManager.Instance.OnWaitingForSpawnBall();
    }

    public void SpawnBall(GridTile gridTile)
    {
        if (!IsServer) return;

        Vector3 tilePosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        spawnedBall = Instantiate(ballObjectPrefab, tilePosition, Quaternion.identity);
        spawnedBall.GetComponent<NetworkObject>().Spawn(true);

        CurrentTile = gridTile;

        if (gridTile.occupyingCharacter != null)
        {
            SetBallOwner(gridTile.occupyingCharacter.NetworkObjectId);
        }
    }

    // 서버 전용 메서드: 공 소유자 설정
    public void SetBallOwner(ulong playerNetworkId)
    {
        if (!IsServer) return;
        BallOwnerNetworkId.Value = playerNetworkId;
    }

    // 서버 전용 메서드: 공 이동
    public void MoveBall(GridTile targetTile)
    {
        if (!IsServer) return;

        CurrentTile = targetTile;
        BallOwnerNetworkId.Value = 0;  // 소유권 해제
        spawnedBall.transform.position = GridManager.Instance.GetNearestGridCenter(targetTile.transform.position);

        if (targetTile.occupyingCharacter != null)
        {
            SetBallOwner(targetTile.occupyingCharacter.NetworkObjectId);
        }
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

        SetBallOwner(stealer.NetworkObjectId);
        MoveBall(GridManager.Instance.GetGridTileAtPosition(stealer.GridPosition));
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
}
