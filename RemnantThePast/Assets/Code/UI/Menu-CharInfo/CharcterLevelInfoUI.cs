using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI
{
    public class CharcterLevelInfoUI : MonoBehaviour
    {
        public TextMeshProUGUI Level;
        public Image LevelOutLine;
        public Image[] ExpBar;
        public TextMeshProUGUI ExpText;
        public void Load(RoleData data)
        {
            Level.text = data.level.ToString();
            LevelOutLine.fillAmount = (data.level % 20) / 20f;

        }
    }
}