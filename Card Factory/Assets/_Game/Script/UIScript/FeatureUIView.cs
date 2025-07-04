using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FeatureUIView : MonoBehaviour
{

    [Header("Feature")]
    public GameObject featureUnlockUI;
    public Image Progress;
    public Image progressBg;
    public TMP_Text progressText;
    public GameObject giftBox;
    public Animator giftBoxAnim;
    private AnimatorStateInfo AnimatorStateInfo;
    public Button featureNextButton;
    public GameObject featureIcon;
    public TMP_Text descriptionText;

    private Dictionary<GameObject, Vector3> originalScales = new();
    public void OnShowPopUp(GameObject popUp, bool isActive)
    {
        GameObject popUpScale = popUp.transform.GetChild(1).transform.GetChild(0).gameObject;
        if (!originalScales.ContainsKey(popUpScale))
            originalScales[popUpScale] = popUpScale.transform.localScale;
        if (isActive)
        {
            popUp.SetActive(isActive);
            popUpScale.transform.localScale = Vector3.zero;
            popUpScale.transform.DOScale(originalScales[popUpScale], 0.3f)
                .SetUpdate(true);
        }
        else
        {
            popUpScale.transform.DOScale(Vector3.zero, 0.3f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    popUp.SetActive(isActive);
                });
        }
    }

    public void OnShowProgressFeature()
    {
        OnShowPopUp(featureUnlockUI, true);
        featureNextButton.onClick.AddListener(UIManager.Ins.OnContinuePress);
        float progress = GameManager.Ins.GetFeatureProgress(GameManager.Ins.currentLevel -1);
        float currentProgress = GameManager.Ins.GetFeatureProgress(GameManager.Ins.currentLevel - 2);
        if (progress >= 1)
        {
            featureNextButton.onClick.RemoveListener(UIManager.Ins.OnContinuePress);
        }
        progressText.text = ((int)(currentProgress * 100)).ToString() + "%";
        Progress.DOFillAmount(progress, 0.5f)
            .SetUpdate(true)
            .OnUpdate(() =>
            {
                float pro = Progress.fillAmount;
                progressText.text = ((int)(pro * 100)).ToString() + "%";
            })
            .OnComplete(() =>
            {
                if(progress >= 1)
                {
                    Progress.gameObject.SetActive(false);
                    progressBg.enabled = false;
                    progressText.gameObject.SetActive(false);
                    giftBox.SetActive(true);
                    StartCoroutine(WaitAnimation());
                }
            });
    }

    IEnumerator WaitAnimation()
    {
        while (!giftBoxAnim.GetCurrentAnimatorStateInfo(0).IsName("GiftBoxOpen"))
        {
            yield return null;
        }

        while (giftBoxAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f ||
                giftBoxAnim.IsInTransition(0))
        {
            yield return null;
        }

        giftBox.transform.DOScale(Vector3.zero, 0.3f)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                featureIcon.SetActive(true);
                Vector3 featureScale = featureIcon.transform.localScale;
                featureIcon.transform.localScale = Vector3.zero;
                featureIcon.GetComponent<Image>().sprite = GameManager.Ins.currentFeatureUnlock.featureIcon;
                featureIcon.transform.DOScale(featureScale, 0.3f)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    featureNextButton.onClick.AddListener(UIManager.Ins.OnContinuePress);
                    descriptionText.text = GameManager.Ins.currentFeatureUnlock.description;
                    GameManager.Ins.NextFeature();
                });
            });
    }
}
