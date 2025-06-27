using Dreamteck;
using UnityEngine;

public class HiddenPackMechanic : BasePackMechanic
{

    public override void UpdateVisual()
    {
        Debug.Log("Change Color visual",this.gameObject);
        foreach(var rend in packOwner.rends)
        {
            Material mat = rend.material;
            ColorSetup.SetColorHidden(mat);
        }
    }

    public override void CheckRemoveRule()
    {
        if (packOwner.holderGroup.cardHolders.IndexOf(packOwner) <= 0)
        {
            RemoveMechanic();
        }
    }

    public override void RemoveMechanic()
    {
        base.RemoveMechanic();
        foreach (var rend in packOwner.rends)
        {
            Material mat = rend.material;
            ColorSetup.SetCardColor(packOwner.colorHolder,mat);
        }
    }

    public override void SetMechanic()
    {
        packMechanic = PackMechanic.HiddenColor;
        packOwner = gameObject.GetComponent<CardHolder>();
        packOwner.packMechanic = this;
        packOwner.currentMechanic = packMechanic;
        UpdateVisual();
    }
}
