using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Collections.Generic;
using QFSW.QC;

public class TestLobby : MonoBehaviour
{

    private Lobby hostLobby;
    private float heartbeatTimer;
    private string playerName;
    [SerializeField] private string Code;

    private bool isSigningIn = false; // 로그인 상태를 추적할 플래그
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn && !isSigningIn)
        {
            isSigningIn = true;

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                Debug.LogError("Authentication failed: " + e.Message);
            }
            finally
            {
                isSigningIn = false;
            }
        }
        else
        {
            Debug.Log("Already signed in or login is in progress.");
        }

        playerName = "TestName" + UnityEngine.Random.Range(10, 99);
        Debug.Log("Player Name : " + playerName);
    }



    private void Update()
    {
        HandleLobbyHeartbeat();
        
    }


    private void HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    [Command]
    private  async void CreateLobby()
    {
        try
        {
            string lobbyName = "new lobby";
            int maxPlayers = 4;
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public, "CaptureTheFlag") }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            hostLobby = lobby;
            PrintPlayer(hostLobby);
            Debug.Log("Create Lobby ! " + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);
        }catch(LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
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
            foreach (Lobby lobby in response.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }catch (LobbyServiceException e)
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

            Lobby joinLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            Debug.Log("Join Lobby with code " + lobbyCode);
            PrintPlayer(joinLobby);
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
        catch(LobbyServiceException e)
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
    private void PrintPlayer(Lobby lobby)
    {
        Debug.Log("player in Lobby" + lobby.Name + " " + lobby.Data["GameMode"].Value);

        foreach(Player player in lobby.Players)
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
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
}
