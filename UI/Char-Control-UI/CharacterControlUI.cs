using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterControlUI : MonoBehaviour
{
    bool InAttack = false;
    Animator animator;
    public GameObject ChooseButtons;
    public GameObject SelectButton;
    public Button movebtn;
    public Image ChooseButtonIcon;
    public SkillUIButton skill0;
    public SkillUIButton skill1;
    public SkillUIButton skill2;
    public SkillUIButton skill3;
    public List<SkillUIInfo> SkillIndex;
    CharacterBase PreChar;
    public void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void SetButton(bool atk)
    {
        if (InAttack != atk)
        {
            animator.SetBool("Attack", atk);
            animator.SetTrigger("To");
            InAttack = atk;
        }
        else
        {
            if (atk)
            {
                Debug.Log("点击了攻击按键");
                Skill(0);
            }
            else
            {
                Debug.Log("点击了移动按键");
                Move();
            }
        }
    }

    public void Move()
    {
        //if (movecancel.activeSelf)
        //{
        //    movecancel.SetActive(false);
        //    GameApp.Instance.InputSelect(null, "CancelGrid");
        //}
        //else
        //{
        //    movecancel.SetActive(true);
        //    GameApp.Instance.InputSelect(null, "ToGrid");
        //}
        GameApp.Instance.InputSelect(null, "ToGrid");
        StartChoose(PoolManage.Instance.GetSprite("icon/sprite_direction_thumb_cancel"));
    }
    public void Skill(int index)
    {
        //if (atkcancel.activeSelf)
        //{
        //    atkcancel.SetActive(false);
        //    GameApp.Instance.InputSelect(index, "CancelSkill");
        //}
        //else
        //{
        //    if (index > 3)
        //    {
        //        Debug.Log("选择项超出索引范围");
        //        return;
        //    }
        //    if (SkillIndex[index] == null)
        //    {
        //        Debug.Log("所选技能为空");
        //        return;
        //    }
        //    atkcancel.SetActive(true);
        //    GameApp.Instance.InputSelect(SkillIndex[index], "ToSkill");
        //}
        if (index > 3)
        {
            Debug.Log("选择项超出索引范围");
            return;
        }
        if (SkillIndex[index] == null)
        {
            Debug.Log("所选技能为空");
            return;
        }
        GameApp.Instance.InputSelect(SkillIndex[index], "ToSkill");
        StartChoose(index == 0 ? PoolManage.Instance.GetSprite("icon/sprite_kill_cancel") : SkillIndex[index].sprite);
    }
    public void OnClickCharacter(CharacterBase character)
    {
        PreChar = character;
        SkillIndex = character.GetSkills();
        if (SkillIndex == null)
        {
            Debug.Log("输入的技能组为空，请检查设置");
            return;
        }
        SkillIndex.RemoveAll(x => x == null);
        if (SkillIndex.Count > 4)
        {
            Debug.Log("输入了超过4个技能");
            return;
        }
        skill1.gameObject.SetActive(false);
        skill2.gameObject.SetActive(false);
        skill3.gameObject.SetActive(false);
        foreach (SkillUIInfo skill in character.GetSkills())
        {
            WriteSkill(skill);
        }
        SetDefaultButtonInteractable();
    }

    public void WriteSkill(SkillUIInfo skills)
    {
        switch (skills.UIindex)
        {
            case -1:
                SkillIndex[0] = skills;
                skill0.Load(skills);
                break;
            case 0:
                SkillIndex[0] = skills;
                skill0.Load(skills);
                break;
            case 1:
                SkillIndex[1] = skills;
                skill1.Load(skills);
                break;
            case 2:
                SkillIndex[2] = skills;
                skill2.Load(skills);
                break;
            case 3:
                SkillIndex[3] = skills;
                skill3.Load(skills);
                break;
        }
    }
    public void CancelAny()
    {
        GameApp.Instance.InputSelect(null, "Cancel");
    }
    public void StartChoose(Sprite icon)
    {
        ChooseButtonIcon.sprite = icon;
        ChooseButtons.gameObject.SetActive(false);
        SelectButton.gameObject.SetActive(true);
    }
    public void CancelChoose()
    {
        ChooseButtons.gameObject.SetActive(true);
        SelectButton.gameObject.SetActive(false);
    }
    public void SetButtonInteractable(bool moveinter, bool atkinter)
    {
        movebtn.interactable = moveinter;
        skill0.button.interactable = atkinter;
    }
    public void SetDefaultButtonInteractable()
    {
        if (PreChar == null)
        {
            Debug.Log("未选择角色");
            return;
        }
        movebtn.interactable = PreChar.CanMove();
        skill0.button.interactable = PreChar.CanAttack();
        if (!skill0.button.interactable)
        {
            skill1.button.interactable = false;
            skill2.button.interactable = false;
            skill3.button.interactable = false;
        }
    }
}
