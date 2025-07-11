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
    public ObjectColor objectColor; 
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
    public GameObject plate;



    public bool canPress;

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

    private void Update()
    {

    }

    private void OnMouseDown()
    {
        if (!canPress) return;
        if (GameManager.Ins.isPause) return;
        if (GameManager.Ins.IsPointerOverUIObject())
        {
            return;
        }
        List<Card> cardSame = new List<Card>();
        if (isOnQueue)
        {
            cardSame = GetListConsecutiveCard(this, queueManager.cardInQueue);
        }
        if (isOnSpawnQueue)
        {
            cardSame = GetCardGroupConsecutuve(cardList, cardQueue.cardLists);
        }
        if (conveyorManager.isFullConvey(cardSame))
        {
            EnableCardOutLine(false);
            canPress = true;
            return;
        }
        if (isOnSpawnQueue)
        {
            if (cardQueue.cards.IndexOf(cardSame[0]) != 0)
            {
                EnableCardOutLine(false);
                canPress = true;
                return;
            }
        }
        //conveyorManager.OnAddFollower(follower);
        EnableCardOutLine(true);
        if (GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
        {
            Tutorial currentTut = TutorialInGameManager.Ins.GetCurrentTut();
            if (currentTut != null)
            {
                if (currentTut.currentTutStageIndex <= 2)
                {
                    if(TutorialInGameManager.Ins.currentTutIndex == 2)
                    {
                        if (LevelManager.Ins.queues.IndexOf(cardList.queue) > 0) return;
                    }
                    TutorialStage currentStage = currentTut.GetCurrentStage();
                    currentStage.OnEndStage();
                }
            }
        }
    }

    private void OnMouseUp()
    {
        if (GameManager.Ins.IsPointerOverUIObject())
        {
            return;
        }
        if (!canPress) return;
        List<Card> cardSame = new List<Card>();
        if (isOnQueue)
        {
            cardSame = GetListConsecutiveCard(this, queueManager.cardInQueue);
        }
        if (isOnSpawnQueue)
        {
            cardSame = GetListConsecutiveCard(this, cardQueue.cards);
        }
        foreach (var card in cardSame)
        {
            card.cardOutline.enabled = false;
        }
        if (conveyorManager.isFullConvey(cardSame))
        {
            canPress = true;
            return;
        }
        if (isOnSpawnQueue)
        {
            if (cardQueue.cards.IndexOf(cardSame[0]) != 0)
            {
                canPress = true;
                return;
            }
        }
        if (GameManager.Ins.isPause) return;
        OnCardPress();
    }

    private void EnableCardOutLine(bool isActive)
    {
        List<Card> cardSame = new List<Card>();
        int colorIndex = 0;
        if (isOnQueue)
        {
            cardSame = GetListConsecutiveCard(this, queueManager.cardInQueue);
        }
        if (isOnSpawnQueue)
        {
            cardSame = GetListConsecutiveCard(this, cardQueue.cards);
        }
        if (isActive)
        {
            colorIndex = 1;
        }
        else
        {
            colorIndex = 0;
        }
        foreach (var card in cardSame)
        {
            card.cardOutline.enabled = true;
            card.cardOutline.color = colorIndex;
        }
    }

    private void OnCardPress()
    {
        if (GameManager.Ins.BoosterManager.currentBooster != BoosterType.None) return;
        canPress = false;
        List<Card> cardSame = GetCardGroupConsecutuve(cardList, cardQueue.cardLists);
        if (cardQueue.cards.Contains(this))
        {
            if (GameManager.Ins.isFirstTime)
            {
                if (GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.AddQueueSlot))
                {
                    queueManager.CountForTut(cardSame.Count);
                    if (queueManager.countForTut >= queueManager.avaliableQueues.Count)
                    {
                        foreach (var queue in LevelManager.Ins.queues)
                        {
                            foreach (var card in queue.cards)
                            {
                                card.canPress = false;
                            }
                        }
                        StartCoroutine(queueManager.WaitToFullQueue());
                    }
                }
            }
            if (cardQueue.cards[0] == cardSame[0])
            {
                StartCoroutine(CardSameMoveToConvey(cardSame, () =>
                {
                    cardQueue.AllCardOnQueueMove();

                }));
            }
            else
            {
                canPress = true;
            }

        }
        else if (queueManager.cardInQueue.Contains(this))
        {
            List<Card> cardSameColor = new List<Card>();
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
                                card.canPress = true;
                            }
                        });
                    }
                }
            }
            cardSameColor = GetListConsecutiveCard(this, queueManager.cardInQueue);
            StartCoroutine(CardSameMoveToConvey(cardSameColor));
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
            if (moveList.Contains(cardEnter))
            {
                cardMove = moveList;
            }
        }
        if (!conveyorManager.cardsHash.Contains(cardMove))
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
        Sequence s = DOTween.Sequence();
        queueSlot.SetCardOnQueue(this);
        currentQueueSlot = queueSlot;
        for (int i = 0; i < queueManager.cardInQueue.Count; i++)
        {
            if (queueManager.cardInQueue[i] == null)
            {
                queueManager.cardInQueue[i] = this;
                break;
            }
        }
        if (TutorialInGameManager.Ins.isOnTutorial && GameManager.Ins.isFirstTime)
        {
            if (TutorialInGameManager.Ins.GetCurrentTutStage() == (2, 3))
            {
                TutorialInGameManager.Ins.OnActiveTutorial(GameManager.Ins.QueueManager.handPointTutPos.position);
            }
        }
        s.Append(this.transform.DOJump(queueSlot.queuePos.position, 3f, 1, 0.5f));
        s.Join(this.transform.DOLocalRotate(new Vector3(90, 90, 90), 0.5f));
        s.SetUpdate(true)
            .OnComplete(() =>
            {
                canPress = true;
                if (TutorialInGameManager.Ins.isOnTutorial && GameManager.Ins.isFirstTime)
                {
                    if (GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.AddQueueSlot))
                    {
                        canPress = false;
                    }
                }
            });
        CheckList();
    }

    private void CardMoveToConvey(Action onComplete = null)
    {
        Sequence jumpConVey = DOTween.Sequence();
        onComplete.Invoke();
        cardList.queue.cardLists.Remove(cardList);
        AudioManager.Ins.PlaySound("CardSound");
        jumpConVey.Append(transform.DOJump(conveyorManager.doorEnterance.position, 8f, 1, 0.6f));
        jumpConVey.Append(transform.DOMove(conveyorManager.conveyEntrace.position, 0.3f));
        jumpConVey.OnComplete(() =>
        {
            conveyorManager.SetSpline(follower);
            conveyorManager.OnAddFollower(follower);
            follower.enabled = false;
            this.plate.SetActive(true);
            conveyorManager.CheckSpacing(follower);
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
            card.canPress = false;
            if (cardSame.All(x => queueManager.cardInQueue.Contains(x)))
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
                List<Card?> newValues = Enumerable.Repeat<Card?>(null, cardSame.Count).ToList();
                queueManager.cardInQueue.AddRange(newValues);
                queueManager.ReOrderQueue();
            }

            float delayForFollower = i * spacingDelay;

            card.CardMoveToConvey(() =>
            {
                if (card.isOnSpawnQueue)
                {
                    completeCount++;
                    card.isOnSpawnQueue = false;
                    cardQueue.cards.Remove(card);
                    LevelManager.Ins.cards.Remove(card);
                    if (completeCount == total)
                    {
                        CardMoveToConveyTut();
                        oncomplete?.Invoke();
                    }
                }
                else if (card.isOnQueue)
                {
                    completeCount++;
                    card.isOnQueue = false;
                    oncomplete?.Invoke();
                    if (completeCount == total)
                    {
                        oncomplete?.Invoke();
                    }
                }
            });

            yield return new WaitForSecondsRealtime(0.1f);
        }
    }

    private void CardMoveToConveyTut()
    {
        if (GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
        {
            var tutStage = TutorialInGameManager.Ins.GetCurrentTutStage();
            Debug.Log(tutStage == (2, 3));
            if (TutorialInGameManager.Ins.currentTutIndex != 1)
            {
                if (tutStage != (2, 3))
                {
                    TutorialInGameManager.Ins.OnActiveTutorial(LevelManager.Ins.queues[0].handPointTutPos.position);
                }
            }
        }
        if (GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.AddConvey))
        {
            if (TutorialInGameManager.Ins.currentTutIndex == 4)
            {
                TutorialInGameManager.Ins.StartTut(() =>
                {
                    GameManager.Ins.BoosterManager.boosterCount[BoosterType.AddConvey] = 10;
                    Time.timeScale = 0.0f;
                    foreach (var queue in LevelManager.Ins.queues)
                    {
                        foreach (var card in queue.cards)
                        {
                            card.canPress = false;
                        }
                    }
                });
                TutorialStage currentStage = TutorialInGameManager.Ins.GetCurrentTut().GetCurrentStage();
                currentStage.SetHandPointPos(GameManager.Ins.BoosterManager.addConveyHAndPos.position);
            }
        }
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

    public List<Card> GetCardGroupConsecutuve(CardList cardGroup, List<CardList> cardGroupes)
    {
        List<Card> cardSame = new List<Card>();
        int index = cardGroupes.IndexOf(cardGroup);
        cardSame.AddRange(cardGroup.cards);

        for(int i = index - 1;i>= 0;i--)
        {
            if (cardGroupes[i] == null) continue;
            if (cardGroupes[i].HaveMechanic) break;
            if (cardGroupes[i].listColor == cardGroup.listColor)
            {
                cardSame.InsertRange(0, cardGroupes[i].cards);
            }
            else break;
        }

        for(int i = index + 1;i < cardGroupes.Count; i++)
        {
            if (cardGroupes[i] == null) continue;
            if (cardGroupes[i].HaveMechanic) break;
            if (cardGroupes[i].listColor == cardGroup.listColor)
            {
                cardSame.AddRange(cardGroupes[i].cards);
            }
            else break;
        }
        return cardSame;
    }

    Transform holderPos;
    public void MoveToHolder(CardHolder cupsHolder)
    {
        if (cupsHolder.colorHolder != color) return;
        RemovePlate();
        AudioManager.Ins.PlaySound("CardSound");

        // Đăng ký card đang di chuyển
        cupsHolder.RegisterMovingCard();
        Sequence moveToHolder = DOTween.Sequence();

        foreach (var holdercard in cupsHolder.cardHolderPos)
        {
            if (holdercard.childCount <= 0)
            {
                holderPos = holdercard;

                // Lưu scale hiện tại trước khi setParent
                Vector3 originalScale = this.transform.localScale;

                // SetParent với worldPositionStays = false để giữ nguyên local scale
                this.transform.SetParent(holderPos);

                // Khôi phục scale gốc
                this.transform.localScale = originalScale;

                break;
            }
        }

        if (holderPos == null)
        {
            cupsHolder.UnregisterMovingCard();
            return;
        }

        follower.spline = null;
        follower.enabled = false;

        Tween jumpTween = transform.DOLocalJump(Vector3.zero, 3f, 1, 0.5f);
        moveToHolder.Append(jumpTween);

        moveToHolder.OnComplete(() =>
        {
            cupsHolder.packAnimation.Play();
            conveyorManager.OnRemoveFollower(follower);
            cardList.cards.Remove(this);
            cardList.CheckCard();
            CheckList();
            cupsHolder.UnregisterMovingCard();

            // Chỉ set scale = Vector3.one nếu bạn thực sự muốn
            // Nếu muốn giữ nguyên scale gốc thì comment dòng này
            // this.transform.localScale = Vector3.one;
        });
    }

    private void RemovePlate()
    {
        StartCoroutine(RemovePlateCoroutine());
    }

    private IEnumerator RemovePlateCoroutine()
    {
        SplineFollower plateFollow = plate.GetComponent<SplineFollower>();
        double currentPercent = follower.GetPercent();
        plate.transform.SetParent(null);
        plateFollow.spline = follower.spline;
        plateFollow.enabled = true;

        yield return null;

        plateFollow.SetPercent(currentPercent);
        plateFollow.follow = true;
        plateFollow.onEndReached += OnPlateReachEnd;
    }

    private void OnPlateReachEnd(double d)
    {
        Destroy(plate);
    }

    public void ChangeCardColor()
    {
        if (cardList.HaveMechanic)
        {
            if (cardList.mechanicType == CardMechanic.HiddenColor) return;
        }
        foreach (var render in rend)
        {
            Material newMat = objectColor.GetColor(color);
            render.material = newMat;
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
}
