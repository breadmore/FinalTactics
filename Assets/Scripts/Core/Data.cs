using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class PlayerData
{
    public string PlayerId;   // �÷��̾� ���� ID
    public int JoinOrder;     // ���� ����
    public int Experience;    // ����ġ

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
    public string LobbyName;  // �κ� �̸�
    public string LobbyAddress; // �κ� �ּ� (IP, Relay Code ��)

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