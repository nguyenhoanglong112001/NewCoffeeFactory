using ColorBlockJam;
using DG.Tweening;
using Dreamteck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = System.Random;

public enum BoosterType
{
    AddConvey = 1,
    RemovePack = 2,
    Swap = 3,
    AddQueueSlot = 4,
    None = 5
}
public class BoosterManager : MonoBehaviour
{
    public BoosterType currentBooster;
    public BoosterConfig config;
    [SerializeField] private int numberAdd;
    [SerializeField] private Transform queueCover;
    [SerializeField] private int numberSlotAdd;
    public RectTransform addConveyBt;
    public Transform targetPos;

    public int addConveyCount;
    public int swapCount;
    public int removePackCount;

    public Dictionary<BoosterType, int> boosterCount;
    public Dictionary<BoosterType, GameObject> boosterImageLock;

    public GameObject hammmer;
    public GameObject starVfx;
    public GameObject addQueuePopUp;

    public GameObject addConveyLockImage;
    public GameObject removePackLockImage;
    public GameObject swapLockImage;

    private Tween shakeTween;

    [Header("HandTutPos")]
    public Transform swapTutHandPos;
    public Transform addConveyHAndPos;

    public int NumberSlotAdd { get => numberSlotAdd; set => numberSlotAdd = value; }
    public int NumberAdd { get => numberAdd; set => numberAdd = value;}


    public void InitBooster()
    {
        currentBooster = BoosterType.None;
        boosterCount = new Dictionary<BoosterType, int>
        {
            {BoosterType.Swap,swapCount },
            {BoosterType.AddConvey,addConveyCount},
            {BoosterType.RemovePack,removePackCount},
        };
        boosterImageLock = new Dictionary<BoosterType, GameObject>
        {
            {BoosterType.Swap,swapLockImage },
            {BoosterType.RemovePack,removePackLockImage},
            {BoosterType.AddConvey,addConveyLockImage},
        };
    }
    public void SetCurrentBooster(int boosterChoose)
    {
        if(CheckForTutBooster((BoosterType)boosterChoose))
        {
            if(!TutorialInGameManager.Ins.isOnTutorial)
            {
                return;
            }
        }
        if (boosterCount[(BoosterType)boosterChoose] <= 0)
        {
            if (GameManager.Ins.currentLevel < config.GetBoosterByType((BoosterType)boosterChoose).levelUnlock)
            {
                BoosterLockAnima((BoosterType)boosterChoose);
                return;
            }
            if(GameManager.Ins.currentGold < config.GetBoosterByType((BoosterType)boosterChoose).boosterCost)
            {
                UIManager.Ins.OnShowShopUI();
                return;
            }
            else
            {
                GameManager.Ins.OnUpdateCoin(-config.GetBoosterByType((BoosterType)boosterChoose).boosterCost);
                currentBooster = (BoosterType)boosterChoose;
                OnSaveBoosterCount(1);
                UseBooster();
                return;
            }
        }
        currentBooster = (BoosterType)boosterChoose;
        UseBooster();
    }

    private void BoosterLockAnima(BoosterType boosterType)
    {
        if (shakeTween != null && shakeTween.IsPlaying())
            return; // ?ang shake thì không shake n?a
        RectTransform rect = boosterImageLock[boosterType].GetComponent<RectTransform>();
        shakeTween = rect.DOShakeAnchorPos(
            duration: 0.5f,
            strength: new Vector2(30,40),
            vibrato: 10,
            randomness: 90,
            snapping: false,
            fadeOut: true
        );
}

    public void UseBooster()
    {
        switch (currentBooster)
        {
            case BoosterType.AddConvey:
                {
                    OnUseAddConvey();
                    break;
                }
            case BoosterType.RemovePack:
                {
                    UIManager.Ins.OnShowBoosterNotice(true);
                    foreach (var holdergroup in LevelManager.Ins.holderGroups)
                    {
                        Collider holderCollider = holdergroup.cardHolders[0].GetComponent<Collider>();
                        Vector3 upPos = holdergroup.cardHolders[0].transform.position + Vector3.up * 0.5f;
                        Vector3 downPos = holdergroup.cardHolders[0].transform.position;
                        holdergroup.cardHolders[0].transform.DOMoveY(upPos.y, 1.5f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                        holdergroup.cardHolders[0].SetPacksOutLine(true);
                        holderCollider.enabled = true;
                    }
                    break;
                }
            case BoosterType.Swap:
                {
                    OnUseSwapBooster();
                    break;
                }
        }
    }

    public void OnUseAddConvey()
    {
        if (TutorialInGameManager.Ins.isOnTutorial)
        {
            if (CheckForTutBooster(BoosterType.AddConvey))
            {
                TutorialInGameManager.Ins.OnEndStage(() =>
                {
                    Time.timeScale = 1.0f;
                    foreach (var queue in LevelManager.Ins.queues)
                    {
                        foreach (var card in queue.cards)
                        {
                            card.canPress = true;
                        }
                    }
                });
            }
            else
            {
                return;
            }
        }
        RectTransform mainCanvas = UIManager.Ins.mainCanvas.GetComponent<RectTransform>();
        Vector3 uiPos = Help.ConvertWorldToUIPosition(targetPos.position, mainCanvas);
        UIManager.Ins.OnShowAddConveyPopUp(addConveyBt, uiPos, () =>
        {
            GameManager.Ins.poolManager.vfxPool.Prefab = starVfx;
            GameObject vfx = GameManager.Ins.poolManager.vfxPool.Spawn(targetPos.position, starVfx.transform.rotation, GameManager.Ins.poolManager.poolPopup.transform);
            GameManager.Ins.ConveyorManager.AddMaxCardOnConvey(NumberAdd);
            OnSaveBoosterCount(-1);
            currentBooster = BoosterType.None;
        });
    }

    private void OnSaveBoosterCount(int amountCount)
    {
        boosterCount[currentBooster] += amountCount;
        GameDataManager.Ins.gamedata.SetBoosterCount(currentBooster, boosterCount[currentBooster]);
        if(!CheckForTutBooster(currentBooster))
        {
            GameManager.Ins.SaveData();
        }
        EventManager.OnBoosterCountChange.Invoke();
    }

    private void OnAddBoosterCount(BoosterType type)
    {
        boosterCount[type]++;
        EventManager.OnBoosterCountChange.Invoke();
    }

    public void OnUseDestroyPack(CardHolder holder)
    {
        CardColor holderColor = holder.colorHolder;
        GameManager.Ins.poolManager.objectPool.Prefab = hammmer;
        Vector3 hammerPosSpawn = Help.ConvertUIToWorldPosition(UIManager.Ins.removePack.GetComponent<RectTransform>());
        GameObject hammerBooster = GameManager.Ins.poolManager.objectPool.Spawn(hammerPosSpawn, hammmer.transform.rotation, GameManager.Ins.poolManager.objectPool.transform);
        Animator hammeranim = hammerBooster.GetComponent<Animator>();
        hammerBooster.transform.DOMove(holder.transform.position, 0.5f)
            .OnComplete(() =>
            {
                hammeranim.SetTrigger("Hit");
            });
        int destroyCount = 6 - holder.cardHolder.Count;
        UIManager.Ins.OnShowBoosterNotice(false);
        StartCoroutine(WaitForAnimationEnd(hammeranim, "HammerSmash" , () =>
        {
            GameManager.Ins.poolManager.vfxPool.Prefab = starVfx;
            GameObject vfx = GameManager.Ins.poolManager.vfxPool.Spawn(holder.transform.position, starVfx.transform.rotation, GameManager.Ins.poolManager.poolPopup.transform);
            List<Card> cards = LevelManager.Ins.cards.ToList();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                if (destroyCount >= 6) break;
                if (cards[i].color == holderColor)
                {
                    LevelManager.Ins.cards.Remove(cards[i]);
                    cards[i].cardQueue.cards.Remove(cards[i]);
                    cards[i].cardQueue.AllCardOnQueueMove();
                    GameManager.Ins.poolManager.cardPool.Despawn(cards[i].gameObject);
                    destroyCount++;
                }
            }
            holder.transform.DOScale(Vector3.zero, 0.3f)
            .OnComplete(() =>
            {
            foreach (var holdergroup in LevelManager.Ins.holderGroups)
            {
                Collider holderCollider = holdergroup.cardHolders[0].GetComponent<Collider>();
                holdergroup.cardHolders[0].transform.DOKill();
                holdergroup.cardHolders[0].transform.localPosition = holdergroup.holderPos[0];
                holderCollider.enabled = false;
                holdergroup.cardHolders[0].SetPacksOutLine(false);
            }
            CardHolderGroup holderGroup = holder.holderGroup;
            GameManager.Ins.poolManager.packPool.Despawn(holder.gameObject);
            holderGroup.AllHolderMoveFront(() =>
            {
                holderGroup.cardHolders.Remove(holder);
                foreach (var holder in holderGroup.cardHolders)
                {
                    holder.CheckToShowHolder();
                }
            });
        });
            OnSaveBoosterCount(-1);
            currentBooster = BoosterType.None;
        }));
    }

    IEnumerator WaitForAnimationEnd(Animator animator, string stateName,Action actionA = null)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.75f)
            yield return null;
        actionA?.Invoke();

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
        GameManager.Ins.poolManager.objectPool.Despawn(animator.gameObject);
    }

    public void OnUseSwapBooster()
    {
        Time.timeScale = 1.0f;
        foreach (var tile in LevelManager.Ins.queues)
        {
            foreach (var card in tile.cards)
            {
                card.canPress = false;
            }
        }
        Sequence gatherTween = DOTween.Sequence();
        currentBooster = BoosterType.Swap;
        OnSaveBoosterCount(-1);
        List<CardList> cardGroups = new List<CardList>();
        Dictionary<CardQueue,int> currentQueueCount = new Dictionary<CardQueue,int>();
        foreach (var queue in LevelManager.Ins.queues)
        {
            queue.cards.Clear();
            currentQueueCount[queue] = queue.cardLists.Count;
            foreach (var cardGroup in queue.cardLists.ToList())
            {
                if (cardGroup.HaveMechanic) continue;
                cardGroups.Add(cardGroup);
                if (cardGroup.queue != null) 
                {
                    int index = cardGroup.queue.cardLists.IndexOf(cardGroup);
                    cardGroup.queue.cardLists[index] = null;
                    cardGroup.queue = null;
                }
                foreach (var card in cardGroup.cards)
                {
                    gatherTween.Join(card.transform.DOMove(Vector3.zero, 0.3f));
                }
            }
        }
        gatherTween.OnComplete(() =>
        {
            ShufferPos(cardGroups,currentQueueCount);
        });
    }


    public void ShufferPos(List<CardList> cardGroups,Dictionary<CardQueue,int> queuesCount)
    {
        Sequence moveToPos = DOTween.Sequence();
        Constans.Shuffle(cardGroups);
        foreach(var queue in queuesCount.Keys)
        {
            if (queuesCount[queue] <= 0) continue;
            for(int i = 0; i< queuesCount[queue]; i++)
            {
                if (queue.cardLists[i] != null) continue;
                queue.cardLists[i] = cardGroups[0];
                cardGroups[0].transform.SetParent(queue.transParent);
                cardGroups[0].transform.localPosition = Vector3.zero;
                cardGroups[0].queue = queue;
                foreach (var card in cardGroups[0].cards)
                {
                    card.cardQueue = queue;
                }
                cardGroups.RemoveAt(0);
            }
            foreach (var card in queue.cardLists)
            {
                queue.cards.AddRange(card.cards);
            }
            int cardCount = queue.cards.Count;
            int posCount = queue.cardPos.Count;
            if (cardCount > posCount)
            {
                for (int i = 0;i< cardCount - posCount; i++)
                {
                    Vector3 newPos = queue.cardPos[queue.cardPos.Count - 1] + new Vector3(1, 0, 0);
                    queue.cardPos.Add(newPos);
                }
            }
            for (int i = 0; i<queue.cards.Count; i++)
            {
                moveToPos.Join(queue.cards[i].transform.DOLocalMove(queue.cardPos[i], 0.3f));
            }
        }
        moveToPos.OnComplete(() =>
        {
            currentBooster = BoosterType.None;
            foreach (var tile in LevelManager.Ins.queues)
            {
                foreach (var card in tile.cards)
                {
                    card.canPress = true;
                }
            }
            if(TutorialInGameManager.Ins.isOnTutorial)
            {
                OnCompleteTutBooster();
            }
        });
    }

    public void OnAddQueueSlot()
    {
        if(GameManager.Ins.QueueManager.extendSlot.Count <= 0)
        {
            return;
        }
        if(TutorialInGameManager.Ins.isOnTutorial)
        {
            OnCompleteTutBooster();
        }
        else
        {
            if (GameManager.Ins.currentGold < config.GetBoosterByType(BoosterType.AddQueueSlot).boosterCost)
            {
                UIManager.Ins.OnShowShopUI();
                return;
            }
            GameManager.Ins.OnUpdateCoin(-config.GetBoosterByType(BoosterType.AddQueueSlot).boosterCost);
        }
        addQueuePopUp.SetActive(true);
        StartCoroutine(HandlePopUpAnim(addQueuePopUp.GetComponent<Animator>(), "Textspawn", () => addQueuePopUp.SetActive(false)));
        AddSlotToQueue();
    }

    private float distanceMove = 1.4f;
    private void AddSlotToQueue()
    {
        int firstRowIndex = GameManager.Ins.QueueManager.avaliableQueues.Count / 2;
        for (int i = 0; i < NumberSlotAdd; i++)
        {
            GameManager.Ins.QueueManager.cardInQueue.Add(null);
        }
        List<QueueSlot> extend = GameManager.Ins.QueueManager.extendSlot.ToList();
        for (int i = 0; i < NumberSlotAdd; i++)
        {
            QueueSlot slotAdd = extend[i];
            GameManager.Ins.QueueManager.extendSlot.Remove(slotAdd);
            slotAdd.gameObject.SetActive(true);
            if (i % 2 == 0)
            {
                GameManager.Ins.QueueManager.avaliableQueues.Insert(firstRowIndex + i / 2, slotAdd);
            }
            else
            {
                GameManager.Ins.QueueManager.avaliableQueues.Add(slotAdd);
            }
        }
        EventManager.OnQueueCountChange.Invoke();
        QueueManager queue = GameManager.Ins.QueueManager;
        Sequence s = DOTween.Sequence();
        distanceMove = distanceMove - numberSlotAdd / 10f;
        if(distanceMove <= 0)
        {
            distanceMove = 0;
            Vector3 originalScale = queueCover.localScale;
            Vector3 originalPos = queueCover.position;

            // Tính khoảng cách cần di chuyển để giữ bên trái cố định
            float scaleChange = originalScale.x - 0.1f;
            float moveDistance = scaleChange * 0.5f;

            // Scale X và move sang trái cùng lúc
            s.Append(queueCover.DOScaleX(scaleChange, 0.1f));
            s.Join(queueCover.DOMoveX(originalPos.x - moveDistance, 0.1f)); // Đổi dấu thành âm

            s.SetEase(Ease.OutQuad);
        }
        else
        {
            s.Append(queueCover.DOLocalMove(new Vector3(queueCover.localPosition.x - 1.4f * (NumberSlotAdd / 2), queueCover.localPosition.y, queueCover.localPosition.z), 0.1f));
            s.Append(queue.transform.DOMoveX(queue.transform.position.x + distanceMove, 0.1f));
        }
        s.OnComplete(() =>
        {
            GameManager.Ins.QueueManager.ReOrderQueue();
        });
    }

    public void OnCompleteTutBooster()
    {
        TutorialInGameManager.Ins.OnEndStage(() =>
        {
            if(GameManager.Ins.QueueManager.cardInQueue.Count > 0)
            {
                foreach (var cards in GameManager.Ins.QueueManager.cardInQueue)
                {
                    if (cards != null) cards.canPress = true;
                }
            }
            foreach (var queue in LevelManager.Ins.queues)
            {
                foreach (var card in queue.cards)
                {
                    card.canPress = true;
                }
            }
        });
        GameManager.Ins.isFirstTime = false;
    }

    IEnumerator HandlePopUpAnim(Animator animator,string stateName,Action callback)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
        callback?.Invoke();
    }

    public bool CheckForTutBooster(BoosterType type)
    {
        return GameManager.Ins.currentLevel == GameManager.Ins.boosterConfig.GetBoosterByType(type).levelUnlock;
    }

    public void InitSwapTut()
    {
        if (CheckForTutBooster(BoosterType.Swap))
        {
            if (TutorialInGameManager.Ins.currentTutIndex == 5)
            {
                TutorialInGameManager.Ins.StartTut(() =>
                {
                    boosterCount[BoosterType.Swap] = 4;
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
                currentStage.SetHandPointPos(swapTutHandPos.position);
            }
        }
    }
}
