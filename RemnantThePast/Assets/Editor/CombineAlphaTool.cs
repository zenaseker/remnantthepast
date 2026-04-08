using UnityEngine;
using UnityEditor;
using System.IO;

public class CombineAlphaTool : EditorWindow
{
    enum UsePass
    {
        R,
        G, 
        B,
        A
    }
    private string folderPath = "Assets/Arks/Sprite/ui";
    private string baseName = "myimage";
    private Texture2D basetex;
    private Texture2D alphatex;
    string extUsed = "";
    UsePass usePass = UsePass.R;

    [MenuItem("Tools/Combine Alpha from Image")]
    static void ShowWindow()
    {
        GetWindow<CombineAlphaTool>("Combine Alpha Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("文件夹路径", GUILayout.Width(100));
        folderPath = EditorGUILayout.TextField(folderPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("选择文件夹", folderPath, "");
            if (!string.IsNullOrEmpty(selected))
            {
                // 转换为项目相对路径
                if (selected.StartsWith(Application.dataPath))
                {
                    folderPath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择项目内的文件夹", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        baseName = EditorGUILayout.TextField("基础名称", baseName);
        if (GUILayout.Button("搜索图片"))
        {
            FindTexture();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        GUILayout.Box(basetex, GUILayout.Width(200), GUILayout.Height(200));
        GUILayout.Box(alphatex, GUILayout.Width(200), GUILayout.Height(200));
        usePass = (UsePass)EditorGUILayout.EnumPopup("使用通道",usePass);
        EditorGUILayout.EndHorizontal();


        if (GUILayout.Button("执行合并"))
        {
            CombineAlpha();
        }
    }
    void FindTexture()
    {
        // 查找图片扩展名（优先尝试png，若不存在则尝试其他常见格式）
        string[] extensions = { ".png", ".jpg", ".jpeg", ".tga", ".bmp" };
        string basePath = null;
        string alphaPath = null;

        foreach (string ext in extensions)
        {
            string testBase = Path.Combine(folderPath, baseName + ext);
            if (File.Exists(testBase))
            {
                basePath = testBase;
                extUsed = ext;
                break;
            }
        }
        if (basePath == null)
        {
            EditorUtility.DisplayDialog("错误", $"找不到基础图片: {baseName} (尝试了 {string.Join(", ", extensions)})", "确定");
            return;
        }

        foreach (string ext in extensions)
        {
            string testAlpha = Path.Combine(folderPath, baseName + "a" + ext);
            if (File.Exists(testAlpha))
            {
                alphaPath = testAlpha;
                break;
            }
        }
        if (alphaPath == null)
        {
            EditorUtility.DisplayDialog("错误", $"找不到透明度图片: {baseName}a (尝试了 {string.Join(", ", extensions)})", "确定");
            return;
        }

        // 加载纹理
        basetex = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
        alphatex = AssetDatabase.LoadAssetAtPath<Texture2D>(alphaPath);

        if (basetex == null || alphatex == null)
        {
            EditorUtility.DisplayDialog("错误", "无法加载纹理，请确保它们是可读的。", "确定");
            return;
        }

        // 确保纹理可读写
        string baseAssetPath = basePath;
        string alphaAssetPath = alphaPath;

        var baseImporter = AssetImporter.GetAtPath(baseAssetPath) as TextureImporter;
        var alphaImporter = AssetImporter.GetAtPath(alphaAssetPath) as TextureImporter;

        bool needReimport = false;
        if (baseImporter != null && !baseImporter.isReadable)
        {
            baseImporter.isReadable = true;
            baseImporter.SaveAndReimport();
            needReimport = true;
        }
        if (alphaImporter != null && !alphaImporter.isReadable)
        {
            alphaImporter.isReadable = true;
            alphaImporter.SaveAndReimport();
            needReimport = true;
        }
        if (needReimport)
        {
            // 重新加载纹理
            basetex = AssetDatabase.LoadAssetAtPath<Texture2D>(baseAssetPath);
            alphatex = AssetDatabase.LoadAssetAtPath<Texture2D>(alphaAssetPath);
        }

        // 检查尺寸
        if (basetex.width != alphatex.width || basetex.height != alphatex.height)
        {
            EditorUtility.DisplayDialog("错误", $"图片尺寸不一致：基础图 {basetex.width}x{basetex.height}，透明度图 {alphatex.width}x{alphatex.height}", "确定");
            return;
        }
    }
    private void CombineAlpha()
    {

        // 创建新纹理，使用 RGBA32 格式确保可读写
        Texture2D resultTex = new Texture2D(basetex.width, basetex.height, TextureFormat.RGBA32, false);
        Color[] basePixels = basetex.GetPixels();
        Color[] alphaPixels = alphatex.GetPixels();
        switch (usePass)
        {
            case UsePass.R:
                for (int i = 0; i < basePixels.Length; i++)
                {
                    basePixels[i].a = alphaPixels[i].r;
                }
                break;
            case UsePass.G:
                for (int i = 0; i < basePixels.Length; i++)
                {
                    basePixels[i].a = alphaPixels[i].g;
                }
                break;
            case UsePass.B:
                for (int i = 0; i < basePixels.Length; i++)
                {
                    basePixels[i].a = alphaPixels[i].b;
                }
                break;
            case UsePass.A:
                for (int i = 0; i < basePixels.Length; i++)
                {
                    basePixels[i].a = alphaPixels[i].a;
                }
                break;
        }

        resultTex.SetPixels(basePixels);
        resultTex.Apply();

        // 保存为PNG
        string resultPath = Path.Combine(folderPath, baseName + "_a" + extUsed);
        byte[] pngData = resultTex.EncodeToPNG();
        File.WriteAllBytes(resultPath, pngData);

        // 刷新资源数据库
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完成", $"已保存至: {resultPath}", "确定");
    }
}