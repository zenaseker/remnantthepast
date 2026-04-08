using Newtonsoft.Json;
using SkillComponent;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using UnitBuf;
using UnityEditor;
using UnityEngine;

public class EquipDataEditorWindow : EditorWindow
{
    protected List<EquipInfo> equipinfos = new List<EquipInfo>();
    protected EquipInfo select;
    protected Vector2 scrollview;

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
            if (GUILayout.Button(bufinfo.Name))
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
        select.Name = EditorGUILayout.TextField("名称", select.Name);
        EditorGUILayout.PrefixLabel("描述");
        select.Description = EditorGUILayout.TextArea(select.Description, GUILayout.Height(50));
        select.Rarity = EditorGUILayout.IntField("稀有度", select.Rarity);
        EditorGUILayout.LabelField("稀有度：旧物/遗痕/典藏/绝章/转捩");
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("删除"))
        {
            equipinfos.Remove(select);
            select = null;
        }
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
