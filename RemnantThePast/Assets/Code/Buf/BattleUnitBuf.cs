using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnitBuf
{
    public abstract class BattleUnitBuf
    {
        public string ID; //ID
        public BattleUnitBufInfo bufInfo; //文本信息
        public IUnitBuf owner { get; private set; }//Buf单位
        public int stack {  get; private set; } //层数
        public int strength { get; private set; } //强度

        public bool Destoryed = false;//在本帧结束时移除
        public void Init(IUnitBuf target)
        {
            owner = target;
            OnInit();
        }
        /// <summary>
        /// Buf生成时，必须将生效方法注入至控制器的委托中
        /// </summary>
        public abstract void OnInit();
        #region Buf的增减
        public void AddStack(int _stack)
        {
            stack += _stack;
            OnAddStack(_stack);
        }
        public void AddStack(int _stack, int _strength)
        {
            AddStack(_stack);
            strength += _strength;
            OnAddStrength(strength);
        }
        public void ReduceStack(int _stack)
        {
            stack -= _stack;
            OnReduceStack(_stack);
            if (stack <= 0) owner.BufComponent.RemoveBuf(this);
        }
        public void ReduceStrength(int _strength)
        {
            strength -= _strength;
            OnReduceStrength(strength);
        }
        public virtual void OnAddStack(int _stack) { }
        public virtual void OnAddStrength(int _strength) { }
        public virtual void OnReduceStack(int _stack) { }
        public virtual void OnReduceStrength(int _strength) { }
        #endregion
        public void Destory()
        {
            owner.BufComponent.RemoveBuf(this);
        }
        /// <summary>
        /// Buf移除时，必须将生效方法移除出控制器的委托中
        /// </summary>
        public abstract void OnDestory();
    }
    public class BattleUnitBufInfo
    {
        public string ID;
        public string Name;
        public string Description;
        public static BattleUnitBufInfo NullBuf = new BattleUnitBufInfo()
        {
            ID = "null",
            Name = "异常Buf",
            Description = "未查询到该Buf的文本信息"
        };
    }
}