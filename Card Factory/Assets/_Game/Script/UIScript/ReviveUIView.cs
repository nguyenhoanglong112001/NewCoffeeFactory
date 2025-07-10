using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReviveUIView : MonoBehaviour
{
    public Button noRevive;
    public Button reviveBt;
    public TMP_Text reviveCostText;

    public CanvasGroup canvasGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        noRevive.onClick.AddListener(OnNoRevivePress);
        reviveBt.onClick.AddListener(OnRevivePress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void OnShowReviveUI()
    {
        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        reviveCostText.text = GameManager.Ins.rewardConfig.reviveCost.ToString();
    }

    public void OnNoRevivePress()
    {
        this.gameObject.SetActive(false);
        UIManager.Ins.OnShowPopUp(UIManager.Ins.levelFailView.gameObject, true);
    }

    public void OnRevivePress()
    {
        if (GameManager.Ins.currentGold < GameManager.Ins.rewardConfig.reviveCost)
        {
            UIManager.Ins.OnShowShopUI();
        }
        else
        {
            Time.timeScale = 1.0f;
            GameManager.Ins.BoosterManager.OnAddQueueSlot();
            LevelManager.Ins.reviveTime--;
            LevelManager.Ins.isReviveWait = false;

            this.gameObject.SetActive(false);
        }
    }
}
