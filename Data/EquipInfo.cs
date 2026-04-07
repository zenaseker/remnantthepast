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
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int Rarity { get; set; }//1~5

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
        switch (Rarity)
        {
            default:
                return Color.white;
        }
    }
}
