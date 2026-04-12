using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static MapTile;

public class MapEditorWindow : EditorWindow
{
    protected MapInstance m_Instance;
    protected Vector2Int SelectTile;
    protected Vector2Int m_Scale;
    protected Vector2 scrollview;
    protected int SelectType = 0;
    protected int GrilSize = 50;
    protected MapManager m_Manager;

    [MenuItem("Jobs/编辑地图")]
    public static void ShowWindow()
    {
        MapEditorWindow window = MapEditorWindow.GetWindow<MapEditorWindow>("地图编辑", true);
        window.Open();
    }

    public void Open()
    {
        m_Instance = new MapInstance();
    }

    public void Open(string id)
    {
        m_Instance = MapInstance.LoadByJson(id);
    }
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal("box", GUILayout.MaxHeight(25));
        m_Scale.x = EditorGUILayout.IntField("地图宽度", m_Scale.x);
        m_Scale.y = EditorGUILayout.IntField("地图高度", m_Scale.y);
        if (GUILayout.Button("重设地图(此操作会重置地图中的设置!)"))
        {
            m_Instance.InitMap(m_Scale.x, m_Scale.y);
        }
        GUILayout.EndHorizontal();
        if (m_Instance.MapGrids != null)
        {
            EditorGUILayout.BeginHorizontal("box");
            DrawMainTileMap();
            if (SelectTile != null)
            {
                DrawTileInfo();
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("地图是空的!", MessageType.Warning);
        }
        m_Instance.ID = EditorGUILayout.TextField("地图ID", m_Instance.ID);
        if (GUILayout.Button("读取数据"))
        {
            if (m_Instance.ID == null)
            {
                Debug.Log("请先设置地图ID！");
            }
            else
            {
                m_Instance = MapInstance.LoadByJson(m_Instance.ID);
            }
        }
        if (GUILayout.Button("保存数据"))
        {
            if (m_Instance == null ||  m_Instance.ID == null)
            {
                Debug.Log("请先设置地图ID！");
            }
            else
            {
                MapInstance.WriteByJson(m_Instance);
                AssetDatabase.Refresh();
            }
        }
        if (GUILayout.Button("在新窗口打开"))
        {
            if (m_Instance != null && m_Instance.ID != null)
            {
                MapEditorWindow window = CreateInstance<MapEditorWindow>();
                window.name = "地图编辑：" + m_Instance.ID;
                window.Open(m_Instance.ID); 
                window.Show();
            }
        }
        GUILayout.EndVertical();
    }
    private void DrawMainTileMap()
    {
        EditorGUILayout.BeginVertical();
        GrilSize = EditorGUILayout.IntField("网格大小", GrilSize);
        scrollview = EditorGUILayout.BeginScrollView(scrollview);
        Rect gridRect = GUILayoutUtility.GetRect(m_Instance.MapRowCount * GrilSize, m_Instance.MapColCount * GrilSize);
        Event e = Event.current;
        // 处理鼠标点击事件
        if (e.isMouse && gridRect.Contains(e.mousePosition))
        {
            Vector2 mousePos = e.mousePosition - gridRect.position;
            int cellX = Mathf.FloorToInt(mousePos.x / GrilSize);
            int cellY = m_Instance.MapColCount - 1 - Mathf.FloorToInt(mousePos.y / GrilSize);
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (!m_Instance.IsInBounds(cellX, cellY)) return;
                switch (SelectType)
                {
                    case 0:
                        SelectTile = new Vector2Int(cellX, cellY);
                        break;
                    case 3:
                        m_Instance.CenterMove = -new Vector2Int(cellX, cellY);
                        break;
                }
                SelectType = 0;
            }
        }
        // 绘制单元格颜色
        for (int x = 0; x < m_Instance.MapRowCount; x++)
        {
            for (int y = 0; y < m_Instance.MapColCount; y++)
            {
                Rect cellRect = new Rect(gridRect.x + x * GrilSize, gridRect.y + (m_Instance.MapColCount - 1) * GrilSize - y * GrilSize, GrilSize, GrilSize);
                Vector2Int tile = new Vector2Int(x, y);
                MapTile mapTile = m_Instance.GetGrid(tile.x, tile.y);
                if (mapTile == null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                    GUI.color = Color.grey;
                    GUI.Label(cellRect, tile.ToString() + "\r\n是空的");
                    continue;
                }
                if (mapTile.FacilitiesType != MapTile.IFacilities.FacilitiesType.None)
                {
                    switch (mapTile.FacilitiesType)
                    {
                        case IFacilities.FacilitiesType.Monster:
                            GUI.color = Color.yellow;
                            break;
                        case IFacilities.FacilitiesType.Cross:
                            GUI.color = Color.magenta;
                            break;
                        case IFacilities.FacilitiesType.StartDoor:
                            GUI.color = Color.blue;
                            break;
                        case IFacilities.FacilitiesType.EndDoor:
                            GUI.color = Color.red;
                            break;
                    }
                }
                else
                {
                    GUI.color = mapTile.CanTo ? Color.white : Color.black;
                }
                GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = Color.grey;
                tile += m_Instance.CenterMove;
                GUI.Label(cellRect, $"{tile}\r\n{mapTile.MoveToCost}");
            }
        }
        GUI.color = Color.white;
        // 绘制网格线
        Handles.color = Color.black;
        for (int x = 0; x < m_Instance.MapRowCount; x++)
        {
            float xPos = gridRect.x + x * GrilSize;
            Handles.DrawLine(new Vector3(xPos, gridRect.y), new Vector3(xPos, gridRect.y + m_Instance.MapColCount * GrilSize));
        }
        for (int y = 0; y < m_Instance.MapColCount; y++)
        {
            float yPos = gridRect.y + y * GrilSize;
            Handles.DrawLine(new Vector3(gridRect.x, yPos), new Vector3(gridRect.x + m_Instance.MapRowCount * GrilSize, yPos));
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.LabelField("视像边界");
        EditorGUILayout.LabelField("最左X；最右X；最下Y；最上Y");
        EditorGUILayout.BeginHorizontal();
        m_Instance.mapMinX = EditorGUILayout.FloatField(m_Instance.mapMinX);
        m_Instance.mapMaxX = EditorGUILayout.FloatField(m_Instance.mapMaxX);
        m_Instance.mapMinY = EditorGUILayout.FloatField(m_Instance.mapMinY);
        m_Instance.mapMaxY = EditorGUILayout.FloatField(m_Instance.mapMaxY);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"原点坐标：{m_Instance.CenterMove}");
        if (GUILayout.Button("设置原点"))
        {
            SelectType = 3;
        }
        if (SelectType == 3)
        {
            EditorGUILayout.HelpBox("正在设置原点...", MessageType.Info);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }
    private void DrawTileInfo()
    {
        EditorGUILayout.BeginVertical("Box");
        MapTile tile = m_Instance.GetGrid(SelectTile.x, SelectTile.y);
        EditorGUILayout.LabelField($"视像坐标:{SelectTile + m_Instance.CenterMove},网格坐标:({tile.Row},{tile.Col})");
        if (tile != null)
        {
            tile.CanTo = EditorGUILayout.Toggle("本格允许通行", tile.CanTo);
            tile.MoveToCost = EditorGUILayout.IntField("本格进入代价",tile.MoveToCost);
        }
        else
        {
            EditorGUILayout.HelpBox("当前网格是空的", MessageType.Warning);
        }
        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField("地图设施");
        tile.FacilitiesType = (IFacilities.FacilitiesType)EditorGUILayout.EnumPopup("设施类型", tile.FacilitiesType);
        if (tile.Facilities != null)
        {
            tile.Facilities.DrawByEditor();
            if (GUILayout.Button("删除设施"))
            {
                tile.Facilities = null;
                tile.FacilitiesType = IFacilities.FacilitiesType.None;
            }
        }
        else if (GUILayout.Button("新建设施"))
        {
            switch (tile.FacilitiesType)
            {
                case IFacilities.FacilitiesType.Monster:
                    tile.Facilities = new MapTile.Monster();
                    break;
                case IFacilities.FacilitiesType.Cross:
                    tile.Facilities = new MapTile.Cross();
                    break;
                case IFacilities.FacilitiesType.StartDoor:
                    tile.Facilities = new MapTile.Door(true);
                    break;
                case IFacilities.FacilitiesType.EndDoor:
                    tile.Facilities = new MapTile.Door(false);
                    break;
            }
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
    }
}
