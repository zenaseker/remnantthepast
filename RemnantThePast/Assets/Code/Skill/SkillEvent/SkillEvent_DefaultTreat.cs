using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_DefaultTreat : SkillEvent
    {
        IHpUnit target;
        public override List<IRoundQueneObject> GetTarget()
        {
            List<IRoundQueneObject> targets = new List<IRoundQueneObject>();
            return targets;
        }

        public override void Skill(params object[] _target)
        {
            target = ((GameObject)_target[0]).GetComponent<IHpUnit>(); 
            owner.AnimationAction += Trigger;
            Vector3 face = target.GetGameObject().transform.position - owner.transform.position;
            if (face.x < 0)
            {
                owner.SetFace(false);
            }
            if (face.x > 0)
            {
                owner.SetFace(true);
            }
            owner.Animator.SetTrigger("Attack");
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Heal", owner);
        }

        public void Trigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Exit && name == "Attack")
            {
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Heal");
                owner.AnimationAction -= Trigger;
                UIManager.Instance.actionTimeLine.TimeMove(skill.GetSkillUseTime());
            }
            if (type == AnimatorTrigger.AnimatorEventType.Trigger && name == "OnAttack")
            {
                if (target != null)
                {
                    int damage = owner.Damage();
                    target.Heal(damage);
                }
            }
        }
        public override string SelectControl()
        {
            return "SkillSelectControl_Heal";
        }
    }
}