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
    public CanvasGroup canvasgroup;

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
        Vector3 orgScale = popUpHolder.transform.localScale;
        Sequence s = DOTween.Sequence();
        s.Append(popUpHolder.transform.DOScale(Vector3.zero, 0.5f));
        s.Join(canvasgroup.DOFade(0.0f, 0.5f));
        s.SetUpdate(true);
        s.OnComplete(() =>
        {
            Time.timeScale = 1.0f;
            this.gameObject.SetActive(false);
            popUpHolder.transform.localScale = orgScale;
        });
    }

    public void OnShowSettingView()
    {
        Sequence s = DOTween.Sequence();
        Vector3 orgScale = popUpHolder.transform.localScale;
        popUpHolder.transform.localScale = Vector3.zero;
        s.Append(popUpHolder.transform.DOScale(orgScale, 0.5f));
        canvasgroup.alpha = 0.0f;
        s.Join(canvasgroup.DOFade(1.0f, 0.5f));
        s.SetUpdate(true);
        Time.timeScale = 0.0f;
    }
}
