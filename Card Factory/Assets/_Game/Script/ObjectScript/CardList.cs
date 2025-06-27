using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardList : MonoBehaviour
{
    public List<Card> cards;
    public CardColor listColor;
    public CardQueue queue;
    public int numberCard;

    public GameObject cardPrefab;
    public CardMechanic mechanicType;
    public BaseCardMechanics currentMechanic;
    public bool HaveMechanic => mechanicType != CardMechanic.None;

    private void Start()
    {
    }

    public void SetCard(CardQueue currentQueue)
    {
        for (int i = 0; i < numberCard; i++)
        {
            GameManager.Ins.poolManager.cardPool.Prefab = cardPrefab;
            GameObject card = GameManager.Ins.poolManager.cardPool.Spawn(this.transform.position,cardPrefab.transform.rotation,this.transform);
            Card cardObj = card.GetComponent<Card>();
            cards.Add(cardObj);
            cardObj.color = listColor;
            queue = currentQueue;
            cardObj.cardList = this;
            cardObj.cardQueue = queue;
            cardObj.cardQueue.cards.Add(cardObj);
            cardObj.ChangeCardColor();
        }
        if (mechanicType == CardMechanic.HiddenColor)
        {
            currentMechanic = this.AddComponent<HiddenMechanic>();
            currentMechanic.SetMechanic();
        }
    }

    public void CheckCard()
    {
        if(cards.Count <= 0)
        {
            queue.cardLists.Remove(this);
            GameManager.Ins.poolManager.cardGroupPool.Despawn(this.gameObject);
        }
    }
}
