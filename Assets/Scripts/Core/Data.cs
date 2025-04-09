using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
public class PlayerData
{
    public Player player { get; private set; }
    public TeamName team { get; private set; }
    public bool isReady { get; private set; } = false;

    public bool IsInTeamA => player.Data["PlayerTeam"].Value == "False";

    public PlayerData(Player player)
    {
        this.player = player;
        team = IsInTeamA ? TeamName.TeamA : TeamName.TeamB;
    }

    public void SetReady(bool state) => isReady = state;
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