using System;
using TMPro;
using UnityEngine;

public class TutorialStage : MonoBehaviour
{
    public Tutorial tutorial;
    public int tutorialStage;
    public GameObject handPoint;
    public GameObject popUpIntruction;
    public TMP_Text intructionText;

    public string intruction;

    public void SetIntructionText()
    {
        popUpIntruction.SetActive(true);
        intructionText.text = intruction;
    }

    public void OnEndStage(Action callback = null)
    {
        Debug.Log("Tutorial Stage + " + tutorialStage + " Complete");
        this.gameObject.SetActive(false);
        if(popUpIntruction != null)
        {
            popUpIntruction.SetActive(false);
        }
        tutorial.NextStage();
        callback?.Invoke();
    }

    public void SetHandPointPos(Vector3 pos, Vector3 offset)
    {

        handPoint.transform.position = pos + offset;
        Debug.Log("Setpoit : " + handPoint.transform.position);
    }
}
