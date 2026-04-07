using DG.Tweening;
using MapControl;
using SaveLoad;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 关卡地图管理器
/// </summary>
public class MapManager : Singleton<MapManager>
{
    /// <summary>
    /// 地图格委托
    /// </summary>
    public class MapGridAction
    {
        public Action<GameObject> OnPlayerIn;//玩家进入本格时
        public Action<GameObject> OnPlayerOut;//玩家离开本格时
    }
    public float mapMinX, mapMaxX, mapMinY, mapMaxY = 0;//地图边界
    public string NowMapID;//当前地图ID
    public MapInstance MainmapInstance { get; set; }//主地图
    public Dictionary<string,MapInstance> mapInstances = new Dictionary<string, MapInstance>();//备选地图
    public GameObject BlueDoor;//蓝门Obj
    public GameObject RedDoor;//红门Obj
    public List<Vector2Int> MonsterWarningRangeObjs = new List<Vector2Int>();//全体敌人警戒范围
    protected override void OnAwake()
    {
        LoadMap(DataLibrary.Instance.MapID,Vector2Int.zero);//加载默认地图（当前为0-01）
    }
    /// <summary>
    /// 加载地图
    /// </summary>
    /// <param name="mapid">地图ID</param>
    /// <param name="worldoffest">地图世界坐标偏移</param>
    void LoadMap(string mapid,Vector2Int worldoffest)
    {
        if (mapid == null)
        {
            Debug.Log("未查询到关卡信息");
            return;
        }
        MapInstance instance = MapInstance.LoadByJson(mapid);//读取地图信息
        if (mapInstances.ContainsKey(mapid)) return;//不重复加载地图
        if (MainmapInstance == null)
        {
            MainmapInstance = instance;
            mapInstances.Add(mapid, instance);
            StartCoroutine(LoadMapModel(instance));
        }
        else
        {
            mapInstances.Add(mapid, instance);
            StartCoroutine(LoadSubMapModel(instance, new Vector3(worldoffest.x,worldoffest.y, 10)));
        }
        instance.WorldOffest = worldoffest;
        //调整地图边界
        mapMinX = Mathf.Min(worldoffest.x + instance.mapMinX, mapMinX);
        mapMaxX = Mathf.Max(worldoffest.x + instance.mapMaxX, mapMaxX);
        mapMinY = Mathf.Min(worldoffest.y + instance.mapMinY, mapMinY);
        mapMaxY = Mathf.Max(worldoffest.y + instance.mapMaxY, mapMaxY);
        for (int i = 0; i < instance.MapGrids.GetLength(0); i++)
        {
            for (int j = 0; j < instance.MapGrids.GetLength(1); j++)
            {
                if (instance.MapGrids[i, j].CanTo)
                {
                    instance.MapGrids[i, j].MapGridAction = new MapGridAction();//注册委托
                }
            }
        }
        StartCoroutine(WriteMaskTexture());
        StartCoroutine(LoadMapComponents(instance));
        StartCoroutine(LoadUnit(instance));
    }
    IEnumerator LoadMapModel(MapInstance mapInstance)//加载初始地图
    {
        GameObject map = Instantiate(Resources.Load<GameObject>("Prefab/Map/" + mapInstance.ID), transform);
        map.transform.localPosition = Vector3.zero;
        map.GetComponent<MapControlBase>()?.MapCreate();//加载地图设施
        yield return null;
    }

    IEnumerator LoadSubMapModel(MapInstance mapInstance, Vector3 trans)//加载备用地图
    {
        GameObject map = Instantiate(Resources.Load<GameObject>("Prefab/Map/" + mapInstance.ID), transform);
        map.transform.localPosition = trans;
        map.transform.DOLocalMoveZ(0, 1f);//载入动画
        map.GetComponent<MapControlBase>()?.MapCreate();
        yield return null;
    }
    IEnumerator WriteMaskTexture()
    {
        CreateMaskTexture();//更新坐标网格
        yield return null;
        UpdateRange();//清除坐标网格图像
    }
    IEnumerator LoadMapComponents(MapInstance mapInstance)//加载地图设施
    {
        yield return null;//停帧等待初始化
        if (mapInstance.HasStartDoor && mapInstance.BlueDoor != null)
        {
            Vector2Int worldpos = mapInstance.BlueDoor + mapInstance.WorldOffest + mapInstance.CenterMove;
            Vector3 pos = new Vector3(worldpos.x + 0.5f, worldpos.y, 0);
            GameObject.Instantiate(BlueDoor, transform.GetChild(0)).transform.localPosition = pos;
            yield return null;
        }
        if (mapInstance.HasEndDoor && mapInstance.RedDoor != null)
        {
            Vector2Int worldpos = mapInstance.RedDoor + mapInstance.WorldOffest + mapInstance.CenterMove;
            Vector3 pos = new Vector3(worldpos.x + 0.5f, worldpos.y, 0);
            GameObject.Instantiate(RedDoor, transform.GetChild(0)).transform.localPosition = pos;
            yield return null;
        }
    }
    IEnumerator LoadUnit(MapInstance mapInstance)//加载单位
    {
        yield return null;//停帧等待初始化
        if (mapInstance.HasStartDoor && mapInstance.BlueDoor != null)
        {
            Vector2Int worldpos = mapInstance.BlueDoor + mapInstance.WorldToLocalOffest;
            Vector3 pos2 = new Vector3(worldpos.x, worldpos.y, 0);
            for (int i = 0; i < DataLibrary.Instance.Save.team.roleIds.Length; i++)
            {
                int teamid = DataLibrary.Instance.Save.team.roleIds[i];
                if (teamid != 0)
                {
                    RoleData roledata = DataLibrary.Instance.Save.unlockedRoles.Find(x => x.roleId == teamid);
                    if (roledata == null) { Debug.Log("干员" + teamid + "的存档信息未找到"); continue; }
                    PoolManage.Instance.GetPoolGameObject("Char", "Player", pos2, Quaternion.identity)
                        .GetComponent<CharacterBase>().Init(roledata, i == 0);//只有第一个干员会显示，其余在生成时隐藏
                }
                yield return null;
            }
        }
        yield return null;//停帧等待初始化
        for (int i = 0; i < mapInstance.MapGrids.GetLength(0); i++)
        {
            for (int j = 0; j < mapInstance.MapGrids.GetLength(1); j++)
            {
                MapTile tile = mapInstance.GetGrid(i, j);
                if (tile.Monster != null && tile.Monster != "")
                {
                    Vector2Int worldpos = new Vector2Int(tile.Row,tile.Col) + mapInstance.WorldToLocalOffest;
                    Vector3 pos = new Vector3(worldpos.x, worldpos.y, 0);
                    PoolManage.Instance.GetPoolGameObject("Enemy", "Enemy", pos, Quaternion.identity)
                .GetComponent<EnemyBase>().Init(tile.Monster);//加载敌人
                }
            }
        }
        yield return null;
    }
    /// <summary>
    /// 添加格子委托
    /// </summary>
    /// <param name="vector2Int">世界坐标</param>
    public void AddGridAction(Vector2Int vector2Int, Action<GameObject> into, Action<GameObject> _out)
    {
        if(HasMapWithWorldPos(vector2Int,out MapInstance map))
        {
            vector2Int -= map.WorldToLocalOffest;
            MapTile maptile = map.GetGrid(vector2Int);
            if (maptile.CanTo)
            {
                maptile.MapGridAction.OnPlayerIn += into;
                maptile.MapGridAction.OnPlayerOut += _out;
            }
        }
    }
    /// <summary>
    /// 取消格子委托
    /// </summary>
    /// <param name="vector2Int">世界坐标</param>
    public void RemoveGridAction(Vector2Int vector2Int, Action<GameObject> into, Action<GameObject> _out)
    {
        if (HasMapWithWorldPos(vector2Int, out MapInstance map))
        {
            vector2Int -= map.WorldToLocalOffest;
            MapTile maptile = map.GetGrid(vector2Int);
            if (maptile.CanTo)
            {
                maptile.MapGridAction.OnPlayerIn -= into;
                maptile.MapGridAction.OnPlayerOut -= _out;
            }
        }
    }
    /// <summary>
    /// 添加敌人警戒范围
    /// </summary>
    /// <param name="vectors">敌人警戒范围（相对坐标）</param>
    /// <param name="monster">敌人</param>
    public void AddRange(List<Vector2Int> vectors,IRoundQueneObject monster)
    {
        Vector3 pos = monster.GetGameObject().transform.position;
        Vector2Int mappos = new Vector2Int((int)pos.x, (int)pos.y);
        if (HasMapWithWorldPos(mappos, out MapInstance map))
        {
            mappos -= map.WorldToLocalOffest;
            map.GetGrid(mappos).UnitInGrid = (monster as IBlocked);
            foreach (Vector2Int vector in vectors)
            {
                Vector2Int vectorInMap = vector - map.WorldToLocalOffest;
                if (map.IsInBounds(vectorInMap) && map.GetGrid(vectorInMap).CanTo)
                {
                    if (!MonsterWarningRangeObjs.Contains(vector))
                        MonsterWarningRangeObjs.Add(vector);
                    MapTile Grid = map.GetGrid(vectorInMap);
                    if (Grid.Monsters == null)
                        Grid.Monsters = new List<IRoundQueneObject>();
                    Grid.Monsters.Add(monster);
                }
            }
            UpdateRange("monster");
        }
    }
    /// <summary>
    /// 取消敌人警戒范围
    /// </summary>
    /// <param name="vectors">敌人警戒范围（相对坐标）</param>
    /// <param name="monster">敌人</param>
    public void RemoveRange(List<Vector2Int> vectors, IRoundQueneObject monster)
    {
        Vector3 pos = monster.GetGameObject().transform.position;
        Vector2Int mappos = new Vector2Int((int)pos.x, (int)pos.y);
        if (HasMapWithWorldPos(mappos, out MapInstance map))
        {
            mappos -= map.WorldToLocalOffest;
            map.GetGrid(mappos).UnitInGrid = null;
            foreach (Vector2Int vector in vectors)
            {
                Vector2Int vectorInMap = vector - map.WorldToLocalOffest;
                if (map.IsInBounds(vectorInMap) && map.GetGrid(vectorInMap).CanTo && MonsterWarningRangeObjs.Contains(vector))
                {
                    MapTile Grid = map.GetGrid(vectorInMap);
                    if (Grid.Monsters != null)
                        Grid.Monsters.Remove(monster);
                    if (Grid.Monsters.Count <= 0)
                        MonsterWarningRangeObjs.Remove(vector);
                }
            }
            UpdateRange("monster");
        }
    }
    /// <summary>
    /// 当玩家进入任意格子时触发
    /// </summary>
    /// <param name="_char">玩家</param>
    /// <param name="PreviousGrid">玩家上一个格子</param>
    /// <param name="NowGrid">当前格子</param>
    /// <param name="Stop">是否停止玩家移动</param>
    public void OnUnitIntoGrid(GameObject _char, Vector2Int PreviousGrid,Vector2Int NowGrid,out bool Stop)
    {
        Stop = false;
        Vector2Int LocalPreviousGrid = PreviousGrid - MainmapInstance.WorldToLocalOffest;
        Vector2Int LocalNowGrid = NowGrid - MainmapInstance.WorldToLocalOffest;
        if (MainmapInstance.IsInBounds(LocalPreviousGrid))
        {
            MainmapInstance.GetGrid(LocalPreviousGrid).UnitInGrid = null;
            MainmapInstance.GetGrid(LocalPreviousGrid).MapGridAction.OnPlayerOut?.Invoke(_char);
        }
        if (HasMapWithWorldPos(PreviousGrid, out MapInstance map1) &&
            HasMapWithWorldPos(NowGrid, out MapInstance map2)
             && map1 != map2)//如果前后格子不在同一地图，更新当前地图
        {
            Debug.Log($"正在跨越地图：{map1.ID} => {map2.ID}");
            MainmapInstance = map2;
            NowMapID = map2.ID;
            LocalNowGrid = NowGrid - MainmapInstance.WorldToLocalOffest;
        }
        if (MonsterWarningRangeObjs.Contains(NowGrid))//进入敌人警戒区域
        {
            foreach (IRoundQueneObject monster in MainmapInstance.GetGrid(LocalNowGrid).Monsters)
            {
                if (!monster.InCombat)
                {
                    Stop = true;
                    GameApp.Instance.AddCombatUnit(monster);
                    TimeDestory.Instance.Add(
                        PoolManage.Instance.GetPoolGameObject("VFX", "MonsterVigilance", ((EnemyBase)monster).Effects),
                        1.3f, 
                        TimeDestory.EndExecute.PushPool);
                }
            }
        }
        if (MainmapInstance.IsInBounds(LocalNowGrid))
        {
            MapTile grid = MainmapInstance.GetGrid(LocalNowGrid);
            grid.UnitInGrid = _char.GetComponent<IBlocked>();
            grid.MapGridAction.OnPlayerIn?.Invoke(_char);
            if (grid.Doors != null)//当前格为门，加载新地图
            {
                foreach(var door in grid.Doors)
                {
                    Vector2Int offest = MainmapInstance.WorldOffest + door.MapOffest;
                    LoadMap(door.TargetMapId, offest);
                }
            }
        }
    }


    #region 玩家可行格显示
    public MeshRenderer MeshRenderer;//网格组件
    private Material rangeMaterial;//网格材质
    private Texture2D maskTexture;//网格图像
    private int textureResolutionX = 10; //地图大小

    void CreateMaskTexture()
    {
        //更新网格大小，调整网格位置和大小
        Vector3 Position = new Vector3((mapMaxX + mapMinX) / 2 + 0.5f, (mapMaxY + mapMinY) / 2, 0);
        MeshRenderer.gameObject.transform.localPosition = Position;
        if ((mapMaxX - mapMinX) > (mapMaxY - mapMinY))//将网格修正为正方形以保证贴图比例
        {
            textureResolutionX = (int)(mapMaxX - mapMinX);
            int textureResolutionY = (int)(mapMaxY - mapMinY);
            Position.y += (textureResolutionX - textureResolutionY) / 2;
        }
        else
        {
            textureResolutionX = (int)(mapMaxY - mapMinY);
            int textureResolutionY = (int)(mapMaxX - mapMinX);
            Position.x += (textureResolutionX - textureResolutionY) / 2;
        }
        Position.x -= 0.5f;
        MeshRenderer.gameObject.transform.localScale = new Vector3(textureResolutionX, textureResolutionX, 1);
        rangeMaterial = MeshRenderer.materials[0];
        //创建网格图像
        maskTexture = new Texture2D(textureResolutionX, textureResolutionX, TextureFormat.ARGB32, false);
        maskTexture.filterMode = FilterMode.Point;
        maskTexture.wrapMode = TextureWrapMode.Repeat;
        ClearTexture();
        if (rangeMaterial != null) 
        {
            rangeMaterial.SetColor("_RangeColor1", ColorLibrary.FindColor(ColorLibrary.ColorEnum.PlayerRange));
            rangeMaterial.SetColor("_RangeColor2", ColorLibrary.FindColor(ColorLibrary.ColorEnum.PlayerMoveWarningRange));
            rangeMaterial.SetColor("_RangeColor3", ColorLibrary.FindColor(ColorLibrary.ColorEnum.MonsterWarningRange));
            rangeMaterial.SetColor("_RangeColor4", Color.red);
        }
        rangeMaterial.SetTexture("_RangeMask", maskTexture);
        rangeMaterial.SetFloat("_GridSize", textureResolutionX);
        rangeMaterial.SetVector("_GridOrigin", new Vector4(Position.x,Position.y));
    }
    /// <summary>
    /// 清空网格图像
    /// </summary>
    void ClearTexture()
    {
        Color32[] clearPixels = new Color32[textureResolutionX * textureResolutionX];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = new Color32(0, 0, 0, 255);
        maskTexture.SetPixels32(clearPixels);
    }
    public HashSet<Vector2Int> reachableCells;//玩家移动范围
    public HashSet<Vector2Int> skillrangeCells;//干员技能范围
    /// <summary>
    /// 更新网格显示
    /// </summary>
    /// <param name="show">"player":玩家移动范围;"monster":敌人警戒范围;"skill":干员技能范围</param>
    public void UpdateRange(params string[] show)
    {
        ClearTexture();
        //将世界坐标转换到像素坐标
        Vector2Int textureCenter = -new Vector2Int(Mathf.FloorToInt(mapMinX + 0.5f), Mathf.FloorToInt(mapMinY + 0.5f));
        reachableCells = null;
        foreach (string s in show)
        {
            switch (s)
            {
                case "player":
                    //获取玩家移动范围
                    reachableCells = GameApp.Instance._nowSelect.GetComponent<PlayerControl>().CalculateReachableCells();
                    foreach (Vector2Int cell in reachableCells)
                    {
                        Vector2Int pixelPos = textureCenter + cell;
                        if (IsInTextureBounds(pixelPos))
                        {
                            Color color = maskTexture.GetPixel(pixelPos.x, pixelPos.y);
                            color.b = 1;
                            maskTexture.SetPixel(pixelPos.x, pixelPos.y, color);
                        }
                    }
                    break;
                case "monster":
                    foreach (Vector2Int cell in MonsterWarningRangeObjs)
                    {
                        Vector2Int pixelPos = textureCenter + cell;
                        if (IsInTextureBounds(pixelPos))
                        {
                            Color color = maskTexture.GetPixel(pixelPos.x, pixelPos.y);
                            color.r = 1;
                            maskTexture.SetPixel(pixelPos.x, pixelPos.y, color);
                        }
                    }
                    break;
                case "skill":
                    foreach (Vector2Int cell in skillrangeCells)
                    {
                        Vector2Int pixelPos = textureCenter + cell;
                        if (IsInTextureBounds(pixelPos))
                        {
                            Color color = maskTexture.GetPixel(pixelPos.x, pixelPos.y);
                            color.g = 1;
                            maskTexture.SetPixel(pixelPos.x, pixelPos.y, color);
                        }
                    }
                    break;
            }
        }
        maskTexture.Apply();
        rangeMaterial.SetTexture("_RangeMask", maskTexture);
    }
    /// <summary>
    /// 显示单个敌人的警戒范围
    /// </summary>
    /// <param name="enemy">敌人</param>
    public void UpdateOneEnemyRange(IRoundQueneObject enemy)
    {
        ClearTexture();
        Vector2Int textureCenter = -new Vector2Int(Mathf.FloorToInt(mapMinX + 0.5f), Mathf.FloorToInt(mapMinY + 0.5f));
        foreach (Vector2Int cell in MonsterWarningRangeObjs)
        {
            MapTile tile = MainmapInstance.GetGrid(cell - MainmapInstance.CenterMove);
            if (tile != null && tile.Monsters.Contains(enemy))
            {
                Vector2Int pixelPos = textureCenter + cell;
                if (IsInTextureBounds(pixelPos))
                {
                    Color color = maskTexture.GetPixel(pixelPos.x, pixelPos.y);
                    color.r = 1;
                    maskTexture.SetPixel(pixelPos.x, pixelPos.y, color);
                }
            }
        }
        maskTexture.Apply();
        rangeMaterial.SetTexture("_RangeMask", maskTexture);
    }
    /// <summary>
    /// 坐标在图像内
    /// </summary>
    /// <param name="pos">像素坐标</param>
    /// <returns></returns>
    bool IsInTextureBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < textureResolutionX && pos.y >= 0 && pos.y < textureResolutionX;
    }
    #endregion



    /// <summary>
    /// 寻路（MapInstance中的寻路输入和返回的是内部坐标，需要将其转换成世界坐标）
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
    {
        if (HasMapWithWorldPos(new Vector2Int((int)startPos.x,(int)startPos.y),out MapInstance map1) &&
            HasMapWithWorldPos(new Vector2Int((int)endPos.x, (int)endPos.y), out MapInstance map2)
             && map1 != map2)//不在同一地图
        {
            Debug.Log("起点与终点位于两个地图");
            MapTile tile = map1.GetGrid(startPos - map1.WorldToLocalOffest);
            MapTile tile2 = map2.GetGrid(endPos - map2.WorldToLocalOffest);
            if (tile.Doors?.Count > 0)//起始点是门
            {
                MapTile.Door door = tile.Doors.Find(x => x.TargetMapId == map2.ID);
                if (door != null && door.TargetMapDoorPos == new Vector2Int(tile2.Row,tile2.Col))//终点在门列表中
                {
                    List<Vector2Int> path2 = new List<Vector2Int>
                    {
                        startPos,
                        endPos
                    };
                    return path2;

                }
            }
            Debug.Log("两地图之间仅能通过门进出");
            return null;
        }
        //获取寻路路径
        List<Vector2Int> path = MainmapInstance.FindPath(startPos - MainmapInstance.WorldToLocalOffest, endPos - MainmapInstance.WorldToLocalOffest);
        if (path == null) return null;
        for (int i = 0;i < path.Count;i++)
        {
            path[i] += MainmapInstance.WorldToLocalOffest;
        }
        return path;
    }
    /// <summary>
    /// 获取当前地图的格子
    /// </summary>
    /// <param name="pos">世界坐标</param>
    /// <returns></returns>
    public MapTile GetTile(Vector2Int pos)
    {
        return MainmapInstance.GetGrid(pos - MainmapInstance.WorldToLocalOffest);
    }
    /// <summary>
    /// 地块是否可以进入
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public bool GrilCanTo(Vector2Int targetPos)
    {
        return MainmapInstance.GrilCanTo(targetPos.x - MainmapInstance.WorldToLocalOffest.x,targetPos.y - MainmapInstance.WorldToLocalOffest.y);
    }
    /// <summary>
    /// 地块是否在某一地图内
    /// </summary>
    /// <param name="worldpos">世界坐标</param>
    /// <param name="map">所在地图</param>
    /// <returns></returns>
    public bool HasMapWithWorldPos(Vector2Int worldpos,out MapInstance map)
    {
        foreach(MapInstance instance in mapInstances.Values)
        {
            if (instance.IsInBoundsWorld(worldpos))
            {
                map = instance; 
                return true;
            }
        }
        map = null;
        return false;
    }
    //绘制地图边界
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // 绘制地图边界
        if (!this.enabled) return;
        Gizmos.color = Color.green;
        Vector3 bottomLeft = new Vector3(mapMinX, mapMinY, 0);
        Vector3 bottomRight = new Vector3(mapMaxX, mapMinY, 0);
        Vector3 topRight = new Vector3(mapMaxX, mapMaxY, 0);
        Vector3 topLeft = new Vector3(mapMinX, mapMaxY, 0);

        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
    }
#endif

}
