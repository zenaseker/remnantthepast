using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_Chen_skill_3 : SkillEvent
    {
        public override List<IRoundQueneObject> GetTarget()
        {
            return base.AllTargetInRange();
        }
        public override void Skill(params object[] _target)
        {
            owner.Animator.SetTrigger("Skill3");
            owner.AnimationAction += Trigger;
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
                List<IRoundQueneObject> roundQueneObjects = GetTarget();
                if (roundQueneObjects.Count > 0)
                {
                    IHpUnit target = roundQueneObjects.GetRandom() as IHpUnit;
                    int damage = (int)(owner.Damage() * skill.Number[0] / 100);
                    owner.AttackAction?.Invoke(damage);
                    DamageInfo dmg = PoolManage.Instance.GetClass<DamageInfo>();
                    dmg.Init(damage, DamageInfo.DamageType.physics, owner, target);
                    target.Damage(dmg);
                }
            }
        }
    }
}