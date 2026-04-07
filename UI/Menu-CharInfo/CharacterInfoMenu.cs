using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI
{
    public class CharacterInfoMenu : MonoBehaviour
    {
        CharacterInfo info;
        RoleData roleData;

        public TextMeshProUGUI HpText;
        public TextMeshProUGUI AttackText;
        public TextMeshProUGUI SpeedText;
        public TextMeshProUGUI MagicText;
        public TextMeshProUGUI BlockText;
        public TextMeshProUGUI MagicBlockText;

        public Image Profession;
        public TextMeshProUGUI Name;
        public Image Star;

        public CharcterLevelInfoUI CharcterLevelInfo;
        public CharacterSkillInfoUI CharacterSkillInfo;
        public CharacterPassiveInfoUI CharacterPassiveInfoUI;
        public CharlIllustration CharlIllustration;
        public CharEquipsInfoUI charEquipsInfoUI;

        public void Load(CharacterInfo charinfo, RoleData role)
        {
            info = charinfo;
            roleData = role;
            LoadDefaultInfo();
            LoadNumberText();
            CharcterLevelInfo.Load(role);
            CharacterSkillInfo.Load(charinfo.Skills, role.SkillLevles);
            CharacterPassiveInfoUI.Load(charinfo.Passives);
            CharlIllustration.Load(info.Icon,role.level);
            charEquipsInfoUI.Load(role.equipments);
        }
        void LoadDefaultInfo()
        {
            Profession.sprite = PoolManage.Instance.GetSprite($"profession/icon_profession_{info._Profession.ToString().ToLower()}_large");
            Name.text = info.Name;
            Star.sprite = PoolManage.Instance.GetSprite("Star/star_" + info.Rarity);
            Star.GetComponent<RectTransform>().sizeDelta = new Vector2(36 * Star.sprite.textureRect.width / Star.sprite.textureRect.height, 36);
        }
        void LoadNumberText()
        {
            HpText.text = (info.MaxHp + roleData.level * info.IncreaseHp).ToString();
            AttackText.text = (info.Attack + roleData.level * info.IncreaseAttack).ToString();
            MagicText.text = info.StartMagic.ToString() + "/" + ((int)(info.MaxMagic + roleData.level * info.IncreaseMagic)).ToString();
            SpeedText.text = info.MoveDistance.ToString();
            
        }
        public void Comeback()
        {
            AllCharacterSelect.Instance.Using(ShowChar, AllCharacterSelect.Filterby.OnlyHas);
            this.gameObject.SetActive(false);
        }
        public void OnClick()
        {
            AllCharacterSelect.Instance.Using(ShowChar, AllCharacterSelect.Filterby.OnlyHas);
        }
        public void ShowChar(int id)
        {
            if (DataLibrary.Instance.characterInfos.TryGetValue(id, out var info))
            {
                RoleData data = DataLibrary.Instance.Save.unlockedRoles.Find(x => x.roleId == id);
                if (DataLibrary.Instance.Save.unlockedRoles.Find(x => x.roleId == id) != null)
                {
                    Load(info, data);
                }
            }
            gameObject.SetActive(true);
        }
    }

}