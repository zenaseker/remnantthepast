using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionTimeLineUI : Singleton<ActionTimeLineUI>
{
    public List<EnemyTimeLineIconUI> ActionUIs = new List<EnemyTimeLineIconUI>();//意图图标列表
    public Transform IconTrs;
    public GameObject CombatLine;
    public RectTransform OutLine;
    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI TimeAddText;
    public Animator Animator;
    public float TotalTime = 0;
    void Start()
    {
        Animator = GetComponent<Animator>();
    }
    #region 时间轴显示
    /// <summary>
    /// 添加行为
    /// </summary>
    /// <param name="action"></param>
    public void AddAction(EnemyActionTime action)
    {
        GameObject actionUI = PoolManage.Instance.GetPoolGameObject("UI", "EnemyActionIcon", IconTrs);//从缓存池中获取对象
        EnemyTimeLineIconUI ui = actionUI.GetComponent<EnemyTimeLineIconUI>();
        ui.Add(action);
        if (action.StaticTime)
        {
            action.absoluteTime = action.time;
        }
        else
        {
            float accumulated = 0;
            for(int i = ActionUIs.Count - 1; i >= 0; i--)
            {
                if (!ActionUIs[i].action.StaticTime)
                {
                    accumulated = ActionUIs[i].action.absoluteTime;
                    break;
                }
            }
            action.absoluteTime = accumulated + action.time;
        }
        actionUI.GetComponent<RectTransform>().localPosition = new Vector3(480f, 0, 0);
        ActionUIs.Add(ui);
        Sort();
    }
    public void RemoveAction(EnemyActionTime action)
    {
        if (ActionUIs.Count <= 0) return;
        EnemyTimeLineIconUI ui = ActionUIs.Find(x => x.action.Equal(action));
        if (ui != null)
        {
            ActionUIs.Remove(ui);
            PoolManage.Instance.PushGameObject(ui.gameObject);
        }
        Sort();
    }
    public void RemoveAllAction(Predicate<EnemyTimeLineIconUI> predicate)
    {
        if (ActionUIs.Count <= 0) return;
        List<EnemyTimeLineIconUI> remove = ActionUIs.FindAll(predicate);
        foreach (EnemyTimeLineIconUI ui in remove)
        {
            ActionUIs.Remove(ui);
            PoolManage.Instance.PushGameObject(ui.gameObject);
        }
        Sort();
    }
    //排序
    //时间轴总长为4
    //时间轴坐标为-480~480(RectTransform)
    //超出时间的Icon放在480
    void Sort()
    {
        // 按绝对时间排序
        ActionUIs.Sort((a, b) => a.action.absoluteTime.CompareTo(b.action.absoluteTime));
        foreach (EnemyTimeLineIconUI ui in ActionUIs)
        {
            if (ui.action.absoluteTime <= 4)
            {
                ui.GetComponent<RectTransform>().DOAnchorPos(new Vector2(960f * ui.action.absoluteTime / 4 - 480f, 0),0.3f).SetEase(Ease.Linear);
            }
            else
            {
                ui.GetComponent<RectTransform>().DOAnchorPos(new Vector2(480f, 0), 0.3f).SetEase(Ease.Linear);
            }
            ui.SetAnimation(false);
        }
    }
    public void InCombat(bool flag)
    {
        CombatLine.SetActive(flag);
        if (!flag)
        {
            while (ActionUIs.Count > 0)
            {
                PoolManage.Instance.PushGameObject(ActionUIs[0].gameObject);
                ActionUIs.RemoveAt(0);
            }
            ActionUIs.Clear();
        }
    }
    public void ShowAddTime(float time)
    {
        Animator.Play("ShowAddTime");
        TimeAddText.gameObject.SetActive(true);
        if (time < 0)
        {
            TimeAddText.text = "-" + time.ToString("F2");
            TimeAddText.color = Color.green;
        }
        else
        {
            TimeAddText.text = "+" + time.ToString("F2");
            TimeAddText.color = Color.yellow;
            OutLine.sizeDelta = new Vector2(960f * time / 4, 50);
            foreach (EnemyTimeLineIconUI ui in ActionUIs)
            {
                ui.SetAnimation(ui.action.absoluteTime <= time);
            }
        }
    }
    public void HideAddTime()
    {
        Animator.Play("TimeLineDefault");
        TimeAddText.gameObject.SetActive(false);
        OutLine.sizeDelta = new Vector2(0, 50);
        foreach (EnemyTimeLineIconUI ui in ActionUIs)
        {
            ui.SetAnimation(false);
        }
    }
    public void EndAddTimeAnimator()
    {
        OutLine.sizeDelta = new Vector2(0, 50);
    }
    #endregion
    public void TimeMove(float time)
    {
        SetAddTime(time);
        if (time < 0)
        {
            TimeAddText.text = "-" + time.ToString("F2");
            TimeAddText.color = Color.green;
        }
        else
        {
            TimeAddText.text = "+" + time.ToString("F2");
            TimeAddText.color = Color.yellow;
            OutLine.sizeDelta = new Vector2(960f * time / 4, 50);
        }
        Animator.Play("AddTime");
        TimeManager.Instance.TimeMoveAction?.Invoke(time);
        List<EnemyActionTime> readyaction = new List<EnemyActionTime>();
        for (int i = 0; i < ActionUIs.Count;)
        {
            ActionUIs[i].action.absoluteTime -= time;
            if (ActionUIs[i].action.absoluteTime <= 0)
            {
                var act = ActionUIs[i];       // 触发后移除
                PoolManage.Instance.PushGameObject(act.gameObject);//移入缓存池
                readyaction.Add(act.action);
                ActionUIs.RemoveAt(i);
            }
            else i++;
        }
        Debug.Log($"时间推进{time}s,激活了:{readyaction.Count}个意图");
        foreach (EnemyActionTime action in readyaction)
        {
            action.OnTrigger();
        }
        Sort();
    }
    public void SetAddTime(float time)
    {
        foreach (EnemyTimeLineIconUI ui in ActionUIs)
        {
            ui.SetAnimation(false);
        }
        TotalTime += time;
        TimeText.text = TotalTime.ToString(time > 1000 ? "F0" : "F1");
    }
}
