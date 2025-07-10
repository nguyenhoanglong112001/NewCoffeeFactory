using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialInGameManager : Singleton<TutorialInGameManager>
{

    public int currentTutIndex;
    public Tutorial[] tutorials;
    public List<int> completeList;

    public bool isOnTutorial;
    public bool IsAllTutorialComplete()
    {
        return tutorials.Length == completeList.Count;
    }

    public void SaveStage()
    {
        GameDataManager.Ins.gamedata.currentTutIndex = currentTutIndex;
        GameDataManager.Ins.gamedata.isCompleteAllTut = IsAllTutorialComplete();
        GameDataManager.Ins.SaveData();
    }

    public void StartTut(Action callBack = null)
    {
        isOnTutorial = true;
        tutorials[currentTutIndex - 1].gameObject.SetActive(true);
        tutorials[currentTutIndex - 1].currentTutStageIndex = 1;
        tutorials[currentTutIndex - 1].StartTutStage();
        callBack?.Invoke();
    }

    public Tutorial GetCurrentTut()
    {
        if(IsAllTutorialComplete())
        {
            return null;
        }
        return tutorials[currentTutIndex -1];
    }

    public void OnEndStage(Action callback = null)
    {
        TutorialStage stage = tutorials[currentTutIndex - 1].GetCurrentStage();
        stage.OnEndStage(callback);
    }

    public void OnActiveTutorial(Vector3 handPoint)
    {
        Tutorial tutorial = GetCurrentTut();
        if (tutorial != null)
        {
            if (!tutorial.isCompleteTut)
            {
                TutorialStage currentStage = tutorial.GetCurrentStage();
                currentStage.SetHandPointPos(handPoint);
                tutorial.StartTutStage(() =>
                {
                    if(currentTutIndex == 1)
                    {
                        foreach (var card in LevelManager.Ins.queues[1].cards)
                        {
                            card.canPress = true;
                        }
                    }
                });
            }
        }
    }

    public (int,int) GetCurrentTutStage()
    {
        Tutorial currentTut = tutorials[currentTutIndex - 1];
        return (currentTutIndex,currentTut.currentTutStageIndex);
    }
}
