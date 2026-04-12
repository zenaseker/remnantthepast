using Newtonsoft.Json;
using SkillComponent;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using UnitBuf;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EquipDataEditorWindow : EditorWindow
{
    protected List<EquipInfo> equipinfos = new List<EquipInfo>();
    protected EquipInfo select;
    protected Vector2 scrollview;
    protected Vector2 scrollview2;

    [MenuItem("Jobs/遗珍信息")]
    public static void ShowWindow()
    {
        Open();
    }

    public static void Open()
    {
        EquipDataEditorWindow window = EquipDataEditorWindow.GetWindow<EquipDataEditorWindow>("遗珍信息");
        window.Inopen();
    }
    public async void Inopen()
    {
        equipinfos = (await DataLibrary.LoadResouceInfo<EquipInfo>("Data/EquipInfo")).ToList();
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
    Texture2D icon;
    private void DrawBufList()
    {
        foreach (var bufinfo in equipinfos)
        {
            if (GUILayout.Button(bufinfo.ID.ToString()))
            {
                select = bufinfo;
                icon = Resources.Load<Texture2D>("icon/Equip/" + select.Icon);
                GUI.FocusControl(null);
                EditorGUI.FocusTextInControl(null);
                Repaint();
            }
        }
        if (GUILayout.Button("新建"))
        {
            EquipInfo bufinfo = new EquipInfo();
            bufinfo.ID = equipinfos.Count + 1;
            select = bufinfo;
            equipinfos.Add(bufinfo);
        }
    }

    void DrawBufInfo()
    {
        if (select == null)
        {
            EditorGUILayout.LabelField("请选择遗珍");
            return;
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box",GUILayout.MaxWidth(150));
        if (select.Icon == null)
        {
            EditorGUILayout.HelpBox("未选择图标", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(110));
            GUILayout.Box(icon, GUILayout.Width(100), GUILayout.Height(100));
            EditorGUILayout.EndVertical();
        }
        DrawSelectCharIcon();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        select.ID = EditorGUILayout.IntField("ID", select.ID);
        select.MinRarity = EditorGUILayout.IntSlider(new GUIContent("最小稀有度:" + EquipInfo.GetRarityName(select.MinRarity), "旧物/遗痕/典藏/绝章/转捩"), select.MinRarity, 0, 4);
        select.MaxRarity = EditorGUILayout.IntSlider(new GUIContent("最大稀有度:" + EquipInfo.GetRarityName(select.MaxRarity), "旧物/遗痕/典藏/绝章/转捩"), select.MaxRarity, 0, 4);
        select.EquipData = EditorGUILayout.TextField("代码类名", select.EquipData);
        if (GUILayout.Button("创建代码文件"))
        {
            CopyCS(select.EquipData);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        scrollview2 = EditorGUILayout.BeginScrollView(scrollview2);
        for (int i = select.MinRarity; i <= select.MaxRarity; i++)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Rarity "+i);
            select.RartityInfos[i].Name = EditorGUILayout.TextField("名称", select.RartityInfos[i].Name);
            EditorGUILayout.EndHorizontal();
            select.RartityInfos[i].Description = EditorGUILayout.TextArea(select.RartityInfos[i].Description, GUILayout.Height(50));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("删除"))
        {
            equipinfos.Remove(select);
            select = null;
        }
    }
    readonly string fileName = "C# Script-NewEquip.cs.txt";
    readonly string sourcePath = Path.Combine(Application.dataPath, "Editor");
    readonly string targetPath = Path.Combine(Application.dataPath, "Code", "Skill", "Equip");
    void CopyCS(string name)
    {
        string sourceFile = Path.Combine(sourcePath, fileName);
        string destFile = Path.Combine(targetPath, $"Equip_{name}.cs");
        if (File.Exists(destFile)) return;
        string content = File.ReadAllText(sourceFile, Encoding.UTF8);
        string className = $"Equip_{name}";
        content = content.Replace("#SCRIPTNAME#", className);
        File.WriteAllText(destFile, content, Encoding.UTF8);
    }
    void DrawSelectCharIcon()
    {
        if (GUILayout.Button("选择图标"))
        {
            string path = EditorUtility.OpenFilePanel("选择图标", Application.dataPath + "/Resources/icon/Equip", "png");
            if (path != null)
            {
                path = Path.GetFileNameWithoutExtension(path);
                if (path != null || path != "")
                {
                    select.ReSetSprite(path);
                    icon = Resources.Load<Texture2D>("icon/Equip/" + path);
                }
            }
        }
    }
    public void OnDestroy()
    {
        if (equipinfos == null || equipinfos.Count == 0)
        {
            Debug.LogError("警告：遗珍信息为空！");
            return;
        }
        DataLibrary.SaveData(equipinfos, Application.dataPath + "/Resources/Data/EquipInfo.json", "Equip");
        AssetDatabase.Refresh();
    }
}
