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
        NetworkObject character;

        if (pooledCharacters.Count > 0)
        {
            character = pooledCharacters.Dequeue();
        }
        else
        {
            Debug.LogWarning("Character pool exhausted! Instantiating additional object.");
            character = Instantiate(characterPrefab, position, rotation).GetComponent<NetworkObject>();
        }

        character.transform.SetPositionAndRotation(position, rotation);
        character.gameObject.SetActive(true);
        activeCharacters.Add(character);
        return character;
    }

    public void ReturnCharacter(NetworkObject character)
    {
        if (character.IsSpawned)
        {
            character.Despawn();
        }

        character.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); // reset
        character.gameObject.SetActive(false); // this can be heavy if done at scale

        pooledCharacters.Enqueue(character);
    }
    public void ReturnAllCharacters()
    {
        foreach (var character in activeCharacters.ToList())
        {
            if (character != null && character.IsSpawned)
            {
                ReturnCharacter(character);
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