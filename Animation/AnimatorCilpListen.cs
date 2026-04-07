using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 动画事件监听
/// </summary>
public class AnimatorCilpListen : StateMachineBehaviour
{
    //在Animator窗口创建并设置
    public bool TriggerEnter = false;
    public bool TriggerExit = false;
    public Action<string> EnterAction;
    public Action<string> ExitAction;
    public string AnimatorCilpName;
    public void Init(Action<string> enterAction, Action<string> exitAction)
    {
        EnterAction = enterAction;
        ExitAction = exitAction;
    }
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (TriggerEnter) EnterAction?.Invoke(AnimatorCilpName);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (TriggerExit) ExitAction?.Invoke(AnimatorCilpName);
    }

}
