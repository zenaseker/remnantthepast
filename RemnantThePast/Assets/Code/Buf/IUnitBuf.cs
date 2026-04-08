using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitBuf
{
    BufComponent BufComponent { get; set; }
    Action<float> TimeLogicUpdate { get; set; }//时间轴推进时触发
    Action<float> UpdateAction { get; set; }//每帧触发
    Action<int,float> HpChangeAction { get; set; }//血量变动时触发(变动值，当前百分比)
    Action<float, float> SpChangeAction { get; set; }//技力变动时触发(变动值，当前百分比)
    Action<int> AttackAction { get; set; }//攻击时触发
    Action<SkillBase> SkillAction { get; set; }//使用技能时触发
    Transform Effects {  get; set; } //特效位置
}
