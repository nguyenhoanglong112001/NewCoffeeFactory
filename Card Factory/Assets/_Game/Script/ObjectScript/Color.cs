using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public enum CardColor
{
    Red = 0,
    Green = 2,
    Blue = 1,
    Yellow = 3,
    Purple = 4,
    Orange = 5,
}
public static class ColorSetup
{
    public static void SetCardColor(CardColor colorSet,Material mat)
    {
        mat.mainTextureScale = Vector2.zero;
        switch (colorSet)
        {
            case CardColor.Red:
                {
                    mat.mainTextureOffset = new Vector2(7f/16f, 2f/4f);
                    break;
                }
            case CardColor.Green:
                {
                    mat.mainTextureOffset = new Vector2(1f / 16f, 2 / 4f);
                    break;
                }
            case CardColor.Blue:
                {
                    mat.mainTextureOffset = new Vector2(9f / 16f, 2 / 4f);
                    break;
                }
            case CardColor.Yellow:
                {
                    mat.mainTextureOffset = new Vector2(3f / 16f, 2 / 4f);
                    break;
                }
            case CardColor.Purple:
                {
                    mat.mainTextureOffset = new Vector2(13f / 16f, 2 / 4f);
                    break;
                }
            case CardColor.Orange:
                {
                    mat.mainTextureOffset = new Vector2(5f / 16f, 2 / 4f);
                    break;
                }
        }
    }

    public static void SetUpColorForUI(CardColor colorSet, Image image)
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

    public static void SetHiddenUI(Image holderImage)
    {
        holderImage.color = new Color(192, 192, 192);
    }

    public static void SetColorHidden(Material mat)
    {
        mat.mainTextureOffset = new Vector2(6f / 16f, 0 / 4f);
    }
}