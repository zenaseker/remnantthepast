using SaveLoad;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveManager
{
    // 存档文件路径
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.sav");

    /// <summary>
    /// 保存存档到文件
    /// </summary>
    public static async void Save(GameSaveData data)
    {
        // 确保版本号是最新的
        data.version = 1;
        // 将对象转为JSON（格式化输出便于阅读）
        string json = JsonUtility.ToJson(data);
        await File.WriteAllTextAsync(SavePath, json);
        Debug.Log("游戏已保存至：" + SavePath);
    }

    /// <summary>
    /// 从文件加载存档，如果文件不存在则返回新存档
    /// </summary>
    public async static Task<GameSaveData> Load()
    {
        Debug.Log("存档位置：" + SavePath);
        if (!File.Exists(SavePath))
        {
            Debug.Log("未找到存档文件，创建新存档");
            GameSaveData data1 = new GameSaveData();
            MigrateAndFixData(data1);
            return data1;
        }
        string json = await File.ReadAllTextAsync(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        // 版本迁移与数据完整性修复
        MigrateAndFixData(data);
        return data;
    }

    /// <summary>
    /// 版本迁移及数据修复（保证存档结构完整）
    /// </summary>
    private static void MigrateAndFixData(GameSaveData data)
    {
        // 如果progress为null，创建新实例
        if (data.progress == null)
            data.progress = new ProgressData();

        if (data.unlockedRoles == null)
        {
            // 确保列表不为null
            data.unlockedRoles = new List<RoleData>
            {
                new RoleData(0),
                new RoleData(1),
                new RoleData(2),
                new RoleData(3)
            };
        }
        if (data.warehouse == null)
        {
            data.warehouse = new List<ItemData>();
        }

        // 确保team不为null，并且roleIds数组长度为4
        if (data.team == null)
        {
            data.team = new TeamData();
        }
        if (data.team.roleIds == null || data.team.roleIds.Length != 4)
        {
            data.team.roleIds = new int[4];
            data.team.roleIds[0] = 1;
        }

        // 确保每个角色的装备数组长度为4，并且元素不为null（可选）
        foreach (var role in data.unlockedRoles)
        {
            if (role.equipments == null || role.equipments.Length != 4)
                role.equipments = new EquipmentData[4];
            else
            {
                // 如果数组内有null元素，可以初始化为默认装备（id=0, level=0）
                for (int i = 0; i < role.equipments.Length; i++)
                {
                    if (role.equipments[i] == null)
                        role.equipments[i] = new EquipmentData();
                }
            }
            if (role.SkillLevles == null || role.SkillLevles.Length == 0)
            {
                role.SkillLevles = new byte[4] { 0,0,0,0 };
            }
        }

        // 版本迁移示例：假设当前版本为1，如果读取到旧版本（version < 1），可以在此添加迁移逻辑
        if (data.version < 1)
        {
            // 例如旧版本没有某个字段，可以在此设置默认值
            // data.someNewField = defaultValue;
        }

        // 始终将版本更新为最新
        data.version = 1;
    }

    /// <summary>
    /// 删除存档文件
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("存档已删除");
        }
    }
}