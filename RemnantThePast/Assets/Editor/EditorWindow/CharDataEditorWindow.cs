using Spine.Unity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Spine.Unity.Editor;
using System.Reflection;
using System;

public class CharDataEditorWindow : EditorWindow
{
    public List<CharacterInfo> characterInfos = new List<CharacterInfo>();
    public List<SkillInfo> SkillInfos = new List<SkillInfo>();
    public Dictionary<string,PassiveInfo> PassiveInfos = new Dictionary<string, PassiveInfo>();
    protected Vector2 scrollview;
    protected Vector2 windowscrollview;
    protected Vector2 audioscrollview;
    protected CharacterInfo select;
    protected SkillInfo[] skillbyselect;
    public Dictionary<SkillInfo.SkillType,List<SkillInfo>> skillbydic;
    protected bool skillselect = false;
    protected int InSelectSkillIndex = -1;
    protected bool ShowModel = false;
    public GenericMenu menu;
    protected int ShowArks = 0;

    [MenuItem("Jobs/干员信息")]
    public static void ShowWindow()
    {
        Open();
    }

    public static void Open()
    {
        CharDataEditorWindow window = CharDataEditorWindow.GetWindow<CharDataEditorWindow>("干员信息");
        window.Inopen();
    }
    public async void Inopen()
    {
        characterInfos = (await DataLibrary.LoadResouceInfo<CharacterInfo>("Data/Characters")).ToList();
        PassiveInfos = (await DataLibrary.LoadResouceInfo<PassiveInfo>("Data/PassiveInfo")).ToDictionary(x => x.ID);
        skillbydic = new Dictionary<SkillInfo.SkillType, List<SkillInfo>>();
        menu = new GenericMenu();
        SkillInfos = (await DataLibrary.LoadResouceInfo<SkillInfo>("Data/Skills")).ToList();
        foreach (SkillInfo skill in SkillInfos)
        {
            if (skill.IsDerivedSkill) continue;
            if (!skillbydic.ContainsKey(skill.Type))
            {
                skillbydic.Add(skill.Type, new List<SkillInfo>());
            }
            skillbydic[skill.Type].Add(skill);
            menu.AddItem(new GUIContent($"{skill.Type}/{skill.Name}({skill.ID})"), false, OnColorSelected, skill);
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
        scrollview = EditorGUILayout.BeginScrollView(scrollview, GUILayout.MaxWidth(150));
        DrawCharList();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        windowscrollview = EditorGUILayout.BeginScrollView(windowscrollview);
        DrawCharData();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }
    private void DrawCharList()
    {
        foreach(CharacterInfo characterInfo in characterInfos)
        {
            if (GUILayout.Button(characterInfo.Name))
            {
                select = characterInfo;
                skillbyselect = new SkillInfo[select.Skills.Length];
                for(int i = 0;i < select.Skills.Length;i++)
                {
                    if (select.Skills[i] != null)
                    {
                        skillbyselect[i] = SkillInfos.Find(x => x.ID == select.Skills[i]);
                    }
                    else
                    {
                        skillbyselect[i] = null;
                    }
                }
                icon = Resources.Load<Texture2D>("icon/char_avatar/" + characterInfo.Icon);
                icon2 = Resources.Load<Texture2D>("icon/char_avatar/" + characterInfo.Icon + "_2");
                portrait = Resources.Load<Texture2D>("icon/char_portrait/" + characterInfo.Icon);
                portrait2 = Resources.Load<Texture2D>("icon/char_portrait/" + characterInfo.Icon + "_2");
                illustration = Resources.Load<Texture2D>("icon/char_illustration/" + characterInfo.Icon);
                illustration2 = Resources.Load<Texture2D>("icon/char_illustration/" + characterInfo.Icon + "_2");
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
            CharacterInfo character = new CharacterInfo();
            character.SetDefaultData();
            character.ID = characterInfos.Count;
            select = character;
            characterInfos.Add(character);
            skillbyselect = new SkillInfo[select.Skills.Length];
        }
    }
    private void DrawCharData()
    {
        if (select == null)
        {
            EditorGUILayout.LabelField("请选择干员");
            return;
        }
        EditorGUILayout.BeginHorizontal();
        if (select.Icon == null)
        {
            EditorGUILayout.HelpBox("未选择头像", MessageType.Warning);
            DrawSelectCharIcon();
        }
        else
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(110));
            GUILayout.Box(icon, GUILayout.Width(100), GUILayout.Height(100));
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.BeginVertical();
        select.ID = EditorGUILayout.IntField("ID", select.ID);
        select.Name = EditorGUILayout.TextField("名称", select.Name);
        EditorGUILayout.LabelField("描述");
        select.Description = EditorGUILayout.TextArea(select.Description, GUILayout.Height(50));
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("编辑数据"))
        {
            ShowArks = 0;
        }
        if (GUILayout.Button("编辑美术及音频"))
        {
            ShowArks = 1;
        }
        if (GUILayout.Button("编辑模型"))
        {
            ShowArks = 2;
        }
        EditorGUILayout.EndHorizontal();
        switch (ShowArks)
        {
            case 0:
                DrawCharInfo();
                break;
            case 1:
                DrawAllSpriteThisChar();
                break;
            case 2:
                DrawCharArt();
                break;
        }
        if (GUILayout.Button("删除此干员"))
        {
            if (characterInfos.Contains(select))
            {
                characterInfos.Remove(select);
            }
            else
            {
                Debug.Log("此干员不存在或已被删除");
            }
            select = null;
            skillselect = false;
            InSelectSkillIndex = -1;
        }
    }
    void DrawCharInfo()
    {
        select.Rarity = EditorGUILayout.IntField("星级", select.Rarity);
        select._Profession = (CharacterInfo.Profession)EditorGUILayout.EnumPopup("职业：" + CharacterInfo.GetProfessionText(select._Profession), select._Profession);
        select.InitialUnLock = EditorGUILayout.Toggle("是否为初始干员", select.InitialUnLock);
        EditorGUILayout.BeginHorizontal();
        select.MaxHp = EditorGUILayout.IntField("最大生命", select.MaxHp);
        select.IncreaseHp = EditorGUILayout.IntField("每级增加生命", select.IncreaseHp);
        EditorGUILayout.LabelField("Lv.80级最大生命：", (select.MaxHp + select.IncreaseHp * 80).ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        select.MaxMagic = EditorGUILayout.IntField("最大技力", select.MaxMagic);
        select.IncreaseMagic = EditorGUILayout.FloatField("每级增加最大技力", select.IncreaseMagic);
        EditorGUILayout.LabelField("Lv.80级最大技力：", ((int)(select.MaxMagic + select.IncreaseMagic * 80)).ToString());
        EditorGUILayout.EndHorizontal();

        select.StartMagic = EditorGUILayout.IntField("初始技力", select.StartMagic);

        EditorGUILayout.BeginHorizontal();
        select.Attack = EditorGUILayout.IntField("攻击力", select.Attack);
        select.IncreaseAttack = EditorGUILayout.IntField("每级增加攻击力", select.IncreaseAttack);
        EditorGUILayout.LabelField("Lv.80级攻击力：", (select.Attack + select.IncreaseAttack * 80).ToString());
        EditorGUILayout.EndHorizontal();

        select.MoveDistance = EditorGUILayout.IntField("移动距离", select.MoveDistance);
        EditorGUILayout.LabelField("技能","box");
        DrawSkillPanel(select);

        EditorGUILayout.LabelField("被动");
        for(int i = 0; i < select.Passives.Length;i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (select.Passives[i] != null && PassiveInfos.TryGetValue(select.Passives[i],out PassiveInfo info))
            {
                EditorGUILayout.LabelField("技能名：", info.Name);
            }
            else
            {
                EditorGUILayout.LabelField("无效的技能id");
            }
            select.Passives[i] = EditorGUILayout.TextField("技能id", select.Passives[i]);
            EditorGUILayout.EndHorizontal();
        }
    }
    void DrawCharArt()
    {
        EditorGUILayout.BeginHorizontal();
        select.Model = EditorGUILayout.TextField(new GUIContent("模型", "该设置为模型所在的文件夹"), select.Model);
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

    }
    Texture2D icon;//精一头像
    Texture2D icon2;//精二头像
    Texture2D portrait;//精一半身像
    Texture2D portrait2;//精二半身像
    Texture2D illustration;//精一立绘
    Texture2D illustration2;//精二立绘
    void DrawAllSpriteThisChar()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("初始头像/精二头像");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box(icon, GUILayout.Width(100), GUILayout.Height(100));
        GUILayout.Box(icon2, GUILayout.Width(100), GUILayout.Height(100));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("初始半身像/精二半身像");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box(portrait, GUILayout.Width(100), GUILayout.Height(200));
        GUILayout.Box(portrait2, GUILayout.Width(100), GUILayout.Height(200));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("初始立绘/精二立绘");
        EditorGUILayout.BeginHorizontal();
        GUILayout.Box(illustration, GUILayout.Width(300 * illustration.width / illustration.height), GUILayout.Height(300));
        GUILayout.Box(illustration2, GUILayout.Width(300 * illustration2.width / illustration2.height), GUILayout.Height(300));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("头像/半身像/立绘图片名相同，", "精二图像在末尾添加“_2”");
        DrawSelectCharIcon();
        EditorGUILayout.EndVertical();
        DrawAudioList();
        EditorGUILayout.EndHorizontal();
    }
    List<AudioClip> Audios = new List<AudioClip>();
    readonly string[] AudiosHeads = new string[38]
    {
        "任命助理",
        "交谈1","交谈2","交谈3",
        "晋升后交谈1","晋升后交谈2",
        "信赖提升后交谈1","信赖提升后交谈2","信赖提升后交谈3",
        "闲置","干员报到","观看作战记录",
        "精英化晋升1","精英化晋升2",
        "编入队伍","任命队长","行动出发","行动开始",
        "选中干员1","选中干员2",
        "部署1","部署2",
        "作战中1","作战中2","作战中3","作战中4",
        "完成高难行动","3星结束行动","非3星结束行动","行动失败",
        "进驻设施","戳一下","信赖触摸",
        "标题",
        "问候",
        "?",
        "??",
        "???",
    };
    void DrawAudioList()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("音频文件夹名需与头像图片名相同");
        if (GUILayout.Button("加载", GUILayout.Width(60)))
        {
            LoadAudioClips();
        }
        audioscrollview = EditorGUILayout.BeginScrollView(audioscrollview);
        if (Audios.Count > 0)
        {
            EditorGUILayout.Space();
            for (int i = 0; i < Audios.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField(AudiosHeads[i]);
                EditorGUILayout.ObjectField(Audios[i], typeof(AudioClip), false);
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("请输入有效的文件夹名称并点击加载", MessageType.Info);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void LoadAudioClips()
    {
        Audios.Clear();
        if (string.IsNullOrEmpty(select.Icon))
        {
            EditorUtility.DisplayDialog("错误", "文件夹名称不能为空", "确定");
            return;
        }
        string targetFolder = Path.Combine("Assets/Resources/Audio/voice", select.Icon);
        string fullPath = Path.Combine(Application.dataPath, "Resources/Audio/voice", select.Icon);
        if (!Directory.Exists(fullPath))
        {
            EditorUtility.DisplayDialog("错误", $"文件夹不存在：\n{fullPath}", "确定");
            return;
        }
        string[] files = Directory.GetFiles(fullPath, "*.wav");
        foreach (string file in files)
        {
            string relativePath = "Assets" + file.Replace(Application.dataPath, "").Replace('\\', '/');
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);
            if (clip != null)
            {
                Audios.Add(clip);
            }
        }
        if (Audios.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "该文件夹下没有找到.wav文件", "确定");
        }
    }

    void DrawSelectCharIcon()
    {
        if (GUILayout.Button("选择图标"))
        {
            string path = EditorUtility.OpenFilePanel("选择图标", Application.dataPath + "/Resources/icon/char_avatar", "png");
            if (path != null)
            {
                path = Path.GetFileNameWithoutExtension(path);
                if (path != null || path != "")
                {
                    select.Icon = path;
                }
            }
        }
    }
    private void DrawSkillPanel(CharacterInfo info)
    {
        for(int i = 0;i < skillbyselect.Length;i++)
        {
            EditorGUILayout.BeginHorizontal("box",GUILayout.MinHeight(50));
            SkillInfo skill = skillbyselect[i];
            bool skillnotnull = skill != null;
            if (skillnotnull)
            {
                if (skill.Image != null)
                {
                    GUILayout.Box(new GUIContent(skill.Image, skill.sprite.texture), GUILayout.MaxWidth(300), GUILayout.Height(45));
                }
                EditorGUILayout.BeginVertical(GUILayout.Width(100));
                EditorGUILayout.LabelField(skill.Name);
                EditorGUILayout.LabelField("技能等级:" + skill.LevelToString());
                EditorGUILayout.LabelField("技能类别:" + skill.SkillTypeToString());
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("未设置的技能", MessageType.Warning);
            }
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
            if (GUILayout.Button("选择技能"))
            {
                InSelectSkillIndex = i;
                menu.ShowAsContext();
            }
            if (skillnotnull && GUILayout.Button("跳转到技能页面"))
            {
                SkillDataEditorWindow.OpenWithSkill(skill.ID);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
    void OnColorSelected(object skill)
    {
        if (InSelectSkillIndex != -1)
        {
            select.Skills[InSelectSkillIndex] = ((SkillInfo)skill).ID;
            skillbyselect[InSelectSkillIndex] = (SkillInfo)skill;
        }
    }
    public void OnDestroy()
    {
        if (characterInfos == null || characterInfos.Count == 0)
        {
            Debug.LogError("警告：Character信息为空！");
            return;
        }
        DataLibrary.SaveData(characterInfos, Application.dataPath + "/Resources/Data/Characters.json", "角色");
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
