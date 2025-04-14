using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ObjectPool : NetworkSingleton<ObjectPool>
{
    private Dictionary<ulong, Dictionary<int, Queue<PlayerCharacter>>> playerCharacterPools = new();

    public List<PlayerCharacter> activePlayerCharacter = new List<PlayerCharacter>();
    private GameObject activeBallObject;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }
    public override void OnDestroy()
    {
        if (IsServer && GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.WaitingForReset)
        {
            ReturnAllObjectsToPool();
        }
    }

    private void ReturnAllObjectsToPool()
    {
        foreach (PlayerCharacter player in activePlayerCharacter.ToList())
        {
            ReturnCharacterToPool(player.OwnerClientId, player);
        }
        // BallManager가 있을 경우:
        //BallManager.Instance.ResetBallPosition(); // 예: 위치 초기화 및 비활성화
    }

    public void PrePoolCharactersForPlayer(ulong clientId)
    {
        if (!IsServer) return;

        List<CharacterData> characterDataList = LoadDataManager.Instance.characterDataReader.GetCharacterDataList();
        Dictionary<int, Queue<PlayerCharacter>> characterQueues = new();

        foreach (var characterData in characterDataList)
        {
            int id = characterData.id;
            if (!characterQueues.ContainsKey(id))
                characterQueues[id] = new Queue<PlayerCharacter>();

                var prefab = LoadDataManager.Instance.characterPrefabManager.GetPlayerCharacterPrefab();
                if (prefab == null)
                {
                    Debug.LogError($"[ObjectPool] ID {id}에 대한 프리팹이 없습니다.");
                    continue;
                }

                var obj = Instantiate(prefab,transform).GetComponent<PlayerCharacter>();
                obj.InitData(characterData);
                obj.gameObject.SetActive(false);

                characterQueues[id].Enqueue(obj);
            }
        

        playerCharacterPools[clientId] = characterQueues;
    }

    public PlayerCharacter GetCharacterFromPool(ulong clientId, int characterId)
    {
        if (playerCharacterPools.TryGetValue(clientId, out var dict) &&
            dict.TryGetValue(characterId, out var queue) &&
            queue.Count > 0)
        {
            var instance = queue.Dequeue();
            instance.gameObject.SetActive(true);


            return instance;
        }

        Debug.LogError($"[ObjectPool] 플레이어 {clientId}용 캐릭터 {characterId}이 풀에 없음");

        return null;
    }

    public void ReturnCharacterToPool(ulong clientId, PlayerCharacter character)
    {
        if (playerCharacterPools.TryGetValue(clientId, out var dict) &&
            dict.TryGetValue(character.CharacterData.id, out var queue))
        {
            Debug.Log("Return!!");

            activePlayerCharacter.Remove(character);
            character.gameObject.SetActive(false);
            queue.Enqueue(character);

            // 클라이언트에게도 알림
            ReturnCharacterToPoolClientRpc(character.NetworkObjectId);
        }
        else
        {
            Debug.LogWarning($"[ObjectPool] 풀에 존재하지 않는 캐릭터를 반환하려 했습니다. ID: {character.CharacterData.id}");
            Destroy(character.gameObject);
        }
    }

    [ClientRpc]
    private void ReturnCharacterToPoolClientRpc(ulong networkObjectId)
    {
        if (IsServer) return; // 서버는 이미 반환했으니 무시

        NetworkObject netObj;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out netObj))
        {
            var character = netObj.GetComponent<PlayerCharacter>();
            if (character != null)
            {
                character.gameObject.SetActive(false);
            }
        }
    }
    public void RegisterActiveCharacter(PlayerCharacter character)
    {
        if (!activePlayerCharacter.Contains(character))
        {
            Debug.Log("Check");
            activePlayerCharacter.Add(character);
        }
        else
        {
            Debug.Log("Exist");
        }
    }

    public void UnregisterActiveCharacter(PlayerCharacter character)
    {
        activePlayerCharacter.Remove(character);
    }
}
