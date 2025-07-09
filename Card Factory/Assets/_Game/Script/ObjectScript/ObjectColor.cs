using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectColor", menuName = "Scriptable Objects/ObjectColor")]
public class ObjectColor : ScriptableObject
{
    public List<ColorObject> colors = new List<ColorObject>();

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
}

[System.Serializable]
public class ColorObject
{
    public CardColor color;
    public Material colorMat;
}
