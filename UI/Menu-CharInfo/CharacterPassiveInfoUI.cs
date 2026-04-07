using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MenuUI
{
    public class CharacterPassiveInfoUI : MonoBehaviour
    {
        public GameObject[] Passive;
        public TextMeshProUGUI[] PassiveNames;
        public void Load(string[] passives)
        {
            for (int i = 0; i < Passive.Length; i++)
            {
                if (i < passives.Length && passives[i] != null && passives[i] != "")
                {
                    Passive[i].gameObject.SetActive(true);
                    PassiveNames[i].text = DataLibrary.Instance.passiveinfos[passives[i]].Name;
                }
                else
                {
                    Passive[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
