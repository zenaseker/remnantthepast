using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnemyActionTree
{
    public abstract class EnemyActionTreeBase
    {
        protected EnemyBase owner { get; set; }
        protected List<MonsterIntentInfo> monsterIntentInfos { get; set; }
        public void Awake(EnemyBase enemy)
        {
            owner = enemy;
            monsterIntentInfos = enemy.MonsterInfo.monsterIntentInfos;
            for (byte i = 0; i < monsterIntentInfos.Count; i++)
            {
                monsterIntentInfos[i].ID = i;
                monsterIntentInfos[i].Trigger = OnTimeActionTrigger(i);
            }
        }
        protected virtual void OnAwake()
        {

        }
        public virtual void IntoCombat()
        {

        }
        public virtual void OutCombat()
        {

        }
        public abstract Action OnTimeActionTrigger(byte index);
        public bool TryGetIntent(byte _byte, out MonsterIntentInfo Intent)
        {
            if (monsterIntentInfos.Count > _byte)
            {
                Intent = monsterIntentInfos[_byte];
                return true;
            }
            Intent = null;
            return false;
        }
        protected virtual void OnDestory()
        {

        }
    }
    public class EnemyAction
    {
        protected EnemyBase owner { get; set; }
        public void OnTrigger()
        {

        }
    }
}