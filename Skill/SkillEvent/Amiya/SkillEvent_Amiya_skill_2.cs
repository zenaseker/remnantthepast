using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_Amiya_skill_2 : SkillEvent
    {
        GameObject effect;
        public override List<IRoundQueneObject> GetTarget()
        {
            return null;
        }

        public override void Skill(params object[] _target)
        {
            owner.Animator.SetTrigger("Skill");
            owner.AnimationAction += Trigger;
            owner.BufComponent.OnCombatEnd += OnCombatEnd;
            owner.BufComponent.CanMove.Add(CanMove);
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Skill_Begin",owner);
            effect = PoolManage.Instance.GetPoolGameObject("VFX", "Amiya_skill2", owner.Effects);
            if (DataLibrary.Instance.skillInfos.TryGetValue("Amiya_skill_2_Atk", out SkillInfo skillInfo))
            {
                SkillBase skillbase = new SkillBase(skillInfo, owner, 0);
                owner.skills[0] = skillbase;
            }
        }
        public void Trigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Exit && name == "Skill_Begin")
            {
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Skill_Begin");
                owner.AnimationAction -= Trigger;
            }
            if (type == AnimatorTrigger.AnimatorEventType.Exit && name == "Skill_End")
            {
                GameApp.Instance.SetEventTrue($"Character{owner.CharacterInfo.ID}Skill_End");
                owner.AnimationAction -= Trigger;
            }
        }
        public override void EndSkill()
        {
            owner.Animator.SetTrigger("EndSkill");
            owner.AnimationAction += Trigger;
            owner.BufComponent.CanMove.Remove(CanMove);
            owner.RestoreSkill();
            GameApp.Instance.AddEvent($"Character{owner.CharacterInfo.ID}Skill_End", owner);
            PoolManage.Instance.PushGameObject(effect, true);
        }
        void OnCombatEnd()
        {
            owner.skills[skill.index].Duracting(9999);
            owner.BufComponent.OnCombatEnd -= OnCombatEnd;
        }
        bool CanMove()
        {
            return false;
        }
        public override string SelectControl()
        {
            return "SkillSelectControl_PowerToSelf";
        }
    }

}