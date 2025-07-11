using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardQueue : MonoBehaviour
{
    public List<Card> cards;
    public List<CardList> cardLists;
    [HideInInspector] public List<Vector3> cardPos;
    public Vector3 spacing;
    public Transform posSpawnGroup;

    public Transform transParent;
    public Transform handPointTutPos;

    private void Start()
    {
    }

    public void SpawnCardList()
    {
        foreach (var card in cardLists)
        {
            card.SetCard(this);
        }
        OnSetPositionOnQueue();
    }


    private void OnSetPositionOnQueue()
    {
        Vector3 posOnQueue = Vector3.zero;
        foreach (var card in cards)
        {
            card.transform.localPosition = posOnQueue;
            cardPos.Add(posOnQueue);
            posOnQueue += spacing;
        }
    }

    public void SetCardPosition()
    {
        Vector3 posOnQueue = Vector3.zero;
        {
            foreach(var card in cards)
            {
                cardPos.Add(posOnQueue);
                posOnQueue += spacing;
            }
        }
    }


    public void AllCardOnQueueMove()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.DOLocalMove(cardPos[i], 0.2f);
            if(i == 0)
            {
                if (cards[i].cardList.HaveMechanic)
                {
                    CheckForHidden(cards[i].cardList);
                    CheckForChain(cards[i].cardList);
                }
            }
        }
    }

    private void CheckForHidden(CardList cardlist)
    {
        if(cardlist.mechanicType == CardMechanic.HiddenColor)
        {
            cardlist.currentMechanic.RemoveMechanic();
        }
    }

    private void CheckForChain(CardList cardList)
    {
        if(cardList.mechanicType == CardMechanic.Chain)
        {
            cardList.currentMechanic.CheckRemoveRule();
        }
    }
}
