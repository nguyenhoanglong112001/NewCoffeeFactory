using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Config", menuName = "Config/BoosterConfig")]
public class BoosterConfig : ScriptableObject
{
    public List<Booster> boosters = new List<Booster>();

    public Booster GetBooster(int id)
    {
        foreach (Booster booster in boosters)
        {
            if(booster.id == id)
            {
                return booster;
            }
        }
        return null;
    }
    
    public Booster GetBoosterByType(BoosterType type)
    {
        foreach (Booster booster in boosters)
        {
            if (booster.boosterType == type)
            {
                return booster;
            }
        }
        return null;
    }
}

[System.Serializable]
public class Booster
{
    public int id;
    public string boosterName;
    public string Description;
    public Sprite boosterIcon;
    public string boosterDes;
    public BoosterType boosterType;
    public int boosterCost;
    public int levelUnlock;
}
