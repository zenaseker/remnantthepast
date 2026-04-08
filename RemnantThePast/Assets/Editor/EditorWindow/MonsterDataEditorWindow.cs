using Spine.Unity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Spine.Unity.Editor;
using System;

public class MonsterDataEditorWindow : EditorWindow
{
    public List<MonsterInfo> MonsterInfos = new List<MonsterInfo>();
    protected Vector2 scrollview;
    protected Vector2 windowscrollview;
    protected Vector2 intentscrollview;
    protected MonsterInfo select;
    protected bool ShowModel = false;
    protected bool ShowIntent = false;

    [MenuItem("Jobs/敌人信息")]
    public static void ShowWindow()
    {
        Open();
    }

    public static void Open()
    {
        MonsterDataEditorWindow window = MonsterDataEditorWindow.GetWindow<MonsterDataEditorWindow>("敌人信息");
        window.Inopen();
    }
    public async void Inopen()
    {
        MonsterInfos = (await DataLibrary.LoadResouceInfo<MonsterInfo>("Data/Monsters")).ToList();
    }

    void OnGUI()
    {
        if (MonsterInfos == null) return;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
        scrollview = EditorGUILayout.BeginScrollView(scrollview, GUILayout.MaxWidth(150));
        DrawCharList();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        windowscrollview = EditorGUILayout.BeginScrollView(windowscrollview);
        DrawMonsterData();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        if (ShowIntent)
        {
            DrawMonsterIntent();
        }
        EditorGUILayout.EndHorizontal();
    }
    private void DrawCharList()
    {
        foreach(MonsterInfo MonsterInfo in MonsterInfos)
        {
            if (GUILayout.Button(MonsterInfo.ID))
            {
                select = MonsterInfo;
                var objects = Resources.LoadAll("spine/" + select.Model);
                foreach(var obj in objects)
                {
                    if (obj is SkeletonDataAsset)
                    {
                        skeletonData = obj as SkeletonDataAsset;
                        spinePreview.Initialize(this.Repaint, skeletonData, "");
                        break;
                    }
                }
                GUI.FocusControl(null);
                EditorGUI.FocusTextInControl(null);
                Repaint();
            }
        }
        if (GUILayout.Button("新建"))
        {
            MonsterInfo character = new MonsterInfo();
            character.SetDefaultData();
            character.ID = "NoNameMos_" + MonsterInfos.Count;
            select = character;
            MonsterInfos.Add(character);
        }
    }
    private void DrawMonsterData()
    {
        if (select == null)
        {
            EditorGUILayout.LabelField("请选择敌人");
            return;
        }
        EditorGUILayout.BeginHorizontal();
        if (select.Icon == null)
        {
            EditorGUILayout.HelpBox("未选择图片", MessageType.Warning);
            DrawSelectMonsterIcon();
        }
        else if (select.sprite == null)
        {
            EditorGUILayout.HelpBox("图标读取错误", MessageType.Warning);
        }
        else
        {
            GUILayout.Box(select.sprite.texture, GUILayout.Width(45), GUILayout.Height(45));
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
            EditorGUILayout.LabelField(select.Icon);
            DrawSelectMonsterIcon();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.BeginVertical();
        select.ID = EditorGUILayout.TextField("ID", select.ID);
        select.Name = EditorGUILayout.TextField("名称", select.Name);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("描述");
        select.Description = EditorGUILayout.TextArea(select.Description, GUILayout.Height(50));
        select.MaxHp = EditorGUILayout.IntField("生命", select.MaxHp);
        select.Attack = EditorGUILayout.IntField("攻击力", select.Attack);
        DrawList(select.WarningRange, "警戒范围");
        DrawList(select.AttackRange, "攻击范围");

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("意图");
        if (ShowIntent)
        {
            if (GUILayout.Button("结束编辑意图"))
            {
                ShowIntent = false;
            }
        }
        else
        {
            if (GUILayout.Button("编辑意图"))
            {
                ShowIntent = true;
            }
        }
        EditorGUILayout.EndHorizontal();
        if (select.monsterIntentInfos != null)
        {
            foreach (MonsterIntentInfo intent in select.monsterIntentInfos)
            {
                EditorGUILayout.LabelField(intent.Name);
            }
        }
        else
        {
            select.monsterIntentInfos = new List<MonsterIntentInfo>();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        select.Model = EditorGUILayout.TextField(new GUIContent("模型","该设置为模型所在的文件夹"), select.Model);
        if (GUILayout.Button("使用敌人ID作为模型设置"))
        {
            select.Model = select.ID;
        }
        if (select.Model == select.ID && !Directory.Exists(Application.dataPath + "/Resources/spine/" + select.Model))
        {
            EditorGUILayout.HelpBox("模型文件夹与敌人ID不匹配", MessageType.Info);
        }
        if (GUILayout.Button("刷新"))
        {
            try
            {
                var objects = Resources.LoadAll("spine/" + select.Model);
                foreach (var obj in objects)
                {
                    if (obj is SkeletonDataAsset)
                    {
                        skeletonData = obj as SkeletonDataAsset;
                        spinePreview.Initialize(this.Repaint, skeletonData, "");
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        if (!Directory.Exists(Application.dataPath + "/Resources/spine/" + select.Model))
        {
            EditorGUILayout.HelpBox("模型文件夹不存在！", MessageType.Warning);
        }
        else
        {
            ShowModel = EditorGUILayout.Toggle("预览模型", ShowModel);
        }
        EditorGUILayout.EndHorizontal();
        if (ShowModel)
        {
            DrawModel();
        }
        if (GUILayout.Button("删除此敌人"))
        {
            if (MonsterInfos.Contains(select))
            {
                MonsterInfos.Remove(select);
            }
            else
            {
                Debug.Log("此敌人不存在或已被删除");
            }
            select = null;
        }
    }
    void DrawSelectMonsterIcon()
    {
        if (GUILayout.Button("选择图标"))
        {
            string path = EditorUtility.OpenFilePanel("选择图标", Application.dataPath + "/Resources/icon/enemies", "png");
            if (path != null)
            {
                path = Path.GetFileNameWithoutExtension(path);
                if (path != null || path != "")
                {
                    select.ReSetSprite(path);
                }
            }
        }
    }
    void DrawSelectMonsterintentIcon(MonsterIntentInfo intent)
    {
        string path = EditorUtility.OpenFilePanel("选择图标", Application.dataPath + "/Resources/icon/enemies", "png");
        if (path != null)
        {
            path = Path.GetFileNameWithoutExtension(path);
            if (path != null || path != "")
            {
                intent.ReSetSprite(path);
            }
        }
    }

    void DrawMonsterIntent()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("意图");
        EditorGUILayout.BeginScrollView(intentscrollview, "box");
        foreach(MonsterIntentInfo intent in select.monsterIntentInfos)
        {
            EditorGUILayout.BeginVertical("box");
            intent.Name = EditorGUILayout.TextField("名称", intent.Name);
            EditorGUILayout.LabelField("描述");
            intent.Description = EditorGUILayout.TextArea(intent.Description, GUILayout.Height(50));
            intent.time = EditorGUILayout.FloatField("时间", intent.time);
            intent.strength = EditorGUILayout.IntSlider("强度", intent.strength, 0, 2);
            if (intent.icon == null)
            {
                EditorGUILayout.HelpBox("未选择图标", MessageType.Warning);
                if (GUILayout.Button("选择图标"))
                {
                    DrawSelectMonsterintentIcon(intent);
                }
            }
            else if (intent.sprite == null)
            {
                EditorGUILayout.HelpBox("图标读取错误", MessageType.Warning);
                if (GUILayout.Button("选择图标"))
                {
                    DrawSelectMonsterintentIcon(intent);
                }
            }
            else
            {
                GUILayout.Box(intent.sprite.texture, GUILayout.Width(45), GUILayout.Height(45));
                EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(50));
                EditorGUILayout.LabelField(intent.icon);
                if (GUILayout.Button("选择图标"))
                {
                    DrawSelectMonsterintentIcon(intent);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("移除意图"))
            {
                select.monsterIntentInfos.Remove(intent);
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
        if (GUILayout.Button("新建意图"))
        {
            MonsterIntentInfo intent = new MonsterIntentInfo();
            if (select.Icon != null)
            {
                intent.ReSetSprite(select.Icon);
            }
            select.monsterIntentInfos.Add(intent);
        }
        EditorGUILayout.EndVertical();
    }
    bool foldout = false;
    void DrawList(List<Vector2Int> list,string name)
    {
        foldout = EditorGUILayout.Foldout(foldout, name);
        if (foldout)
        {
            for(int i = 0; i < list.Count; i++)
            {
                list[i] = EditorGUILayout.Vector2IntField(i.ToString(), list[i]);
                
            }
            EditorGUILayout.BeginHorizontal("box", GUILayout.MaxWidth(50));
            if (GUILayout.Button("+"))
            {
                list.Add(Vector2Int.zero);
            }
            if (GUILayout.Button("-") && list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
    public void OnDestroy()
    {
        if (MonsterInfos == null || MonsterInfos.Count == 0)
        {
            Debug.LogError("警告：敌人信息为空！");
            return;
        }
        DataLibrary.SaveData(MonsterInfos, Application.dataPath + "/Resources/Data/Monsters.json", "敌人");
        AssetDatabase.Refresh();
    }
    #region 预览模型绘制

    private SkeletonInspectorPreview spinePreview;
    private SkeletonDataAsset skeletonData;
    private Editor spineEditor;
    private void OnEnable()
    {
        spinePreview = new SkeletonInspectorPreview();
        EditorApplication.update += OnEditorUpdate;
    }
    private void OnDisable()
    {
        spinePreview?.Clear();
        EditorApplication.update -= OnEditorUpdate;
    }
    private void DrawModel()
    {
        if (skeletonData != null)
        {
            if (spineEditor == null)
            {
                spineEditor = Editor.CreateEditor(skeletonData);
            }
            EditorGUILayout.BeginHorizontal();
            DrawAnimationList();
            var previewRect = GUILayoutUtility.GetRect(position.width - 400, position.height - 250);
            spinePreview.HandleInteractivePreviewGUI(previewRect, GUIStyle.none);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("未读取到Spine文件", MessageType.Warning);
        }
    }
    private void DrawAnimationList()
    {
        var _skeletonData = skeletonData.GetSkeletonData(false);
        if (_skeletonData == null) return;
        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(100));
        EditorGUILayout.LabelField("动画列表");
        foreach (var anim in _skeletonData.Animations)
        {
            if (GUILayout.Button(anim.Name))
            {
                spinePreview.PlayPauseAnimation(anim.Name, true);
            }
        }
        if (GUILayout.Button("停止"))
        {
            spinePreview.ClearAnimationSetupPose();
        }
        EditorGUILayout.EndVertical();
    }

    private void OnEditorUpdate()
    {
        spinePreview?.HandleEditorUpdate();
    }

    #endregion
}
