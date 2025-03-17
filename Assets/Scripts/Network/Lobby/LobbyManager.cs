using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using QFSW.QC;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LobbyManager : DontDestroySingleton<LobbyManager>
{
    public static event Action OnLobbyListUpdate;
    public LobbyListUI lobbyListUI;
    public PlayerListUI playerListUI;

    private Lobby hostLobby;
    private Lobby joinedLobby;

    private QueryResponse queryResponse;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float lobbyListUpdateTimer;

    private string playerName;
    private bool playerReady;
    private bool playerTeam;

    private void OnEnable()
    {
        AuthenticateUI.OnAuthenticationSuccess += OnAuthenticationSuccess; // 이벤트 구독
        hostLobby = null;
        joinedLobby = null;
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
        playerName = AuthenticationService.Instance.PlayerName;
        OnLobbyListUpdate?.Invoke();
    }


    private void Update()
    {
        if (LoadingManager.Instance.isLoading)
        {
            HandleLobbyPollForUpdates();
        }
        if (SceneManager.GetActiveScene().name == "Intro")
        {
            HandleLobbyHeartbeat();
            HandleLobbyPollForUpdates();
            LobbyListUpdates();
        }
        else
        {
            return;
        }

    }

    private void LobbyListUpdates()
    {
        if (joinedLobby == null)
        {
            lobbyListUpdateTimer -= Time.deltaTime;
            if (lobbyListUpdateTimer < 0f)
            {
                float lobbyListUpdateTimerMax = 5f;
                lobbyListUpdateTimer = lobbyListUpdateTimerMax;

                OnLobbyListUpdate?.Invoke();
            }
        }
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

                try
                {
                    Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = updatedLobby;

                    playerListUI.UpdatePlayerListInLobby(joinedLobby);

                    if (LoadingManager.Instance.isLoading)
                    {
                        return;
                    }
                    if (joinedLobby.Data.ContainsKey("GameStarted") && joinedLobby.Data["GameStarted"].Value == "true")
                    {
                        Debug.Log("Game has started! Loading Main scene...");
                        await GetLoadSceneType();
                        await LoadingManager.Instance.DecideNextScene();

                        //await ConnectRelay();

                    }
                }
                catch (LobbyServiceException e)
                {
                    if (e.Reason == LobbyExceptionReason.RateLimited)
                    {
                        Debug.LogWarning("Rate limit exceeded. Adjusting poll interval.");
                        lobbyUpdateTimer = 5.1f;
                    }
                    else
                    {
                        Debug.LogError($"Failed to update lobby: {e}");
                    }
                }
            }
        }
    }



    public async Task CreateLobby(string lobbyName, int maxPlayers, Dictionary<string, DataObject> lobbyData)
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
            
            Debug.Log("joined Lobby Update : create lobby");
            PrintPlayers(hostLobby);
            Debug.Log("Create Lobby ! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async Task ListLobbies()
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

            queryResponse = response;

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            playerReady = false;
            playerTeam = false;
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;

            Debug.Log("joined Lobby Update : by code");
            PrintPlayers(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public async Task JoinLobbyById(string lobbyId)
    {
        try
        {
            playerReady = false;
            playerTeam = false;
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinLobbyByIdOptions);
            joinedLobby = lobby;


            Debug.Log("joined Lobby Update : by id");

        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

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
    public Player GetPlayer()
    {
        Player player = new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            { 
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerName) },
                {"PlayerReady",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerReady.ToString()) },
                {"PlayerTeam",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerTeam.ToString()) },

            }
        };

        return player;
    }
    private void PrintPlayers()
    {
        PrintPlayers(joinedLobby);
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("player in Lobby" + lobby.Name + " " + lobby.Data["GameMode"].Value);

        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value + " " + player.Data["PlayerReady"].Value);
        }
    }

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


            Debug.Log("joined Lobby Update : update mode");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerName) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async Task UpdatePlayerTeam()
    {
        try
        {
            playerTeam = !playerTeam;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { "PlayerTeam", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerTeam.ToString()) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async Task UpdatePlayerReady()
    {
        try
        {
            playerReady = !playerReady;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                { 
                    { "PlayerReady", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,playerReady.ToString()) }
                }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async Task LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

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

    private async void MigrateLobbyHost()
    {
        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                HostId = joinedLobby.Players[1].Id
            });
            joinedLobby = hostLobby;


            Debug.Log("joined Lobby Update : migrate lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

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


    public Lobby GetHostLobby()
    {
        return hostLobby;
    }

    public Lobby GetJoinedLobby()
    {
        return joinedLobby;
    }

    public void SyncJoinLobby(Lobby _lobby)
    {
        _lobby = joinedLobby;
    }
    public QueryResponse GetQueryResponse()
    {
        return queryResponse;
    }

    public async Task UpdateLobbyData(string key, string value)
    {
        if (hostLobby == null) return; // 호스트가 아니면 실행 안 함

        try
        {
            hostLobby = await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { key, new DataObject(DataObject.VisibilityOptions.Public, value) }
            }
            });
            joinedLobby = hostLobby; // 변경 사항을 반영
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to update lobby data: {e}");
        }
    }


    public async Task StartGame()
    {
        if (hostLobby == null)
        {
            Debug.LogWarning("Only the host can start the game!");
            return;
        }

        try
        {
            // Load Type Main으로 설정
            await UpdateLobbyData("LoadingType", "Main");

            await UpdateLobbyData("GameStarted", "true");
            Debug.Log("Game started! All players should load the Main scene.");

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to start game: {e}");
        }
    }

    private async Task GetLoadSceneType()
    {

        string loadSceneType = "";

        while (string.IsNullOrEmpty(loadSceneType))
        {
            loadSceneType = joinedLobby.Data["LoadingType"].Value;
            await Task.Delay(500);
        }

        switch (loadSceneType)
        {
            case "Intro":
                LoadingManager.Instance.SetLoadSceneType(LoadSceneType.Intro);
                break;
            case "Main":
                LoadingManager.Instance.SetLoadSceneType(LoadSceneType.Main);
                break;
            case "Loading":
                LoadingManager.Instance.SetLoadSceneType(LoadSceneType.Loading);
                break;

        }
    }

    /// <summary>
    /// Quantum Console Command
    /// </summary>
    [Command]
    public void ShowHostId()
    {
        Debug.Log(joinedLobby.HostId);
    }
    [Command]
    public void ShowMyId()
    {
        Debug.Log(AuthenticationService.Instance.PlayerId);
    }
    [Command]
    public void PrintRelayCode()
    {
        Debug.Log(joinedLobby.Data["RelayCode"].Value);
    }
}
