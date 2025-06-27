using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int currentCoin;
    public List<BoosterData> boosterList;
    public int currentLevel;
    public long lastHeartRefillTimestamp;
    public int currentHeart;
    public bool isFirstTime;
    public bool isCompleteAllTut;
    public int currentTutIndex;
    public List<int> tutorialComplete;
    public int currentFeatureId;

    public GameData()
    {
    }

    public void InitNewGame()
    {
        currentCoin = 0;
        currentLevel = 1;
        currentHeart = 5;
        isFirstTime = true;
        isCompleteAllTut = true;
        currentTutIndex = 1;
        boosterList = new List<BoosterData>()
        {
            new BoosterData(BoosterType.AddConvey,0),
            new BoosterData(BoosterType.Swap,0),
            new BoosterData(BoosterType.RemovePack,0),
        };
        tutorialComplete = new List<int>();
        currentFeatureId = 1;
    }

    public void SetBoosterCount(BoosterType id,int boosterCount)
    {
        foreach (var booster in boosterList)
        {
            if(booster.boosterType == id)
            {
                booster.count = boosterCount;
                return;
            }
        }
    }

    public int GetBoosterData(BoosterType id)
    {
        foreach (var booster in boosterList)
        {
            if (booster.boosterType == id)
            {
                return booster.count;
            }
        }
        return 0;
    }

    public void AddTutorialComplete(int index)
    {
        tutorialComplete.Add(index);
    }
}

[System.Serializable]
public class BoosterData
{
    public BoosterType boosterType;
    public int count;

    public BoosterData(BoosterType type,int boostercount)
    {
        boosterType = type;
        count = boostercount;
    }
}