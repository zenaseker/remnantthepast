using SkillComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SkillInfo;

/// <summary>
/// 技能类，用于储存信息，所有效应均通过Player和CharacterBase类间接控制
/// </summary>
public class SkillBase
{
    public CharacterBase owner;
    public SkillInfo SkillInfo;
    public string Description;//描述
    public byte index;//技能索引
    public int Level;//技能等级
    public int NeedSp;//技力需求
    public float UseTime;//耗费时点
    public int Chargingcount;//充能次数
    public float Duration;//持续时间
    public float DuringTime;//技能倒计时
    public Sprite sprite;
    public bool InSkill = false;//技能正在持续
    public int UIindex;//UI位置索引
    public Color OutColor;//UI外框颜色
    public SkillEvent SkillEvent;//技能效果
    public List<Vector2Int> SkillRange;//技能范围
    public float[] Number;//数值
    public SkillBase(SkillInfo skillInfo, CharacterBase owner,byte _index)
    {
        this.owner = owner;
        SkillInfo = skillInfo;
        Level = 0;
        if (Level > 5)
        {
            Debug.Log("技能等级超出上限");
            Level = 0;
        }
        index = _index;
        NeedSp = SkillInfo.skillLevelNumbers[Level].needSp;
        UseTime = SkillInfo.skillLevelNumbers[Level].UseTime;
        Chargingcount = SkillInfo.skillLevelNumbers[Level].Chargingcount;
        Duration = SkillInfo.skillLevelNumbers[Level].Duraction;
        Number = SkillInfo.skillLevelNumbers[Level].Number;
        DuringTime = 0;
        try
        {
            Description = string.Format(SkillInfo.skillLevelNumbers[Level].Description, Array.ConvertAll(Number, x => (object)x));
            Description = ColorLibrary.TextDrawColor(Description);
        }
        catch(Exception e)
        {
            Debug.Log($"技能{skillInfo.Name}文本错误:\n{e}");
        }
        sprite = SkillInfo.sprite;
        switch (skillInfo.Rarity)
        {
            case 1:
                UIindex = 1;
                break;
            case 2:
                UIindex = 2;
                break;
            case 3:
                UIindex = 3;
                break;
            case 4:
                UIindex = 0;
                break;
            case 5:
                UIindex = 1;
                break;
            case 6:
                UIindex = 2;
                break;
            case 7:
                UIindex = 3;
                break;
            default:
                UIindex = -1;
                break;
        }
        switch (skillInfo.Type)
        {
            case SkillType.PowerAttack:
                OutColor = Color.red;
                break;
            case SkillType.AttackUp:
                OutColor = Color.magenta;
                break;
            case SkillType.Defend:
                OutColor = Color.yellow;
                break;
            case SkillType.Heal:
                OutColor = Color.green;
                break;
            case SkillType.DeBuff:
                OutColor = Color.black;
                break;
            case SkillType.Buff:
                OutColor = Color.blue;
                break;
            default:
                OutColor = Color.white;
                break;
        }
        SkillEvent = DataLibrary.Instance.GetSkillEvent("SkillEvent_" + skillInfo.ID);
        SkillEvent?.Init(owner,this);
        if (DataLibrary.Instance.skillranges.TryGetValue(skillInfo.skillLevelNumbers[Level].RangeID, out SkillRange skillRange))
        {
            SkillRange = new List<Vector2Int>(skillRange.GridRange);
        }
    }
    public HashSet<Vector2Int> GetRange()
    {
        HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
        foreach(var dir in SkillRange)
        {
            Vector2Int nextPos = new Vector2Int((int)owner.transform.position.x, (int)owner.transform.position.y) + dir;
            if (reachable.Contains(nextPos)) continue;
            reachable.Add(nextPos);
        }
        return reachable;
    }
    public SkillUIInfo GetUIInfo()
    {
        SkillUIInfo info = new SkillUIInfo
        {
            index = index,
            name = SkillInfo.Name,
            description = Description,
            sprite = sprite,
            SpCost = NeedSp,
            Duaction = Duration,
            DuringTime = DuringTime,
            Usetime = GetSkillUseTime(),
            UIindex = UIindex,
            OutColor = OutColor,
            Range = GetRange(),
            skillEvent = SkillEvent
        };
        info.skillButtonType = SkillUIInfo.SkillButtonType.CanUse;
        if (owner.Hp.CurrentSp < NeedSp) info.skillButtonType = SkillUIInfo.SkillButtonType.NotSp;
        if (InSkill) info.skillButtonType = SkillUIInfo.SkillButtonType.InSkill;
        return info;
    }
    public void Skilling(params object[] target)
    {
        InSkill = true;
        DuringTime = Duration;
        SkillEvent?.Skill(target);
        TimeManager.Instance.TimeMoveAction += Duracting;
    }
    public void Duracting(float time)
    {
        DuringTime -= time;
        if (DuringTime <= 0)
        {
            InSkill = false;
            SkillEvent.EndSkill();
            TimeManager.Instance.TimeMoveAction -= Duracting;
        }
    }
    public float GetSkillUseTime()
    {
        float time = UseTime;
        if (owner == null || owner.BufComponent == null)
            return UseTime;
        if (SkillInfo.Rarity == 0 || SkillInfo.Rarity == 4)
        {
            time = owner.BufComponent.GetDefaultAttackTimeReduce(time);
        }
        return time;
    }
}
//技能向UI信息包
public class SkillUIInfo
{
    public enum SkillButtonType
    {
        CanUse,//可用
        NotSp,//技力不足
        InOtherSkill,//其他技能持续中
        InSkill//技能持续中
    }
    public byte index;//技能索引
    public string name;//技能名
    public string description;//技能描述
    public SkillEvent skillEvent;//技能效果
    public Sprite sprite;//技能图标
    public SkillButtonType skillButtonType;//技能状态
    public int SpCost;//花费技力
    public float Duaction;//技能持续时间
    public float Usetime;//技能所需时间
    public float DuringTime;//剩余技能持续时间
    public int UIindex;//UI位置索引
    public Color OutColor;//技能外框颜色
    public HashSet<Vector2Int> Range;//技能范围
}