using UnityEngine;

public enum PackMechanic
{
    None,
    HiddenColor,
    ChainPack
}
public abstract class BasePackMechanic : MonoBehaviour, IBaseMechanic
{
    public CardHolder packOwner;
    public PackMechanic packMechanic;
    public virtual void RemoveMechanic()
    {
        packOwner.currentMechanic = PackMechanic.None;
        packOwner.packMechanic = null;
        Destroy(gameObject);
    }

    public abstract void CheckRemoveRule();

    public virtual void SetMechanic()
    {
    }

    public abstract void UpdateVisual();
}
