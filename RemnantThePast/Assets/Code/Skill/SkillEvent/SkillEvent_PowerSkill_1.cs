using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_PowerSkill_1 : SkillEvent
    {
        GameObject vfx;
        public override List<IRoundQueneObject> GetTarget()
        {
            return null;
        }

        public override void Skill(params object[] _target)
        {
            owner.BufComponent.AddBuf("DefaultAttackSpeedAdd", (int)skill.Number[0]);
            vfx = PoolManage.Instance.GetPoolGameObject("VFX", "SkillPower_vfx", owner.Effects);
            MusicManage.Instance.PlayEffect($"voice/btl_snd_0/b_char_atkboost");
        }
        public override void EndSkill()
        {
            owner.BufComponent.ReduceBufStack("DefaultAttackSpeedAdd", (int)skill.Number[0]);
            if (vfx != null)
            {
                PoolManage.Instance.PushGameObject(vfx, true);
            }
        }
        public override string SelectControl()
        {
            return "SkillSelectControl_PowerToSelf";
        }
    }

}