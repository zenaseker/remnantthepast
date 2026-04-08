using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillComponent
{
    /// <summary>
    /// 技能范围
    /// </summary>
    public class SkillRange
    {
        public string ID {  get; set; }//ID
        /// <summary>
        /// 该坐标为以原点为(0,0)点的相对位置
        /// </summary>
        public List<Vector2Int> GridRange { get; set; }//位置
        public int MaxSize { get; set; }//图格范围
        public Vector2Int Center { get; set; }//中心
    }
}
