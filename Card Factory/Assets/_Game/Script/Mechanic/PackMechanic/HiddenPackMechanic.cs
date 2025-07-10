using Dreamteck;
using UnityEngine;

public class HiddenPackMechanic : BasePackMechanic
{

    public override void UpdateVisual()
    {
        Debug.Log("Change Color visual",this.gameObject);
        foreach(var rend in packOwner.rends)
        {
            Material newMat = packOwner.objectcolor.GetPackColor(CardColor.Hidden); // lấy material mới từ objectColor
            rend.material = newMat; // GÁN lại vào renderer để áp dụng
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
            Material newMat = packOwner.objectcolor.GetPackColor(packOwner.colorHolder); // lấy material mới từ objectColor
            rend.material = newMat; // GÁN lại vào renderer để áp dụng
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
