using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_Chen_skill_1 : SkillEvent
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
            Vector3 face = target.transform.position - owner.transform.position;
            if (face.x < 0)
            {
                owner.SetFace(false);
            }
            if (face.x > 0)
            {
                owner.SetFace(true);
            }
            owner.Animator.SetTrigger("Skill");
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Skill", owner);
        }
        public void Trigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Exit && name == "Skill_End")
            {
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Skill");
                owner.AnimationAction -= Trigger;
                UIManager.Instance.actionTimeLine.TimeMove(skill.GetSkillUseTime());
            }
            if (type == AnimatorTrigger.AnimatorEventType.Trigger && name == "OnAttack")
            {
                if (target != null)
                {
                    int damage = (int)(owner.Damage() * skill.Number[0] / 100f);
                    owner.AttackAction?.Invoke(damage);
                    DamageInfo dmg = PoolManage.Instance.GetClass<DamageInfo>();
                    dmg.Init(damage, DamageInfo.DamageType.physics, owner, target);
                    target.Damage(dmg);
                }
            }
        }
    }
}