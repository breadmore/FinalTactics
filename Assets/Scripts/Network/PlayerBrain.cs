using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

public class PlayerBrain : NetworkBehaviour
{
    private PlayerData thisPlayerData;

    public override void OnNetworkSpawn()
    {
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
        if (GameManager.Instance.PlayerDataDict.TryGetValue(AuthenticationService.Instance.PlayerId, out thisPlayerData))
        {
            Debug.Log($"Player {thisPlayerData.player.Data["PlayerName"].Value} assigned to team {thisPlayerData.team}");

            CameraManager.Instance.SetInitialCameraPosition(thisPlayerData.team);
        }
        
    
    }

    public void SpawnPlayer(GridTile gridTile)
    {
        if (!IsOwner || gridTile == null) return;

        int characterId = GameManager.Instance.SelectedCharacterData.id;
        Vector3 centerPosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        Quaternion rotation = thisPlayerData.IsInTeamA ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
        Vector2Int gridPosition = gridTile.gridPosition;

        SpawnPlayerServerRpc(centerPosition, rotation, gridPosition, characterId, AuthenticationService.Instance.PlayerId);
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(Vector3 position, Quaternion rotation, Vector2Int gridPosition, int characterId, string playerId, ServerRpcParams rpcParams = default)
    {
        Debug.Log("Select Id : " + characterId);
        ulong requesterClientId = rpcParams.Receive.SenderClientId;

        // 풀에서 꺼내기
        var netObj = PlayerCharacterNetworkPool.Instance.GetCharacter(position, rotation);
        PlayerCharacterNetworkPool.Instance.activeCharacters.Add(netObj);
        if (netObj == null)
        {
            Debug.LogError("캐릭터 풀에서 꺼내기 실패");
            return;
        }

        var character = netObj.GetComponent<PlayerCharacter>();
        if (character == null)
        {
            Debug.LogError("네트워크 오브젝트에 PlayerCharacter 컴포넌트가 없습니다.");
            return;
        }

        // 초기화

        // 스폰 및 소유권 변경
        netObj.Spawn(true);
        netObj.ChangeOwnership(requesterClientId);

        var playerData = GameManager.Instance.PlayerDataDict[playerId];
        character.Initialize(characterId,playerData.team, gridPosition);
        // 그리드 설정
        GridTile tile = GridManager.Instance.GetGridTileAtPosition(gridPosition);
        if (tile != null)
        {
            tile.SetOccupied(character);
        }

        // 클라이언트와 동기화
        SyncGridTileClientRpc(gridPosition, netObj.NetworkObjectId);
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

    public TeamName GetMyTeam()
    {
        return thisPlayerData.team;
    }

    [Command]
    public void ShowTeam()
    {
        Debug.Log(thisPlayerData.team.ToString());
    }

   
}
