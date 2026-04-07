using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI
{
    public class CharEquipsInfoUI : MonoBehaviour
    {
        public CharacterinfoEquipUI[] equips;
        public void Load(EquipmentData[] data)
        {
            for (int i = 0; i < 4; i++)
            {
                equips[i].Load(data[i]);
            }
        }
    }

}