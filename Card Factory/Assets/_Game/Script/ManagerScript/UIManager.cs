using Christina.UI;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public Canvas mainCanvas;
    public FeatureUIView featureUIView;
    public BoosterPopUp boosterView;
    public LevelCompleteView levelCompleteView;
    public ReviveUIView reviveUIView;
    public LevelFailView levelFailView;
    public SettingUIView settingUIView;
    public QuitNotifyView quitNotifyView;

    public Button addQueueButton;
    public Button OnCloseShopUI;


    public Button addCapacityConvey;
    public Button swapBt;
    public Button removePack;

    public Sprite canBuyImage;
    public Sprite cantBuyImage;

    public TMP_Text addSlotTxt;
    public TMP_Text coinText;


    [Header("====BoosterUI====")]
    public TMP_Text addConveyCost;
    public TMP_Text addConvetCount;
    public TMP_Text swapCost;
    public TMP_Text swapCount;
    public TMP_Text removePackCost;
    public TMP_Text removePackCount;
    public TMP_Text addQueuesCost;
    public TMP_Text levelText;

    public GameObject addConveyPopUp;
    public GameObject addQueueBooster;
    public GameObject swapBooster;
    public GameObject destroyPackBooster;


    public Dictionary<BoosterType, TMP_Text> boostersCostText;
    public Dictionary<BoosterType, TMP_Text> boostersCountText;
    public Dictionary<BoosterType, GameObject> boosterObject;

    public GameObject shopUI;
    public GameObject boosterNotice;
    private Vector2 _noticeShowPos;
    private Vector2 _noticeHidePos;
    BoosterManager boosterManager;


    public void InitUI()
    {
        boosterManager = GameManager.Ins.BoosterManager;
        boostersCostText = new Dictionary<BoosterType, TMP_Text>
        {
            { BoosterType.Swap,swapCost },
            {BoosterType.AddConvey,addConveyCost},
            {BoosterType.RemovePack,removePackCost},
        };

        boostersCountText = new Dictionary<BoosterType, TMP_Text>
        {
            {BoosterType.Swap,swapCount },
            {BoosterType.AddConvey,addConvetCount},
            {BoosterType.RemovePack,removePackCount},
        };

        boosterObject = new Dictionary<BoosterType, GameObject>
        {
            {BoosterType.Swap,swapBooster },
            {BoosterType.AddConvey,addQueueBooster},
            {BoosterType.RemovePack,destroyPackBooster},
        };
        OnCheckQueueCount();
        EventManager.OnQueueCountChange.AddListener(OnCheckQueueCount);
        EventManager.OnCoinchange.AddListener(OnShowCoinUI);
        EventManager.OnBoosterCountChange.AddListener(OnShowBoosterCount);
        EventManager.OnBoosterCountChange.AddListener(OnShowBoosterCost);
        OnShowCoinUI(GameDataManager.Ins.gamedata.currentCoin);
        SetAddSlotText();
        RectTransform ogrNoticePos = boosterNotice.GetComponent<RectTransform>();
        _noticeShowPos = ogrNoticePos.anchoredPosition;
        OnShowLevelText();
        OnShowBoosterCost();
        OnShowBoosterCount();
        OnShowAddSlotCost();    
        OnCheckLevelBooster();
        float canvasWidth = ((RectTransform)mainCanvas.transform).rect.width;
        _noticeHidePos = new Vector2(canvasWidth, _noticeShowPos.y);
    }


    public void OnCheckQueueCount()
    {
        if(GameManager.Ins.isFirstTime)
        {
            StartCoroutine(WaitTostartAddQueueTut());
        }
        addQueueButton.image.sprite = GameManager.Ins.QueueManager.extendSlot.Count > 0 ? canBuyImage : cantBuyImage;
    }

    IEnumerator WaitTostartAddQueueTut()
    {
        addQueueButton.gameObject.SetActive(false);
        yield return new WaitUntil(() => TutorialInGameManager.Ins.isOnTutorial);
        addQueueButton.gameObject.SetActive(true);
        addQueuesCost.text = "Free";
        yield return new WaitUntil(() => !TutorialInGameManager.Ins.isOnTutorial);
        OnShowAddSlotCost();
    }

    public void SetAddSlotText()
    {
        addSlotTxt.text = "+ " + GameManager.Ins.BoosterManager.NumberSlotAdd + " Slots";
    }

    public void OnShowPopUp(GameObject popUp, bool isShow)
    {
        popUp.SetActive(isShow);
    }

    public void OnShowSettingPopUp(GameObject pannel)
    {
        if(settingUIView.gameObject.activeSelf)
        {
            settingUIView.OnResumePress();
            return;
        }
        settingUIView.gameObject.SetActive(true);
    }

    public void OnReplayPress()
    {
        if (!LevelManager.Ins.isGameOver)
        {
            if (GameManager.Ins.GetCurrentHearts() <= 0)
            {
                return;
            }
            else
            {
                GameManager.Ins.SpendHeart();
            }
        }
        GameManager.Ins.OnChangeGameState(GameManager.GameState.GamePlay);
        GameManager.Ins.InitGame();
    }


    public void OnShowCoinUI(int currentCoin)
    {
        coinText.text = currentCoin.ToString();
    }

    public void OnShowAddSlotCost()
    {
        addQueuesCost.text = "+ " + boosterManager.config.GetBoosterByType(BoosterType.AddQueueSlot).boosterCost.ToString();
        addQueuesCost.color = GameManager.Ins.currentGold >= boosterManager.config.GetBoosterByType(BoosterType.AddQueueSlot).boosterCost ? Color.white : Color.red;
    }

    public void OnShowBoosterCount()
    {
        foreach (var key in boostersCountText.Keys)
        {
            boostersCountText[key].transform.parent.gameObject.SetActive(boosterManager.boosterCount[key] > 0);
            boostersCountText[key].text = boosterManager.boosterCount[key].ToString();
        }
    }

    public void OnShowBoosterCost()
    {
        foreach (var key in boostersCostText.Keys)
        {
            boostersCostText[key].transform.parent.gameObject.SetActive(GameManager.Ins.currentLevel >= GameManager.Ins.boosterConfig.GetBoosterByType(key).levelUnlock);
            if (GameManager.Ins.BoosterManager.CheckForTutBooster(key))
            {
                boostersCostText[key].transform.parent.gameObject.SetActive(false);
            }
            else
            {
                boostersCostText[key].transform.parent.gameObject.SetActive(boosterManager.boosterCount[key] <= 0);
            }
            boostersCostText[key].text = boosterManager.config.GetBoosterByType(key).boosterCost.ToString();
            boostersCostText[key].color = GameManager.Ins.currentGold >= boosterManager.config.GetBoosterByType(key).boosterCost ? Color.white : Color.red;
        }
    }

    public void OnShowShopUI()
    {
        shopUI.SetActive(true);
        RectTransform rect = shopUI.transform.GetChild(1).GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(0, Screen.height);
        rect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
    }


    public void OnShowAddConveyPopUp(RectTransform posSpawn, Vector3 uiTargetPos, Action onComplete)
    {
        GameManager.Ins.poolManager.poolPopup.Prefab = addConveyPopUp;
        GameObject popUp = GameManager.Ins.poolManager.poolPopup.Spawn(
            Vector3.zero,
            addConvetCount.transform.rotation,
            mainCanvas.transform
        );

        popUp.GetComponentInChildren<TMP_Text>().text = "+ " + GameManager.Ins.BoosterManager.NumberAdd.ToString();

        RectTransform popupRect = popUp.GetComponent<RectTransform>();
        popupRect.pivot = new Vector2(0.5f, 0.5f);
        popupRect.localScale = Vector3.one;

        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(mainCanvas.worldCamera, posSpawn.position);

        Vector2 spawnAnchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.GetComponent<RectTransform>(),
            screenPos,
            mainCanvas.worldCamera,
            out spawnAnchoredPos
        );

        popupRect.anchoredPosition = spawnAnchoredPos;
        popupRect.localPosition = new Vector3(popupRect.localPosition.x, popupRect.localPosition.y, 0);
        popupRect.DOAnchorPos(uiTargetPos, 1f)
            .OnComplete(() =>
            {
                GameManager.Ins.poolManager.poolPopup.Despawn(popUp);
                onComplete?.Invoke();
            });
    }


    public void OnShowBoosterNotice(bool isActive)
    {
        RectTransform rect = boosterNotice.GetComponent<RectTransform>();
        if (isActive)
        {
            boosterNotice.gameObject.SetActive(true);

            rect.anchoredPosition = _noticeHidePos;
            rect.DOAnchorPos(_noticeShowPos, 0.5f).SetEase(Ease.OutCubic);
        }
        else
        {
            rect.DOAnchorPos(_noticeHidePos, 0.5f).SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    boosterNotice.gameObject.SetActive(false);
                });
        }
    }

    public void OnDontUseBooster()
    {
        boosterManager.currentBooster = BoosterType.None;
        RectTransform rect = boosterNotice.GetComponent<RectTransform>();

        rect.DOAnchorPos(_noticeHidePos, 0.5f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            boosterNotice.gameObject.SetActive(false);
        });
        foreach (var holdergroup in LevelManager.Ins.holderGroups)
        {
            Collider holderCollider = holdergroup.cardHolders[0].GetComponent<Collider>();
            holdergroup.cardHolders[0].transform.DOKill();
            holdergroup.cardHolders[0].transform.localPosition = holdergroup.holderPos[0];
            holdergroup.cardHolders[0].SetPacksOutLine(false);
            holderCollider.enabled = false;
        }
    }

    public void OnCheckLevelBooster()
    {
        foreach (var boosterType in boosterObject.Keys)
        {
            bool isUnlock = GameManager.Ins.currentLevel >= GameManager.Ins.boosterConfig.GetBoosterByType(boosterType).levelUnlock;
            for (int i = 0; i < boosterObject[boosterType].transform.childCount; i++)
            {
                if (i == boosterObject[boosterType].transform.childCount - 1)
                {
                    boosterObject[boosterType].transform.GetChild(i).gameObject.SetActive(!isUnlock);
                    TMP_Text text = boosterObject[boosterType].transform.GetChild(i).GetComponentInChildren<TMP_Text>();
                    text.text = "Lvl " + GameManager.Ins.boosterConfig.GetBoosterByType(boosterType).levelUnlock;
                }
                else if (i == 0)
                {
                    boosterObject[boosterType].transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(isUnlock);
                    boosterObject[boosterType].transform.GetChild(i).transform.GetChild(1).gameObject.SetActive(!isUnlock);
                }
                else
                {
                    if (!boosterObject[boosterType].transform.GetChild(i).gameObject.activeSelf) continue;
                    boosterObject[boosterType].transform.GetChild(i).gameObject.SetActive(isUnlock);
                }
            }
        }
    }

    public void OnContinuePress()
    {
        if(GameManager.Ins.state == GameManager.GameState.WinGame)
        {
            if(GameManager.Ins.isFirstTime)
            {
                GameManager.Ins.OnChangeGameState(GameManager.GameState.GamePlay);
                LevelManager.Ins.InitForTut(GameManager.Ins.currentLevel);
                return;
            }
            GameManager.Ins.OnChangeGameState(GameManager.GameState.MainMenu);
            return;
        }
        GameManager.Ins.OnChangeGameState(GameManager.GameState.MainMenu);
        GameManager.Ins.SpendHeart();
    }

    public void OnShowLevelText()
    {
        levelText.text = "Level " + GameManager.Ins.currentLevel;
    }
}
