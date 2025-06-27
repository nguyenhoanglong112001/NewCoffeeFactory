using cakeslice;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardHolder : MonoBehaviour
{
    public CardHolderGroup holderGroup;
    public List<Transform> cardHolderPos;
    public List<Card> cardHolder;
    private int cardCount;
    public CardColor colorHolder;
    public MeshRenderer[] rends;

    public BasePackMechanic packMechanic;
    public PackMechanic currentMechanic;
    public bool HaveMechanic => currentMechanic != PackMechanic.None;

    public Animator animator;
    public Animation packAnimation;
    public string doneStateName = "Done";
    public string packHintClipName = "PackHint";

    public Outline[] outlines;
    public bool isFull => cardHolder.Count >= cardCount;

    private void Start()
    {
        cardCount = cardHolderPos.Count;
        if(currentMechanic == PackMechanic.HiddenColor)
        {
            packMechanic = this.AddComponent<HiddenPackMechanic>();
            packMechanic.SetMechanic();
        }
        else if (currentMechanic == PackMechanic.ChainPack)
        {
            packMechanic.UpdateVisual();
            ChangeCardColor();
        }
        else if (currentMechanic == PackMechanic.None)
        {
            ChangeCardColor();
        }
       
        CheckToShowHolder();
        SetPacksOutLine(false);
    }

    public void CheckHolder()
    {
        if(isFull)
        {
            if(GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
            {
                if(TutorialInGameManager.Ins.currentTutIndex == 1)
                {
                    TutorialInGameManager.Ins.OnActiveTutorial(LevelManager.Ins.queues[1].cardPos[0], new Vector3(2.5f, 3, -9));
                }
            }
            AudioManager.Ins.PlaySound("CompletePack");
            if(HaveMechanic && currentMechanic == PackMechanic.ChainPack)
            {
                packMechanic.CheckRemoveRule();
                return;
            }
            OnRemoveHolder();
        }
    }

    public void OnRemoveHolder()
    {
        DG.Tweening.Sequence s = DOTween.Sequence();
        s.Append(transform.DOMoveY(transform.position.y + 2, 0.2f));
        s.Join(transform.DOMoveZ(transform.position.z - 6, 0.2f));
        s.AppendInterval(0.2f);
        holderGroup.AllHolderMoveFront(() =>
        {
            if (holderGroup.cardHolders.Contains(this))
            {
                holderGroup.cardHolders.Remove(this);
                holderGroup.CheckHolder();
            }
            foreach (var holder in holderGroup.cardHolders)
            {
                holder.CheckToShowHolder();
            }
        });
        s.OnComplete(() =>
        {
            animator.transform.parent.gameObject.SetActive(true);
            animator.SetTrigger("PackDone");
            PlayAnimation();
        });
    }

    private void PlayAnimation()
    {
        StartCoroutine(HandleHammerAnim(animator));
    }

    public IEnumerator HandleHammerAnim(Animator animator)
    {
        AnimationClip animclip = animator.runtimeAnimatorController.animationClips.First(c => c.name == "Inboxes");
        yield return new WaitForSeconds(animclip.length);
        float zOffset = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 offScreenRight = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, zOffset));

        transform.DOMoveX(offScreenRight.x, 0.2f).SetEase(Ease.Linear)
                .OnComplete(() =>
            {
                LevelManager.Ins.OnCheckWingame();
                GameManager.Ins.poolManager.packPool.Despawn(this.gameObject);
            });
    }

    public void ChangeCardColor()
    {
        foreach(var rend in rends)
        {
            Material mat = rend.material;
            ColorSetup.SetCardColor(colorHolder, mat);
        }
    }

    private void OnMouseDown()
    {
        if(GameManager.Ins.BoosterManager.currentBooster == BoosterType.RemovePack)
        {
            GameManager.Ins.BoosterManager.OnUseDestroyPack(this);
        }
    }

    public void CheckToShowHolder()
    {
        if (holderGroup.cardHolders.Count < 1) return;
        if(holderGroup.cardHolders.IndexOf(this) > 0)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);
    }

    public void SetPacksOutLine(bool isActive)
    {
        foreach (var ouline in outlines)
        {
            ouline.enabled = isActive;
        }
    }
}
