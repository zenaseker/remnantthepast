using Newtonsoft.Json;
using SkillComponent;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnitBuf;
using UnityEditor;
using UnityEngine;

public class BufDataEditorWindow : EditorWindow
{
    protected List<BattleUnitBufInfo> battleUnitBufInfos = new List<BattleUnitBufInfo>();
    protected BattleUnitBufInfo select;
    protected Vector2 scrollview;
    protected static bool CreateCS = false;
    protected string newCSname;//新文件名

    [MenuItem("Jobs/Buf信息")]
    public static void ShowWindow()
    {
        Open();
    }

    private void OnEnable()
    {
        if (battleUnitBufInfos == null || battleUnitBufInfos.Count == 0) Open();
    }
    public static void Open()
    {
        BufDataEditorWindow window = BufDataEditorWindow.GetWindow<BufDataEditorWindow>("Buf信息");
        window.Inopen();
    }
    public async void Inopen()
    {
        battleUnitBufInfos = (await DataLibrary.LoadResouceInfo<BattleUnitBufInfo>("Data/BufInfo")).ToList();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        scrollview = EditorGUILayout.BeginScrollView(scrollview, "box", GUILayout.MaxWidth(200), GUILayout.ExpandHeight(true));
        DrawBufList();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.BeginVertical();
        DrawBufInfo();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
    private void DrawBufList()
    {
        CreateCS = EditorGUILayout.Toggle(new GUIContent("同步创建文件", "在创建技能信息时会同步创建BattleUnitBuf类"), CreateCS);
        foreach (var bufinfo in battleUnitBufInfos)
        {
            if (GUILayout.Button(bufinfo.ID))
            {
                select = bufinfo; 
                GUI.FocusControl(null);
                EditorGUI.FocusTextInControl(null);
                Repaint();
            }
        }
        if (CreateCS)
        {
            newCSname = EditorGUILayout.TextField("新Buf名", newCSname);
            if (newCSname == null || newCSname == "")
            {
                EditorGUILayout.HelpBox("Buf名不能为空！", MessageType.Warning);
            }
            else if (GUILayout.Button("新建"))
            {
                BattleUnitBufInfo buf = new BattleUnitBufInfo();
                buf.ID = newCSname;
                select = buf;
                battleUnitBufInfos.Add(buf);
                CopyCS(newCSname);
            }
        }
        else
        {
            if (GUILayout.Button("新建"))
            {
                BattleUnitBufInfo bufinfo = new BattleUnitBufInfo();
                bufinfo.ID = "NewBuf_" + battleUnitBufInfos.Count;
                select = bufinfo;
                battleUnitBufInfos.Add(bufinfo);
            }
        }
    }

    readonly string fileName = "C# Script-NewBufInfo.cs.txt";
    readonly string sourcePath = Path.Combine(Application.dataPath, "Editor");
    readonly string targetPath = Path.Combine(Application.dataPath, "Code", "Buf", "UnitBufs");
    void CopyCS(string name)
    {
        string sourceFile = Path.Combine(sourcePath, fileName);
        string className = $"BattleUnitBuf_{name}";
        string destFile = Path.Combine(targetPath, $"{className}.cs");
        string content = File.ReadAllText(sourceFile, Encoding.UTF8);
        content = content.Replace("#SCRIPTNAME#", className);
        File.WriteAllText(destFile, content, Encoding.UTF8);
    }

    void DrawBufInfo()
    {
        if (select == null)
        {
            EditorGUILayout.LabelField("请选择技能");
            return;
        }
        select.ID = EditorGUILayout.TextField("ID", select.ID);
        select.Name = EditorGUILayout.TextField("名称", select.Name);
        select.Description = EditorGUILayout.TextField("描述", select.Description);
        if (GUILayout.Button("删除"))
        {
            battleUnitBufInfos.Remove(select);
            select = null;
        }
    }
    public void OnDestroy()
    {
        if (battleUnitBufInfos == null || battleUnitBufInfos.Count == 0)
        {
            Debug.LogError("警告：Buf信息为空！");
            return;
        }
        DataLibrary.SaveData(battleUnitBufInfos, Application.dataPath + "/Resources/Data/BufInfo.json", "Buf");
        AssetDatabase.Refresh();
    }
}
