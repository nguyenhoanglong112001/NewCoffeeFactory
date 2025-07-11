using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueSlot : MonoBehaviour
{
    [SerializeField] private Card currentCard;
    public Transform queuePos;
    public bool IsEmpty => currentCard == null;

    public void SetCardOnQueue(Card card)
    {
        currentCard = card;
    }
}
