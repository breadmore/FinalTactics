using GoogleSheetsToUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Reader", menuName = "Data Reader/CharacterDataReader", order = int.MaxValue)]
public class CharacterDataReader : DataReaderBase
{
    [Header("스프레드시트에서 읽혀져 직렬화 된 오브젝트")]
    [SerializeField] public List<CharacterData> DataList = new List<CharacterData>();

    public void UpdateStats(List<GSTU_Cell> list, int characterID)
    {
        int id = 0;
        string name = "";
        int speed = 0, pass = 0, shoot = 0, dribble = 0, tackle = 0, stamina = 0, type = 0;

        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].columnId)
            {
                case "id":
                    id = int.Parse(list[i].value);
                    break;
                case "name":
                    name = list[i].value;
                    break;
                case "speed":
                    speed = int.Parse(list[i].value);
                    break;
                case "pass":
                    pass = int.Parse(list[i].value);
                    break;
                case "shoot":
                    shoot = int.Parse(list[i].value);
                    break;
                case "dribble":
                    dribble = int.Parse(list[i].value);
                    break;
                case "tackle":
                    tackle = int.Parse(list[i].value);
                    break;
                case "stamina":
                    stamina = int.Parse(list[i].value);
                    break;
                case "type":
                    type = int.Parse(list[i].value);
                    break;
            }
        }

        CharacterStat characterStat = new CharacterStat(speed, pass, shoot, dribble, tackle, stamina, type);
        DataList.Add(new CharacterData(id, characterStat));
    }

    public CharacterData GetCharacterDataById(int characterID)
    {
        return DataList.Find(data => data.id == characterID);
    }
}
