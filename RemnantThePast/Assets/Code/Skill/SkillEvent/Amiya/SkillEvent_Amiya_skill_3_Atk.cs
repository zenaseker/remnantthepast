using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_Amiya_skill_3_Atk : SkillEvent
    {
        EnemyBase target;
        public override List<IRoundQueneObject> GetTarget()
        {
            return base.AllTargetInRange();
        }

        public override void Skill(params object[] _target)
        {
            owner.AnimationAction += Trigger;
            target = ((GameObject)_target[0]).GetComponent<EnemyBase>();
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Attack", owner);
        }
        public void Trigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Trigger && name == "OnAttack")
            {
                if (target != null)
                {
                    int damage = (int)(owner.Damage() * skill.Number[0]);
                    owner.AttackAction?.Invoke(damage);
                    DamageInfo dmg = PoolManage.Instance.GetClass<DamageInfo>();
                    dmg.Init(damage, DamageInfo.DamageType.physics, owner, target);
                    target.Damage(dmg);
                }
                owner.AnimationAction -= Trigger;
                UIManager.Instance.actionTimeLine.TimeMove(skill.GetSkillUseTime());
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Attack");
            }
        }
    }
}