using SaveLoad;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 干员类，用于储存信息和统筹状态机
/// </summary>
public class CharacterBase : MonoBehaviour, IHpUnit, AnimatorEvent, IRoundQueneObject, IUnitBuf, IBlocked
{
    public CharacterInfo CharacterInfo { get; private set; }//干员信息
    public RoleData RoleData { get; private set; }//干员存档信息
    Transform Spine;
    public Transform Effects { get;set; }
    public Dictionary<byte, SkillBase> orginskills;//初始技能组
    public Dictionary<byte,SkillBase> skills;//技能组
    public bool IsPlayer { get; set; }
    public GameObject SelectRing { get; set; }//选择环
    public bool InCombat { get; set; } = false;//是否在场
    #region 数据计算
    public int MaxHp => CharacterInfo.MaxHp + CharacterInfo.IncreaseHp * RoleData.level;
    public int MaxSp => (int)(CharacterInfo.MaxMagic + CharacterInfo.IncreaseMagic * RoleData.level);
    public int Attack => CharacterInfo.Attack + CharacterInfo.IncreaseAttack * RoleData.level;
    #endregion
    public void Init(RoleData role,bool maincontrol)
    {
        //载入基础数据
        RoleData = role;
        CharacterInfo = DataLibrary.Instance.characterInfos[role.roleId].Clone();
        Hp = new HpComponent(this, MaxHp, MaxSp, CharacterInfo.StartMagic);//载入血量技力组件
        IsPlayer = true;
        orginskills = new Dictionary<byte, SkillBase>();
        //载入技能
        for (byte i = 0; i < CharacterInfo.Skills.Length; i++)
        {
            if (CharacterInfo.Skills[i] != null && DataLibrary.Instance.skillInfos.TryGetValue(CharacterInfo.Skills[i], out SkillInfo skillInfo))
            {
                SkillBase skillbase = new SkillBase(skillInfo, this, i);
                orginskills.Add(i, skillbase);
            }
        }
        skills = new Dictionary<byte, SkillBase>(orginskills);
        BufComponent = new BufComponent(this);//初始化Buf组件
        TimeManager.Instance.TimeMoveAction += LogicUpdate;
        GameApp.Instance.AddCharacter(this);

        SelectRing = transform.GetChild(0).gameObject;
        Spine = GameObject.Instantiate(Resources.Load<GameObject>("spine/" + CharacterInfo.Model + "/Spine"),transform).transform;
        Effects = transform.Find("effects");
        Animator = Spine.GetComponent<Animator>();
        if (!maincontrol)//非主控去后台等着
        {
            gameObject.SetActive(false);
            Hp.HideUI();
            return;
        }
        //主控干员入场
        InCombat = true;
        CameraManager.Instance.Player = GetComponent<PlayerControl>();
        UIManager.Instance.teamUIContorl.ChangeCharState(CharacterInfo.ID, TeamCharUI.StatusType.MainControl);
        MusicManage.Instance.PlayRamdonEffect($"voice/{CharacterInfo.Icon}/CN_023", $"voice/{CharacterInfo.Icon}/CN_024");
        MusicManage.Instance.PlayEffect($"voice/btl_snd_1/b_char_set");
    }
    public Transform GetTransform()
    {
        return transform;
    }
    public bool CanMove()
    {
        return BufComponent.CanToMove();
    }
    public bool CanAttack()
    {
        return TimeManager.Instance.InBattle();
    }
    public int Damage()
    {
        float num = Attack;
        num *= BufComponent.GetDamageAdd();
        return (int)num;
    }
    #region 技能
    public CharacterInfoUI.CharacterInfoToUI GetInfoToUI()
    {
        return new CharacterInfoUI.CharacterInfoToUI()
        {
            icon = PoolManage.Instance.GetSprite("char_avatar/" + CharacterInfo.Icon),
            name = CharacterInfo.Name,
            level = RoleData.level,
            hp = (Hp.CurrentHp, Hp.MaxHp),
            sp = ((int)Hp.CurrentSp, (int)Hp.MaxSp),
            attack = Damage(),
            profession = CharacterInfo._Profession
        };
    }
    public List<SkillUIInfo> GetSkills()
    {
        List<SkillUIInfo> list = new List<SkillUIInfo>(skills.Count);
        int index = -1;
        foreach(SkillBase skill in skills.Values)
        {
            SkillUIInfo info = skill.GetUIInfo();
            list.Add(info);
            if (info.skillButtonType == SkillUIInfo.SkillButtonType.InSkill) index = list.Count - 1;
        }
        if (index != -1)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].skillButtonType = (i == index)? SkillUIInfo.SkillButtonType.InSkill: SkillUIInfo.SkillButtonType.InOtherSkill;
            }
        }
        return list;
    }
    /// <summary>
    /// 使用技能
    /// </summary>
    /// <param name="index"></param>
    /// <param name="target"></param>
    public void UsingSkill(byte index,params object[] target)
    {
        if (skills.TryGetValue(index,out SkillBase skill))
        {
            if (skill == null) return;
            Debug.Log("触发技能：" + index + "-" + skill.SkillInfo.Name);
            MusicManage.Instance.PlayRamdonEffect(
                $"voice/{CharacterInfo.Icon}/CN_025",
                $"voice/{CharacterInfo.Icon}/CN_026",
                $"voice/{CharacterInfo.Icon}/CN_027",
                $"voice/{CharacterInfo.Icon}/CN_028"
                );
            SkillAction?.Invoke(skill);
            Hp.UseSp(skill.NeedSp);
            skill?.Skilling(target);
        }
        else
        {
            Debug.Log("未搜索到回传的技能：" + index);
        }
    }
    /// <summary>
    /// 初始化技能组
    /// </summary>
    public void RestoreSkill()
    {
        skills.Clear();
        skills.AddRange(orginskills);
    }
    #endregion
    #region 动画
    public Animator Animator { get; set; }
    public Action<AnimatorTrigger.AnimatorEventType, string> AnimationAction { get; set; }//动画事件监听
    public void StateEnter(string state)//某动画开始
    {
        Debug.Log(state + "Enter");
        AnimationAction?.Invoke(AnimatorTrigger.AnimatorEventType.Enter, state);
    }
    public void StateExit(string state)//某动画结束
    {
        Debug.Log(state + "Exit");
        AnimationAction?.Invoke(AnimatorTrigger.AnimatorEventType.Exit, state);
        switch (state)
        {
            case "Die":
                if (GameApp.Instance._nowSelect == gameObject) GameApp.Instance.FinishSelect();
                GameApp.Instance.SetEventTrue($"Character{CharacterInfo.ID}Die");
                GameApp.Instance.UnitExitCombat(this);
                GameApp.Instance.ClearActionByOnce(this);//清除自身所有意图
                GameApp.Instance.characterList.Remove(this);
                Hp.OnDead();//血条执行死亡逻辑
                GameObject.Destroy(this.gameObject);
                break;
        }
    }
    public void SetFace(bool right)//调整干员朝向
    {
        Spine.eulerAngles = right ? Right : Left;
    }
    static readonly Vector3 Left = new Vector3(30, 180, 0);
    static readonly Vector3 Right = new Vector3(-30, 0, 0);

    public void OnAttack()
    {
        Debug.Log("OnAttack");
        AnimationAction?.Invoke(AnimatorTrigger.AnimatorEventType.Trigger, "OnAttack");
    }
    public void ChangeAttackState(bool inattack)//调整干员站立状态
    {
        Animator.SetBool("InBattle", inattack);
    }
    #endregion
    #region 血量
    public HpComponent Hp;
    public Action<float, float> SpChangeAction { get; set; }
    public Action<int, float> HpChangeAction { get; set; }
    public void Damage(DamageInfo damage)
    {
        Hp.Damage(damage.Damage);
        PoolManage.Instance.Return(damage);
    }

    public void Dying()
    {
        Animator.SetTrigger("Die");
        GameApp.Instance.CharacterDead(this);
        GameApp.Instance.AddEvent($"Character{CharacterInfo.ID}Die", this);
    }

    public void Heal(int heal)
    {
        Hp.Heal(heal);
    }
    #endregion
    #region 战斗
    public void OnIntoCombat()
    {
        ChangeAttackState(true);
    }

    public void OnExitCombat()
    {
        ChangeAttackState(false);
        BufComponent.OnCombatEnd?.Invoke();
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
