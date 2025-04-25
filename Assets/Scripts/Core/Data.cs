using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
public class PlayerData
{
    public Player player { get; private set; }
    public TeamName team { get; private set; }
    public bool isReady { get; private set; } = false;
    public string playerName => GetPlayerName();

    // 더 직관적인 팀 확인 프로퍼티
    public bool IsRedTeam => team == TeamName.Red;
    public bool IsBlueTeam => team == TeamName.Blue;

    public PlayerData(Player player)
    {
        this.player = player;
        ParseTeamFromLobbyData();
        ValidateTeamAssignment();
    }
    private void ParseTeamFromLobbyData()
    {
        try
        {
            if (player.Data.TryGetValue("PlayerTeam", out var teamData))
            {
                // 더 명확한 팀 구분 방식 (1: Red, 2: Blue)
                team = teamData.Value switch
                {
                    "1" => TeamName.Red,
                    "2" => TeamName.Blue,
                    _ => TeamName.None
                };
            }
            else
            {
                Debug.LogWarning($"Team data not found for player: {player.Id}");
                team = TeamName.None;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse team data: {e.Message}");
            team = TeamName.None;
        }
    }

    private void ValidateTeamAssignment()
    {
        if (team == TeamName.None)
        {
            // 기본 팀 할당 (또는 랜덤 할당)
            team = UnityEngine.Random.Range(0, 2) == 0 ? TeamName.Red : TeamName.Blue;
            Debug.Log($"Assigned random team {team} to player: {player.Id}");
        }
    }

    private string GetPlayerName()
    {
        return player.Data.TryGetValue("PlayerName", out var nameData)
               ? nameData.Value
               : "Unknown";
    }

    public void SetReady(bool state)
    {
        isReady = state;
        Debug.Log($"Player {playerName} (Team: {team}) ready state: {state}");
    }

    // Quantum Console용 디버그 메서드
    [Command]
    public void DebugPlayerInfo()
    {
        Debug.Log($"Player: {playerName}\n" +
                 $"ID: {player.Id}\n" +
                 $"Team: {team}\n" +
                 $"Ready: {isReady}");
    }
}

public class PlayerTeam
{
    public TeamName Name { get; private set; }
    public List<PlayerData> Players { get; private set; } = new();
    public int score = 0;

    public PlayerTeam(TeamName name) => Name = name;
}

[Serializable]
public class LobbyData
{
    public string LobbyName;  // 로비 이름
    public string LobbyAddress; // 로비 주소 (IP, Relay Code 등)

    public LobbyData(string name, string address)
    {
        LobbyName = name;
        LobbyAddress = address;
    }
}

[Serializable]
public class CharacterDataList
{
    public CharacterData[] CharacterData;
}

[Serializable]
public class CharacterData
{
    //public Sprite characterSprite;
    public int id;
    public CharacterStat characterStat;

    public CharacterData(int id, CharacterStat characterStat)
    {
        this.id = id;
        this.characterStat = characterStat;
    }
}



[Serializable]
public class CharacterStat
{
    public int speed;
    public int pass;
    public int shoot;
    public int dribble;
    public int tackle;
    public int stamina;
    public int type;
    public CharacterStat(int speed, int pass, int shoot, int dribble, int tackle, int stamina, int type)
    {
        this.speed = speed;
        this.pass = pass;
        this.shoot = shoot;
        this.dribble = dribble;
        this.tackle = tackle;
        this.stamina = stamina;
        this.type = type;
    }
}