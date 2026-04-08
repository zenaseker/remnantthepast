using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗单位
/// </summary>
public interface IRoundQueneObject
{
    void OnIntoCombat();
    void OnExitCombat();
    bool IsPlayer {  get; set; }
    /// <summary>
    /// 战斗状态（玩家与敌人的使用方法不同，请前往对应的代码查看）
    /// </summary>
    bool InCombat { get; set; }
    GameObject SelectRing { get; set; }
}
public class AbstractRoundQueneObject : IRoundQueneObject
{
    public int Speed { get; set; }
    public bool IsPlayer { get; set; }
    public GameObject SelectRing { get; set; }
    public bool InCombat { get; set; }

    public void OnExitCombat()
    {

    }

    public void OnIntoCombat()
    {

    }
}
/// <summary>
/// 伤害信息
/// </summary>
public class DamageInfo : ICacheable
{
    public enum DamageType
    {
        NoSet,
        physics,
        Magic,
        Real
    }
    public int Damage { get; set; }
    public DamageType damageType { get; set; }
    public IHpUnit Source { get; set; }
    public IHpUnit Target { get; set; }

    public void Init(int dmg, DamageType type, IHpUnit source, IHpUnit target)
    {
        Damage = dmg;
        damageType = type;
        Source = source; 
        Target = target;
    }
    public void Init(int dmg,DamageType type)
    {
        Damage = dmg;
        damageType = type;
    }
    public void Reset()
    {
        Damage = 0;
        damageType = DamageType.NoSet;
        Source = null;
        Target = null;
    }
}