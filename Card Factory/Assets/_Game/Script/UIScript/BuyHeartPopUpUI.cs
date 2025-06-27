using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyHeartPopUpUI : MonoBehaviour
{
    public GoldConfig config;

    public Button closeBuyHeartBt;
    public TMP_Text heartAmountText;
    public TMP_Text hearttimeText;

    public TMP_Text costAddHeart;
    public TMP_Text costRefillHeart;

    public Button addHeartBt;
    public Button refillFullHeart;

    private void Start()
    {
        closeBuyHeartBt.onClick.AddListener(() => MainMenuManager.Ins.OnShowPopUp(this.gameObject,false));
    }

    private void OnEnable()
    {
        UpdateHeartDisplay();
        OnShowCostPerHeart();
        OnShowCostRefill();
        EventManager.onHeartChange.AddListener(UpdateHeartDisplay);
        EventManager.onHeartChange.AddListener(OnShowCostRefill);
        addHeartBt.onClick.AddListener(OnAddHeart);
        refillFullHeart.onClick.AddListener(OnRefillHeart);
    }

    private void Update()
    {
        MainMenuManager.Ins.UpdateTimeDisplay(hearttimeText);

    }

    private void OnDisable()
    {
        EventManager.onHeartChange.RemoveListener(UpdateHeartDisplay);
        EventManager.onHeartChange.RemoveListener(OnShowCostRefill);
    }

    private void UpdateHeartDisplay()
    {
        int currentHearts = GameManager.Ins.GetCurrentHearts();
        heartAmountText.text = $"{currentHearts}";
    }

    private void OnShowCostPerHeart()
    {
        costAddHeart.text = config.perHeartCost.ToString();
        costAddHeart.color = GameManager.Ins.currentGold > config.perHeartCost ? Color.white : Color.red;  
    }

    private void OnShowCostRefill()
    {
        int heartRefill = GameManager.Ins.Maxheart - GameManager.Ins.GetCurrentHearts();
        costRefillHeart.text = (config.perHeartCost * heartRefill).ToString();
        costRefillHeart.color = GameManager.Ins.currentGold > config.perHeartCost ? Color.white : Color.red;
    }

    private void OnAddHeart()
    {
        if (GameManager.Ins.currentGold < config.perHeartCost) return;

        GameManager.Ins.OnUpdateCoin(-config.perHeartCost);
        GameManager.Ins.AddHeart(1);
        this.gameObject.SetActive(false);
    }

    private void OnRefillHeart()
    {
        int heartRefill = GameManager.Ins.Maxheart - GameManager.Ins.GetCurrentHearts();
        if (GameManager.Ins.currentGold < config.perHeartCost * heartRefill) return;

        GameManager.Ins.OnUpdateCoin(-config.perHeartCost * heartRefill);
        GameManager.Ins.AddHeart(heartRefill);
        this.gameObject.SetActive(false);
    }
}
