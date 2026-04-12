using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using SelectControl;
using System.Reflection;
using System;
using SkillComponent;
using UnitBuf;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using SaveLoad;
using UnityEngine.SceneManagement;
using EnemyActionTree;
using Equip;

/// <summary>
/// 数据库
/// </summary>
public class DataLibrary
{
    static DataLibrary _instance;
    public static DataLibrary Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DataLibrary();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
#if UNITY_EDITOR
    public bool Informationed = false;
#endif
    #region 游戏数据
    public GameSaveData Save { get; set; }
    public string MapID { get; set; }
    #endregion


    #region 数据库
    public Dictionary<int,CharacterInfo> characterInfos;
    public Dictionary<int,EquipInfo> equipinfos;
    public Dictionary<string, Type> equipdatas;
    public Dictionary<string,SkillInfo> skillInfos;
    public Dictionary<string, SkillRange> skillranges;
    public Dictionary<string, MonsterInfo> monsterInfos;
    public Dictionary<string, Type> selectcontrols;
    public Dictionary<string, Type> skillevents;
    public Dictionary<string, Type> battleunitbufs;
    public Dictionary<string, BattleUnitBufInfo> bufinfos;
    public Dictionary<string, PassiveInfo> passiveinfos;
    public Dictionary<string, Type> enemyactions;
    #endregion

    [MenuItem("Jobs/Load Library By Editor")]
    public static void LoadLibrary()
    {
        Instance.LoadingInformation();
        Instance.GetDeriveds();
        Instance.LoadGame();
        SceneManager.sceneUnloaded += MathHelper.ClearCache;
#if UNITY_EDITOR
        Instance.Informationed = true;
#endif
    }
    //异步加载Json数据
    public static async Task<IEnumerable<T>> LoadResouceInfo<T>(string path)
    {
        TextAsset jsonFile = await MathHelper.LoadResourcesAsync<TextAsset>(path);
        Debug.Log(typeof(T).Name + "is Load");
        return JsonConvert.DeserializeObject<List<T>>(jsonFile.text);
    }
    public async void LoadingInformation()
    {
        //载入角色信息
        characterInfos = (await LoadResouceInfo<CharacterInfo>("Data/Characters")).ToDictionary(s => s.ID);
        //载入技能信息
        skillInfos = (await LoadResouceInfo<SkillInfo>("Data/Skills")).ToDictionary(s => s.ID);
        //载入敌人信息
        monsterInfos = (await LoadResouceInfo<MonsterInfo>("Data/Monsters")).ToDictionary(s => s.ID);
        //载入敌人信息技能范围
        skillranges = (await LoadResouceInfo<SkillRange>("Data/SkillRange")).ToDictionary(s => s.ID);
        //载入Buf信息
        bufinfos = (await LoadResouceInfo<BattleUnitBufInfo>("Data/BufInfo")).ToDictionary(s => s.ID);
        //载入被动信息
        passiveinfos = (await LoadResouceInfo<PassiveInfo>("Data/PassiveInfo")).ToDictionary(s => s.ID);
        //载入遗珍信息
        equipinfos = (await LoadResouceInfo<EquipInfo>("Data/EquipInfo")).ToDictionary(s => s.ID);
    }
    void GetDeriveds()
    {
        //载入输入控制
        selectcontrols = GetDerivedTypesInNamespace<ObjectSelectControl>("SelectControl").ToDictionary(x =>x.Name);
        //载入技能效果
        skillevents = GetDerivedTypesInNamespace<SkillEvent>("SkillComponent").ToDictionary(x => x.Name);
        //载入Buf
        battleunitbufs = new Dictionary<string, Type>();
        List<Type> UnitBufs = GetDerivedTypesInNamespace<BattleUnitBuf>("UnitBuf");
        foreach (Type type in UnitBufs)
        {
            string Name = type.Name.Replace("BattleUnitBuf_", "");
            if (!battleunitbufs.ContainsKey(Name))
            {
                battleunitbufs.Add(Name, type);
            }
        }
        //载入敌人意图树
        enemyactions = GetDerivedTypesInNamespace<EnemyActionTreeBase>("EnemyActionTree").ToDictionary(x => x.Name);

        //载入装备代码
        equipdatas = GetDerivedTypesInNamespace<EquipBase>("Equip").ToDictionary(x => x.Name);
    }
    async void LoadGame()
    {
        Save = await SaveManager.Load();
    }
    public void SaveGame()
    {
        SaveManager.Save(Save);
    }
    public ObjectSelectControl GetSelectControl(string name)
    {
        if (selectcontrols.TryGetValue(name,out Type type))
        {
            try
            {
                // 创建实例并转换为基类类型
                var instance = Activator.CreateInstance(type) as ObjectSelectControl;
                if (instance != null)
                {
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建类型 {type.Name} 的实例时出错：{ex.Message}");
            }
        }
        return null;
    }
    public SkillEvent GetSkillEvent(string name)
    {
        if (skillevents.TryGetValue(name, out Type type))
        {
            try
            {
                // 创建实例并转换为基类类型
                var instance = Activator.CreateInstance(type) as SkillEvent;
                if (instance != null)
                {
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建类型 {type.Name} 的实例时出错：{ex.Message}");
            }
        }
        return null;
    }
    public BattleUnitBuf GetBuf(string id)
    {
        if (battleunitbufs.TryGetValue(id, out Type type))
        {
            try
            {
                // 创建实例并转换为基类类型
                var instance = Activator.CreateInstance(type) as BattleUnitBuf;
                if (instance != null)
                {
                    instance.ID = id;
                    if (bufinfos.TryGetValue(id,out BattleUnitBufInfo info))
                    {
                        instance.bufInfo = info;
                    }
                    else
                    {
                        instance.bufInfo = BattleUnitBufInfo.NullBuf;
                    }
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建类型 {type.Name} 的实例时出错：{ex.Message}");
            }
        }
        return null;
    }
    public EquipBase GetEquip(EquipInfo info)
    {
        if (equipdatas.TryGetValue("Equip_" + info.EquipData, out Type type))
        {
            try
            {
                // 创建实例并转换为基类类型
                var instance = Activator.CreateInstance(type) as EquipBase;
                if (instance != null)
                {
                    instance.EquipInfo = info;
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建类型 {type.Name} 的实例时出错：{ex.Message}");
            }
        }
        return null;
    }
    public EnemyActionTreeBase GetEnemyActionTree(string name)
    {
        if (enemyactions.TryGetValue(name, out Type type))
        {
            try
            {
                // 创建实例并转换为基类类型
                var instance = Activator.CreateInstance(type) as EnemyActionTreeBase;
                if (instance != null)
                {
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"创建类型 {type.Name} 的实例时出错：{ex.Message}");
            }
        }
        return null;
    }
    /// <summary>
    /// 获取命名空间所有类
    /// </summary>
    /// <typeparam name="T">类</typeparam>
    /// <param name="targetNamespace">命名空间名</param>
    /// <returns></returns>
    public static List<Type> GetDerivedTypesInNamespace<T>(string targetNamespace)
    {
        Type baseType = typeof(T);
        return Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.Namespace == targetNamespace          // 限定命名空间
                        && t.IsClass && !t.IsAbstract          // 非抽象类
                        && baseType.IsAssignableFrom(t))       // 是子类
            .ToList();
    }
    void SaveData()
    {
        SaveData(characterInfos.Values.ToList(), Application.dataPath + "/Resources/Data/Characters.json", "角色");
        SaveData(skillInfos.Values.ToList(), Application.dataPath + "/Resources/Data/Skills.json", "技能");
        SaveData(monsterInfos.Values.ToList(), Application.dataPath + "/Resources/Data/Monsters.json", "敌人");
        SaveData(skillranges.Values.ToList(), Application.dataPath + "/Resources/Data/SkillRange.json", "技能范围");
        SaveData(bufinfos.Values.ToList(), Application.dataPath + "/Resources/Data/BufInfo.json", "Buf信息");
    }
    public static async void SaveData(object save,string path,string log)
    {
        string jsonString = JsonConvert.SerializeObject(save, Formatting.Indented);
        await File.WriteAllTextAsync(path, jsonString);
        Debug.Log($"{log}JSON数据已储存: {path}");
    }
    [MenuItem("Jobs/创建信息副本")]
    public static void CreateReplica()
    {
        string sourcePath = Path.Combine(Application.dataPath, "Resources","Data");
        List<string> replicas = new List<string>()
        {
            "Characters",
            "Skills",
            "Monsters",
            "SkillRange",
            "BufInfo",
            "EquipInfo",
            "PassiveInfo"
        };
        for (int i = 0;i < replicas.Count; i++)
        {
            string sourceFile = Path.Combine(sourcePath, replicas[i] +".json");
            string destFile = Path.Combine(sourcePath, "Replica", replicas[i] + " replica.json");
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
            File.Copy(sourceFile, destFile);
        }
    }
}

/// <summary>
/// 拓展
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// 从列表中随机返回一个元素。若列表为空或为 null，则返回 default(T)。
    /// </summary>
    public static T GetRandom<T>(this IEnumerable<T> list)
    {
        if (list == null)return default(T);
        T[] array = list as T[] ?? list.ToArray();
        if (array.Length == 0)return default(T);
        int index = UnityEngine.Random.Range(0, array.Length); // Unity 随机范围 [0, array.Length)
        return array[index];
    }

    private static readonly System.Random rng = new System.Random();

    /// <summary>
    /// 打乱列表
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    /// <summary>
    /// 获取T所在MonoBehaviour的GameObject
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static GameObject GetGameObject<T>(this T obj)
    {
        if (obj is MonoBehaviour mb) return mb.gameObject;
        else throw new InvalidOperationException(typeof(T).Name + " 不是 MonoBehaviour，无法获取 GameObject");
    }
    #region Resource异步加载
    public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest request) => new ResourceRequestAwaiter(request);
    public static async Task<T> LoadResourcesAsync<T>(string path) where T : UnityEngine.Object
    {
        var gres = Resources.LoadAsync(path);
        await gres;
        return gres.asset as T;
    }
    public class ResourceRequestAwaiter : INotifyCompletion
    {
        public Action Continuation;
        public ResourceRequest resourceRequest;
        public bool IsCompleted => resourceRequest.isDone;
        public ResourceRequestAwaiter(ResourceRequest resourceRequest)
        {
            this.resourceRequest = resourceRequest;
            this.resourceRequest.completed += Accomplish;
        }
        public void OnCompleted(Action continuation) => this.Continuation = continuation;
        public void Accomplish(AsyncOperation asyncOperation) => Continuation?.Invoke();
        public void GetResult() { }
    }
    #endregion

    /// <summary>
    /// 根据技能方向旋转攻击范围（世界位置）
    /// </summary>
    /// <param name="worldRange">初始朝向右侧的范围集合</param>
    /// <param name="center">原点位置</param>
    /// <param name="direction">技能方向（必须是上下左右单位向量）</param>
    /// <returns>旋转后的范围集合</returns>
    public static HashSet<Vector2Int> RotateWorldRange(HashSet<Vector2Int> worldRange, Vector2Int center, Vector2Int direction)
    {
        var rotatedWorld = new HashSet<Vector2Int>();
        var relative = new HashSet<Vector2Int>();
        foreach (var p in worldRange)
        {
            relative.Add(p - center);
        }
        var rotatedRelative = RotateRange(relative, direction);
        foreach (var r in rotatedRelative)
        {
            rotatedWorld.Add(center + r);
        }
        return rotatedWorld;
    }
    /// <summary>
    /// 根据技能方向旋转攻击范围（相对位置）
    /// </summary>
    /// <param name="originalRange">初始朝向右侧的范围集合</param>
    /// <param name="direction">技能方向（必须是上下左右单位向量）</param>
    /// <returns>旋转后的范围集合</returns>
    public static HashSet<Vector2Int> RotateRange(HashSet<Vector2Int> originalRange, Vector2Int direction)
    {
        var rotated = new HashSet<Vector2Int>();
        if (direction == Vector2Int.zero)
        {
            rotated.Add(Vector2Int.zero);
            return rotated;
        }
        foreach (var pos in originalRange)
        {
            int x = pos.x, y = pos.y;
            Vector2Int newPos;
            if (direction == Vector2Int.right) newPos = pos;
            else if (direction == Vector2Int.up) newPos = new Vector2Int(-y, x);
            else if (direction == Vector2Int.left) newPos = new Vector2Int(-x, -y);
            else if (direction == Vector2Int.down) newPos = new Vector2Int(y, -x);
            else newPos = pos; // 默认不变，或抛出异常
            rotated.Add(newPos);
        }
        return rotated;
    }
    /// <summary>
    /// 清除缓存
    /// </summary>
    /// <param name="scene"></param>
    public static void ClearCache(Scene scene)
    {
        GC.Collect();
        Resources.UnloadUnusedAssets(); 
        PoolManage.ClearAll();
        Debug.Log($"场景 {scene.name} 已卸载，资源已清理");
    }
}