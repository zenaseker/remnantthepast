using System.Collections.Generic;
using UnityEngine;

namespace UnitBuf
{
    public class BattleUnitBuf_DamageAdd : BattleUnitBuf
    {
        public override void OnInit()
        {
            owner.BufComponent.DamageAdd.Add(DamageAdd);
        }
        public float DamageAdd()
        {
            return this.stack * 0.01f;
        }
        public override void OnDestory()
        {
            owner.BufComponent.DamageAdd.Remove(DamageAdd);
        }
    }
}