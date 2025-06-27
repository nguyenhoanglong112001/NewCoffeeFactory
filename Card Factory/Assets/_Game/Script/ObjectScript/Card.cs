using cakeslice;
using DG.Tweening;
using Dreamteck;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private SplineFollower follower;
    public CardColor color;
    public QueueSlot currentQueueSlot;
    public Rigidbody cardrb;
    public CardList cardList;
    public Collider cardCollider;

    private QueueManager queueManager;
    private ConveyorManager conveyorManager;
    public CardQueue cardQueue;
    public Renderer[] rend;
    public Transform objectRotate;

    public bool isOnSpawnQueue;
    public bool isOnQueue;

    public Outline cardOutline;

    [HideInInspector] public Vector3 lastPosOnQueue;

    private void Start()
    {
        queueManager = GameManager.Ins.QueueManager;
        conveyorManager = GameManager.Ins.ConveyorManager;
        objectRotate = conveyorManager.rotateObject;
        isOnSpawnQueue = true;
    }

    private void OnEnable()
    {
        follower.onEndReached += OnCardEndConvey;
        cardrb.isKinematic = true;
        cardrb.useGravity = false;
        cardCollider.isTrigger = true;
        cardOutline.enabled = false;
    }

    private void OnDisable()
    {
        follower.onEndReached -= OnCardEndConvey;
    }

    private void OnMouseDown()
    {
        EnableCardOutLine(true);
        if (GameManager.Ins.isPause) return;
        if (GameManager.Ins.IsPointerOverUIObject())
        {   
            return;
        }
        if (conveyorManager.isAllCardMove) return;
        if(GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
        {
            Tutorial currentTut = TutorialInGameManager.Ins.GetCurrentTut();
            if(currentTut != null)
            {
                if(currentTut.currentTutStageIndex <= 2)
                {
                    TutorialStage currentStage = currentTut.GetCurrentStage();
                    currentStage.OnEndStage();
                }
            }
        }
    }

    private void OnMouseUp()
    {
        EnableCardOutLine(false);
        if (GameManager.Ins.isPause) return;
        if (conveyorManager.isAllCardMove) return;
        OnCardPress();
    }

    private void EnableCardOutLine(bool isActive)
    {
        List<Card> cardSame = new List<Card>();
        if (isOnQueue)
        {
            cardSame = GetListConsecutiveCard(this, queueManager.cardInQueue);
        }
        if(isOnSpawnQueue)
        {
            cardSame = GetListConsecutiveCard(this, cardQueue.cards);
        }
        foreach (var card in cardSame)
        {
            card.cardOutline.enabled = isActive;
        }
    }

    private void OnCardPress()
    {
        if (GameManager.Ins.BoosterManager.currentBooster != BoosterType.None) return;
        conveyorManager.isAllCardMove = true;
        List<Card> cardSameColor = new List<Card>();
        if (cardQueue.cards.Contains(this))
        {
            cardSameColor = GetListConsecutiveCard(this, cardQueue.cards);
            if (GameManager.Ins.isFirstTime)
            {
                if (GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.AddQueueSlot))
                {
                    queueManager.CountForTut(cardSameColor.Count);
                    if (queueManager.countForTut >= queueManager.avaliableQueues.Count)
                    {
                        foreach (var queue in LevelManager.Ins.queues)
                        {
                            foreach (var card in queue.cards)
                            {
                                card.GetComponent<MeshCollider>().enabled = false;
                            }
                        }
                        StartCoroutine(queueManager.WaitToFullQueue());
                    }
                }
            }
            if (conveyorManager.isFullConvey(cardSameColor))
            {
                conveyorManager.isAllCardMove = false;
            }
            if (cardQueue.cards[0] == cardSameColor[0])
            {
                StartCoroutine(CardSameMoveToConvey(cardSameColor, () =>
                {
                    cardQueue.AllCardOnQueueMove();

                }));
            }
        }
        else if (queueManager.cardInQueue.Contains(this))
        {
            if (GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
            {
                if (TutorialInGameManager.Ins.currentTutIndex == 2)
                {
                    Tutorial tutorial = TutorialInGameManager.Ins.GetCurrentTut();
                    if (tutorial.CheckCurrentStage(3))
                    {
                        tutorial.GetCurrentStage().OnEndStage(() =>
                        {
                            foreach (var card in LevelManager.Ins.queues[1].cards)
                            {
                                card.GetComponent<MeshCollider>().enabled = true;
                            }
                        });
                    }
                }
            }
            cardSameColor = GetListConsecutiveCard(this, queueManager.cardInQueue);
            if (conveyorManager.isFullConvey(cardSameColor))
            {
                conveyorManager.isAllCardMove = false;
            };
            StartCoroutine(CardSameMoveToConvey(cardSameColor, () =>
            {
            }));
        }
    }

    private void OnCardEndConvey(double d)
    {
        follower.enabled = false;
        follower.spline = null;
        conveyorManager.OnRemoveFollower(follower);
        CheckColorOnEnter(this);
    }

    public void CheckColorOnEnter(Card cardEnter)
    {
        List<Card> cardMove = new List<Card>();
        foreach (var moveList in conveyorManager.cardsMove)
        {
            if(moveList.Contains(cardEnter))
            {
                cardMove = moveList;
            }
        }
        if(!conveyorManager.cardsHash.Contains(cardMove))
        {
            conveyorManager.cardsHash.Add(cardMove);
            queueManager.CheckColorEnterQueue(cardMove);
        }

        foreach (var queue in queueManager.avaliableQueues)
        {
            if (!queue.IsEmpty) continue;
            cardEnter.MoveToQueue(queue);
            cardMove.Remove(cardEnter);
            cardEnter.isOnQueue = true;
            conveyorManager.CheckCardList(cardMove);
            return;
        }
    }

    private void MoveToQueue(QueueSlot queueSlot)
    {
        queueSlot.SetCardOnQueue(this);
        currentQueueSlot = queueSlot;
        for (int i = 0;i < queueManager.cardInQueue.Count;i++)
        {
            if(queueManager.cardInQueue[i] == null)
            {
                queueManager.cardInQueue[i] = this;
                break;
            }
        }
        if(TutorialInGameManager.Ins.isOnTutorial && GameManager.Ins.isFirstTime)
        {
            if (TutorialInGameManager.Ins.GetCurrentTutStage() == (2,3))
            {
                TutorialInGameManager.Ins.OnActiveTutorial(GameManager.Ins.QueueManager.gameObject.transform.position, new Vector3(-5, 2, -2));
            }
        }
        this.transform.DOMove(queueSlot.transform.position, 0.3f).SetUpdate(true)
            .OnComplete(() =>
            {
                this.GetComponent<MeshCollider>().enabled = true;
                if (TutorialInGameManager.Ins.isOnTutorial && GameManager.Ins.isFirstTime)
                {
                    if (GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.AddQueueSlot))
                    {
                        this.GetComponent<MeshCollider>().enabled = false;
                    }
                }
            });
        CheckList();
    }

    private void CardMoveToConvey(Action onComplete)
    {
        AudioManager.Ins.PlaySound("CardSound");
        Sequence moveTween = DOTween.Sequence();
        moveTween.Append(this.transform.DOJump(conveyorManager.conveyEntrace.position, 4f, 1, 0.5f));
        moveTween.Join(this.transform.DORotate(new Vector3(this.transform.rotation.x, objectRotate.transform.rotation.y, transform.rotation.z), 0.1f));
        moveTween.OnComplete(() =>
        {
            cardList.queue.cardLists.Remove(cardList);
            conveyorManager.SetSpline(follower);
            conveyorManager.OnAddFollower(follower);
            follower.enabled = false;
            onComplete.Invoke();
        });
    }

    private IEnumerator CardSameMoveToConvey(List<Card> cardsSameColor, Action oncomplete = null)
    {
        int completeCount = 0;
        int total = cardsSameColor.Count;
        float spacingDelay = 0.15f;

        conveyorManager.cardsMove.Add(new List<Card>(cardsSameColor));
        List<Card> cardSame = cardsSameColor.ToList();

        for (int i = 0; i < cardSame.Count; i++)
        {
            Card card = cardSame[i];
            if (card.cardList.mechanicType == CardMechanic.Chain) yield break;
            card.GetComponent<MeshCollider>().enabled = false;
            if(cardSame.All(x => queueManager.cardInQueue.Contains(x)))
            {
                foreach (var cardinQueue in cardSame)
                {
                    if (cardinQueue != null)
                    {
                        cardinQueue.currentQueueSlot?.SetCardOnQueue(null);
                        cardinQueue.currentQueueSlot = null;
                    }
                }
                queueManager.cardInQueue.RemoveAll(x => cardSame.Contains(x));
                List<Card?> newValues = Enumerable.Repeat<Card?>(null,cardSame.Count).ToList();
                queueManager.cardInQueue.AddRange(newValues);
                queueManager.ReOrderQueue((index, card) =>
                {
                    if (card == null) return;
                    card.currentQueueSlot?.SetCardOnQueue(null);
                    card.currentQueueSlot = queueManager.avaliableQueues[index];
                    queueManager.avaliableQueues[index].SetCardOnQueue(card);
                });
            }

            float delayForFollower = i * spacingDelay;

            card.CardMoveToConvey(() =>
            {
                if (card.isOnSpawnQueue)
                {
                    completeCount++;
                    cardQueue.cards.Remove(card);
                    LevelManager.Ins.cards.Remove(card);
                    card.isOnSpawnQueue = false;

                    if (completeCount == total)
                    {
                        if (GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
                        {
                            var tutStage = TutorialInGameManager.Ins.GetCurrentTutStage();
                            Debug.Log(tutStage == (2,3));
                            if (TutorialInGameManager.Ins.currentTutIndex != 1)
                            {
                                if(tutStage != (2,3))
                                {
                                    TutorialInGameManager.Ins.OnActiveTutorial(LevelManager.Ins.queues[0].cardPos[0], new Vector3(-1, 3, -9));
                                }
                            }
                        }
                        if(GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.AddConvey))
                        {
                            if(TutorialInGameManager.Ins.currentTutIndex == 4)
                            {
                                TutorialInGameManager.Ins.StartTut(() =>
                                {
                                    GameManager.Ins.BoosterManager.boosterCount[BoosterType.AddConvey] = 10;
                                    Time.timeScale = 0.0f;
                                    foreach (var queue in LevelManager.Ins.queues)
                                    {
                                        foreach (var card in queue.cards)
                                        {
                                            card.GetComponent<MeshCollider>().enabled = false;
                                        }
                                    }
                                });
                                TutorialStage currentStage = TutorialInGameManager.Ins.GetCurrentTut().GetCurrentStage();
                                currentStage.SetHandPointPos(UIManager.Ins.addCapacityConvey.gameObject.transform.position, new Vector3(1, 1, -1.5f));
                            }
                        }
                        StartCoroutine(DelaySetIsAllCardMove());
                        oncomplete?.Invoke();
                    }
                }
                else if (card.isOnQueue)
                {
                    completeCount++;
                    card.isOnQueue = false;
                    if (completeCount == total)
                    {
                        StartCoroutine(DelaySetIsAllCardMove());
                        oncomplete?.Invoke();
                    }
                }
                StartCoroutine(StartFollowerWithDelay(card.follower, delayForFollower));
            });

            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private IEnumerator DelaySetIsAllCardMove()
    {
        yield return new WaitForSeconds(0.05f);
        conveyorManager.isAllCardMove = false;
    }

    private IEnumerator StartFollowerWithDelay(SplineFollower follower, float delay)
    {
        yield return new WaitForSeconds(delay);

        follower.SetPercent(0f);
        follower.enabled = true;
    }

    public List<Card> GetListConsecutiveCard(Card card, List<Card> cards)
    {
        List<Card> cardSameColor = new List<Card>();

        int index = cards.IndexOf(card);
        cardSameColor.Add(card);

        for (int i = index - 1; i >= 0; i--)
        {
            if (cards[i] == null) continue;
            if (cards[i].cardList.HaveMechanic) break;
            if (cards[i].color == card.color)
                cardSameColor.Insert(0, cards[i]);
            else break;
        }

        for (int i = index + 1; i < cards.Count; i++)
        {
            if (cards[i] == null) continue;
            if (cards[i].cardList != null)
            {
                if (cards[i].cardList.HaveMechanic) break;
            }
            if (cards[i].color == card.color)
                cardSameColor.Add(cards[i]);
            else break;
        }
        return cardSameColor;
    }

    Transform holderPos;
    public void MoveToHolder(CardHolderGroup holder)
    {
        if (holder.cardHolders[0].colorHolder != color) return;
        if (holder.cardHolders[0].isFull) return;
        AudioManager.Ins.PlaySound("CardSound");
        Sequence moveToHolder = DOTween.Sequence();
        CardHolder cupsHolder = holder.cardHolders[0];

        foreach (var holdercard in cupsHolder.cardHolderPos)
        {
            if(holdercard.childCount <= 0)
            {
                holderPos = holdercard;
                this.transform.SetParent(holderPos);
                break;
            }
        }
        if (holderPos == null) return;
        follower.spline = null;
        follower.enabled = false;
        moveToHolder.Append(transform.DOJump(
            holderPos.position,   
            2f,                   
            1,                    
            0.6f                  
        ));
        moveToHolder.Join(transform.DORotate(Vector3.zero, 0.6f));
        Vector3 orgScale = cupsHolder.transform.localScale;
        cupsHolder.packAnimation.Play();
        moveToHolder.OnComplete(() =>
        {
            conveyorManager.OnRemoveFollower(follower);
            cardList.cards.Remove(this);
            cardList.CheckCard();
            holder.cardHolders[0].cardHolder.Add(this);
            holder.cardHolders[0].CheckHolder();
            CheckList();
        });
    }

    public void OnSplineTriggerEnter()
    {
        conveyorManager.cardTrigger = this;
    }

    public void ChangeCardColor()
    {
        if (cardList.HaveMechanic)
        {
            if (cardList.mechanicType == CardMechanic.HiddenColor) return;
        }
        foreach (var render in rend)
        {
            Material mat = render.material;
            ColorSetup.SetCardColor(color, mat);
        }
    }   

    private void CheckList()
    {
        foreach (var cardMove in conveyorManager.cardsMove.ToList())
        {
            if (cardMove.ToList().Contains(this))
            {
                cardMove.Remove(this);
                conveyorManager.CheckCardList(cardMove);
            }
        }
        foreach (var cardMove in conveyorManager.cardsHash.ToList())
        {
            if (cardMove.ToList().Contains(this))
            {
                cardMove.Remove(this);
                conveyorManager.CheckCardList(cardMove);
            }
        }
    }

    private void CardFallOutConvey()
    {
        follower.enabled = false;
        follower.spline = null;
        conveyorManager.OnRemoveFollower(follower);
        OnSplineEnd();
                follower.enabled = false;
        cardrb.isKinematic = false;
        cardrb.useGravity = true;
        cardCollider.isTrigger = false;
    }
    public void OnCardCantEnterQueue()
    {
        follower.onEndReached -= OnCardEndConvey;
        CardFallOutConvey();
    }

    void OnSplineEnd()
    {
        Vector3 dir = (transform.forward + Vector3.up * 0.5f).normalized;
        cardrb.linearVelocity = dir * new Vector3(2f, 4f, 0f).magnitude;
        LevelManager.Ins.OnLevelFail();
    }
}
