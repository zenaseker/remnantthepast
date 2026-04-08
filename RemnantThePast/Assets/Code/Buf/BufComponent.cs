using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitBuf;
using System;
/// <summary>
/// Buf组件
/// </summary>
public class BufComponent
{
    IUnitBuf owner;
    public List<BattleUnitBuf> BufList = new List<BattleUnitBuf>();
    /// <summary>
    /// 战斗结束时
    /// </summary>
    public Action OnCombatEnd;
    public BufComponent(IUnitBuf _owner)
    {
        owner = _owner;
        _owner.UpdateAction += Update;
    }

    #region Buf的添加和移除
    public BattleUnitBuf AddNewBuf(BattleUnitBuf battleUnitBuf)
    {
        BufList.Add(battleUnitBuf);
        battleUnitBuf.Init(owner);
        return battleUnitBuf;
    }
    public BattleUnitBuf AddNewBuf(string id)
    {
        BattleUnitBuf battleUnitBuf = DataLibrary.Instance.GetBuf(id);
        BufList.Add(battleUnitBuf);
        battleUnitBuf.Init(owner);
        return battleUnitBuf;
    }
    public BattleUnitBuf AddBuf(string id,int stack)
    {
        BattleUnitBuf buf = BufList.Find(x => x.ID == id);
        if (buf == null)
        {
            buf = AddNewBuf(id);
        }
        buf?.AddStack(stack);
        return buf;
    }
    public BattleUnitBuf AddBuf(string id, int stack, int strength)
    {
        BattleUnitBuf buf = BufList.Find(x => x.ID == id);
        if (buf == null)
        {
            buf = AddNewBuf(id);
        }
        buf.AddStack(stack, strength);
        return buf;
    }
    public void ReduceBufStack(string id, int stack)
    {
        BattleUnitBuf buf = BufList.Find(x => x.ID == id);
        if (buf == null) return;
        buf.ReduceStack(stack);
    }
    public void RemoveBuf(BattleUnitBuf battleUnitBuf)
    {
        battleUnitBuf.Destoryed = true;
    }
    public void RemoveBuf(string id)
    {
        BattleUnitBuf buf = BufList.Find(x => x.ID == id && !x.Destoryed);
        if (buf != null)
        {
            buf.Destoryed = true;
        }
    }

    #endregion
    public bool TryGetBuf(string id, out BattleUnitBuf buf)
    {
        if (id == null)
        {
            buf = null;
            return false;
        }
        buf = BufList.Find(x => x.ID == id);
        return buf != null;
    }
    /// <summary>
    /// 清除已注销的Buf
    /// </summary>
    /// <param name="time"></param>
    void Update(float time)
    {
        for (int i = 0; i < BufList.Count;)
        {
            if (BufList[i].Destoryed)
            {
                BufList[i].OnDestory();
                BufList.RemoveAt(i); 
            }
            else i++;
        }
    }

    /// <summary>
    /// 普通攻击消耗时间减免(叠乘)
    /// </summary>
    public List<Func<float, float>> DefaultAttackTimeReduce = new List<Func<float, float>>();
    public float GetDefaultAttackTimeReduce(float time)
    {
        foreach (var timereduce in DefaultAttackTimeReduce)
        {
            time = timereduce(time);
        }
        return time;
    }

    /// <summary>
    /// 干员可否移动
    /// </summary>
    public List<Func<bool>> CanMove = new List<Func<bool>>();
    public bool CanToMove()
    {
        foreach (var move in CanMove)
        {
            if (!move()) return false;
        }
        return true;
    }

    /// <summary>
    /// 干员攻击力加成
    /// </summary>
    public List<Func<float>> DamageAdd = new List<Func<float>>();
    /// <summary>
    /// 攻击力提升(加算)
    /// </summary>
    /// <param name="defaultdamage"></param>
    /// <returns></returns>
    public float GetDamageAdd()
    {
        float damage = 1;
        foreach(var dmgadd in DamageAdd)
        {
            damage += dmgadd();
        }
        return damage;
    }
}
