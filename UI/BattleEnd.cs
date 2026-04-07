using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEnd : MonoBehaviour
{
    Action AnimatorEnd;
    public void GoAnimator(bool win, Action animatorEnd)
    {
        AnimatorEnd = animatorEnd;
        if (win)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            MusicManage.Instance.PlayEffect("voice/vox/v_bat_f_04");
        }
        else
        {
            MusicManage.Instance.PlayEffect("voice/vox/v_bat_f_05");
        }
    }
    public void OnAnimatorEnd()
    {
        AnimatorEnd?.Invoke();
    }
}
