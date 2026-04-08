using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MonsterInfo
{
    public string ID {  get; set; }//ID
    public string Name { get; set; }//名称
    public string Description { get; set; }//描述
    public string Icon { get; set; }//图标
    public int MaxHp { get; set; }//生命
    public int Attack { get; set; }//攻击力
    public List<Vector2Int> WarningRange { get; set; }//警戒范围
    public List<Vector2Int> AttackRange { get; set; }//攻击范围
    public List<MonsterIntentInfo> monsterIntentInfos { get; set; }//意图组
    public string Model { get; set; }//模型
    [JsonIgnore]
    Sprite _sprite;
    [JsonIgnore]
    public Sprite sprite
    {
        get
        {
            if (_sprite == null)
            {
                _sprite = PoolManage.Instance.GetSprite("enemies/" + Icon);
            }
            return _sprite;
        }
    }//技能图标
    public void ReSetSprite(string sprite)
    {
        Icon = sprite;
        _sprite = Resources.Load<Sprite>("icon/enemies/" + Icon);
    }
    public void SetDefaultData()
    {
        Name = "New Monster";
        Description = "";
        WarningRange = new List<Vector2Int>();
        AttackRange = new List<Vector2Int>();
        monsterIntentInfos = new List<MonsterIntentInfo>();
    }
    public MonsterInfo Clone()
    {
        MonsterInfo info = new MonsterInfo
        {
            ID = ID,
            Name = Name,
            Description = Description,
            Attack = Attack,
            MaxHp = MaxHp,
            Icon = Icon,
            _sprite = _sprite,
            WarningRange = WarningRange,
            AttackRange = AttackRange,
            monsterIntentInfos = new List<MonsterIntentInfo>(monsterIntentInfos.Count),
            Model = Model
        };
        foreach (var intent in monsterIntentInfos)
        {
            info.monsterIntentInfos.Add(intent.Clone());
        }
        return info;
    }
}
//敌人意图
[System.Serializable]
public class MonsterIntentInfo
{
    public byte ID { get; set; }//ID
    public string Name { get; set; }//名称
    public string Description { get; set; }//描述
    public float time { get; set; }//时间
    public int strength { get; set; }//警告强度(0~2)
    public string icon { get; set; }//图片
    [JsonIgnore]
    Sprite _sprite;
    [JsonIgnore]
    public Sprite sprite
    {
        get
        {
            if (_sprite == null)
            {
                _sprite = PoolManage.Instance.GetSprite("enemies/" + icon);
            }
            return _sprite;
        }
    }//技能图标
    public void ReSetSprite(string sprite)
    {
        icon = sprite;
        _sprite = Resources.Load<Sprite>("icon/enemies/" + icon);
    }
    [JsonIgnore]
    public Action Trigger;//触发
    public MonsterIntentInfo Clone()
    {
        MonsterIntentInfo newinfo = new MonsterIntentInfo
        {
            ID = ID,
            Name = Name,
            Description = Description,
            time = time,
            icon = icon,
            strength = strength,
            _sprite = _sprite
        };
        return newinfo;
    }
}