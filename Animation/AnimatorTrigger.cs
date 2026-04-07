using System;
using UnityEngine;

/// <summary>
/// 动画事件监听跳转
/// </summary>
public class AnimatorTrigger : MonoBehaviour
{
    public AnimatorEvent Event;
    public void OnEnable()
    {
        if (Event == null)
        {
            Event = transform.parent.GetComponent<AnimatorEvent>();
            if (Event == null)
            {
                Debug.Log("未查询到可用接口");
                return;
            }
        }
        Animator animator = GetComponent<Animator>();
        var listens = animator.GetBehaviours<AnimatorCilpListen>();
        foreach (var listen in listens)
        {
            listen.Init(Event.StateEnter, Event.StateExit);
        }
    }
    public void OnAttack()
    {
        Event?.OnAttack();
    }
    public enum AnimatorEventType
    {
        Enter,//动画开始
        Exit,//动画结束
        Trigger//动画事件触发
    }
}

public interface AnimatorEvent
{
    Animator Animator { get; set; }

    void OnAttack();

    Action<AnimatorTrigger.AnimatorEventType, string> AnimationAction { get; set; }

    void StateEnter(string state);

    void StateExit(string state);
}