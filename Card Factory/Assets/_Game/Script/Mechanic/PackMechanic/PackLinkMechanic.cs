using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PackLinkMechanic : BasePackMechanic
{
    public List<CardHolder> packLink;
    public override void CheckRemoveRule()
    {
        if(CheckLinkPack())
        {
            RemoveMechanic();
        }
    }

    private bool CheckLinkPack()
    {
        foreach (var pack in packLink)
        {
            if(!pack.isFull)
            {
                return false;
            }
        }
        return true;
    }

    public override void UpdateVisual()
    {
        foreach (var pack in packLink)
        {
            pack.currentMechanic = PackMechanic.ChainPack;
            pack.packMechanic = this;
        }
    }

    public override void RemoveMechanic()
    {
        foreach (var pack in packLink)
        {
            pack.OnRemoveHolder();
            pack.currentMechanic = PackMechanic.None;
            pack.packMechanic = null;
        }
        Destroy(gameObject);
    }
}
