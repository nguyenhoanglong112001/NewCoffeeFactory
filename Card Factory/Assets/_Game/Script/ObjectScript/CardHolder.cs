using cakeslice;
using ColorBlockJam;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;

public class CardHolder : MonoBehaviour
{
    public CardHolderGroup holderGroup;
    public List<Transform> cardHolderPos;
    public List<Card> cardHolder;
    public List<GameObject> slotDisplay;
    private int cardCount;
    public CardColor colorHolder;
    public MeshRenderer[] rends;

    public BasePackMechanic packMechanic;
    public PackMechanic currentMechanic;
    public bool HaveMechanic => currentMechanic != PackMechanic.None;

    public Animator animator;
    public Animation packAnimation;
    public string doneStateName = "Done";

    public Outline[] outlines;
    public bool isFull => cardHolder.Count >= cardCount;
    private int movingCardsCount = 0;

    [Header("packs Vfx")]
    public GameObject packsVfx;
    public ParticleSystem[] colorVfx;

    public Tween moveOutOfScreen;

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

        foreach (var p in colorVfx)
        {
            var main = p.main;
            main.startColor = ColorSetup.GetColor(colorHolder);
        }
    }

    public void CheckHolder()
    {
        if(isFull)
        {
            if (GameManager.Ins.isFirstTime && TutorialInGameManager.Ins.isOnTutorial)
            {
                if(TutorialInGameManager.Ins.currentTutIndex == 1)
                {
                    TutorialInGameManager.Ins.OnActiveTutorial(LevelManager.Ins.queues[1].cardPos[0], new Vector3(2.5f, 3, -9));
                }
            }
            AudioManager.Ins.PlaySound("CompletePack");
            PlayVfx();
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
        s.Append(transform.DOMoveY(transform.position.y + 1, 0.2f));
        s.Join(transform.DOMoveZ(transform.position.z - 6, 0.2f));
        animator.transform.parent.gameObject.SetActive(true);
        animator.SetTrigger("PackDone");
        PlayAnimation();
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
        });
    }

    private void PlayAnimation()
    {
        StartCoroutine(HandlePackAnim(animator));
    }

    public IEnumerator HandlePackAnim(Animator animator)
    {
        AnimationClip animclip = animator.runtimeAnimatorController.animationClips.First(c => c.name == "Inboxes");
        yield return new WaitForSeconds(animclip.length);
        float zOffset = Mathf.Abs(Camera.main.transform.position.z - transform.position.z);
        Vector3 offScreenRight = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, zOffset));

        transform.DOMove(Help.GetOffscreenWorldPos(Vector2.right,this.transform), 0.5f)
                          .SetEase(Ease.Linear)
                          .OnComplete(() =>
                          {
                              LevelManager.Ins.OnCheckWingame();
                              DOTween.Kill(moveOutOfScreen);
                              GameManager.Ins.poolManager.packPool.Despawn(this.gameObject);
                          });
    }

    public void ChangeCardColor()
    {
        foreach(var rend in rends)
        {
            Material mat = rend.material;
            ColorSetup.SetMatColor(colorHolder, mat);
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
        int holderIndex = holderGroup.cardHolders.IndexOf(this);
        if (holderIndex > 0)
        {
            if(holderIndex == 1)
            {
                this.gameObject.SetActive(true);
                OnCheckSlotDisPlay(false);
                return;
            }
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);
        AnimateHolder();
    }

    public void SetPacksOutLine(bool isActive,int colorIndex = 0)
    {
        foreach (var ouline in outlines)
        {
            if (ouline == null) continue;
            ouline.enabled = isActive;
            if(isActive == true)
            {
                ouline.color = colorIndex;
            }
        }
    }

    public void AnimateHolder()
    {
        ColorSetup.SetCustomOutlineColor(colorHolder);
        Vector3 orgScale = this.gameObject.transform.localScale;
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScale(orgScale * 1.2f, 0.2f).SetEase(Ease.OutQuad));
        s.Append(transform.DOScale(orgScale, 0.2f).SetEase(Ease.OutQuad));
    }

    public void PlayVfx()
    {
        packsVfx.SetActive(true);
        ParticleSystem ps = packsVfx.GetComponent<ParticleSystem>();
        ps.Play();
    }

    public void OnCheckSlotDisPlay(bool isActive)
    {
        slotDisplay[0].SetActive(isActive);
        slotDisplay[1].SetActive(isActive);
    }

    public void RegisterMovingCard()
    {
        movingCardsCount++;
    }

    public void UnregisterMovingCard()
    {
        movingCardsCount--;
        if (movingCardsCount <= 0)
        {
            CheckHolder();
        }
    }
}
