using AmplifyShaderEditor;
using EnemyActionTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IHpUnit,AnimatorEvent, IRoundQueneObject,IUnitBuf, IBlocked
{
    public MonsterInfo MonsterInfo { get; private set; }
    public Transform Effects { get; set; }
    public bool IsPlayer { get; set; }
    public GameObject SelectRing { get; set; }
    public bool InCombat { get; set; }//是否处于战斗状态

    public Transform GetTransform()
    {
        return transform;
    }
    public void Init(string id)
    {
        MonsterInfo = DataLibrary.Instance.monsterInfos[id].Clone();
        IsPlayer = false;
        Hp = new HpComponent(this, MonsterInfo.MaxHp);

        ActionTree = DataLibrary.Instance.GetEnemyActionTree("EnemyActionTree_" + id);
        ActionTree.Awake(this);

        BufComponent = new BufComponent(this);
        TimeManager.Instance.TimeMoveAction += LogicUpdate;

        Vector2Int now = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.y);
        List<Vector2Int> WorldWarningRange = new List<Vector2Int>();
        foreach (Vector2Int po in MonsterInfo.WarningRange)
        {
            WorldWarningRange.Add(now + po);
        }
        MapManager.Instance.AddRange(WorldWarningRange, this);

        Spine = GameObject.Instantiate(Resources.Load<GameObject>("spine/" + MonsterInfo.Model + "/Spine"), transform).transform;

        SelectRing = transform.Find("SelectRing").gameObject;
        Effects = transform.Find("effects");
        Animator = Spine.GetComponent<Animator>();
    }
    void Dead()
    {
        GameApp.Instance.SetEventTrue($"Monster{MonsterInfo.ID}Die");
        GameApp.Instance.UnitExitCombat(this);//将自身移出战斗序列
        GameApp.Instance.ClearActionByOnce(this);//清除自身所有意图
        //移除警戒范围
        Vector2Int now = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.y);
        List<Vector2Int> WorldWarningRange = new List<Vector2Int>();
        foreach (Vector2Int po in MonsterInfo.WarningRange)
        {
            WorldWarningRange.Add(now + po);
        }
        MapManager.Instance.RemoveRange(WorldWarningRange, this);
        Hp.OnDead();//血条执行死亡逻辑
        TimeManager.Instance.TimeMoveAction -= LogicUpdate;
        Destroy(gameObject);//移除自身
    }

    public EnemyInfoUI.EnemyInfoToUI GetInfoToUI()
    {
        return new EnemyInfoUI.EnemyInfoToUI()
        {
            icon = MonsterInfo.sprite,
            name = MonsterInfo.Name,
            hp = (Hp.CurrentHp, Hp.MaxHp),
        };
    }
    #region 意图
    public EnemyActionTreeBase ActionTree;
    #endregion
    #region 动画
    Transform Spine;
    public Animator Animator { get; set; }
    public Action<AnimatorTrigger.AnimatorEventType, string> AnimationAction { get; set; }
   
    public void StateEnter(string state)
    {
        Debug.Log(state + "Enter");
        AnimationAction?.Invoke(AnimatorTrigger.AnimatorEventType.Enter, state);
    }
    public void StateExit(string state)
    {
        Debug.Log(state + "Exit");
        AnimationAction?.Invoke(AnimatorTrigger.AnimatorEventType.Exit, state);
        switch (state)
        {
            case "Die":
                if (GameApp.Instance._nowSelect == gameObject) GameApp.Instance.FinishSelect();
                Dead();
                break;
        }
    }
    public void OnAttack()
    {
        Debug.Log("OnAttack");
        AnimationAction?.Invoke(AnimatorTrigger.AnimatorEventType.Trigger, "OnAttack");
    }
    #endregion
    #region 血量
    public HpComponent Hp;
    public void Heal(int heal)
    {
        Hp.Heal(heal);
    }
    public void Damage(DamageInfo damage)
    {
        Hp.Damage(damage.Damage);
        PoolManage.Instance.Return(damage);
    }
    public void Dying()
    {
        Animator.SetTrigger("Die");
        GameApp.Instance.AddEvent($"Monster{MonsterInfo.ID}Die", this);
    }
    public Action<float, float> SpChangeAction { get; set; }
    public Action<int, float> HpChangeAction { get; set; }
    #endregion
    #region 战斗
    public void OnIntoCombat()
    {
        ActionTree?.IntoCombat();
    }

    public void OnExitCombat()
    {
        ActionTree?.OutCombat();
    }
    #endregion

    #region Buf
    public BufComponent BufComponent { get; set; }
    public Action<float> TimeLogicUpdate { get; set; }
    public Action<float> UpdateAction { get; set; }
    public Action<int> AttackAction { get; set; }
    public Action<SkillBase> SkillAction { get; set; }
    public void LogicUpdate(float time)
    {
        TimeLogicUpdate?.Invoke(time);
    }
    public void Update()
    {
        UpdateAction?.Invoke(Time.deltaTime);
    }
    #endregion
}
