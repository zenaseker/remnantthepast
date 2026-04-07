using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_DefaultAttack_double : SkillEvent
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
            owner.Animator.SetTrigger("Attack");
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Attack", owner);
            //TimeDestory.Instance.Add(PoolManage.Instance.GetPoolGameObject("VFX", "Amiya_Attack_VFX", owner.Effects), 2f, TimeDestory.EndExecute.PushPool);

        }
        public void Trigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Exit && name == "Attack")
            {
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Attack");
                owner.AnimationAction -= Trigger;
                UIManager.Instance.actionTimeLine.TimeMove(skill.GetSkillUseTime());
            }
            if (type == AnimatorTrigger.AnimatorEventType.Trigger && name == "OnAttack")
            {
                if (target != null)
                {
                    int damage = owner.Damage();
                    owner.AttackAction?.Invoke(damage);
                    DamageInfo dmg = PoolManage.Instance.GetClass<DamageInfo>();
                    dmg.Init(damage, DamageInfo.DamageType.physics, owner, target);
                    target.Damage(dmg);
                    //TimeDestory.Instance.Add(PoolManage.Instance.GetPoolGameObject("VFX", "Amiya_atk_hit", target.Effects), 0.2f, TimeDestory.EndExecute.PushPool);
                }
            }
        }
    }
}