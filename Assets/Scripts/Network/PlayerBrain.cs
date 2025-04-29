using Cysharp.Threading.Tasks;
using QFSW.QC;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerBrain : NetworkBehaviour
{
    private PlayerData thisPlayerData;
    private bool isInitialized = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            GameManager.Instance.thisPlayerBrain = this;
            InitializePlayer();
        }
    }

    private async void InitializePlayer()
    {
        if (isInitialized) return;

        // PlayerDataDict가 비어있으면 로드 시도
        if (GameManager.Instance.PlayerDataDict.Count == 0)
        {
            await UniTask.WaitUntil(() => GameManager.Instance.PlayerDataDict.Count > 0);
        }

        string playerId = AuthenticationService.Instance.PlayerId;
        Debug.Log($"Trying to initialize player: {playerId}");

        if (GameManager.Instance.PlayerDataDict.TryGetValue(playerId, out thisPlayerData))
        {
            Debug.Log($"Player initialized: {thisPlayerData.player.Data["PlayerName"].Value}, Team: {thisPlayerData.team}");
            CameraManager.Instance.SetInitialCameraPosition(thisPlayerData.team);
            isInitialized = true;
        }
        else
        {
            Debug.LogError($"Player data not found for ID: {playerId}");
            Debug.Log($"Available IDs: {string.Join(", ", GameManager.Instance.PlayerDataDict.Keys)}");
        }
    }

    public void SpawnPlayer(GridTile gridTile)
    {
        if (!IsOwner || gridTile == null || !isInitialized) return;


        int characterId = GameManager.Instance.SelectedCharacterData.id;
        Vector3 centerPosition = GridManager.Instance.GetNearestGridCenter(gridTile.transform.position);
        Quaternion rotation = thisPlayerData.IsRedTeam ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
        Vector2Int gridPos = gridTile.gridPosition;
        string nickname = AuthenticationService.Instance.PlayerName;

        Debug.Log("Spawn Id : "+characterId);

        SpawnPlayerServerRpc(
            centerPosition,
            rotation,
            gridPos,
            characterId,
            nickname,
            AuthenticationService.Instance.PlayerId
        );
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(
        Vector3 position,
        Quaternion rotation,
        Vector2Int gridPosition,
        int characterId,
        string nickname,
        string playerId,
        ServerRpcParams rpcParams = default)
    {

        ulong requesterClientId = rpcParams.Receive.SenderClientId;

        // 풀에서 캐릭터 가져오기
        var netObj = PlayerCharacterNetworkPool.Instance.GetCharacter(position, rotation);
        if (netObj == null)
        {
            Debug.LogError("Failed to get character from pool");
            return;
        }

        var character = netObj.GetComponent<PlayerCharacter>();
        if (character == null)
        {
            Debug.LogError("PlayerCharacter component missing");
            return;
        }

        // 스폰 및 초기화
        netObj.SpawnWithOwnership(requesterClientId);
        //netObj.SpawnWithOwnership(requesterClientId);
        Debug.Assert(character.gameObject.activeSelf, "Character not active!");

        // 플레이어 데이터 가져오기
        if (!GameManager.Instance.PlayerDataDict.TryGetValue(playerId, out var playerData))
        {
            Debug.LogError($"Player data not found for ID: {playerId}");
            return;
        }

        // 닉네임 설정
        //SetPlayerNicknameClientRpc(netObj.NetworkObjectId, nickname, requesterClientId);

        // 캐릭터 초기화
        character.Initialize(characterId, nickname,playerData.team, gridPosition);
    }

    [ClientRpc]
    private void SetPlayerNicknameClientRpc(ulong networkObjectId, string nickname, ulong ownerClientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
            networkObjectId, out var netObj))
        {
            var character = netObj.GetComponent<PlayerCharacter>();
            if (character != null && netObj.OwnerClientId == ownerClientId)
            {
                character.PlayerNickname.Value = nickname;
            }
        }
    }

    [ServerRpc]
    public void UpdateReadyStateServerRpc(string playerId, bool isReady)
    {
        if (GameManager.Instance.PlayerDataDict.TryGetValue(playerId, out var playerData))
        {
            playerData.SetReady(isReady);
            UpdateReadyStateClientRpc(playerId, isReady);
        }
    }

    [ClientRpc]
    private void UpdateReadyStateClientRpc(string playerId, bool isReady)
    {
        if (!IsHost && GameManager.Instance.PlayerDataDict.TryGetValue(playerId, out var playerData))
        {
            playerData.SetReady(isReady);
        }
    }

    public TeamName GetMyTeam()
    {
        return thisPlayerData.team;
    }
}