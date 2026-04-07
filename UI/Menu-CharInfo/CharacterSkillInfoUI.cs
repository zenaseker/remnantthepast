using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI
{
    public class CharacterSkillInfoUI : MonoBehaviour
    {
        SkillInfo[] skillinfos = new SkillInfo[4];
        byte[] skilllevels = new byte[4];
        public Image[] SkillIcons;
        public TextMeshProUGUI[] SkillNames;
        Animator animator;

        public TextMeshProUGUI SelectName;
        public TextMeshProUGUI SelectRank;
        public TextMeshProUGUI SelectUseSp;
        public TextMeshProUGUI SelectUseTime;
        public TextMeshProUGUI SelectDuraction;
        public TextMeshProUGUI SelectDescription;
        private void Awake()
        {
            animator = transform.parent.GetComponent<Animator>();
        }
        public void Load(string[] skills, byte[] levels)
        {
            for (int i = 0; i < skillinfos.Length; i++)
            {
                skillinfos[i] = null;
            }
            skilllevels = levels;
            for (int i = 0; i < skills.Length; i++)
            {
                if (DataLibrary.Instance.skillInfos.TryGetValue(skills[i],out skillinfos[i]))
                {
                    SkillIcons[i].gameObject.SetActive(true);
                    SkillIcons[i].sprite = skillinfos[i].sprite;
                    SkillNames[i].text = skillinfos[i].Name;
                }
            }
            for (int i = 0; i < skillinfos.Length; i++)
            {
                if (skillinfos[i] == null)
                {
                    SkillIcons[i].gameObject.SetActive(false);
                    SkillNames[i].text = "";
                }
            }
        }
        public void OnClickHideButton()
        {
            if (animator.GetBool("ShowSkill"))
            {
                animator.SetBool("ShowSkill", false);
            }
            else
            {
                OnClickSkillIcon(0);
            }
        }
        public void OnClickSkillIcon(int index)
        {
            if (skillinfos[index] == null) return;
            animator.SetBool("ShowSkill", true);
            WriteSkillDetail(index);
        }
        void WriteSkillDetail(int index)
        {
            SkillInfo skillinfo = skillinfos[index];
            int rank = skilllevels[index];
            SelectName.text = skillinfo.Name;
            SelectRank.text = rank.ToString();
            SelectUseSp.text = skillinfo.skillLevelNumbers[rank].needSp.ToString();
            SelectUseTime.text = skillinfo.skillLevelNumbers[rank].UseTime.ToString();
            SelectDuraction.text = skillinfo.skillLevelNumbers[rank].Duraction.ToString();
            try
            {
                string Description = string.Format(skillinfo.skillLevelNumbers[rank].Description,
                    Array.ConvertAll(skillinfo.skillLevelNumbers[rank].Number, x => (object)x));
                Description = ColorLibrary.TextDrawColor(Description);
                SelectDescription.text = Description;
            }
            catch (Exception e)
            {
                Debug.Log($"技能{skillinfo.Name}文本错误:\n {e}");
            }
        }
    }
}
