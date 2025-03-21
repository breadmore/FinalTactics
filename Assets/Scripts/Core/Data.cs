using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PlayerData
{
    public string PlayerId;   // 플레이어 고유 ID
    public int JoinOrder;     // 입장 순서
    public int Experience;    // 경험치

    public PlayerData(string playerId, int joinOrder, int experience)
    {
        PlayerId = playerId;
        JoinOrder = joinOrder;
        Experience = experience;
    }
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
    public Sprite characterSprite;
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

    public CharacterStat(int speed, int pass, int shoot, int dribble, int tackle, int stamina)
    {
        this.speed = speed;
        this.pass = pass;
        this.shoot = shoot;
        this.dribble = dribble;
        this.tackle = tackle;
        this.stamina = stamina;
    }
}