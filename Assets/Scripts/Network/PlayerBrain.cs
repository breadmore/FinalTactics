using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerBrain : NetworkBehaviour
{
    [SerializeField] private Transform spawnPlayerPrefab;
    private Player thisPlayer;
    private Transform spawnedPlayerTransform;
    private bool gameTeam = false;

    private struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes _message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref _message);
        }
    }

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<MyCustomData> customData = new NetworkVariable<MyCustomData>(new MyCustomData
    {
        _int = 56,
        _bool = true
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + "; randomNumber : " + randomNumber.Value);
        };

        customData.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue._message);
        };
    }

    private void SetPlayer()
    {
        Player player = LobbyManager.Instance.GetJoinedLobby()?.Players?.Find(p => p.Id == AuthenticationService.Instance.PlayerId);
        if (player != null)
        {
            thisPlayer = player;

            if (player.Data.TryGetValue("PlayerTeam", out var playerTeamData))
            {
                bool.TryParse(playerTeamData.Value, out gameTeam);
            }
            Debug.Log($"Player {thisPlayer.Data["PlayerName"].Value} is " + gameTeam + " Team. ");
        }
        else
        {
            Debug.LogError("Player not found in the lobby.");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        if (GameManager.Instance.selectedGridTile != null)
        {
            Debug.Log("Brain Detect Base Grid");
            SpawnPlayer(GameManager.Instance.selectedGridTile);
            GameManager.Instance.selectedGridTile = null;
        }
    }

    private void Start()
    {
        SetPlayer();
    }

    private void SpawnPlayer(GridTile gridTile)
    {
        Debug.Log("spawn");
        Quaternion rotation = gameTeam ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0);
        Vector3 centerPosition = GridManager.Instance.GetNearestGridCenter(GameManager.Instance.selectedGridTile.transform.position);

        Debug.Log(centerPosition);

        if (IsHost) // Host(����)�� ���
        {
            SpawnPlayerServer(centerPosition, rotation);
        }
        else if (IsOwner) // Ŭ���̾�Ʈ�� ���
        {
            SpawnPlayerServerRpc(centerPosition, rotation);
        }
    }

    private void SpawnPlayerServer(Vector3 position, Quaternion rotation)
    {
        GameObject playerCharacter = Instantiate(spawnPlayerPrefab.gameObject, position, rotation);

        NetworkObject networkObject = playerCharacter.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // ��� Ŭ���̾�Ʈ���� ����ȭ
        SpawnPlayerClientRpc(networkObject.NetworkObjectId, position, rotation);
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(Vector3 position, Quaternion rotation, ServerRpcParams serverRpcParams = default)
    {
        GameObject playerCharacter = Instantiate(spawnPlayerPrefab.gameObject, position, rotation);
        NetworkObject networkObject = playerCharacter.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // ��� Ŭ���̾�Ʈ���� ����ȭ
        SpawnPlayerClientRpc(networkObject.NetworkObjectId, position, rotation);
    }

    [ClientRpc]
    private void SpawnPlayerClientRpc(ulong networkObjectId, Vector3 position, Quaternion rotation, ClientRpcParams clientRpcParams = default)
    {
        if (IsHost) return; // Host�� �̹� �������� ó�������Ƿ� �ߺ� ���� ����

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(networkObjectId))
        {
            return; // �̹� ������ ��� ����
        }

        GameObject playerCharacter = Instantiate(spawnPlayerPrefab.gameObject, position, rotation);
    }
}
