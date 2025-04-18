using System.Collections.Generic;
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
        if (IsServer)
        {
            PrewarmPool();
            BallManager.Instance.PreSpawnBall();
        }

        NetworkManager.Singleton.PrefabHandler.AddHandler(characterPrefab, new GenericPrefabHandler(this));
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < maxCharacterCount; i++)
        {
            GameObject obj = Instantiate(characterPrefab);
            var networkObj = obj.GetComponent<NetworkObject>();
            networkObj.gameObject.SetActive(false);
            pooledCharacters.Enqueue(networkObj);
        }
    }

    public NetworkObject GetCharacter(Vector3 position, Quaternion rotation)
    {
        if (pooledCharacters.Count > 0)
        {
            var character = pooledCharacters.Dequeue();
            character.transform.SetPositionAndRotation(position, rotation);
            character.gameObject.SetActive(true);
            return character;
        }

        Debug.LogWarning("Character pool exhausted! Instantiating additional object.");
        GameObject obj = Instantiate(characterPrefab, position, rotation);
        return obj.GetComponent<NetworkObject>();
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
    public void ReturnAllCharacter()
    {
        foreach (var character in activeCharacters)
        {
            ReturnCharacter(character);
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
