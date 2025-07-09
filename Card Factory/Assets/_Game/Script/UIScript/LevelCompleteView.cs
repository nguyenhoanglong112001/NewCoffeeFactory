using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompleteView : MonoBehaviour
{
    public UIManager uimanager;
    public Button continueBt;
    public TMP_Text rewardText;

    public CanvasGroup canvasGroup;

    private void OnEnable()
    {
        OnShowPopUp();
    }
    private void Start()
    {
        uimanager = UIManager.Ins;
        continueBt.onClick.AddListener(uimanager.featureUIView.OnShowProgressFeature);
    }

    public void OnShowRewardText()
    {
        rewardText.text = GameManager.Ins.rewardConfig.coinReward.ToString();
    }

    public void OnShowPopUp()
    {
        OnShowRewardText();
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.3f)
            .SetUpdate(true);
    }
}
