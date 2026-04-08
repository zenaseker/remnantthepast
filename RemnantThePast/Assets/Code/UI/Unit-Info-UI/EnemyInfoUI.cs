using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoUI : MonoBehaviour
{
    public class EnemyInfoToUI
    {
        public Sprite icon;
        public string name;
        public (int, int) hp;
    }
    public Image Icon;
    public RectTransform Hp;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI HpText;
    public void OnClickEnemy(EnemyInfoToUI infoToUI)
    {
        Icon.sprite = infoToUI.icon;
        Name.text = infoToUI.name;
        Hp.localScale = new Vector3((float)infoToUI.hp.Item1 / (float)infoToUI.hp.Item2, 1, 1);
        HpText.text = $"{infoToUI.hp.Item1}/{infoToUI.hp.Item2}";
    }
}
