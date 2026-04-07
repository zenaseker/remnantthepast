using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoUI_Icon : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI charname;
    public RectTransform hp;
    public TextMeshProUGUI hptext;
    public RectTransform sp;
    public TextMeshProUGUI sptext;

    public void OnClickCharacter(CharacterInfoUI.CharacterInfoToUI uiinfo)
    {
        icon.sprite = uiinfo.icon;
        charname.text = uiinfo.name;
        hp.localScale = new Vector3((float)uiinfo.hp.Item1 / (float)uiinfo.hp.Item2, 1, 1);
        hptext.text = $"{uiinfo.hp.Item1}/{uiinfo.hp.Item2}";
        sp.localScale = new Vector3((float)uiinfo.sp.Item1 / (float)uiinfo.sp.Item2, 1, 1);
        sptext.text = $"{uiinfo.sp.Item1}/{uiinfo.sp.Item2}";
    }
}
