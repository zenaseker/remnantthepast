using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_Amiya_skill_2_Atk : SkillEvent
    {
        public override List<IRoundQueneObject> GetTarget()
        {
            return base.AllTargetInRange();
        }
        public override void Skill(params object[] _target)
        {
            owner.AnimationAction += Trigger;
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Attack", owner);
        }
        public void Trigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Trigger && name == "OnAttack")
            {
                List<IRoundQueneObject> roundQueneObjects = GetTarget();
                for (int i = 0; i < skill.Number[1]; i++)
                {
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
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Attack");
                owner.AnimationAction -= Trigger;
                UIManager.Instance.actionTimeLine.TimeMove(skill.GetSkillUseTime());
            }
        }
    }
}