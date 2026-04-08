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

public class PassiveDataEditorWindow : EditorWindow
{
    protected List<PassiveInfo> passiveInfos = new List<PassiveInfo>();
    protected PassiveInfo select;
    protected Vector2 scrollview;
    protected static bool CreateCS = false;
    protected string newCSname;//新文件名

    [MenuItem("Jobs/被动信息")]
    public static void ShowWindow()
    {
        Open();
    }

    private void OnEnable()
    {
        if (passiveInfos == null || passiveInfos.Count == 0) Open();
    }
    public static void Open()
    {
        PassiveDataEditorWindow window = GetWindow<PassiveDataEditorWindow>("被动信息");
        window.Inopen();
    }
    public async void Inopen()
    {
        passiveInfos = (await DataLibrary.LoadResouceInfo<PassiveInfo>("Data/PassiveInfo")).ToList();
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
        CreateCS = EditorGUILayout.Toggle(new GUIContent("同步创建文件", "在创建技能信息时会同步创建PassiveBase类"), CreateCS);
        foreach (var bufinfo in passiveInfos)
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
            newCSname = EditorGUILayout.TextField("新被动ID", newCSname);
            if (newCSname == null || newCSname == "")
            {
                EditorGUILayout.HelpBox("被动ID不能为空！", MessageType.Warning);
            }
            else if (GUILayout.Button("新建"))
            {
                PassiveInfo buf = new PassiveInfo();
                buf.ID = newCSname;
                select = buf;
                passiveInfos.Add(buf);
                CopyCS(newCSname);
            }
        }
        else
        {
            if (GUILayout.Button("新建"))
            {
                PassiveInfo bufinfo = new PassiveInfo();
                bufinfo.ID = "NewPassive_" + passiveInfos.Count;
                select = bufinfo;
                passiveInfos.Add(bufinfo);
            }
        }
    }

    readonly string fileName = "C# Script-NewPassiveInfo.cs.txt";
    readonly string sourcePath = Path.Combine(Application.dataPath, "Editor");
    readonly string targetPath = Path.Combine(Application.dataPath, "Code", "Skill", "Passive");
    void CopyCS(string name)
    {
        string sourceFile = Path.Combine(sourcePath, fileName);
        string className = $"Passive_{name}";
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
            passiveInfos.Remove(select);
            select = null;
        }
    }
    public void OnDestroy()
    {
        if (passiveInfos == null || passiveInfos.Count == 0)
        {
            Debug.LogError("警告：Passive信息为空！");
            return;
        }
        DataLibrary.SaveData(passiveInfos, Application.dataPath + "/Resources/Data/PassiveInfo.json", "Passive");
        AssetDatabase.Refresh();
    }
}
