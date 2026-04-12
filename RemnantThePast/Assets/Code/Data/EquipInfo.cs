using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装备信息
/// </summary>
[System.Serializable]
public class EquipInfo
{
    public int ID {  get; set; }
    public string Icon { get; set; }
    public int MinRarity { get; set; }//0~4
    public int MaxRarity { get; set; }
    public string EquipData { get; set; }
    public RartityInfo[] RartityInfos { get; set; }
    public EquipInfo()
    {
        MinRarity = 0;
        MaxRarity = 4;
        RartityInfos = new RartityInfo[5]
        {
            new RartityInfo(),
            new RartityInfo(),
            new RartityInfo(),
            new RartityInfo(),
            new RartityInfo()
        };
    }
    public class RartityInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    [JsonIgnore]
    Sprite _sprite;
    [JsonIgnore]
    public Sprite sprite
    {
        get
        {
            if (_sprite == null)
            {
                _sprite = PoolManage.Instance.GetSprite("Equip/" + Icon);
            }
            return _sprite;
        }
    }//技能图标
    public void ReSetSprite(string sprite)
    {
        Icon = sprite;
        _sprite = Resources.Load<Sprite>("icon/Equip/" + Icon);
    }
    public Color GetLight()
    {
        switch (MinRarity)
        {
            default:
                return Color.white;
        }
    }
    public static string GetRarityName(int rarity)
    {
        switch (rarity)
        {
            case 0:
                return "旧物";
            case 1:
                return "遗痕";
            case 2:
                return "典藏";
            case 3:
                return "绝章";
            case 4:
                return "转捩";
            default:
                return "未知";
        }
    }
}
