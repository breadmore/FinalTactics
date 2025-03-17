using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    
    private Transform spawnedObjectTransform;

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
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<MyCustomData> customData = new NetworkVariable<MyCustomData>(new MyCustomData
    {
        _int = 56,
        _bool = true
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previouseValue, int newValue)=>{
            Debug.Log(OwnerClientId + "; randomNumber : " + randomNumber.Value);
        };

        customData.OnValueChanged += (MyCustomData previouseValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool +"; " +newValue._message);
        };
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (spawnedObjectPrefab != null)
            {
                spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
                spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {

            spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
            //Destroy(spawnedObjectTransform.gameObject);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            TestServerRPC(new ServerRpcParams());
            //randomNumber.Value = Random.Range(0, 100);
            //customData.Value = new MyCustomData
            //{
            //    _int = 10,
            //    _bool = false,
            //    _message = "Hello Im On!"
            //};
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            TestClientRPC(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
        }
            Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    // Only Server
    [ServerRpc]
    private void TestServerRPC(ServerRpcParams serverRpcParams)
    {
        Debug.Log("TestServerRPC" + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    // Only Client
    [ClientRpc]
    private void TestClientRPC(ClientRpcParams clientRpcParams)
    {
        Debug.Log("TestClientRPC" );
    }
}
