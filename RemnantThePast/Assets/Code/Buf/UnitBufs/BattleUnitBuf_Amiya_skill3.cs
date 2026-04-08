using System.Collections.Generic;
using UnityEngine;

namespace UnitBuf
{
    public class BattleUnitBuf_Amiya_skill3 : BattleUnitBuf
    {
        public override void OnInit()
        {
            owner.BufComponent.DamageAdd.Add(DamageAdd);
        }
        public override void OnAddStrength(int _strength)
        {
            base.OnAddStrength(_strength);
            (owner as CharacterBase).Hp.AddMaxHp(_strength);
        }
        public float DamageAdd()
        {
            return this.stack * 0.01f;
        }
        public override void OnReduceStrength(int _strength)
        {
            base.OnReduceStrength(_strength);
            (owner as CharacterBase).Hp.ReduceMaxHp(_strength);
        }
        public override void OnDestory()
        {
            owner.BufComponent.DamageAdd.Remove(DamageAdd);
            (owner as CharacterBase).Hp.ReduceMaxHp(this.strength);
        }
    }
}