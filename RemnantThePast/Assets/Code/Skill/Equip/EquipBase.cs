using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Equip
{
    /// <summary>
    /// 装备代码
    /// </summary>
    public abstract class EquipBase
    {
        public abstract int EquipInfoID { get; }
        public EquipInfo EquipInfo { get; set; }
        public EquipmentData EquipmentData { get; set; }
        public IRoundQueneObject owner { get; set; }
        public void Equip(IRoundQueneObject _owner)
        {
            owner = _owner;
            OnEquip();
        }
        public abstract void OnEquip();//委托绑在BufCompenent里面
        public void RemoveEuqip()
        {
            owner = null;
            OnRemoveEuqip();
        }
        public abstract void OnRemoveEuqip();
    }

}