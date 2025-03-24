using QFSW.QC;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerBrain : NetworkBehaviour
{
    [SerializeField] private PlayerCharacter spawnPlayerPrefab;
    private PlayerData thisPlayerData;

    private void Update()
    {
        if (!IsOwner) return;

        if (GameManager.Instance.selectedGridTile != null)
        {
            // 지정할 수 없는 타일일 경우
            if (!GameManager.Instance.selectedGridTile.CanPlaceCharacter())
            {
                return;
            }
            SpawnPlayer(GameManager.Instance.selectedGridTile);

            GameManager.Instance.selectedGridTile = null;
        }
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

        Vector3 centerPosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        Quaternion rotation = thisPlayerData.IsInTeamA ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);

        SpawnPlayerServerRpc(centerPosition, rotation);
    }

    public TeamName GetPlayerTeam()
    {
        return thisPlayerData.team;
    }


    [ServerRpc]
    private void SpawnPlayerServerRpc(Vector3 position, Quaternion rotation)
    {
        PlayerCharacter playerCharacter = Instantiate(spawnPlayerPrefab.gameObject, position, rotation).GetComponent<PlayerCharacter>();
        NetworkObject networkObject = playerCharacter.GetComponent<NetworkObject>();
        networkObject.Spawn();

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