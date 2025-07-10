using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelFailView : MonoBehaviour
{
    public Button giveUpBt;
    public Button replayBt;

    public CanvasGroup canvasGroup;

    private void OnEnable()
    {
        OnShowFailUI();
    }
    void Start()
    {
        giveUpBt.onClick.AddListener(UIManager.Ins.OnContinuePress);
        replayBt.onClick.AddListener(UIManager.Ins.OnReplayPress);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnShowFailUI()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
    }

}
