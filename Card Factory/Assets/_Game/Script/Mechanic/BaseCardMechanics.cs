using UnityEngine;


public enum CardMechanic
{
    None,
    HiddenColor,
    Chain
}
public abstract class BaseCardMechanics : MonoBehaviour,IBaseMechanic
{
    public CardList cardOwner;

    public CardMechanic mechanicType;

    public abstract void UpdateVisual();
    public virtual void SetMechanic()
    {
        
    }

    public virtual void RemoveMechanic()
    {
        cardOwner.currentMechanic = null;
        cardOwner.mechanicType = CardMechanic.None;
    }

    public abstract void CheckRemoveRule();
}
