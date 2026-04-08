using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIButton : MonoBehaviour
{
    public Animator animator;
    public Button button;
    public Image outline;
    public Image Icon;
    public Image erroricon;
    protected static Sprite sp_lack;
    protected static Sprite time_lack;
    bool InUse = false;
    bool invoke = false;
    private void OnEnable()
    {
        if (animator != null)
        {
            animator.SetBool("InUse", InUse);
            animator.SetBool("Invoke", invoke);
        }
    }
    public virtual void Load(SkillUIInfo info)
    {
        if (sp_lack == null)
        {
            sp_lack = PoolManage.Instance.GetSprite("icon/sp_lack");
            if (sp_lack == null)
            {
                Debug.LogError("未加载到sp_lack");
            }
        }
        if (time_lack == null)
        {
            time_lack = PoolManage.Instance.GetSprite("icon/time_lack");
            if (time_lack == null)
            {
                Debug.LogError("未加载到time_lack");
            }
        }
        this.gameObject.SetActive(true);
        Icon.sprite = info.sprite;
        outline.fillAmount = 1f;
        button.interactable = false;
        switch (info.skillButtonType)
        {
            case SkillUIInfo.SkillButtonType.CanUse:
                erroricon.gameObject.SetActive(false);
                button.interactable = true;
                info.OutColor.a = 1f;
                InUse = false;
                invoke = true;
                break;
            case SkillUIInfo.SkillButtonType.NotSp:
                erroricon.gameObject.SetActive(true);
                erroricon.sprite = sp_lack;
                info.OutColor.a = 0.5f;
                InUse = false;
                invoke = false;
                break;
            case SkillUIInfo.SkillButtonType.InOtherSkill:
                erroricon.gameObject.SetActive(true);
                erroricon.sprite = time_lack;
                outline.fillAmount = info.DuringTime / info.Duaction;
                info.OutColor.a = 0.5f;
                InUse = false;
                invoke = false;
                break;
            case SkillUIInfo.SkillButtonType.InSkill:
                erroricon.gameObject.SetActive(false);
                info.OutColor.a = 1f;
                InUse = true;
                invoke = true;
                break;
        }
        outline.color = info.OutColor;
    }
}
