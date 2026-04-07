using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyTimeLineIconUI : MonoBehaviour
{
    public Image icon;
    public Image outline;
    public EnemyActionTime action;
    public Animator Animator;

    public void SetAnimation(bool anim)
    {
        Animator.enabled = anim;
    }
    public void Add(EnemyActionTime queneObject)
    {
        action = queneObject;
        if (queneObject.unit == null || queneObject.unit is AbstractRoundQueneObject || queneObject.icon == null)
        {
            switch (queneObject.strength)
            {
                case 0:
                    icon.sprite = PoolManage.Instance.GetSprite("icon/sprite_warning_white");
                    break;
                case 1:
                    icon.sprite = PoolManage.Instance.GetSprite("icon/sprite_warning_yellow");
                    break;
                case 2:
                    icon.sprite = PoolManage.Instance.GetSprite("icon/sprite_warning_red");
                    break;
            }
        }
        else
        {
            icon.sprite = PoolManage.Instance.GetSprite("enemies/" + queneObject.icon);
        }
        switch (queneObject.strength)
        {
            case 0:
                outline.color = Color.white;
                break;
            case 1:
                outline.color = Color.yellow;
                break;
            case 2:
                outline.color = Color.red;
                break;
        }
    }

    public void OnClick()
    {
        if (action == null || action.unit == null)
        {
            Debug.Log("行为意图目标已消失");
        }
        GameApp.Instance.SetSelectObj(action.unit.GetGameObject(), "Monster");
    }
}
public class EnemyActionTime
{
    public IRoundQueneObject unit;//指向单位
    public MonsterIntentInfo intentinfo;//意图
    public float time;//时间
    public float absoluteTime;  // 剩余绝对时间
    public int strength;//警告强度(0~2)
    public string icon;//图片
    public bool StaticTime = false;//是否为静态时间
    public Action Trigger { get; set; } //触发
    public EnemyActionTime(IRoundQueneObject unit, MonsterIntentInfo intent)
    {
        this.unit = unit;
        this.time = intent.time;
        this.strength = intent.strength;
        this.icon = intent.icon;
        Trigger = intent.Trigger;
    }
    public void OnTrigger()
    {
        Trigger?.Invoke();
        this.unit = null;
        this.time = 0;
        this.strength = 0;
        this.icon = null;
        Trigger = null;
    }
    public bool Equal(EnemyActionTime obj)
    {
        return unit == obj.unit && time == obj.time;
    }

}