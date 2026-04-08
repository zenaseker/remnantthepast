using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoUI : MonoBehaviour
{
    public class CharacterInfoToUI
    {
        public Sprite icon;
        public string name;
        public int level;
        public (int, int) hp;
        public (int, int) sp;
        public int attack;
        public CharacterInfo.Profession profession;
    }
    Animator animator;
    public TextMeshProUGUI Level;
    SkillUIInfo[] skillUIInfos = new SkillUIInfo[4];
    public CharacterInfoUI_Icon IconUI;
    public CharacterInfoSkillUI[] characterInfoSkillUIs;
    public CharacterinfoEquipUI[] characterEquipinfoUIs;
    public TextMeshProUGUI Attack;
    public Image Profession;
    public void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void OnClickCharacter(CharacterBase character)
    {
        CharacterInfoToUI uiinfo = character.GetInfoToUI();
        IconUI.OnClickCharacter(uiinfo);
        Attack.text = uiinfo.attack.ToString();
        List<SkillUIInfo> SkillIndex = character.GetSkills();
        if (SkillIndex == null)
        {
            Debug.Log("输入的技能组为空，请检查设置");
            return;
        }
        SkillIndex.RemoveAll(x => x == null);
        if (SkillIndex == null)
        {
            Debug.Log("输入的技能组没有可用内容");
            return;
        }
        skillUIInfos = new SkillUIInfo[4];
        foreach (SkillUIInfo skill in SkillIndex)
        {
            if (skill.UIindex == -1)skill.UIindex = 0;
            if (skill.UIindex > 3)
            {
                Debug.Log($"技能{skill.name}的UI索引错误：超出范围");
                continue;
            }
            skillUIInfos[skill.UIindex] = skill;
        }
        for (int i = 0; i < 4; i++)
        {
            characterInfoSkillUIs[i].WriteSkillUI(skillUIInfos[i]);
        }
        Profession.sprite = CharacterInfo.GetProfessionSprite(uiinfo.profession);
        Level.text = "lv." + uiinfo.level;
        RoleData data = character.RoleData;
        for(int i = 0;i < 4;i++)
        {
            characterEquipinfoUIs[i].Load(data.equipments[i]);
        }
    }
    public void TotalInfoShow(bool flag)
    {
        animator.Play(flag ? "LittleToAll" : "AllToLittle");
    }
}
