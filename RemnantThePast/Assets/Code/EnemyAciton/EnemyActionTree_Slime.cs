using EnemyActionTree;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyActionTree
{
    public class EnemyActionTree_Slime : EnemyActionTreeBase
    {

        public override void IntoCombat()
        {
            if (TryGetIntent(0, out MonsterIntentInfo intent))
            {
                UIManager.Instance.actionTimeLine.AddAction(new EnemyActionTime(owner, intent));
            }
        }
        public override Action OnTimeActionTrigger(byte index)
        {
            switch (index)
            {
                case 0:
                    return DefaultAttack;
            }
            return null;
        }
        void DefaultAttack()
        {
            owner.SkillAction?.Invoke(null);
            owner.AnimationAction += OnAnimatorTrigger;
            owner.Animator.SetTrigger("Attack");
            GameApp.Instance.AddEvent($"Monster{owner.MonsterInfo.ID}Attack", owner);
        }
        public void OnAnimatorTrigger(AnimatorTrigger.AnimatorEventType type, string name)
        {
            if (type == AnimatorTrigger.AnimatorEventType.Trigger && name == "OnAttack")
            {
                if (GameApp.Instance.CombatCharacterList.Count <= 0) return;
                IHpUnit target = GameApp.Instance.CombatCharacterList.GetRandom() as IHpUnit;
                if (target != null)
                {
                    int damage = owner.MonsterInfo.Attack;
                    DamageInfo dmg = PoolManage.Instance.GetClass<DamageInfo>();
                    dmg.Init(damage, DamageInfo.DamageType.physics, owner, target);
                    target.Damage(dmg);
                    owner.AttackAction?.Invoke(damage);
                }
            }
            if (type == AnimatorTrigger.AnimatorEventType.Exit && name == "Attack")
            {
                owner.AnimationAction -= OnAnimatorTrigger;
                GameApp.Instance.SetEventTrue($"Monster{owner.MonsterInfo.ID}Attack");
                if (TryGetIntent(0, out MonsterIntentInfo intent))
                {
                    UIManager.Instance.actionTimeLine.AddAction(new EnemyActionTime(owner, intent));
                }
            }
        }
    }

}