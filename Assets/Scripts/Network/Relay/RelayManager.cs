using QFSW.QC;
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : DontDestroySingleton<RelayManager>
{
    private string currentRelayCode = ""; // 현재 Relay 코드 저장

    public async Task CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            currentRelayCode = joinCode;

            await LobbyManager.Instance.UpdateLobbyData("RelayCode", joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to create relay: {e}");
        }
    }

    public async Task JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            currentRelayCode = joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Failed to join relay: {e}");
        }
    }

public async Task ConnectRelay()
{
    bool isHost = LobbyManager.Instance.GetHostLobby() != null;
    if (isHost)
    {
        // Relay 서버 생성 후 joinCode 가져오기
        await CreateRelay();
        Debug.Log("Host Start");
    }
    else
    {
        Debug.Log("Client is waiting for RelayCode...");
        string relayJoinCode = "";

        // relayJoinCode가 변경될 때까지 계속해서 확인
        while (string.IsNullOrEmpty(relayJoinCode))
        {
            Debug.Log("Finding...");
            // 로비 데이터에서 RelayCode를 가져오는 부분을 정확히 업데이트된 데이터를 확인하는 방식으로 수정
            var lobbyData = LobbyManager.Instance.GetJoinedLobby().Data;
            if (lobbyData != null && lobbyData.ContainsKey("RelayCode"))
            {
                relayJoinCode = lobbyData["RelayCode"].Value;
            }

            // relayJoinCode를 확인하고 값이 채워질 때까지 대기
            if (string.IsNullOrEmpty(relayJoinCode))
            {
                await Task.Delay(500); // 값이 없으면 계속 대기
            }
        }

        Debug.Log($"Client joining Relay with code: {relayJoinCode}");
        await JoinRelay(relayJoinCode);
    }
}



    public async Task WaitForRelayConnection()
    {
        // 클라이언트가 모두 연결될 때까지 대기
        while (NetworkManager.Singleton.ConnectedClients.Count < LobbyManager.Instance.GetJoinedLobby().Players.Count)
        {
            Debug.Log("wait other players...");
            //Debug.Log("Currunt Code : " + currentRelayCode);
            //Debug.Log("Server Code : " + LobbyManager.Instance.GetJoinedLobby().Data["RelayCode"].Value);
            await Task.Delay(500); // 500ms마다 확인
        }

        Debug.Log("All players connected to Relay.");
        LoadingManager.Instance.isLoading = false;
    }

    [Command]
    public string GetRelayCode()
    {
        return currentRelayCode;
    }

    [Command]
    public void PrintRelayInfo()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
            Debug.Log($"Relay Code: {currentRelayCode}\nConnected Players: {playerCount}");
        }
        else
        {
            Debug.Log("Not connected to a relay.");
        }
    }

    [Command]
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
    [Command]
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    [Command]
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
