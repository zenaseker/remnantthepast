using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI
{
    public class TeamCharBox : MonoBehaviour
    {
        public Image CharIcon;
        public Image Profession;
        public TextMeshProUGUI Name;
        /// <summary>
        /// 创建在队伍中
        /// </summary>
        /// <param name="info"></param>
        public void Init(CharacterInfo info)
        {
            if (info == null || info.Rarity <= 0) return;
            CharIcon.sprite = PoolManage.Instance.GetSprite("char_portrait/" + info.Icon);
            Profession.sprite = PoolManage.Instance.GetSprite($"profession/icon_profession_{info._Profession.ToString().ToLower()}_large_white");
            Name.text = info.Name;
        }
    }


}