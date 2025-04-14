using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class PlayerBrain : NetworkBehaviour
{
    [SerializeField] private PlayerCharacter spawnPlayerPrefab;
    private PlayerData thisPlayerData;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                Debug.Log("This is Server!!!!");
                ObjectPool.Instance.PrePoolCharactersForPlayer(clientId);
            }

        }
        if (IsOwner)
        {
            GameManager.Instance.thisPlayerBrain = this;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

    }
    private void Start()
    {
        if (IsOwner)
            GameManager.Instance.thisPlayerBrain = this;

        if (GameManager.Instance.PlayerDataDict.TryGetValue(AuthenticationService.Instance.PlayerId, out thisPlayerData))
            Debug.Log($"Player {thisPlayerData.player.Data["PlayerName"].Value} assigned to team {thisPlayerData.team}");
    }

    public void SpawnPlayer(GridTile gridTile)
    {
        if (!IsOwner || gridTile == null) return;

        int characterId = GameManager.Instance.SelectedCharacterData.id;
        Vector3 centerPosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        Quaternion rotation = thisPlayerData.IsInTeamA ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
        Vector2Int gridPosition = gridTile.gridPosition;  //서버에서 사용할 위치 값 넘기기

        // 서버에 생성 요청 (소유권을 클라이언트로 설정)
        SpawnPlayerServerRpc(centerPosition,rotation,gridPosition, characterId, OwnerClientId);
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(Vector3 centerPosition, Quaternion rotation, Vector2Int gridPosition, int characterId, ulong ownerClientId)
    {
        Debug.Log("This is Server");
        var character = ObjectPool.Instance.GetCharacterFromPool(ownerClientId, characterId);
        if (character == null)
        {
            Debug.LogError("캐릭터 풀에서 꺼내기 실패");
            return;
        }

        var netObj = character.GetComponent<NetworkObject>();

        character.transform.position = centerPosition;
        character.transform.rotation = rotation;

        character.Initialize(
            thisPlayerData.team,
            gridPosition
        );

        netObj.SpawnWithOwnership(ownerClientId);
        GridManager.Instance.GetGridTileAtPosition(gridPosition).SetOccupied(character);

        // 클라이언트에 알리기
        SyncGridTileClientRpc(gridPosition, netObj.NetworkObjectId);
        SyncPlayerCharacterClientRpc(netObj.NetworkObjectId, characterId, gridPosition);
    }



    [ClientRpc]
    private void SyncGridTileClientRpc(Vector2Int gridPosition, ulong networkObjectId)
    {
        if (IsHost) return; // 서버에서는 실행하지 않음

        GridTile gridTile = GridManager.Instance.GetGridTileAtPosition(gridPosition);
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject obj))
        {
            PlayerCharacter playerCharacter = obj.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                gridTile.SetOccupied(playerCharacter);
                Debug.Log($"[Client] GridTile 동기화 완료: {gridPosition}");
            }
        }
    }


    [ClientRpc]
    private void SyncPlayerCharacterClientRpc(ulong objectId, int characterId, Vector2Int gridPosition)
    {
        if (IsHost) return;

        StartCoroutine(WaitForNetworkObject(objectId, characterId, gridPosition));
    }

    private IEnumerator WaitForNetworkObject(ulong objectId, int characterId, Vector2Int gridPosition)
    {
        Debug.Log("[Client] SyncPlayerCharacterClientRpc 진입");

        float timeout = 3f;
        while (timeout > 0)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out NetworkObject obj))
            {
                PlayerCharacter playerCharacter = obj.GetComponent<PlayerCharacter>();
                if (playerCharacter != null)
                {
                    playerCharacter.Initialize(
                        thisPlayerData.team,
                        gridPosition
                    );
                    GameManager.Instance.SelectedGridTile.SetOccupied(playerCharacter);
                    Debug.Log($"[Client] 플레이어 동기화 완료 - Character ID: {characterId}");
                    yield break;
                }
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        Debug.LogError("[Client] 네트워크 오브젝트를 찾을 수 없습니다!");
    }

    [ServerRpc]
    public void UpdateReadyStateServerRpc(string playerId, bool isReady)
    {
        if (GameManager.Instance.PlayerDataDict.TryGetValue(playerId, out var playerData))
        {
            playerData.SetReady(isReady);
            Debug.Log($"Player {playerId} ready state updated to: {isReady}");
            UpdateReadyStateClientRpc(playerId, isReady);
        }
    }

    [ClientRpc]
    private void UpdateReadyStateClientRpc(string playerId, bool isReady)
    {
        if (IsHost) return;

        if (GameManager.Instance.PlayerDataDict.TryGetValue(playerId, out var playerData))
        {
            playerData.SetReady(isReady);
            Debug.Log($"Player {playerId} ready state synced to: {isReady}");
        }
    }

    [Command]
    public void ShowTeam()
    {
        Debug.Log(thisPlayerData.team.ToString());
    }



}