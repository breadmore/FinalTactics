using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine.UI;

public class LobbyManager : Singleton<LobbyManager>
{
    [SerializeField] private LobbyListUI lobbyListUI;
    [SerializeField] private PlayerListUI playerListUI;
    private Lobby hostLobby;
    private Lobby joinedLobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;

    private string playerName;


    private void OnEnable()
    {
        AuthenticateUI.OnAuthenticationSuccess += OnAuthenticationSuccess; // 이벤트 구독
    }

    private void OnDisable()
    {
        AuthenticateUI.OnAuthenticationSuccess -= OnAuthenticationSuccess; // 이벤트 구독 해제
    }

    private void OnAuthenticationSuccess()
    {
        Debug.Log("Authentication succeeded. Initializing LobbyManager...");
        InitializeLobbyManager();
    }

    private void InitializeLobbyManager()
    {
        Debug.Log("Lobby Manager Initialized");
        ListLobbies();
    }


    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }


    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }
        }
    }

    [Command]
    public async void CreateLobby(string lobbyName, int maxPlayers, Dictionary<string, DataObject> lobbyData)
    {
        try
        {
            // 로비 생성 옵션 설정
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = lobbyData
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            hostLobby = lobby;
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
            // 플레이어 리스트 UI 출력
            UIManager.Instance.SetState(UIState.Lobby, UIState.JoinedLobby);
            playerListUI.CreateAllPlayerList(joinedLobby);

            Debug.Log("Create Lobby ! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    public async void ListLobbies()
    {
        try
        {
            // 로비 쿼리 옵션 설정
            var lobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };
            var response = await LobbyService.Instance.QueryLobbiesAsync(lobbiesOptions);
            Debug.Log("Lobbies found : " + response.Results.Count);
            
            // 기존 로비 삭제
            lobbyListUI.DestroyAllLobbyList();

            foreach (Lobby lobby in response.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
                lobbyListUI.CreateLobbyListSingleUI(lobby);
             }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            Debug.Log("Join Lobby with code " + lobbyCode);
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }
    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log("Quick Join In Lobby!");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    [Command]
    private Player GetPlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName) }
                    }
        };

        return player;
    }
    [Command]
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("player in Lobby" + lobby.Name + " " + lobby.Data["GameMode"].Value);

        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }


    [Command]
    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                {"GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
            }
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member,playerName) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void KickPlayer(string playerId)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
