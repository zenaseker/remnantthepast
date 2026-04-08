using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    /// <summary>
    /// 技能效果
    /// </summary>
    public abstract class SkillEvent
    {
        protected CharacterBase owner;
        protected SkillBase skill;
        public void Init(CharacterBase _owner,SkillBase _base)
        {
            owner = _owner;
            skill = _base;
        }
        public virtual void Skill(params object[] _target)//技能启动
        {

        }
        public abstract List<IRoundQueneObject> GetTarget();//目标敌人
        protected List<IRoundQueneObject> AllTargetInRange()
        {
            List<IRoundQueneObject> targets = new List<IRoundQueneObject>();
            foreach (IRoundQueneObject target in GameApp.Instance.CombatMonsterList)
            {
                if (target != null)
                {
                    Vector3 pos = ((EnemyBase)target).gameObject.transform.position;
                    if (skill.GetRange().Contains(new Vector2Int((int)pos.x, (int)pos.y)))
                    {
                        targets.Add(target);
                    }
                }
            }
            return targets;
        }
        public virtual void Update(float time)
        {

        }
        public virtual void EndSkill()//技能结束
        {

        }
        /// <summary>
        /// 技能释放许可，在输入点击内容后
        /// </summary>
        /// <param name="input"></param>
        public virtual bool SkillReleasePermit(object input, string type)
        {
            return true;
        }
        /// <summary>
        /// 技能控制器
        /// </summary>
        /// <returns></returns>
        public virtual string SelectControl()
        {
            return "CharacterDefaultSelectControl_Skill";
        }
    }
}
