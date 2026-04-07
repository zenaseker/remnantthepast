using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace MenuUI
{
    public class TeamCharSelectButton : MonoBehaviour
    {
        int id;
        AllCharacterSelect control;
        GameObject box;
        public void Init(CharacterInfo info, AllCharacterSelect control)
        {
            this.control = control;
            if (box != null) PoolManage.Instance.PushGameObject(box);
            if (info == null)
            {
                id = 0;
                box = PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "Char Box-0", transform);
            }
            else
            {
                id = info.ID;
                box = PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "Char Box-" + info.Rarity, transform);
                box.GetComponent<RectTransform>().localScale = Vector3.one;
            }
            box.GetComponent<TeamCharBox>().Init(info);
            box.GetComponent<RectTransform>().localPosition = Vector3.zero;
        }
        public void OnClick()
        {
            control.OnChoose(id);
        }
    }

}
