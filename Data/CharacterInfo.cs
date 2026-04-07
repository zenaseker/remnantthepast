using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 干员信息
/// </summary>
[System.Serializable]
public class CharacterInfo
{
    public int ID{get;set;}//ID
    public string Name{get;set;}//名称
    public string Description{get;set;}//描述
    public int Rarity{get;set;}//干员星级
    public Profession _Profession { get;set;}//干员职业
    public int MaxHp{get;set; }//最大生命
    public int IncreaseHp { get; set; }//每级增加的最大生命
    public int StartMagic{get;set;}//初始技力
    public int MaxMagic{get;set; }//最大技力
    public float IncreaseMagic { get; set; }//每级增加的最大技力
    public int Attack { get;set; }//攻击力
    public int IncreaseAttack { get; set; }//每级增加的攻击力
    public int MoveDistance { get; set; }//移动距离
    public string[] Skills{get;set; }//技能列表
    public string[] Passives { get; set; } = new string[2];//被动列表
    public bool InitialUnLock{get;set; }//是否为初始干员
    public string Icon { get; set; }//头像
    public string Model{get;set;}//干员模型(注意：Model中的内容实际为干员模型素材所对应的文件夹，导入时文件夹下的内容会按固定格式导入)
    public void SetDefaultData()
    {
        Name = "New character";
        Rarity = 1;
        MaxHp = 100;
        StartMagic = 20;
        MaxMagic = 100;
        Skills = new string[4];
        InitialUnLock = false;
    }
    public enum Profession
    {
        Caster,
        Medic,
        Pioneer,
        Sniper,
        Special,
        Support,
        Tank,
        Warrior
    }
    public static string GetProfessionText(Profession profession)
    {
        switch(profession)
        {
            case Profession.Caster:
                return "术士";
            case Profession.Medic:
                return "医疗";
            case Profession.Pioneer:
                return "先锋";
            case Profession.Sniper:
                return "狙击";
            case Profession.Special:
                return "特种";
            case Profession.Support:
                return "辅助";
            case Profession.Tank:
                return "重装";
            case Profession.Warrior:
                return "近卫";
        }
        return "Error";
    }
    public static Sprite GetProfessionSprite(Profession profession)
    {
        switch (profession)
        {
            case Profession.Caster:
                return PoolManage.Instance.GetSprite("profession/icon_profession_caster_large_white");
            case Profession.Medic:
                return PoolManage.Instance.GetSprite("profession/icon_profession_medic_large_white");
            case Profession.Pioneer:
                return PoolManage.Instance.GetSprite("profession/icon_profession_pioneer_large_white");
            case Profession.Sniper:
                return PoolManage.Instance.GetSprite("profession/icon_profession_sniper_large_white");
            case Profession.Special:
                return PoolManage.Instance.GetSprite("profession/icon_profession_special_large_white");
            case Profession.Support:
                return PoolManage.Instance.GetSprite("profession/icon_profession_support_large_white");
            case Profession.Tank:
                return PoolManage.Instance.GetSprite("profession/icon_profession_tank_large_white");
            case Profession.Warrior:
                return PoolManage.Instance.GetSprite("profession/icon_profession_warrior_large_white");
        }
        return PoolManage.Instance.GetSprite("Skill/icon_equip_non$0");
    }
    public CharacterInfo Clone()
    {
        CharacterInfo info = new CharacterInfo
        {
            ID = ID,
            Name = Name,
            Description = Description,
            Attack = Attack,
            IncreaseAttack = IncreaseAttack,
            MaxHp = MaxHp,
            IncreaseHp = IncreaseHp,
            Icon = Icon,
            Rarity = Rarity,
            _Profession = _Profession,
            StartMagic = StartMagic,
            MaxMagic = MaxMagic,
            IncreaseMagic = IncreaseMagic,
            MoveDistance = MoveDistance,
            Skills = Skills,
            Passives = Passives,
            Model = Model
        };
        return info;
    }
}