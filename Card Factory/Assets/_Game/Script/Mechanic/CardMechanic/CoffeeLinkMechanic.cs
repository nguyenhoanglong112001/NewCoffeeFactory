using System.Collections.Generic;
using UnityEngine;

public class CoffeeLinkMechanic : BaseCardMechanics
{
    public List<CardList> cardLink; 
    public override void CheckRemoveRule()
    {
        if(CheckLink())
        {
            RemoveMechanic();
        }
    }

    private bool CheckLink()
    {
        foreach (var card in cardLink)
        {
            if(card.queue.cardLists.IndexOf(card) > 0)
            {
                return false;
            }
        }
        return true;
    }

    public override void UpdateVisual()
    {
        foreach (var card in cardLink)
        {
            card.currentMechanic = this;
            card.mechanicType = CardMechanic.Chain;
        }
    }

    public override void RemoveMechanic()
    {
        foreach(var card in cardLink)
        {
            cardOwner.currentMechanic = null;
            card.mechanicType = CardMechanic.None;
        }
        Destroy(gameObject);
    }
}
