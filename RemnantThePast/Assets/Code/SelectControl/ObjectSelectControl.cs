using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SelectControl
{
    /// <summary>
    /// 输入控制
    /// </summary>
    public abstract class ObjectSelectControl
    {
        public object Select;//当前选择单位
        public virtual void OnInit(object obj) { }
        public void Init(object select)
        {
            Select = select;
            OnInit(select);
        }
        public virtual void SubInit(params object[] obj) { }
        public virtual void OnSelect(object obj, string type) { }
        public virtual void Update() { }
    }
}
