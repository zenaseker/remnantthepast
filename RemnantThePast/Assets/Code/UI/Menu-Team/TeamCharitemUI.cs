using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI
{
    public class TeamCharitemUI : MonoBehaviour
    {
        public GameObject Locked;
        public GameObject NoChar;
        public Transform CharBoxPos;
        public GameObject CharBox;
        int preid = -1;
        public void Init(int id, bool locked = false)
        {
            if (locked)
            {
                Locked.SetActive(true);
            }
            if (preid != id)
            {
                if (CharBox != null) PoolManage.Instance.PushGameObject(CharBox);
                CharBox = null;
                preid = id;
                if (DataLibrary.Instance.characterInfos.TryGetValue(id, out CharacterInfo info))
                {
                    NoChar.SetActive(false);
                    CharBox = PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "Char Box-" + info.Rarity, CharBoxPos);
                    CharBox.GetComponent<RectTransform>().localPosition = Vector3.zero;
                    CharBox.GetComponent<RectTransform>().localScale = Vector3.one;
                    CharBox.GetComponent<TeamCharBox>().Init(info);
                    return;
                }
            }
            NoChar.SetActive(true);
        }
    }


}