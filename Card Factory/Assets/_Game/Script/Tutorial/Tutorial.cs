using System;
using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public int currentTutStageIndex = 1;

    public TutorialStage[] tuttorialStage;

    public bool isCompleteTut => currentTutStageIndex > tuttorialStage.Length;

    public void StartTutStage(Action callback = null)
    {
        Debug.Log("Start tutrial stage + " + currentTutStageIndex);
        foreach (var stage in tuttorialStage)
        {
            if (stage.tutorialStage == currentTutStageIndex)
            {
                if (stage.popUpIntruction != null)
                {
                    stage.SetIntructionText();
                }
                stage.gameObject.SetActive(true);
            }
            else
            {
                stage.gameObject.SetActive(false);
            }
        }
        callback?.Invoke();
    }

    public void NextStage()
    {
        if(!isCompleteTut)
        {
            currentTutStageIndex += 1;
            Debug.Log("Next stage + " + currentTutStageIndex);
            if (isCompleteTut)
            {
                OnCompleteTut();
            }
        }
        else
        {
            OnCompleteTut();
        }
    }

    public bool CheckCurrentStage(int stageCheck)
    {
        return currentTutStageIndex == stageCheck;
    }

    public TutorialStage GetCurrentStage()
    {
        return tuttorialStage[currentTutStageIndex -1 ];
    }

    public void OnCompleteTut()
    {
        Debug.Log("Complete Tutorial ");
        TutorialInGameManager.Ins.isOnTutorial = false;
        TutorialInGameManager.Ins.currentTutIndex++;
        StartCoroutine(WaitToSaveTut());
    }
    IEnumerator WaitToSaveTut()
    {
        yield return new WaitUntil(() => LevelManager.Ins.isLevelComplete);
        if (isCompleteTut)
        {
            GameDataManager.Ins.gamedata.AddTutorialComplete(TutorialInGameManager.Ins.currentTutIndex);
        }
        this.gameObject.SetActive(false);
        GameDataManager.Ins.gamedata.isFirstTime = GameManager.Ins.isFirstTime;
        TutorialInGameManager.Ins.SaveStage();
    }
}
