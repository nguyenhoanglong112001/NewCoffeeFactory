using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDataSO", menuName = "LevelData/LevelDataSO")]
public class LevelDataSO : ScriptableObject
{
    public int level;
    public HolderType[] HolderType;
    
    public HolderPointData[] HolderPoints;

    public List<QueueData> queueData;

    public ConveyorManager ConveyorPrefab;

    public Vector3 holderOffset;

    public CardQueue queuePrefab;
    public CardHolderGroup holderGroupPrefab;

    public CardHolder CardHolder;

    public CardList CoffeCups;

#if UNITY_EDITOR
    [Button]
    public void CheckHolderAndCups()
    {
        int totalHolderSlotCount = 0;
        int totalCupsCount = 0;
        foreach (var holderCount in HolderPoints)
        {
            totalHolderSlotCount += holderCount.holderDatas.Length;
            
        }
        foreach (var queue in queueData)
        {
            foreach (var cups in queue.cardData)
            {
                totalCupsCount += cups.cardNumber;
            }
        }
        Debug.Log("So luong slot " + totalHolderSlotCount * 6);
        Debug.Log("So luong cup " + totalCupsCount);
        if (totalCupsCount == totalHolderSlotCount * 6)
        {
            Debug.Log("Da du so luong");
        }
        else if (totalCupsCount < totalHolderSlotCount * 6)
        {
            Debug.Log("Thieu so luong cups: " + (totalHolderSlotCount * 6 - totalCupsCount));
        }
        else
        {
            Debug.Log("Thieu so luong holder slot: " + (totalCupsCount * 6 - totalHolderSlotCount));
        }
    }
#endif
}

[Serializable]
public class HolderType
{
    public CardColor holderColor;
    public int Count;
}

[Serializable]
public class HolderPointData
{
    [Range(0f, 1f)]
    public float holderPos;
    public Vector3 holderRotate;
    public HolderData[] holderDatas;
}

[Serializable]
public class HolderData
{
    public int holderColorIndex;
    public PackMechanic holderMechanic;
}

[Serializable]
public class CardData
{
    public int cardColorIndex;
    public int cardNumber;
    public CardMechanic cardMechanic;
}

[Serializable]
public class QueueData
{
    public CardData[] cardData;
}