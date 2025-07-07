using Dreamteck.Splines;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : Singleton<LevelManager>
{

    public LevelDataSO level;
    [SerializeField] private int numberHolder;
    private int numberCard;

    public List<CardColor> colorsUse;
    public List<CardHolderGroup> holderGroups;
    public List<CardQueue> queues;
    public List<Card> cards;

    public CardHolder packsPrefab;
    public CardList cardListPrefab;

    public GridLayoutGroup3D objectLayout;

    public int numberCardList;

    public int reviveTime;
    public bool isReviveWait;

    public bool isGameOver;
    public bool isLevelComplete;
    [Header("====Load Level====")]
    [SerializeField] private Transform conveySpawnPos;

    private void Start()
    {

    }

    public void InitLevel()
    {
        if (GameManager.Ins.isFirstTime)
        {
            InitForTut(GameManager.Ins.currentLevel);
        }
        else
        {
            level = Resources.Load<LevelDataSO>("LevelData/Level " + GameManager.Ins.currentLevel);
            LevelLoader();
        }
        numberHolder = GetTotalHolder();
        numberCard = CaculateNumberCard();
        AddCardToList();
        reviveTime = 1;
        isGameOver = false;
        isLevelComplete = false;
        if(GameManager.Ins.BoosterManager.CheckForTutBooster(BoosterType.Swap))
        {
            UIManager.Ins.boosterView.OnShowPopUp();
        }
    }

    public void InitForTut(int levelLoad)
    {
        level = Resources.Load<LevelDataSO>("LevelData/Level " + levelLoad);
        LevelLoader();
        if (levelLoad == 1)
        {
            foreach (var card in queues[1].cards)
            {
                card.canPress = false;
            }
            TutorialInGameManager.Ins.StartTut();
            TutorialStage currentStage = TutorialInGameManager.Ins.GetCurrentTut().GetCurrentStage();
            currentStage.SetHandPointPos(queues[0].cardPos[0], new Vector3(-1, 3, -9));
        }
        if (levelLoad == 2)
        {
            TutorialInGameManager.Ins.StartTut();
            TutorialStage currentStage = TutorialInGameManager.Ins.GetCurrentTut().GetCurrentStage();
            currentStage.SetHandPointPos(queues[0].cardPos[0], new Vector3(-1, 3, -9));
        }
    }


    private void SpawnCardList()
    {
    }

    private int GetNumberHolder()
    {
        return numberHolder;
    }

    private int GetNumberCard()
    {
        return numberCard;
    }

    private void AddCardToList()
    {
        foreach (var cardQueue in queues)
        {
            cards.AddRange(cardQueue.cards);
        }
    }
    private int CaculateNumberCard()
    {
        int card = 0;
        foreach (var cardQueue in queues)
        {
            card += cardQueue.cards.Count;
        }
        return card;
    }

    private int GetTotalHolder()
    {
        int numberHolder = 0;
        foreach (CardHolderGroup group in holderGroups)
        {
            numberHolder += group.cardHolders.Count;
        }
        return numberHolder;
    }

    public void OnCheckWingame()
    {
        numberHolder = GetTotalHolder();
        if (numberHolder <= 0)
        {
            isGameOver = true;
            isLevelComplete = true;
            AudioManager.Ins.PlaySound("CompleteSound");
            UIManager.Ins.OnShowWinUI();
            EventManager.OnLevelComplete.Invoke();
            GameManager.Ins.currentLevel++;
            GameDataManager.Ins.gamedata.currentLevel = GameManager.Ins.currentLevel;
            GameManager.Ins.SaveData();
            GameManager.Ins.OnUpdateCoin(GameManager.Ins.rewardConfig.coinReward);
            GameManager.Ins.OnChangeGameState(GameManager.GameState.WinGame);
            Time.timeScale = 0.0f;
        }
    }

    public void OnLevelFail()
    {
        AudioManager.Ins.PlaySound("FailSound");
        GameManager.Ins.OnChangeGameState(GameManager.GameState.GameOver);
        isGameOver = true;
        if (reviveTime > 0)
        {
            UIManager.Ins.OnShowReviveUI();
            Time.timeScale = 0.0f;
            isReviveWait = true;
        }
        else
        {
            Debug.Log("Level Fail");
            UIManager.Ins.OnShowFailUI();
            Time.timeScale = 0.0f;
        }
    }

    public void LevelLoader()
    {
        SpawnConvey();
        SpawnQueue();
    }
    private void SpawnQueue()
    {
        CardQueue queuePrefab = level.queuePrefab;
        CardList cardPrefab = level.CoffeCups;
        if (queuePrefab != null)
        {
            objectLayout.constraintCount = level.queueData.Count;
            foreach (var queue in level.queueData)
            {
                GameObject queueObj = Instantiate(queuePrefab.gameObject, Vector3.zero, queuePrefab.transform.rotation, objectLayout.transform);
                CardQueue cardqueue = queueObj.GetComponent<CardQueue>();
                queues.Add(cardqueue);
                foreach (var cardList in queue.cardData)
                {
                    GameManager.Ins.poolManager.cardGroupPool.Prefab = cardPrefab.gameObject;
                    GameObject cardObj = GameManager.Ins.poolManager.cardGroupPool.Spawn(Vector3.zero, Quaternion.identity);
                    cardObj.transform.SetParent(cardqueue.transParent);
                    cardObj.transform.localPosition = Vector3.zero;
                    cardObj.transform.localRotation = Quaternion.identity;
                    CardList card = cardObj.GetComponent<CardList>();
                    card.numberCard = cardList.cardNumber;
                    card.listColor = (CardColor)cardList.cardColorIndex;
                    card.mechanicType = cardList.cardMechanic;
                    cardqueue.cardLists.Add(card);
                }
                cardqueue.SpawnCardList();
            }
            objectLayout.ArrangeChildren();
        }
    }
    private void SpawnConvey()
    {
        GameObject conveyPrefab = level.ConveyorPrefab.gameObject;
        SplineComputer splineCom = null;
        CardHolderGroup holderGroupPrefab = level.holderGroupPrefab;
        if (conveyPrefab != null)
        {
            Vector3 objSpawnPos = conveySpawnPos.position + level.ConveyorPrefab.ConveyorOffset;
            GameObject conveyor = Instantiate(conveyPrefab, objSpawnPos, Quaternion.identity, conveySpawnPos);
            ConveyorManager conveyorManager = conveyor.GetComponent<ConveyorManager>();
            GameManager.Ins.BoosterManager.targetPos = conveyorManager.boosterTargetPos;
            GameManager.Ins.ConveyorManager = conveyorManager;
            splineCom = conveyorManager.spline;
            for (int i = 0; i < level.HolderPoints.Length; i++)
            {
                float percent = level.HolderPoints[i].holderPos;
                Vector3 pos = splineCom.EvaluatePosition(percent);
                Vector3 spawnPos = pos + level.holderOffset;

                GameObject group = Instantiate(holderGroupPrefab.gameObject, spawnPos, conveyPrefab.transform.rotation, null);
                Quaternion rotation = Quaternion.Euler(level.HolderPoints[i].holderRotate);
                group.transform.rotation = rotation;


                CardHolderGroup groupHolder = group.GetComponent<CardHolderGroup>();
                groupHolder.groupPos = percent;
                holderGroups.Add(groupHolder);
                Vector3 countRotate = groupHolder.counterTransform.localEulerAngles;
                countRotate.z = level.HolderPoints[i].holderRotate.y;
                groupHolder.counterTransform.localRotation = Quaternion.Euler(countRotate);
                Debug.Log(level.HolderPoints[i].holderRotate.y);

                GameManager.Ins.poolManager.packPool.Prefab = level.CardHolder.gameObject;

                for (int j = 0; j < level.HolderPoints[i].holderDatas.Length; j++)
                {

                    GameObject holderObj = GameManager.Ins.poolManager.packPool.Spawn(groupHolder.starPos.position, Quaternion.identity);
                    holderObj.transform.SetParent(group.transform);
                    CardHolder holder = holderObj.GetComponent<CardHolder>();

                    holder.transform.localRotation = Quaternion.identity;
                    groupHolder.cardHolders.Add(holder);
                    holder.colorHolder = (CardColor)level.HolderPoints[i].holderDatas[j].holderColorIndex;
                    holder.holderGroup = groupHolder;
                    holder.currentMechanic = level.HolderPoints[i].holderDatas[j].holderMechanic;
                }
                groupHolder.OnSetPositionOnQueue();
            }
            conveyorManager.AddTrigger();
        }
    }
}
