using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 战斗阶段管理器
/// </summary>
public class TimeManager : Singleton<TimeManager>
{
    /// <summary>
    /// 时间轴委托
    /// </summary>
    class CountDownAction
    {
        public Action action;
        public float time;
        public CountDownAction(Action action, float time)
        {
            this.action = action;
            this.time = time;
        }
    }
    public enum GameState
    {
        Explore,//探索
        Combat//战斗
    }
    public enum BattleState
    {
        Uninitialized,//未处于战斗状态
        Continue,//进入战斗前等待移动结束
        Player,//玩家控制回合
        InAction,//敌人序列行动中
        Aminator//战斗动画中
    }
    public enum EventState
    {
        Waitinginstruction,//等待输入指令
        Animationexecution,//指令动画执行中
        Datacallback//指令结束回传中
    }
    public GameState _GameState { get; private set; } = GameState.Explore;
    public BattleState _BattleState { get; private set; } = BattleState.Uninitialized;
    public EventState _EventState { get; private set; } = EventState.Waitinginstruction;

    public TextMeshProUGUI GameStateText;
    public TextMeshProUGUI BattleStateText;
    public TextMeshProUGUI EventStateText;
    public Action<float> TimeMoveAction;
    List<CountDownAction> _CountdownAction;
    private void Start()
    {
        TimeMoveAction = Countdown;
        _CountdownAction = new List<CountDownAction>();
    }
    //变更游戏状态
    public void SetGameState(GameState state)
    {
        _GameState = state;
        GameStateText.text = "当前状态：" + _GameState.ToString();
    }

    //变更战斗阶段
    public void SetBattleState(BattleState state)
    {
        if (_GameState == GameState.Explore)
        {
            _BattleState = BattleState.Uninitialized;
            BattleStateText.text = "当前回合：" + _BattleState.ToString();
            Debug.Log("未进入战斗状态，不允许更改战斗阶段");
            return;
        }
        _BattleState = state;
        BattleStateText.text = "当前回合：" + _BattleState.ToString();
    }
    /// <summary>
    /// 推进行动阶段
    /// </summary>
    public void EventCallBack()
    {
        switch (_EventState)
        {
            case EventState.Waitinginstruction:
                _EventState = EventState.Animationexecution;
                break;
            case EventState.Animationexecution:
                _EventState = EventState.Datacallback;
                Debug.Log("行动结束回调");
                EventCallBack();
                break;
            case EventState.Datacallback:
                _EventState = EventState.Waitinginstruction;
                if (_GameState == GameState.Explore)
                {

                }
                else if (_GameState == GameState.Combat)
                {
                    switch (_BattleState)
                    {
                        case BattleState.Continue:
                            GameApp.Instance.StartCombat();
                            break;
                        case BattleState.Uninitialized:
                            break;
                        case BattleState.Aminator:
                            break;
                        default:
                            GameApp.Instance.UpdateCombat();
                            break;
                    }
                }
                break;
        }
        EventStateText.text = "回合状态：" + _EventState.ToString();
    }
    public bool PlayerCanControl()
    {
        return (_GameState == GameState.Explore || _BattleState == BattleState.Player) && _EventState == EventState.Waitinginstruction;
    }
    public bool MonsterCanControl()
    {
        return _GameState == GameState.Combat && _BattleState == BattleState.InAction && _EventState == EventState.Waitinginstruction;
    }
    public bool InBattle()
    {
        return _BattleState == BattleState.Player || _BattleState == BattleState.InAction;
    }

    public void Countdown(float time)
    {
        for(int i = 0;i < _CountdownAction.Count;)
        {
            _CountdownAction[i].time =- time;
            if (_CountdownAction[i].time <= 0)
            {
                _CountdownAction[i].action?.Invoke();
                _CountdownAction.RemoveAt(i);
            }
            else i++;
        }
    }
    /// <summary>
    /// 添加时间轴委托
    /// </summary>
    /// <param name="action"></param>
    /// <param name="time">倒计时</param>
    public void AddCountDownAction(Action action,float time)
    {
        _CountdownAction.Add(new CountDownAction(action,time));
    }
}
