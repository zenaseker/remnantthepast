using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 地图网格信息
/// </summary>
[System.Serializable]
public class MapInstance
{
    public string ID {  get; set; }
    // 存放所有的格子的二维数组 [行, 列]
    [JsonIgnore]
    public MapTile[,] MapGrids {get; set;}
    public List<MapTile> MapTiles { get; set;}//Json读写用，与MapGrids绑定
    // 记录地图宽高（行和列）
    public int MapRowCount { get; set; } // 行数（高度）
    public int MapColCount { get; set; } // 列数（宽度）
    public Vector2Int CenterMove  { get; set; }//起始坐标
    [JsonIgnore]
    public Vector2Int WorldOffest;//世界坐标偏移
    [JsonIgnore]
    public Vector2Int WorldToLocalOffest => WorldOffest + CenterMove;//世界坐标到本地坐标的偏移量
    public bool HasStartDoor { get; set; } = false;//启用出生点
    public Vector2Int BlueDoor { get; set; }//出生点
    public bool HasEndDoor { get; set; } = false;//启用目标点
    public Vector2Int RedDoor { get; set; }//目标点
    public float mapMinX { get; set; }//地图视像左边界-X
    public float mapMaxX { get; set; }//地图视像右边界+X
    public float mapMinY { get; set; }//地图视像下边界-Y
    public float mapMaxY { get; set; }//地图视像上边界+Y
    #region Json相关
    public static void WriteByJson(MapInstance mapInstance)
    {
        mapInstance.MapTiles = new List<MapTile>();
        for (int i = 0; i < mapInstance.MapGrids.GetLength(0); i++)
        {
            for (int j = 0; j < mapInstance.MapGrids.GetLength(1); j++)
            {
                mapInstance.MapTiles.Add(mapInstance.MapGrids[i, j]);
            }
        }
        string jsonString = JsonConvert.SerializeObject(mapInstance, Formatting.Indented);
        string filePath = Application.dataPath + $"/Resources/MapInstance/{mapInstance.ID}.json";
        File.WriteAllText(filePath, jsonString);
        Debug.Log($"地图JSON数据已储存: {filePath}");
    }
    public static MapInstance LoadByJson(string path)
    {
        MapInstance mapInstance = new MapInstance();
        TextAsset jsonFile = Resources.Load<TextAsset>($"MapInstance/{path}");
        mapInstance = JsonConvert.DeserializeObject<MapInstance>(jsonFile.text);
        mapInstance.MapGrids = new MapTile[mapInstance.MapRowCount, mapInstance.MapColCount];
        foreach (MapTile mapTile in mapInstance.MapTiles)
        {
            mapInstance.MapGrids[mapTile.Row, mapTile.Col] = mapTile;
        }
        Debug.Log($"地图JSON数据已读取: 共{mapInstance.MapTiles.Count}个网格");
        return mapInstance;
    }
    #endregion


    /// <summary>
    /// 初始化地图
    /// </summary>
    public void InitMap(int rowCount, int colCount)
    {
        MapRowCount = rowCount;
        MapColCount = colCount;
        MapGrids = new MapTile[rowCount, colCount];
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                MapGrids[i, j] = new MapTile(i, j);
            }
        }
    }

    /// <summary>
    /// 搜索格子
    /// </summary>
    public MapTile GetGrid(int row, int col)
    {
        if (IsInBounds(row, col))
        {
            return MapGrids[row, col];
        }
        return null;
    }
    public MapTile GetGrid(Vector2Int pos)
    {
        if (IsInBounds(pos))
        {
            return MapGrids[pos.x, pos.y];
        }
        return null;
    }
    /// <summary>
    /// 格子是否可以进入
    /// </summary>
    /// <param name="row">行数</param>
    /// <param name="col">列数</param>
    /// <returns></returns>
    public bool GrilCanTo(int row, int col)
    {
        if (IsInBounds(row, col))
        {
            return MapGrids[row, col].CanTo && MapGrids[row,col].UnitInGrid == null;
        }
        return false;
    }

    /// <summary>
    /// 检查坐标是否在地图范围内
    /// </summary>
    public bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < MapRowCount && col >= 0 && col < MapColCount;
    }
    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < MapRowCount && pos.y >= 0 && pos.y < MapColCount;
    }
    /// <summary>
    /// 坐标是否在地图范围中（基于世界原点）
    /// </summary>
    public bool IsInBoundsWorld(Vector2Int pos)
    {
        Vector2Int viewpos = pos - WorldToLocalOffest;
        return viewpos.x >= 0 && viewpos.x < MapRowCount && viewpos.y >= 0 && viewpos.y < MapColCount;
    }
    #region 寻路
    // 开启和关闭列表
    private List<MapTile> _openList = new List<MapTile>();
    private HashSet<MapTile> _closeSet = new HashSet<MapTile>();
    /// <summary>
    /// 查找路径
    /// </summary>
    public List<Vector2Int> FindPath(Vector2 startPos, Vector2 endPos)
    {
        // 转换坐标为格子索引（行，列）
        int startRow = Mathf.RoundToInt(startPos.x);
        int startCol = Mathf.RoundToInt(startPos.y);
        int endRow = Mathf.RoundToInt(endPos.x);
        int endCol = Mathf.RoundToInt(endPos.y);
        // 检查坐标是否在地图范围内
        if (!IsInBounds(startRow, startCol) || !IsInBounds(endRow, endCol))
        {
            Debug.Log("起点或终点不在地图内");
            return null;
        }
        if (startPos == endPos)
        {
            Debug.Log("起点和终点在同一位置");
            return null;
        }
        // 获取起点和终点格子
        MapTile startGrid = GetGrid(startRow, startCol);
        MapTile endGrid = GetGrid(endRow, endCol);
        // 清空上一次寻路数据
        _openList.Clear();
        _closeSet.Clear();
        // 起点加入开放列表
        startGrid.ParentNode = null;
        startGrid.G = 0;
        startGrid.H = CalculateH(startGrid, endGrid);
        _openList.Add(startGrid);
        while (_openList.Count > 0)
        {
            // 找到F值最小的格子
            int minIndex = 0;
            for (int i = 1; i < _openList.Count; i++)
            {
                if (_openList[i].F < _openList[minIndex].F)
                {
                    minIndex = i;
                }
            }
            MapTile currentGrid = _openList[minIndex];
            _openList.RemoveAt(minIndex);
            _closeSet.Add(currentGrid);
            // 如果到达终点，回溯路径
            if (currentGrid == endGrid)
            {
                return RetracePath(startGrid, endGrid);
            }
            CheckAndAddNeighbor(currentGrid, -1, 0, endGrid);
            CheckAndAddNeighbor(currentGrid, 1, 0, endGrid);
            CheckAndAddNeighbor(currentGrid, 0, 1, endGrid);
            CheckAndAddNeighbor(currentGrid, 0, -1, endGrid);
        }
        // 没有找到路径
        return null;
    }
    /// <summary>
    /// 检查并添加相邻格子到开放列表
    /// </summary>
    private void CheckAndAddNeighbor(MapTile currentGrid, int rowOffset, int colOffset, MapTile endGrid)
    {
        int newRow = currentGrid.Row + rowOffset;
        int newCol = currentGrid.Col + colOffset;

        // 检查是否在地图范围内
        if (!IsInBounds(newRow, newCol))
            return;

        MapTile neighbor = GetGrid(newRow, newCol);

        // 检查是否是障碍物或已在关闭列表中
        if (neighbor == null || !GrilCanTo(newRow, newCol) || _closeSet.Contains(neighbor))
            return;

        // 计算移动代价（对角线14，直线10，简化计算）
        int moveCost = 10 + neighbor.MoveToCost;
        int newG = currentGrid.G + moveCost;

        // 如果不在开放列表中，或者找到更优的路径
        if (!_openList.Contains(neighbor) || newG < neighbor.G)
        {
            neighbor.ParentNode = currentGrid;
            neighbor.G = newG;
            neighbor.H = CalculateH(neighbor, endGrid);

            if (!_openList.Contains(neighbor))
            {
                _openList.Add(neighbor);
            }
        }
    }
    /// <summary>
    /// 计算H值（曼哈顿距离）
    /// </summary>
    private int CalculateH(MapTile grid, MapTile endGrid)
    {
        int dx = Mathf.Abs(grid.Col - endGrid.Col);
        int dy = Mathf.Abs(grid.Row - endGrid.Row);
         return 10 * (dx + dy);
    }
    /// <summary>
    /// 回溯路径
    /// </summary>
    private List<Vector2Int> RetracePath(MapTile startGrid, MapTile endGrid)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        MapTile currentGrid = endGrid;

        while (currentGrid != startGrid)
        {
            path.Add(new Vector2Int(currentGrid.Row,currentGrid.Col));
            currentGrid = currentGrid.ParentNode;

            // 防止路径循环导致的死循环
            if (path.Count > MapRowCount * MapColCount)
            {
                Debug.LogError("路径出现循环，可能是寻路逻辑错误");
                return null;
            }
        }

        path.Add(new Vector2Int(startGrid.Row,startGrid.Col));
        path.Reverse();
        return path;
    }
    #endregion
}

public class MapTile
{
    public int Row { get; set; }//横坐标
    public int Col { get; set; }//纵坐标
    public bool CanTo {get; set;}//允许通行
    public int MoveToCost { get; set; }//进入此节点的代价
    public string Monster {  get; set; }//本格初始敌人
    public List<Door> Doors { get; set; }//本格门
    [JsonIgnore]
    public List<IRoundQueneObject> Monsters;//敌人范围索引
    [JsonIgnore]
    public MapManager.MapGridAction MapGridAction;
    [JsonIgnore]
    public IBlocked UnitInGrid;//在本地块的单位
    #region 寻路
    [JsonIgnore]
    public MapTile ParentNode { get; set; } // 父节点
    [JsonIgnore]
    public int G { get; set; } // 起点到当前节点的代价
    [JsonIgnore]
    public int H { get; set; } // 当前节点到终点的估计代价

    [JsonIgnore]
    public int F
    {
        get { return G + H; }
    }
    #endregion

    public MapTile(int row, int col)
    {
        Row = row;
        Col = col;
        CanTo = true;
        MoveToCost = 0;
    }

    public void SetCanTo(bool canTo)
    {
        CanTo = canTo;
        MoveToCost = CanTo ? 1000 : 0;
    }
    public void SetToCost(int cost)
    {
        MoveToCost = cost;
    }
    /// <summary>
    /// 门，地图之间的通道
    /// </summary>
    public class Door
    {
        public string TargetMapId { get; set; }//目标地图id
        public Vector2Int TargetMapDoorPos { get; set; }//目标地图目的地坐标
        public Vector2Int MapOffest {  get; set; }//两地图的世界坐标偏移
        public bool Transfer {  get; set; } = false;//是否通过传送到达目的地

    }
}
/// <summary>
/// 在地图中阻挡行动路径的单位
/// </summary>
public interface IBlocked
{

}