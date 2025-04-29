using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerCharacterNetworkPool : NetworkSingleton<PlayerCharacterNetworkPool>
{
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private int maxCharacterCount = 8;

    private Queue<NetworkObject> pooledCharacters = new Queue<NetworkObject>();
    public List<NetworkObject> activeCharacters = new();
    public override void OnNetworkSpawn()
    {
        InitializePool();
        BallManager.Instance.PreSpawnBall();

        NetworkManager.Singleton.PrefabHandler.AddHandler(characterPrefab, new GenericPrefabHandler(this));
    }

    private void InitializePool()
    {
        for (int i = 0; i < maxCharacterCount; i++)
        {
            GameObject obj = Instantiate(characterPrefab, transform);
            var networkObj = obj.GetComponent<NetworkObject>();
            networkObj.gameObject.SetActive(false);
            pooledCharacters.Enqueue(networkObj);
        }
    }

    public NetworkObject GetCharacter(Vector3 position, Quaternion rotation)
    {
        NetworkObject character = null;

        // Ǯ���� ��ȿ�� ������Ʈ ã��
        while (pooledCharacters.Count > 0 && character == null)
        {
            var candidate = pooledCharacters.Dequeue();
            if (!candidate.IsSpawned) // �������� ���� ������Ʈ�� ���
            {
                character = candidate;
            }
        }

        if (character == null)
        {
            Debug.LogWarning("Creating new character instance");
            character = Instantiate(characterPrefab, position, rotation).GetComponent<NetworkObject>();
        }

        character.transform.SetPositionAndRotation(position, rotation);
        character.gameObject.SetActive(true);
        activeCharacters.Add(character);
        return character;
    }

    public void ReturnCharacter(NetworkObject character)
    {
        if (character == null) return;

        if (!activeCharacters.Contains(character)) return;

        activeCharacters.Remove(character);

        ReturnToPoolInternal(character); // ���� Ǯ�� ��ȯ

        if (character.IsSpawned)
        {
            character.Despawn(true); // �� �� ����
        }
    }


    public void ReturnToPoolInternal(NetworkObject character)
    {
        character.transform.SetParent(transform);
        character.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        character.gameObject.SetActive(false);
        pooledCharacters.Enqueue(character);
    }
    public async UniTask ReturnAllCharactersAsync()
    {
        foreach (var character in activeCharacters.ToList())
        {
            if (character != null && character.IsSpawned)
            {
                ReturnCharacter(character);
                await UniTask.Yield(); // ������ �л� ó��
            }
        }
        activeCharacters.Clear();
    }

    private class GenericPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly PlayerCharacterNetworkPool pool;

        public GenericPrefabHandler(PlayerCharacterNetworkPool pool)
        {
            this.pool = pool;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return pool.GetCharacter(position, rotation);
        }

        public void Destroy(NetworkObject networkObject)
        {
            pool.ReturnCharacter(networkObject);
        }
    }
}