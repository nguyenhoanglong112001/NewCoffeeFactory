using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using cakeslice;
public enum CardColor
{
    Red = 0,
    Green = 2,
    Blue = 1,
    Yellow = 3,
    Purple = 4,
    Orange = 5,
    Hidden = 6
}
public static class ColorSetup
{
    public static void SetMatColor(CardColor colorSet,Material mat)
    {
        switch (colorSet)
        {
            case CardColor.Red:
                {
                    mat.SetColor("_BaseColor", new Color(255,0,0));
                    break;
                }
            case CardColor.Green:
                {
                    mat.SetColor("_BaseColor", new Color(0, 255, 0));
                    break;
                }
            case CardColor.Blue:
                {
                    mat.SetColor("_BaseColor", new Color(0, 191, 255));
                    break;
                }
            case CardColor.Yellow:
                {
                    mat.SetColor("_BaseColor", new Color(255, 255, 0));
                    break;
                }
            case CardColor.Purple:
                {
                    mat.SetColor("_BaseColor", new Color(128, 128, 0));
                    break;
                }
            case CardColor.Orange:
                {
                    mat.SetColor("_BaseColor", new Color(255, 165, 0));
                    break;
                }
        }
    }

    public static void SetUpObjectColor(CardColor colorSet, Image image)
    {
        switch (colorSet)
        {
            case CardColor.Red:
                {
                    image.color = new Color(220f / 255f, 20f / 255f, 60f / 255f);
                    break;
                }
            case CardColor.Green:
                {
                    image.color = new Color(0f / 255f, 255f / 255f, 0f / 255f);
                    break;
                }
            case CardColor.Blue:
                {
                    image.color = new Color(0f / 255f, 0f / 255f, 255f / 255f);
                    break;
                }
            case CardColor.Yellow:
                {
                    image.color = new Color(255f / 255f, 255f / 255f, 0f / 255f);
                    break;
                }
            case CardColor.Purple:
                {
                    // Thêm màu cho Purple, ví d?: RGB (128, 0, 128)
                    image.color = new Color(128f / 255f, 0f / 255f, 128f / 255f);
                    break;
                }
            case CardColor.Orange:
                {
                    // Thêm màu cho Orange, ví d?: RGB (255, 165, 0)
                    image.color = new Color(255f / 255f, 165f / 255f, 0f / 255f);
                    break;
                }
        }
    }

    public static Color GetColor(CardColor colorSet)
    {
        Color color = Color.white;
        switch (colorSet)
        {
            case CardColor.Red:
                {
                    color = new Color(220f / 255f, 20f / 255f, 60f / 255f);
                    break;
                }
            case CardColor.Green:
                {
                    color = new Color(0f / 255f, 255f / 255f, 0f / 255f);
                    break;
                }
            case CardColor.Blue:
                {
                    color = new Color(0f / 255f, 0f / 255f, 255f / 255f);
                    break;
                }
            case CardColor.Yellow:
                {
                    color = new Color(255f / 255f, 255f / 255f, 0f / 255f);
                    break;
                }
            case CardColor.Purple:
                {
                    // Thêm màu cho Purple, ví d?: RGB (128, 0, 128)
                    color = new Color(128f / 255f, 0f / 255f, 128f / 255f);
                    break;
                }
            case CardColor.Orange:
                {
                    // Thêm màu cho Orange, ví d?: RGB (255, 165, 0)
                    color = new Color(255f / 255f, 165f / 255f, 0f / 255f);
                    break;
                }
        }
        return color;
    }

    public static void SetCustomOutlineColor(CardColor colorSet)
    {
        OutlineEffect.Instance.lineColor2 = GetColor(colorSet);
    }

    public static void SetHiddenUI(Image holderImage)
    {
        holderImage.color = new Color(192, 192, 192);
    }

    public static void SetColorHidden(Material mat)
    {
        mat.SetColor("_BaseColor", new Color(128, 128, 128));
    }
}