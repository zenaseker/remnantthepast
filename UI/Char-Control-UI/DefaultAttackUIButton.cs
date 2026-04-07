using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultAttackUIButton : SkillUIButton
{
    public Image BGIcon;
    public override void Load(SkillUIInfo info)
    {
        if (info == null) return;
        if (info.UIindex == -1)
        {
            BGIcon.gameObject.SetActive(false);
            BGIcon.sprite = null;
        }
        else if (info.UIindex == 0)
        {
            BGIcon.gameObject.SetActive(true);
            BGIcon.sprite = info.sprite;
        }
    }
}
