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

    public TMP_Text holderRemain;
    public Image completeImage;
    public Image holderNextImage;
 
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

    public bool isReordering;
    public void AllHolderMoveFront(Action onComplete = null)
    {
        int completeCount = 0;
        isReordering = true;


        if(cardHolders.Count <= 1)
        {
            onComplete.Invoke();
        }
        for (int i =1; i < cardHolders.Count; i++)
        {
            cardHolders[i].transform.DOLocalMove(holderPos[i - 1], 0.2f).
                OnComplete(() =>
                {
                    if (i == 0)
                    {
                        if (cardHolders[i].HaveMechanic)
                        {
                            cardHolders[i].packMechanic.CheckRemoveRule();
                        }
                    }
                    completeCount++;
                    if (completeCount == cardHolders.Count - 1)
                    {
                        isReordering = false;
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
                ColorSetup.SetHiddenUI(holderNextImage);
            }
            else
            {
                ColorSetup.SetUpColorForUI(cardHolders[1].colorHolder,holderNextImage);
            }
        }
        if(cardHolders.Count <= 0)
        {
            holderRemain.gameObject.SetActive(false);
            completeImage.gameObject.SetActive(true);
        }
    }
}
