using Christina.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : Singleton<MainMenuManager>
{

    public TMP_Text levelText;
    public TMP_Text coinText;

    public TMP_Text heartAmount;
    public TMP_Text heartTimeAmount;
    public TMP_Text heartTimePopupText;

    public Button startBt;
    public Button settingBt;
    public Button closeSettingBt;
    public Button buyHeartBtplus;
    public Button buHeartBtBg;

    public Sprite pressImage;
    public Sprite unPressImage;

    public GameObject SettingUI;
    public BuyHeartPopUpUI buyHeartUI;

    public ToggleSwitch musicToggle;
    public ToggleSwitch soundToggle;

    public Slider musicSlider;
    public Slider soundSlider;

    private Dictionary<GameObject, Vector3> originalScales = new();

    private void Start()
    {

    }

    public void InitMenuUI()
    {
        OnShowLevelText();
        OnShowCoinText(GameManager.Ins.currentGold);
        CheckStartBt();
        UpdateHeartDisplay();
        OncheckHeart();
        EventManager.onHeartChange.AddListener(UpdateHeartDisplay);
        settingBt.onClick.AddListener(() => OnShowPopUp(SettingUI, true));
        closeSettingBt.onClick.AddListener(() => OnShowPopUp(SettingUI, false));

        musicSlider.onValueChanged.AddListener(AudioManager.Ins.SetMusicVolume);
        soundSlider.onValueChanged.AddListener(AudioManager.Ins.SetSFXVolume);

        musicToggle.sliderValue = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundToggle.sliderValue = PlayerPrefs.GetFloat("SoundVolume", 1f);

        EventManager.onHeartChange.AddListener(OncheckHeart);
        EventManager.OnCoinchange.AddListener(OnShowCoinText);
    }

    private void Update()
    {
        UpdateTimeDisplay(heartTimeAmount);
    }

    public void OnShowLevelText()
    {
        levelText.text = "Level " + GameManager.Ins.currentLevel;
    }

    public void OnShowCoinText(int amount)
    {
        coinText.text = amount.ToString();
    }

    public void CheckStartBt()
    {
        startBt.GetComponent<Image>().sprite = IsOutOfLevel() ? unPressImage : pressImage;
    }

    public void OnStartPress()
    {
        if (GameManager.Ins.GetCurrentHearts() <= 0) return;
        if(!IsOutOfLevel())
        {
            GameManager.Ins.OnChangeGameState(GameManager.GameState.GamePlay);
        }
    }

    public bool IsOutOfLevel()
    {
        LevelDataSO[] allLevels = Resources.LoadAll<LevelDataSO>("LevelData");
        int maxLevelCount = allLevels.Length;
        if(GameDataManager.Ins.gamedata.currentLevel > maxLevelCount)
        {
            return true;
        }
        return false;
    }

    private void UpdateHeartDisplay()
    {
        int currentHearts = GameManager.Ins.GetCurrentHearts();


        if (currentHearts == int.MaxValue)
        {
            heartAmount.text = "\u221E";
        }
        else
        {
            heartAmount.text = $"{currentHearts}";
        }
    }

    public void UpdateTimeDisplay(TMP_Text heartTimeText)
    {

        TimeSpan timeUntilNext = GameManager.Ins.GetTimeUntilNextHeart();

        if (timeUntilNext == TimeSpan.Zero)
        {
            heartTimeText.text = "Full";
            return;
        }

        if (timeUntilNext.TotalHours >= 1)
        {
            heartTimeText.text = $"{timeUntilNext.Hours:00}:{timeUntilNext.Minutes:00}:{timeUntilNext.Seconds:00}";
        }
        else
        {
            heartTimeText.text = $"{timeUntilNext.Minutes:00}:{timeUntilNext.Seconds:00}";
        }
    }

    private void OncheckHeart()
    {
        if (GameManager.Ins.IsHeartsFull())
        {
            buyHeartBtplus.gameObject.SetActive(false);
            buHeartBtBg.onClick.RemoveAllListeners();
            return;
        }
        buyHeartBtplus.gameObject.SetActive(true);
        buyHeartBtplus.onClick.AddListener(() => OnShowPopUp(buyHeartUI.gameObject, true));
        buHeartBtBg.onClick.AddListener(() => OnShowPopUp(buyHeartUI.gameObject, true));
    }

    public void OnShowPopUp(GameObject popUp,bool isActive)
    {

        GameObject popUpScale = popUp.transform.GetChild(1).transform.GetChild(0).gameObject;

        if (!originalScales.ContainsKey(popUpScale))
            originalScales[popUpScale] = popUpScale.transform.localScale;
        if (isActive)
        {
            popUpScale.transform.localScale = Vector3.zero;
            popUpScale.transform.DOScale(originalScales[popUpScale], 0.3f);
        }
    }

    private void OnDestroy()
    {
        EventManager.onHeartChange.RemoveListener(UpdateHeartDisplay);
    }
}
