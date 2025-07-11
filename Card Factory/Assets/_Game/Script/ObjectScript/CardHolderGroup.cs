using DG.Tweening;
using Dreamteck.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardHolderGroup : MonoBehaviour
{
    public List<CardHolder> cardHolders;

    [SerializeField] public List<Vector3> holderPos;
    public Vector3 spacing;
    public Transform starPos;

    public float groupPos;
    public SplineComputer splineCom;
    public Transform counterTransform;

    public TMP_Text holderRemain;
    public Image completeImage;
 
    private void Start()
    {
        CheckHolder();
    }
    public void OnSetPositionOnQueue()
    {
        Vector3 posOnQueue = starPos.transform.localPosition;
        foreach (var holder in cardHolders)
        {
            holder.transform.localPosition = posOnQueue;
            holderPos.Add(posOnQueue);
            posOnQueue += spacing;
        }
    }

    public void AllHolderMoveFront(Action onComplete = null)
    {
        int completeCount = 0;
        if (cardHolders.Count <= 1)
        {
            onComplete.Invoke();
        }
        if(cardHolders.Count > 1)
        {
            cardHolders[1].OnCheckSlotDisPlay(true);
        }
        for (int i =1; i < cardHolders.Count; i++)
        {
            cardHolders[i].transform.DOLocalMove(holderPos[i - 1], 0.5f).
                OnComplete(() =>
                {
                    if (i - 1 == 0)
                    {
                        if (cardHolders[i].HaveMechanic)
                        {
                            cardHolders[i].packMechanic.CheckRemoveRule();
                        }
                    }
                    completeCount++;
                    if (completeCount == cardHolders.Count - 1)
                    {
                        onComplete?.Invoke();
                    }
                });
        }
    }


    public void CheckHolder()
    {
        holderRemain.text = (cardHolders.Count - 1).ToString();
        if(cardHolders.Count > 1)
        {
            if (cardHolders[1].HaveMechanic)
            {
                foreach(var cardMat in cardHolders[1].rends)
                {
                    ColorSetup.SetColorHidden(cardMat.material);
                }
            }
        }
        if(cardHolders.Count <= 0)
        {
            holderRemain.gameObject.SetActive(false);
            completeImage.gameObject.SetActive(true);
        }
    }
}
