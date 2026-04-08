using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public DamageInfo damageInfo {  get; private set; }
    public virtual void Init(IHpUnit source, IHpUnit target, DamageInfo damageInfo)
    {
        this.damageInfo = damageInfo;
        this.damageInfo.Source = source;
        this.damageInfo.Target = target;
    }
}
