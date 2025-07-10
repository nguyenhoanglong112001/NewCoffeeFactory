using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectColor", menuName = "Scriptable Objects/ObjectColor")]
public class ObjectColor : ScriptableObject
{
    public List<ColorObject> colors = new List<ColorObject>();
    public List<PackColor> packColors = new List<PackColor>();


    public Material GetColor(CardColor colorCheck)
    {
        foreach (var c in colors)
        {
            if(c.color == colorCheck)
            {
                return c.colorMat;
            }
        }
        return null;
    }

    public Material GetPackColor(CardColor packcolor)
    {
        foreach (var c in packColors)
        {
            if (c.packColor == packcolor)
            {
                return c.packMat;
            }
        }
        return null;
    }
}

[System.Serializable]
public class ColorObject
{
    public CardColor color;
    public Material colorMat;
}

[System.Serializable]
public class PackColor
{
    public CardColor packColor;
    public Material packMat;
}
