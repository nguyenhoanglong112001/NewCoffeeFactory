using Christina.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUIView : MonoBehaviour
{
    [Header("Setting")]
    public Slider musicSlider;
    public Slider soundSlider;

    public ToggleSwitch musicToggle;
    public ToggleSwitch soundToggle;

    [Header("Button")]
    public Button quitBt;
    public Button restartBt;
    public Button cotinueBt;

    public TMP_Text buttonRestartText;
    public GameObject popUpHolder;

    private void OnEnable()
    {
        OnCheckBt();
        OnShowSettingView();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        musicSlider.onValueChanged.AddListener(AudioManager.Ins.SetMusicVolume);
        soundSlider.onValueChanged.AddListener(AudioManager.Ins.SetSFXVolume);

        musicToggle.sliderValue = PlayerPrefs.GetFloat("MusicVolume", 1f);
        soundToggle.sliderValue = PlayerPrefs.GetFloat("SoundVolume", 1f);

        cotinueBt.onClick.AddListener(OnResumePress);

        quitBt.onClick.AddListener(() =>
        {
            UIManager.Ins.OnShowPopUp(UIManager.Ins.quitNotifyView.gameObject, true);
            UIManager.Ins.quitNotifyView.leaveBt.onClick.RemoveAllListeners();
            buttonRestartText.text = "Quit";
            UIManager.Ins.quitNotifyView.leaveBt.onClick.AddListener(UIManager.Ins.OnContinuePress);
        });
        restartBt.onClick.AddListener(() =>
        {
            UIManager.Ins.OnShowPopUp(UIManager.Ins.quitNotifyView.gameObject, true);
            UIManager.Ins.quitNotifyView.leaveBt.onClick.RemoveAllListeners();
            buttonRestartText.text = "Replay";
            UIManager.Ins.quitNotifyView.leaveBt.onClick.AddListener(UIManager.Ins.OnReplayPress);
        });
    }

    public void OnCheckBt()
    {
        quitBt.gameObject.SetActive(!GameManager.Ins.isFirstTime);
        restartBt.gameObject.SetActive(!GameManager.Ins.isFirstTime);
    }

    public void OnResumePress()
    {
        Time.timeScale = 1.0f;
        this.gameObject.SetActive(false);
    }

    public void OnShowSettingView()
    {
        Vector3 orgScale = popUpHolder.transform.localScale;
        popUpHolder.transform.localScale = Vector3.zero;
        popUpHolder.transform.DOScale(orgScale, 0.3f)
            .SetUpdate(true);
        Time.timeScale = 0.0f;
    }
}
