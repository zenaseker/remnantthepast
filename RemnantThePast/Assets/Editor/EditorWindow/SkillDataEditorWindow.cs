using SkillComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class SkillDataEditorWindow : EditorWindow
{
    public List<SkillInfo> SkillInfos = new List<SkillInfo>();
    public List<SkillRange> SkillRanges = new List<SkillRange>();
    protected Vector2 scrollview;
    protected Vector2 windowscrollview;
    protected Vector2 levelscrollview;
    protected SkillInfo select;
    protected SkillRange selectrange;
    protected string ScreenAttribution;//归属筛选
    protected bool writerange = false;
    protected string newCSname;//新文件名
    protected static bool CreateCS = false;//同步创建文件

    [MenuItem("Jobs/技能信息")]
    public static void ShowWindow()
    {
        Open();
    }
    public static void Open()
    {
        SkillDataEditorWindow window = SkillDataEditorWindow.GetWindow<SkillDataEditorWindow>("技能信息");
        window.InOpen();
    }

    public static void OpenWithSkill(string skill)
    {
        SkillDataEditorWindow window = SkillDataEditorWindow.GetWindow<SkillDataEditorWindow>("技能信息");
        window.InOpen(skill);
    }
    public async void InOpen(string selectskill = null)
    {
        SkillInfos = (await DataLibrary.LoadResouceInfo<SkillInfo>("Data/Skills")).ToList();
        SkillRanges = (await DataLibrary.LoadResouceInfo<SkillRange>("Data/SkillRange")).ToList();
        foreach (SkillInfo skill in SkillInfos)
        {
            skill.ScreenAttribution = true;
        }
        if (selectskill != null)
        {
            select = SkillInfos.Find(x => x.ID == selectskill);
        }
    }
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        DrawScreenAttribution();
        if (!writerange)
        {
            DrawSkill();
        }
        else
        {
            DrawSkillRange();
        }
        EditorGUILayout.EndVertical();
    }
    #region 技能
    private void DrawSkill()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(200), GUILayout.ExpandHeight(true));
        scrollview = EditorGUILayout.BeginScrollView(scrollview, GUILayout.MaxWidth(200));
        DrawSkillList();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        windowscrollview = EditorGUILayout.BeginScrollView(windowscrollview);
        DrawCharData();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    protected void DrawScreenAttribution()
    {
        EditorGUILayout.BeginHorizontal();
        ScreenAttribution = EditorGUILayout.TextField("归属筛选", ScreenAttribution);
        if (GUILayout.Button("搜索"))
        {
            foreach (SkillInfo skill in SkillInfos)
            {
                skill.ScreenAttribution = skill.Attribution == ScreenAttribution;
            }
        }
        if (GUILayout.Button("取消"))
        {
            ScreenAttribution = null;
            foreach (SkillInfo skill in SkillInfos)
            {
                skill.ScreenAttribution = true;
            }
        }
        if (GUILayout.Button("编辑技能"))
        {
            writerange = false;
        }
        if (GUILayout.Button("编辑技能范围"))
        {
            writerange = true;
        }
        EditorGUILayout.EndHorizontal();
    }
    private void DrawSkillList()
    {
        CreateCS = EditorGUILayout.Toggle(new GUIContent("同步创建文件", "在创建技能信息时会同步创建SkillEvent类"), CreateCS);
        foreach (SkillInfo SkillInfo in SkillInfos)
        {
            if (SkillInfo.ScreenAttribution && GUILayout.Button(SkillInfo.ID))
            {
                select = SkillInfo;
                GUI.FocusControl(null);
                EditorGUI.FocusTextInControl(null);
                Repaint();
            }
        }
        if (CreateCS)
        {
            newCSname = EditorGUILayout.TextField("新技能名", newCSname);
            if (newCSname == null || newCSname == "")
            {
                EditorGUILayout.HelpBox("技能名不能为空！", MessageType.Warning);
            }
            else if (GUILayout.Button("新建"))
            {
                SkillInfo newskill = new SkillInfo();
                newskill.ID = newCSname;
                if (ScreenAttribution != null && ScreenAttribution != "")
                {
                    newskill.Attribution = ScreenAttribution;
                }
                select = newskill;
                SkillInfos.Add(newskill);
                CopyCS(newCSname);
            }
        }
        else
        {
            if (GUILayout.Button("新建"))
            {
                SkillInfo newskill = new SkillInfo();
                newskill.ID = "NoNameSkill_" + SkillInfos.Count;
                if (ScreenAttribution != null && ScreenAttribution != "")
                {
                    newskill.Attribution = ScreenAttribution;
                }
                select = newskill;
                SkillInfos.Add(newskill);
            }
        }
    }

    readonly string fileName = "C# Script-NewSkillEvent.cs.txt";
    readonly string sourcePath = Path.Combine(Application.dataPath, "Editor");
    readonly string targetPath = Path.Combine(Application.dataPath, "Code", "Skill", "SkillEvent");
    void CopyCS(string name)
    {
        string sourceFile = Path.Combine(sourcePath, fileName);
        string destFile = Path.Combine(targetPath, $"SkillEvent_{name}.cs");
        string content = File.ReadAllText(sourceFile, Encoding.UTF8);
        string className = $"SkillEvent_{name}";
        content = content.Replace("#SCRIPTNAME#", className);
        File.WriteAllText(destFile, content, Encoding.UTF8);
    }
    private void DrawCharData()
    {
        if (select == null)
        {
            EditorGUILayout.LabelField("请选择技能");
            return;
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
        if (select.Image == null || select.Image == "")
        {
            EditorGUILayout.HelpBox("未选择图标", MessageType.Warning);
        }
        else if (select.sprite == null)
        {
            EditorGUILayout.HelpBox("图标读取错误", MessageType.Warning);
        }
        else
        {
            GUILayout.Box(select.sprite.texture, GUILayout.Width(70), GUILayout.Height(70));
        }
        if (GUILayout.Button("选择技能图标"))
        {
            string path = EditorUtility.OpenFilePanel("选择技能图标", Application.dataPath + "/Resources/icon/Skill", "png");
            if (path != null)
            {
                path = Path.GetFileNameWithoutExtension(path);
                if (path != null || path != "")
                {
                    select.ReSetSprite(path);
                }
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        select.Attribution = EditorGUILayout.TextField("技能归属", select.Attribution);
        if (select.Attribution == null || select.Attribution == "")
        {
            EditorGUILayout.HelpBox("未设置技能归属", MessageType.Info);
        }
        select.ID = EditorGUILayout.TextField("ID", select.ID);
        select.Name = EditorGUILayout.TextField("名称", select.Name);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        select.Rarity = EditorGUILayout.IntField(new GUIContent("等级", "0级为普通攻击;1~3级为对应等级技能;4级为强化普通攻击;5~7级为对应1~3级被动"), select.Rarity);
        select.Type = (SkillInfo.SkillType)EditorGUILayout.EnumPopup("技能类型", select.Type);
        select.targetType = (SkillInfo.TargetType)EditorGUILayout.EnumPopup("选择目标", select.targetType);
        select.IsDerivedSkill = EditorGUILayout.Toggle("是衍生技能", select.IsDerivedSkill);
        if (GUILayout.Button("创建技能代码"))
        {
            CopyCS(select.ID);
        }
        DrawSkillLevelNumber();
        if (GUILayout.Button("删除此技能"))
        {
            SkillInfos.Remove(select);
            select = null;
        }
    }
    bool lookdes = false;
    void DrawSkillLevelNumber()
    {
        levelscrollview = EditorGUILayout.BeginScrollView(levelscrollview);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(new GUIContent("技能等级","等级的0~5级对应原数值的1,3,5,7,rank3"));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("编辑数据"))
        {
            lookdes = false;
        }
        if (GUILayout.Button("编辑文本"))
        {
            lookdes = true;
        }
        if (GUILayout.Button("重置信息"))
        {
            select.skillLevelNumbers = new SkillInfo.SkillLevelNumber[5];
            SkillInfo.SkillLevelNumber number = new SkillInfo.SkillLevelNumber() { Number = new float[1] };
            for (int i = 0; i < select.skillLevelNumbers.Length; i++)
            {
                select.skillLevelNumbers[i] = number;
            }
        }
        EditorGUILayout.EndHorizontal();
        if (select.skillLevelNumbers == null)
        {
            select.skillLevelNumbers = new SkillInfo.SkillLevelNumber[5];
        }
        if (lookdes)
        {
            DrawSkillLevelNumber_Description();
        }
        else
        {
            DrawSkillLevelNumber_Numbers();
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
    void DrawSkillLevelNumber_Numbers()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("花费Sp", GUILayout.Width(75));
        EditorGUILayout.LabelField("花费时间", GUILayout.Width(75));
        EditorGUILayout.LabelField("持续时间", GUILayout.Width(75));
        EditorGUILayout.LabelField("充能上限", GUILayout.Width(75));
        EditorGUILayout.LabelField("技能范围", GUILayout.Width(75));
        int number = EditorGUILayout.IntField("其他数值", select.skillLevelNumbers[0].Number.Length);
        if (number != select.skillLevelNumbers[0].Number.Length)
        {
            for (int i = 0; i < select.skillLevelNumbers.Length; i++)
            {
                select.skillLevelNumbers[i].Number = new float[number];
            }
        }
        EditorGUILayout.EndHorizontal();
        for (int i = 0; i < select.skillLevelNumbers.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            select.skillLevelNumbers[i].needSp = EditorGUILayout.IntField(select.skillLevelNumbers[i].needSp, GUILayout.Width(75));
            select.skillLevelNumbers[i].UseTime = EditorGUILayout.FloatField(select.skillLevelNumbers[i].UseTime, GUILayout.Width(75));
            select.skillLevelNumbers[i].Duraction = EditorGUILayout.FloatField(select.skillLevelNumbers[i].Duraction, GUILayout.Width(75));
            select.skillLevelNumbers[i].Chargingcount = EditorGUILayout.IntField(select.skillLevelNumbers[i].Chargingcount, GUILayout.Width(75));
            select.skillLevelNumbers[i].RangeID = EditorGUILayout.TextField(select.skillLevelNumbers[i].RangeID, GUILayout.Width(75));
            for (int j = 0; j < select.skillLevelNumbers[i].Number.Length; j++)
            {
                select.skillLevelNumbers[i].Number[j] = EditorGUILayout.FloatField(select.skillLevelNumbers[i].Number[j], GUILayout.Width(75));
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    void DrawSkillLevelNumber_Description()
    {
        for (int i = 0; i < select.skillLevelNumbers.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Level_" + i, GUILayout.Width(50));
            select.skillLevelNumbers[i].Description = EditorGUILayout.TextArea(select.skillLevelNumbers[i].Description,GUILayout.Height(50));
            EditorGUILayout.EndHorizontal();
        }
    }
    #endregion
    #region 技能范围
    private void DrawSkillRange()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
        scrollview = EditorGUILayout.BeginScrollView(scrollview, GUILayout.MaxWidth(150));
        DrawSkillRangeList();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        DrawMainTileMap();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    private void DrawSkillRangeList()
    {
        foreach (SkillRange SkillRange in SkillRanges)
        {
            if (GUILayout.Button(SkillRange.ID))
            {
                selectrange = SkillRange;
            }
        }
        if (GUILayout.Button("新建"))
        {
            SkillRange character = new SkillRange();
            character.ID = "NoNameSkillRange_" + SkillRanges.Count;
            selectrange = character;
            SkillRanges.Add(character);
        }
    }
    readonly byte gridsize = 50;
    private void DrawMainTileMap()
    {
        if (selectrange == null)
        {
            EditorGUILayout.HelpBox("请选择", MessageType.Info);
            return;
        }
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        selectrange.ID = EditorGUILayout.TextField("范围ID", selectrange.ID);
        selectrange.MaxSize = EditorGUILayout.IntField("图格边界", selectrange.MaxSize);
        if (GUILayout.Button("新建范围"))
        {
            selectrange.GridRange = new List<Vector2Int>(selectrange.MaxSize * selectrange.MaxSize);
            int center = (selectrange.MaxSize - 1) / 2;
            selectrange.Center = new Vector2Int(center, center);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.HelpBox("注意：图格边界必须为单数，为负边界至正边界的长度", MessageType.Info);
        EditorGUILayout.HelpBox("注意：显示与实际上下颠倒", MessageType.Warning);
        if (selectrange.GridRange == null)
        {
            EditorGUILayout.HelpBox("点击'新建范围'创建图格", MessageType.Error);
            if (GUILayout.Button("删除"))
            {
                SkillRanges.Remove(selectrange);
                selectrange = null;
            }
            GUILayout.EndVertical();
            return;
        }
        scrollview = EditorGUILayout.BeginScrollView(scrollview);
        Rect gridRect = GUILayoutUtility.GetRect(selectrange.MaxSize * gridsize, selectrange.MaxSize * gridsize);
        Event e = Event.current;

        // 计算网格覆盖的坐标范围（最小值和最大值）
        int minX = selectrange.Center.x - (selectrange.MaxSize - 1);
        int minY = selectrange.Center.y - (selectrange.MaxSize - 1);
        int maxX = selectrange.Center.x;
        int maxY = selectrange.Center.y;

        // 处理鼠标点击事件
        if (e.isMouse && gridRect.Contains(e.mousePosition))
        {
            Vector2 mousePos = e.mousePosition - gridRect.position;
            int cellX = minX + Mathf.FloorToInt(mousePos.x / gridsize);
            int cellY = minY + Mathf.FloorToInt(mousePos.y / gridsize);
            Vector2Int pos = new Vector2Int(cellX, cellY);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Debug.Log("点击在" + pos);
                if (pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY)
                {
                    if (selectrange.GridRange.Contains(pos))
                        selectrange.GridRange.Remove(pos);
                    else
                        selectrange.GridRange.Add(pos);
                }
            }
        }

        // 绘制单元格
        for (int x = 0; x < selectrange.MaxSize; x++)
        {
            for (int y = 0; y < selectrange.MaxSize; y++)
            {
                Rect cellRect = new Rect(gridRect.x + x * gridsize, gridRect.y + y * gridsize, gridsize, gridsize);
                // 计算该格子对应的世界坐标
                int worldX = minX + x;
                int worldY = minY + y;
                Vector2Int tile = new Vector2Int(worldX, worldY);

                GUI.color = selectrange.GridRange.Contains(tile) ? Color.yellow : Color.black;
                GUI.DrawTexture(cellRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color = Color.grey;
                GUI.Label(cellRect, tile.ToString());
            }
        }
        GUI.color = Color.white;
        // 绘制网格线
        Handles.color = Color.black;
        for (int x = 0; x <= selectrange.MaxSize; x++)
        {
            float xPos = gridRect.x + x * gridsize;
            Handles.DrawLine(new Vector3(xPos, gridRect.y), new Vector3(xPos, gridRect.y + selectrange.MaxSize * gridsize));
        }
        for (int y = 0; y <= selectrange.MaxSize; y++)
        {
            float yPos = gridRect.y + y * gridsize;
            Handles.DrawLine(new Vector3(gridRect.x, yPos), new Vector3(gridRect.x + selectrange.MaxSize * gridsize, yPos));
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("删除"))
        {
            SkillRanges.Remove(selectrange);
            selectrange = null;
        }
        GUILayout.EndVertical();
    }





    #endregion
    public void OnDestroy()
    {
        if (SkillInfos == null || SkillInfos.Count == 0)
        {
            Debug.LogError("警告：技能信息为空！");
            return;
        }
        else
        {
            DataLibrary.SaveData(SkillInfos, Application.dataPath + "/Resources/Data/Skills.json", "技能");
        }
        if (SkillRanges == null || SkillRanges.Count == 0)
        {
            Debug.LogError("警告：技能范围信息为空！");
            return;
        }
        else
        {
            DataLibrary.SaveData(SkillRanges, Application.dataPath + "/Resources/Data/SkillRange.json", "技能范围");
        }
        AssetDatabase.Refresh();
    }
}
