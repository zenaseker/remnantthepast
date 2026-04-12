using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Passive
{
    /// <summary>
    /// Ìì¸³´úÂë
    /// </summary>
    public abstract class PassiveBase
    {
        public PassiveInfo PassiveInfo { get; set; }
        public abstract void OnInit();
        public abstract void OnDestory();
    }

}