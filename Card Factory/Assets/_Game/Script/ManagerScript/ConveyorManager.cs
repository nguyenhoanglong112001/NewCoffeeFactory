using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ConveyorManager : MonoBehaviour
{
    [SerializeField] public SplineComputer spline;
    public Card cardTrigger;
    public Transform rotateObject;
    public Transform conveyEntrace;
    [ShowInInspector]
    public List<List<Card>> cardsMove;
    public HashSet<List<Card>> cardsHash;
    public List<SplineFollower> cardOnConvey;

    public Vector3 ConveyorOffset;

    public int maxCardOnTurn;

    public LevelManager levelManager;

    public float followerSpacing;

    public TMP_Text capitalText;

    //public bool isAllCardMove;

    public float spacingDistance;

    public Transform boosterTargetPos;

    private void Start()
    {
        cardsMove = new List<List<Card>>();
        cardsHash = new HashSet<List<Card>>();
        levelManager = LevelManager.Ins;
        OnNumberCardCounter(cardOnConvey.Count, maxCardOnTurn);
        EventManager.OnCapitalChange.AddListener(OnNumberCardCounter);
    }
    public void SetSpline(SplineFollower follower)
    {
        follower.spline = spline;
    }

    public void OnCheckCardHolder(SplineUser user,CardHolderGroup holderGroup)
    {
        if (holderGroup.cardHolders.Count <= 0) return;
        cardTrigger = user.GetComponent<Card>();
        if(cardTrigger != null )
        {
            cardTrigger.MoveToHolder(holderGroup);
        }
    }

    public void CheckCardList(List<Card> cardMove)
    {
        if(cardsMove.Contains(cardMove))
        {
            if(cardMove.Count <= 0)
            {
                cardsMove.Remove(cardMove);
            }
        }
        if(cardsHash.Contains(cardMove))
        {
            if(cardsHash.Count <= 0)
            {
                cardsHash.Remove(cardMove);
            }
        }
    }

    public bool isFullConvey(List<Card> cardEnter)
    {
        if(cardOnConvey.Count + cardEnter.Count > maxCardOnTurn)
        {
            Debug.Log("Convet full");
            return true;
        }
        return false;
    }

    public void OnNumberCardCounter(int currentCard,int maxCard)
    {
        capitalText.text = currentCard.ToString() + "/" + maxCard.ToString(); 
    }

    public void OnRemoveFollower(SplineFollower follower)
    {
        cardOnConvey.Remove(follower);
        EventManager.OnCapitalChange.Invoke(cardOnConvey.Count, maxCardOnTurn);
    }

    public void OnAddFollower(SplineFollower follower)
    {
        cardOnConvey.Add(follower);
        EventManager.OnCapitalChange.Invoke(cardOnConvey.Count, maxCardOnTurn);
    }

    public void AddMaxCardOnConvey(int cardAdd)
    {
        maxCardOnTurn += cardAdd;
        EventManager.OnCapitalChange.Invoke(cardOnConvey.Count, maxCardOnTurn);
    }

    public void AddTrigger()
    {
        foreach (var group in LevelManager.Ins.holderGroups)
        {
            SplineTrigger triggers = spline.triggerGroups[0].AddTrigger(group.groupPos,SplineTrigger.Type.Double);
            triggers.onCross.AddListener((SplineUser user) =>
            {
                OnCheckCardHolder(user, group);
            });
        }
    }

    public void CheckSpacing(SplineFollower nextFollower)
    {
        nextFollower.SetDistance(0f);

        if (cardOnConvey.Count <= 1)
        {
            nextFollower.enabled = true;
            return;
        }

        StartCoroutine(DelayUntilEnoughSpacing(nextFollower));
    }

    private IEnumerator DelayUntilEnoughSpacing(SplineFollower nextFollower)
    {
        nextFollower.enabled = false;

        // Follower đứng trước
        SplineFollower aboveFollower = cardOnConvey[cardOnConvey.Count - 2];

        // Tính distance từ percent
        float splineLength = spline.CalculateLength();

        while (true)
        {
            float abovePercent = (float)aboveFollower.GetPercent();
            float aboveDistance = abovePercent * splineLength;

            if (aboveDistance >= spacingDistance)
                break;

            yield return null;
        }

        nextFollower.SetDistance(0f);
        nextFollower.enabled = true;
    }
}
