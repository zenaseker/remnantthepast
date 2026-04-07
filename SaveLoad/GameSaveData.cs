using System;
using System.Collections.Generic;

namespace SaveLoad
{
    // 主存档数据
    [Serializable]
    public class GameSaveData
    {
        public int version = 1;                      // 存档版本，用于后续扩展
        public string playerId;                       // 玩家ID
        public ProgressData progress;                 // 游戏进度
        public List<RoleData> unlockedRoles = new List<RoleData>(); // 已解锁角色列表
        public List<ItemData> warehouse = new List<ItemData>();     // 仓库物品列表
        public TeamData team;                          // 当前配队
    }
    // 装备数据
    [Serializable]
    public class EquipmentData
    {
        public int id = -1;       // 装备ID
        public int level = 1;    // 装备等级
    }

    // 角色数据（已解锁）
    [Serializable]
    public class RoleData
    {
        public RoleData(int id)
        {
            roleId = id;
        }
        public int roleId;           // 角色唯一ID
        public int level = 1;            // 角色等级
        public EquipmentData[] equipments = new EquipmentData[4]; // 4个装备槽
        public byte[] SkillLevles = new byte[4];//4个技能解锁进度
    }

    // 物品数据（仓库用）
    [Serializable]
    public class ItemData
    {
        public int id;               // 物品ID
        public int count;            // 物品数量
    }

    // 配队数据（4个角色槽位）
    [Serializable]
    public class TeamData
    {
        public int[] roleIds = new int[4]; // 角色ID，0表示空位
    }

    // 进度数据（主题-地图-任务）
    [Serializable]
    public class ProgressData
    {
        public int themeProgress;    // 主题进度
        public int mapProgress;      // 地图进度
        public int missionProgress;  // 任务进度
    }

}