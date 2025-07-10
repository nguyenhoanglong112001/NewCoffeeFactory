using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class QuitNotifyView : MonoBehaviour
{
    public RectTransform notifyRect;
    public Vector2 _notifyOrgPos;

    public Button noLeaveBt;
    public Button leaveBt;
    private void Awake()
    {
        _notifyOrgPos = notifyRect.anchoredPosition;
    }
    private void OnEnable()
    {
        OnShowNotify();
    }

    private void Start()
    {
        noLeaveBt.onClick.AddListener(OnNoLeavePress);
    }
    public void OnShowNotify()
    {
        notifyRect.anchoredPosition = _notifyOrgPos + new Vector2(0, -Screen.height);
        notifyRect.DOAnchorPos(_notifyOrgPos, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnNoLeavePress()
    {
        Vector2 currentPos = notifyRect.anchoredPosition;

        notifyRect.DOAnchorPos(currentPos + new Vector2(0, -Screen.height), 0.3f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                this.gameObject.SetActive(false);
            });
    }
}
