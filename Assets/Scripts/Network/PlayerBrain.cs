using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerBrain : NetworkBehaviour
{
    [SerializeField] private Transform spawnPlayerPrefab;

    private Transform spawnedPlayerTransform;

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
        randomNumber.OnValueChanged += (int previouseValue, int newValue) => {
            Debug.Log(OwnerClientId + "; randomNumber : " + randomNumber.Value);
        };

        customData.OnValueChanged += (MyCustomData previouseValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue._message);
        };
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (GameManager.Instance.selectedGridPosition.HasValue)
        {
            SpawnPlayer(GameManager.Instance.selectedGridPosition.Value);
            GameManager.Instance.selectedGridPosition = null;
        }
    }

    private void SpawnPlayer(Vector3 position)
    {
        if (!IsOwner) return;
        SpawnPlayerServerRpc(position);
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(Vector3 position, ServerRpcParams serverRpcParams = default)
    {
        GameObject playerCharacter = Instantiate(spawnPlayerPrefab.gameObject, position, Quaternion.identity);
        playerCharacter.GetComponent<NetworkObject>().Spawn();
    }
}