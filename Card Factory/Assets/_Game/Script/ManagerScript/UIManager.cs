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

    public Button addQueueButton;
    public Button OnCloseShopUI;
    public Button continueBt;
    public Button giveUpBt;
    public Button replayBt;
    public Button restarQuitBt;
    public Button quitBt;
    public Button restartBt;

    public Button addCapacityConvey;
    public Button swapBt;
    public Button removePack;

    public Sprite canBuyImage;
    public Sprite cantBuyImage;
    public Sprite unlockBtImage;
    public Sprite lockBtImage;

    public TMP_Text addSlotTxt;
    public TMP_Text coinText;
    public TMP_Text rewardText;

    [Header("====BoosterUI====")]
    public TMP_Text addConveyCost;
    public TMP_Text addConvetCount;
    public TMP_Text swapCost;
    public TMP_Text swapCount;
    public TMP_Text removePackCost;
    public TMP_Text removePackCount;
    public TMP_Text addQueuesCost;
    public TMP_Text levelText;
    public TMP_Text reviveCostText;

    public GameObject completeUI;
    public GameObject failUI;
    public GameObject reviveUI;
    public GameObject addConveyPopUp;
    public GameObject addQueueBooster;
    public GameObject swapBooster;
    public GameObject destroyPackBooster;
    public GameObject SettingPopUp;
    public GameObject quitNotify;
    public GameObject notify;


    public Dictionary<BoosterType, TMP_Text> boostersCostText;
    public Dictionary<BoosterType, TMP_Text> boostersCountText;
    public Dictionary<BoosterType, GameObject> boosterObject;

    public Slider musicSlider;
    public Slider soundSlider;

    public ToggleSwitch musicToggle;
    public ToggleSwitch soundToggle;

    public GameObject shopUI;
    public GameObject boosterNotice;
    private Vector2 _noticeShowPos;
    private Vector2 _noticeHidePos;
    private Vector2 _notifyOrgPos;
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
        musicSlider.onValueChanged.AddListener(AudioManager.Ins.SetMusicVolume);
        soundSlider.onValueChanged.AddListener(AudioManager.Ins.SetSFXVolume);

        musicToggle.sliderValue = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundToggle.sliderValue = PlayerPrefs.GetFloat("SoundVolume", 1f);
        OnCheckBt();
    }

    private void Start()
    {
        continueBt.onClick.AddListener(featureUIView.OnShowProgressFeature);
        giveUpBt.onClick.AddListener(OnContinuePress);
        _notifyOrgPos = notify.GetComponent<RectTransform>().anchoredPosition;
        quitBt.onClick.AddListener(() =>
        {
            OnQuitPress();
            restarQuitBt.onClick.RemoveAllListeners();
            restarQuitBt.GetComponentInChildren<TMP_Text>().text = "Quit";
            restarQuitBt.onClick.AddListener(OnContinuePress);
        });
        restartBt.onClick.AddListener(() =>
        {
            OnQuitPress();
            restarQuitBt.onClick.RemoveAllListeners();
            restarQuitBt.GetComponentInChildren<TMP_Text>().text = "Replay";
            restarQuitBt.onClick.AddListener(OnReplayPress);
        });
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

    public void OnShowWinUI()
    {
        CanvasGroup canvasgroup = completeUI.GetComponent<CanvasGroup>();
        OnShowRewardText();
        completeUI.SetActive(true);
        canvasgroup.alpha = 0f;
        canvasgroup.DOFade(1f, 0.3f)
            .SetUpdate(true);
    }

    public void OnShowFailUI()
    {
        CanvasGroup canvasgroup = failUI.GetComponent<CanvasGroup>();
        failUI.SetActive(true);
        canvasgroup.alpha = 0f;
        canvasgroup.DOFade(1f, 0.3f).SetUpdate(true);
    }

    public void OnShowReviveUI()
    {
        CanvasGroup canvasgroup = reviveUI.GetComponent<CanvasGroup>();
        reviveUI.SetActive(true);
        canvasgroup.alpha = 0f;
        canvasgroup.DOFade(1f, 0.3f).SetUpdate(true);
        reviveCostText.text = GameManager.Ins.rewardConfig.reviveCost.ToString();
    }

    public void OnShowSettingPopUp()
    {
        SettingPopUp.SetActive(true);
        Time.timeScale = 0.0f;
    }

    public void OnNoRevivePress()
    {
        reviveUI.SetActive(false);
        OnShowFailUI();
    }

    public void OnRevivePress()
    {
        if(GameManager.Ins.currentGold < GameManager.Ins.rewardConfig.reviveCost)
        {
            OnShowShopUI();
        }
        else
        {
            Time.timeScale = 1.0f;
            GameManager.Ins.BoosterManager.OnAddQueueSlot();
            LevelManager.Ins.reviveTime--;
            LevelManager.Ins.isReviveWait = false;
            
            reviveUI.SetActive(false);
        }
    }

    public void OnShowCoinUI(int currentCoin)
    {
        coinText.text = currentCoin.ToString();
    }

    public void OnShowRewardText()
    {
        rewardText.text = GameManager.Ins.rewardConfig.coinReward.ToString();
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
        rect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnCloseBtPress(GameObject ui)
    {
        ui.SetActive(false);
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
            boosterObject[boosterType].GetComponent<Image>().sprite = isUnlock ? unlockBtImage : lockBtImage;
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

    public void OnReplayPress()
    {
        if(!LevelManager.Ins.isGameOver)
        {
            if(GameManager.Ins.GetCurrentHearts() <= 0)
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

    public void OnResumePress()
    {
        Time.timeScale = 1.0f;
        SettingPopUp.SetActive(false);
    }

    public void OnQuitPress()
    {
        quitNotify.SetActive(true);
        RectTransform rect = notify.GetComponent<RectTransform>();

        rect.anchoredPosition = _notifyOrgPos + new Vector2(0, -Screen.height);
        rect.DOAnchorPos(_notifyOrgPos, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnNoLeavePress()
    {
        RectTransform rect = notify.GetComponent<RectTransform>();
        Vector2 currentPos = rect.anchoredPosition;

        rect.DOAnchorPos(currentPos + new Vector2(0, -Screen.height), 0.3f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                quitNotify.SetActive(false);
            });
    }

    public void OnCheckBt()
    {
        quitBt.gameObject.SetActive(!GameManager.Ins.isFirstTime);
        restartBt.gameObject.SetActive(!GameManager.Ins.isFirstTime);
    }
}
