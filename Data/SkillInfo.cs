using Newtonsoft.Json;
using SelectControl;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillInfo
{
    public string Attribution {get; set;}//技能归属
    public string ID{get;set;}//ID
    public string Name{get;set;}//名称
    public string Image{get;set;}//技能图标
    /// <summary>
    /// 技能等级(0:普通攻击;1~3:技能;4:特殊普攻;5~7:被动)
    /// </summary>
    public int Rarity{get;set; }
    public bool IsDerivedSkill { get; set; } = false;//衍生技能
    public SkillType Type {get; set;}//技能类型
    public SkillLevelNumber[] skillLevelNumbers { get; set;} //技能升级数据
    public  TargetType targetType {get; set;}//技能选择目标


    [JsonIgnore]
    Sprite _sprite;
    [JsonIgnore]
    public Sprite sprite {
        get
        {
            if (Image == null || Image == "")
            {
                _sprite = PoolManage.Instance.GetSprite("icon/image_skill_None");
            }
            if (_sprite == null)
            {
                _sprite = PoolManage.Instance.GetSprite("Skill/" + Image);
            }
            return _sprite;
        }
    }//技能图标
    public void ReSetSprite(string sprite)
    {
        Image = sprite;
        if (Image == null || Image == "")
        {
            _sprite = Resources.Load<Sprite>("icon/icon/image_skill_None");
            return;
        }
        _sprite = Resources.Load<Sprite>("icon/Skill/" + Image);
    }
    [JsonIgnore]
    public ObjectSelectControl selectcontrol;//技能选择控制
#if UNITY_EDITOR
    [JsonIgnore]
    public bool ScreenAttribution = true;//归属筛选
#endif
    public SkillInfo()
    {
        Rarity = 0;
        skillLevelNumbers = new SkillLevelNumber[5];
        for (int i = 0; i < skillLevelNumbers.Length; i++)
        {
            skillLevelNumbers[i] = new SkillLevelNumber();
            skillLevelNumbers[i].Number = new float[1];
        }
    }
    public enum SkillType
    {
        Other,//其他
        PowerAttack,//强力击
        AttackUp,//普攻强化
        Defend,//防御
        Heal,//回复
        DeBuff,//施加减益
        Buff//给予增益
    }
    /// <summary>
    /// 技能选择目标
    /// </summary>
    public enum TargetType
    {
        Self,//自身
        OtherChar,//其他干员
        Char,//干员
        OneTarget,//一名敌人
        AllTarget,//范围内所有目标
        OneGrid//一个地块
    }
    [System.Serializable]
    public class SkillLevelNumber
    {
        public string Description { get; set; }//描述
        public int needSp { get; set; }//技力需求
        public float UseTime { get; set; }//时间花费
        public float Duraction { get; set; }//持续时间
        public int Chargingcount { get; set; }//充能上限
        public string RangeID { get; set; }//技能范围
        public float[] Number { get; set; }//数值
    }
    #region 文本转译
    public string LevelToString()
    {
        switch (Rarity)
        {
            case 0:
                return "普通攻击";
            case 1:
                return "技能1";
            case 2:
                return "技能2";
            case 3:
                return "技能3";
            case 4:
                return "特殊普通攻击";
            case 5:
                return "被动";//5 6 7分别对应技能1 2 3位置，与等级无关
            case 6:
                return "被动";//5 6 7分别对应技能1 2 3位置，与等级无关
            case 7:
                return "被动";//5 6 7分别对应技能1 2 3位置，与等级无关
            default:
                return "不属于任何技能";
        }
    }
    public string SkillTypeToString()
    {
        switch (Type)
        {
            case SkillType.PowerAttack:
                return "强力击";
            case SkillType.AttackUp:
                return "普攻强化";
            case SkillType.Defend:
                return "防御";
            case SkillType.Heal:
                return "回复";
            case SkillType.DeBuff:
                return "削弱";
            case SkillType.Buff:
                return "增幅";
            default:
                return "其他";
        }
    }
    #endregion
}
