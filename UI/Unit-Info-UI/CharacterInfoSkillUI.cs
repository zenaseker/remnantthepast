using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoSkillUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI spcost;
    public TextMeshProUGUI timecost;
    public TextMeshProUGUI duration;
    public TextMeshProUGUI skillname;
    public TextMeshProUGUI description;
    public void WriteSkillUI(SkillUIInfo skills)
    {
        if (skills == null)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        icon.sprite = skills.sprite;
        spcost.text = skills.SpCost.ToString();
        timecost.text = skills.Usetime.ToString();
        duration.text = skills.Duaction.ToString();
        skillname.text = skills.name;
        description.text = skills.description;
    }
}
