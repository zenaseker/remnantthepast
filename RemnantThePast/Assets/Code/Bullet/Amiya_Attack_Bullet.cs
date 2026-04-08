using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Amiya_Attack_Bullet : BulletBase
{
    Transform trail;
    Vector3 tar;
    Vector3 sou;
    float dis;
    float time = 0f;
    private void Awake()
    {
        trail = transform.GetChild(0);
    }
    public override void Init(IHpUnit source, IHpUnit target, DamageInfo damageInfo)
    {
        time = 0f;
        base.Init(source, target, damageInfo);
        tar = target.GetGameObject().transform.position;
        sou = transform.GetChild(0).transform.position = source.GetGameObject().transform.position;
        sou = tar - sou;
        dis = sou.magnitude / 0.33f;
        sou = sou.normalized;
        TimeDestory.Instance.Add(gameObject, 0.5f,TimeDestory.EndExecute.PushPool);
    }
    private void Update()
    {
        time += Time.deltaTime;
        if (time > 0.2f)
        {
            trail.position += sou * dis * Time.deltaTime;
        }
    }
    public void OnDisable()
    {
        if (damageInfo.Target != null)
        {
            TimeDestory.Instance.Add(PoolManage.Instance.GetPoolGameObject("VFX", "Amiya_atk_hit", (damageInfo.Target as EnemyBase).Effects), 0.2f, TimeDestory.EndExecute.PushPool);
            (damageInfo.Source as IUnitBuf).AttackAction?.Invoke(damageInfo.Damage);
            damageInfo.Target.Damage(damageInfo);
        }
        PoolManage.Instance.PushGameObject(gameObject);
    }
}
