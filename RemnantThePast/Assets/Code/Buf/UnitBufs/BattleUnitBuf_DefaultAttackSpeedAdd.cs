using System.Collections.Generic;
using UnityEngine;

namespace UnitBuf
{
    public class BattleUnitBuf_DefaultAttackSpeedAdd : BattleUnitBuf
    {
        public override void OnInit()
        {
            owner.BufComponent.DefaultAttackTimeReduce.Add(DefaultAttackTimeReduce);
        }
        public float DefaultAttackTimeReduce(float time)
        {
            return (1 - stack * 0.01f) * time;
        }

        public override void OnDestory()
        {
            owner.BufComponent.DefaultAttackTimeReduce.Remove(DefaultAttackTimeReduce);
        }
    }
}