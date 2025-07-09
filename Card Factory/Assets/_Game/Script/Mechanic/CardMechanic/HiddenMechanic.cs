using System.Drawing;
using UnityEngine;

public class HiddenMechanic : BaseCardMechanics
{
    public override void CheckRemoveRule()
    {
        if(cardOwner.queue.cardLists.IndexOf(cardOwner) <= 0)
        {
            cardOwner.currentMechanic.RemoveMechanic();
        }
    }

    public override void UpdateVisual()
    {
        foreach (var card in cardOwner.cards)
        {
            foreach (var rend in card.rend)
            {
                Material newMat = card.objectColor.GetColor(CardColor.Hidden); // lấy material mới từ objectColor
                rend.material = newMat; // GÁN lại vào renderer để áp dụng
            }
        }
    }

    public override void RemoveMechanic()
    {
        base.RemoveMechanic();
        foreach (var card in cardOwner.cards)
        {
            foreach (var rend in card.rend)
            {
                Material newMat = card.objectColor.GetColor(card.color); // lấy material mới từ objectColor
                rend.material = newMat; // GÁN lại vào renderer để áp dụng
            }
        }
    }

    public override void SetMechanic()
    {
        mechanicType = CardMechanic.HiddenColor;
        cardOwner = gameObject.GetComponent<CardList>();
        cardOwner.currentMechanic = this;
        UpdateVisual();
    }
}
