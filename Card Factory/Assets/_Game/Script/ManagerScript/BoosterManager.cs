using DG.Tweening;
using Dreamteck;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public GameObject hammmer;
    public GameObject hammerSmashVfx;


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
            if (GameManager.Ins.currentLevel < config.GetBoosterByType((BoosterType)boosterChoose).levelUnlock) return;
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
        if(CheckForTutBooster(BoosterType.AddConvey))
        {
            if (TutorialInGameManager.Ins.isOnTutorial)
            {
                TutorialInGameManager.Ins.OnEndStage(() =>
                {
                    Time.timeScale = 1.0f;
                    foreach (var queue in LevelManager.Ins.queues)
                    {
                        foreach (var card in queue.cards)
                        {
                            card.GetComponent<MeshCollider>().enabled = true;
                        }
                    }
                });
            }
        }    
        Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPos.position);

        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            UIManager.Ins.mainCanvas.GetComponent<RectTransform>(),
            screenPos,
            UIManager.Ins.mainCanvas.GetComponentInParent<Canvas>().worldCamera,
            out uiPos
        );

        UIManager.Ins.OnShowAddConveyPopUp(addConveyBt, uiPos, () =>
        {
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
        GameObject hammerBooster = GameManager.Ins.poolManager.objectPool.Spawn(holder.transform.position, hammmer.transform.rotation, GameManager.Ins.poolManager.objectPool.transform);
        Animator hammeranim = hammerBooster.GetComponent<Animator>();
        int destroyCount = 6 - holder.cardHolder.Count;
        UIManager.Ins.OnShowBoosterNotice(false);
        StartCoroutine(WaitForAnimationEnd(hammeranim, "HammerSmash" , () =>
        {
            GameManager.Ins.poolManager.vfxPool.Prefab = hammerSmashVfx;
            GameObject vfx = GameManager.Ins.poolManager.vfxPool.Spawn(holder.transform.position, hammerSmashVfx.transform.rotation, GameManager.Ins.poolManager.poolPopup.transform);
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
                holderGroup.CheckHolder();
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
        Sequence gatherTween = DOTween.Sequence();
        currentBooster = BoosterType.Swap;
        OnSaveBoosterCount(-1);
        List<CardList> cardGroups = new List<CardList>();
        Dictionary<CardQueue,int> currentQueueCount = new Dictionary<CardQueue,int>();
        foreach (var queue in LevelManager.Ins.queues)
        {
            queue.cardPos.Clear();
            currentQueueCount[queue] = queue.cardLists.Count;
            cardGroups.AddRange(queue.cardLists);
            queue.cards.Clear();
            foreach (var cardGroup in queue.cardLists.ToList())
            {
                if (cardGroup.HaveMechanic) continue;
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
                queue.cards.AddRange(cardGroups[0].cards);
                cardGroups[0].transform.SetParent(queue.transParent);
                cardGroups[0].transform.localPosition = Vector3.zero;
                cardGroups[0].queue = queue;
                cardGroups.RemoveAt(0);
            }
            queue.SetCardPosition();
            for (int i = 0; i<queue.cardPos.Count; i++)
            {
                moveToPos.Join(queue.cards[i].transform.DOLocalMove(queue.cardPos[i], 0.3f));
            }
        }
        moveToPos.OnComplete(() =>
        {
            currentBooster = BoosterType.None;
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
            TutorialInGameManager.Ins.OnEndStage(() =>
            {
                foreach (var cards in GameManager.Ins.QueueManager.cardInQueue)
                {
                    cards.GetComponent<MeshCollider>().enabled = true;
                }
                foreach (var queue in LevelManager.Ins.queues)
                {
                    foreach (var card in queue.cards)
                    {
                        card.GetComponent<MeshCollider>().enabled = true;
                    }
                }
            });
            GameManager.Ins.isFirstTime = false;
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
        int firstRowIndex = GameManager.Ins.QueueManager.avaliableQueues.Count / 2;
        for(int i = 0; i < NumberSlotAdd; i++)
        {
            GameManager.Ins.QueueManager.cardInQueue.Add(null);
        }
        List<QueueSlot> extend = GameManager.Ins.QueueManager.extendSlot.ToList();
        for (int i = 0; i<NumberSlotAdd; i++)
        {
            QueueSlot slotAdd = extend[i];
            GameManager.Ins.QueueManager.extendSlot.Remove(slotAdd);
            slotAdd.gameObject.SetActive(true);
            if(i % 2 == 0)
            {
                GameManager.Ins.QueueManager.avaliableQueues.Insert(firstRowIndex + i/2,slotAdd);
            }
            else
            {
                GameManager.Ins.QueueManager.avaliableQueues.Add(slotAdd);
            }
        }
        EventManager.OnQueueCountChange.Invoke();
        queueCover.DOLocalMove(new Vector3(queueCover.localPosition.x - 1.3f * (NumberSlotAdd / 2), queueCover.localPosition.y, queueCover.localPosition.z), 0.1f)
            .OnComplete(() =>
            {
                GameManager.Ins.QueueManager.ReOrderQueue((index, card) =>
                {
                    if (card == null) return;
                    card.currentQueueSlot?.SetCardOnQueue(null);
                    card.currentQueueSlot = GameManager.Ins.QueueManager.avaliableQueues[index];
                    GameManager.Ins.QueueManager.avaliableQueues[index].SetCardOnQueue(card);
                });
            });
    }

    public bool CheckForTutBooster(BoosterType type)
    {
        return GameManager.Ins.currentLevel == GameManager.Ins.boosterConfig.GetBoosterByType(type).levelUnlock;
    }
}
