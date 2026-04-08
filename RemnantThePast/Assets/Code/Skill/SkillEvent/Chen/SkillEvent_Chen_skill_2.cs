using SelectControl;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillComponent
{
    public class SkillEvent_Chen_skill_2 : SkillEvent
    {

        Vector2Int direction;
        public override List<IRoundQueneObject> GetTarget()
        {
            Vector3 charpos = owner.transform.position;
            HashSet<Vector2Int> rotaterange = MathHelper.RotateWorldRange(skill.GetRange(),
                new Vector2Int((int)charpos.x, (int)charpos.y), direction);
            List<IRoundQueneObject> targets = new List<IRoundQueneObject>();
            foreach (IRoundQueneObject target in GameApp.Instance.CombatMonsterList)
            {
                if (target != null)
                {
                    Vector3 pos = ((EnemyBase)target).gameObject.transform.position;
                    if (rotaterange.Contains(new Vector2Int((int)pos.x, (int)pos.y)))
                    {
                        targets.Add(target);
                    }
                }
            }
            return targets;
        }
        public override void Skill(params object[] _target)
        {
            direction = ((Vector2Int)_target[0]);
            owner.Animator.SetTrigger("Skill2");
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
                roundQueneObjects.Shuffle();
                roundQueneObjects = roundQueneObjects.GetRange(0, Mathf.Min(roundQueneObjects.Count,(int)skill.Number[0]));
                foreach (var target in roundQueneObjects)
                {
                    int damage = (int)(owner.Damage() * skill.Number[1] / 100);
                    owner.AttackAction?.Invoke(damage);
                    //(target as IHpUnit).Damage(damage);

                    owner.AttackAction?.Invoke(damage);
                    //(target as IHpUnit).Damage(damage);
                }
            }
        }
        public override string SelectControl()
        {
            return "SkillSelectControl_DirectionSelection";
        }
    }
}