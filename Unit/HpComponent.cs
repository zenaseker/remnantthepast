using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpComponent
{
    static Transform UnitUI;
    public IHpUnit Unit;
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public float MaxSp { get; private set; }
    public float CurrentSp { get; private set; }
    HpUI HpUI;
    /// <summary>
    /// 使用技力的单位
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="Hp"></param>
    /// <param name="_MaxSp"></param>
    /// <param name="Sp"></param>
    public HpComponent(IHpUnit unit, int Hp, int _MaxSp, int Sp)
    {
        if (UnitUI == null)
        {
            UnitUI = UIManager.Instance.transform;
            if (UnitUI == null)
            {
                Debug.Log("错误：角色UI画布未找到");
            }
        }
        Unit = unit;
        CurrentHp = MaxHp = Hp;
        MaxSp = _MaxSp;
        CurrentSp = Sp;
        TimeManager.Instance.TimeMoveAction += RecoverSpByTime;
        HpUI = PoolManage.Instance.GetPoolGameObject("UI", "HpSpUI", UnitUI).GetComponent<HpUI>();
        HpUI.Init(Unit.GetTransform());
        HpUI.ChangeSp(CurrentSp / MaxSp);
    }
    /// <summary>
    /// 不使用技力的单位
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="Hp"></param>
    public HpComponent(IHpUnit unit, int Hp)
    {
        if (UnitUI == null)
        {
            UnitUI = UIManager.Instance.transform;
            if (UnitUI == null)
            {
                Debug.Log("错误：角色UI画布未找到");
            }
        }
        Unit = unit;
        CurrentHp = MaxHp = Hp;
        CurrentSp = MaxSp = 0;
        HpUI = PoolManage.Instance.GetPoolGameObject("UI", "HpUI", UnitUI).GetComponent<HpUI>();
        HpUI.Init(Unit.GetTransform());
    }
    public void RecoverSpByTime(float time)//随时间自动恢复技力
    {
        Sp(time * 5);
    }
    public void Sp(float sp)
    {
        CurrentSp += sp;
        CurrentSp = Mathf.Min(CurrentSp, MaxSp);
        (Unit as IUnitBuf).SpChangeAction?.Invoke(sp, (float)CurrentSp / (float)MaxSp);
        HpUI.ChangeSp(CurrentSp / MaxSp);
    }
    public void UseSp(float sp)
    {
        CurrentSp -= sp;
        CurrentSp = Mathf.Max(CurrentSp, 0);
        (Unit as IUnitBuf).SpChangeAction?.Invoke(-sp, (float)CurrentSp / (float)MaxSp);
        HpUI.ChangeSp(CurrentSp / MaxSp);
    }
    public void Damage(int damage)
    {
        int Hp = CurrentHp;
        CurrentHp = Mathf.Max(CurrentHp - damage, 0);
        (Unit as IUnitBuf).HpChangeAction?.Invoke(-damage, (float)CurrentHp / (float)MaxHp);
        if (Hp == CurrentHp) return;
        if (CurrentHp <= 0)
        {
            Unit.Dying();
        }
        HpUI.HpChange((float)Hp / MaxHp, (float)CurrentHp / MaxHp);
    }
    public void Heal(int heal)
    {
        CurrentHp += heal;
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
        (Unit as IUnitBuf).HpChangeAction?.Invoke(heal, (float)CurrentHp / (float)MaxHp);
        HpUI.HpChange((float)CurrentHp / (float)MaxHp);
    }
    public void AddMaxHp(int hp)
    {
        MaxHp += hp;
        CurrentHp += hp;
        HpUI.HpChange((float)CurrentHp / (float)MaxHp);
    }
    public void ReduceMaxHp(int hp)
    {
        MaxHp -= hp;
        CurrentHp = Mathf.Min(CurrentHp, MaxHp);
        HpUI.HpChange((float)CurrentHp / (float)MaxHp);
    }
    public void OnDead()
    {
        Debug.LogWarning("Dead");
        PoolManage.Instance.PushGameObject(HpUI.gameObject);
        TimeManager.Instance.TimeMoveAction -= RecoverSpByTime;
    }
    public void HideUI()
    {
        HpUI.gameObject.SetActive(false);
    }
    public void ShowUI()
    {
        HpUI.gameObject.SetActive(true);
    }
}
/// <summary>
/// 使用血量技力组件的单位
/// </summary>

public interface IHpUnit
{
    void Damage(DamageInfo damage);
    void Dying();
    void Heal(int heal);
    Transform GetTransform();
}