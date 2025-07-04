using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterPopUp : MonoBehaviour
{
    public BoosterConfig boosterConfig;
    private Booster boosterData;
    public Image boosterIcon;
    public TMP_Text descriptionText;
    public Button closeBt;
    public Animator popUpAnim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeBt.onClick.AddListener(OnClosePress);
    }

    public void OnClosePress()
    {
        popUpAnim.SetTrigger("Close");
        StartCoroutine(HandleAnimPopUp(popUpAnim,"Close",() =>
        {
            this.gameObject.SetActive(false);
            GameManager.Ins.BoosterManager.InitSwapTut();
        }));
    }

    public void OnSetUpPopUp()
    {
        boosterIcon.sprite = boosterData.boosterIcon;
        descriptionText.text = boosterData.boosterDes;
    }

    public void OnShowPopUp()
    {
        this.gameObject.SetActive(true);
        foreach (var data in boosterConfig.boosters)
        {
            if (data.levelUnlock == GameManager.Ins.currentLevel)
            {
                boosterData = data;
            }
        }
    }

    IEnumerator HandleAnimPopUp(Animator animator,string stateName,Action callback)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
        callback?.Invoke();
    }
}
