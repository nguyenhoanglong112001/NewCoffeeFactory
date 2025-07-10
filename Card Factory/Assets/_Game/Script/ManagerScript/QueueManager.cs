using DG.Tweening;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public List<QueueSlot> avaliableQueues;
    public List<Card> cardInQueue;

    public List<QueueSlot> extendSlot;

    public GameObject extenderObject;
    public Transform handPointTutPos;
    public Transform handPointAddQueueTut;

    public bool isFull => GetCurrentCardQueue() >= avaliableQueues.Count;

    private void Start()
    {
        OnCheckExtentCondition();
        for (int i = 0; i < avaliableQueues.Count; i++)
        {
            cardInQueue.Add(null);
        }
    }

    public bool isFullQueue(List<Card> cardEnter)
    {
        if(GetCurrentCardQueue() + cardEnter.Count > avaliableQueues.Count)
        {
            return true;
        }
        return false;
    }

    public int GetCurrentCardQueue()
    {
        int cardInQueueCount = 0;
        foreach (Card card in cardInQueue)
        {
            if (card != null)
            {
                cardInQueueCount++;
            }
            else
            {
                break;
            }
        }
        return cardInQueueCount;
    }

    public void ReOrderQueue(Action<int, Card> onComplete = null)
    {
        for (int i = 0; i < cardInQueue.Count; i++)
        {
            int index = i;
            Card card = cardInQueue[index];
            if (card == null) continue;
            card.currentQueueSlot?.SetCardOnQueue(null);
            card.currentQueueSlot = avaliableQueues[index];
            avaliableQueues[index].SetCardOnQueue(card);
            card.transform.DOJump(
                    avaliableQueues[index].transform.position,
                    2f,
                    1,
                    0.6f
                )
                .OnComplete(() =>
                {
                    onComplete?.Invoke(index, card);
                });
        }
    }



    public void CheckColorEnterQueue(List<Card> cardEnter)
    {
        if (isFullQueue(cardEnter))
        {
            LevelManager.Ins.OnLevelFail();
            if(!LevelManager.Ins.isReviveWait)
            {
                return;
            }
            else
            {
                StartCoroutine(WaitToRevive());
                return;
            }
        }
        CardEnterQueue(cardEnter);
    }

    public IEnumerator WaitToFullQueue()
    {
        yield return new WaitUntil(() => isFull);
        TutorialInGameManager.Ins.StartTut();
        TutorialStage currentStage = TutorialInGameManager.Ins.GetCurrentTut().GetCurrentStage();
        currentStage.SetHandPointPos(handPointAddQueueTut.position);
    }

    IEnumerator WaitToRevive()
    {
        yield return new WaitUntil(() => LevelManager.Ins.isReviveWait = false);
        foreach(var card in GameManager.Ins.ConveyorManager.cardsMove)
        {
            foreach (var cardRemain in card)
            {
                cardRemain.CheckColorOnEnter(cardRemain);
            }
        }
    }

    private void CardEnterQueue(List<Card> cardEnter)
    {
        List<Card> cards = cardInQueue.ToList();
        List<Card> cardMoveFront = new List<Card>();
        foreach (var card in cards)
        {
            if (card == null) continue;
            if (cardEnter.Count <= 0) return;
            if (card.color == cardEnter[0].color)
            {
                List<Card> cardSame = card.GetListConsecutiveCard(card, cardInQueue);
                int lastIndex = cardInQueue.IndexOf(cardSame.Last());

                for (int i = lastIndex + 1; i < cardInQueue.Count; i++)
                {
                    if (cardInQueue[i] != null)
                    {
                        cardMoveFront.Add(cardInQueue[i]);
                    }
                    else break;
                }
                for (int i = 0; i < cardMoveFront.Count; i++)
                {
                    int index = lastIndex + 1 + i;
                    int indexInQueue = lastIndex + 1 + cardEnter.Count + i;
                    cardInQueue[indexInQueue] = cardMoveFront[i];
                    if (cardInQueue[index] == cardMoveFront[i])
                    {
                        cardInQueue[index] = null;
                        cardInQueue[indexInQueue].currentQueueSlot.SetCardOnQueue(null);
                    }
                    cardInQueue[indexInQueue].currentQueueSlot = null;
                    cardInQueue[indexInQueue].currentQueueSlot = avaliableQueues[indexInQueue];
                    avaliableQueues[indexInQueue].SetCardOnQueue(cardInQueue[indexInQueue]);
                }
                ReOrderQueue();
                break;
            }
        }
    }

    private void OnCheckExtentCondition()
    {
        if(GameManager.Ins.currentLevel < GameManager.Ins.boosterConfig.GetBoosterByType(BoosterType.AddQueueSlot).levelUnlock)
        {
            extenderObject.SetActive(false);
        }
        else
        {
            extenderObject.SetActive(true);
        }
    }

    public int countForTut;
    public void CountForTut(int amount)
    {
        countForTut += amount;
    }
}
