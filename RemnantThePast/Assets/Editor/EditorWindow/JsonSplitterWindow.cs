using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Linq;

public class JsonSplitterWindow : EditorWindow
{
    private string sourceFilePath = "";
    private string outputFolderPath = "";
    private int maxItemsPerFile = 100;

    [MenuItem("Tools/Json Splitter")]
    public static void ShowWindow()
    {
        GetWindow<JsonSplitterWindow>("JSON Splitter");
    }

    private void OnGUI()
    {
        GUILayout.Label("分割 JSON 文件（支持数组和对象）", EditorStyles.boldLabel);

        // 源文件选择
        EditorGUILayout.LabelField("源 JSON 文件");
        EditorGUILayout.BeginHorizontal();
        sourceFilePath = EditorGUILayout.TextField(sourceFilePath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFilePanel("选择 JSON 文件", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                sourceFilePath = path;
                if (string.IsNullOrEmpty(outputFolderPath))
                    outputFolderPath = Path.GetDirectoryName(sourceFilePath);
            }
        }
        EditorGUILayout.EndHorizontal();

        // 输出文件夹
        EditorGUILayout.LabelField("输出文件夹");
        EditorGUILayout.BeginHorizontal();
        outputFolderPath = EditorGUILayout.TextField(outputFolderPath);
        if (GUILayout.Button("浏览", GUILayout.Width(60)))
        {
            string folder = EditorUtility.OpenFolderPanel("选择输出文件夹", outputFolderPath, "");
            if (!string.IsNullOrEmpty(folder))
                outputFolderPath = folder;
        }
        EditorGUILayout.EndHorizontal();

        // 每个文件最大条目数
        maxItemsPerFile = EditorGUILayout.IntField("每个文件最大条目数", maxItemsPerFile);
        if (maxItemsPerFile < 1) maxItemsPerFile = 1;

        EditorGUILayout.Space();

        GUI.enabled = !string.IsNullOrEmpty(sourceFilePath) && File.Exists(sourceFilePath);
        if (GUILayout.Button("分割 JSON", GUILayout.Height(30)))
        {
            SplitJson();
        }
        GUI.enabled = true;
    }

    private void SplitJson()
    {
        try
        {
            string jsonContent = File.ReadAllText(sourceFilePath);
            JToken root = JToken.Parse(jsonContent);

            string baseFileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            string ext = Path.GetExtension(sourceFilePath);

            if (!Directory.Exists(outputFolderPath))
                Directory.CreateDirectory(outputFolderPath);

            if (root is JArray array)
            {
                // 处理数组
                SplitArray(array, baseFileName, ext);
            }
            else if (root is JObject obj)
            {
                // 处理对象（字典）
                SplitObject(obj, baseFileName, ext);
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "根节点既不是数组也不是对象，无法分割。", "确定");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"分割失败: {e.Message}", "确定");
            Debug.LogError(e);
        }
    }

    private void SplitArray(JArray array, string baseFileName, string ext)
    {
        int totalCount = array.Count;
        if (totalCount == 0)
        {
            EditorUtility.DisplayDialog("提示", "JSON 数组为空，无需分割。", "确定");
            return;
        }

        int fileCount = Mathf.CeilToInt((float)totalCount / maxItemsPerFile);
        for (int i = 0; i < fileCount; i++)
        {
            int startIdx = i * maxItemsPerFile;
            int endIdx = Mathf.Min(startIdx + maxItemsPerFile, totalCount);
            JArray subArray = new JArray();
            for (int j = startIdx; j < endIdx; j++)
            {
                subArray.Add(array[j]);
            }

            string fileName = $"{baseFileName}_part{i + 1}{ext}";
            string fullPath = Path.Combine(outputFolderPath, fileName);
            File.WriteAllText(fullPath, subArray.ToString(Newtonsoft.Json.Formatting.Indented));
            Debug.Log($"已生成: {fullPath}");
        }

        EditorUtility.DisplayDialog("完成", $"数组已分割为 {fileCount} 个文件。", "确定");
    }

    private void SplitObject(JObject obj, string baseFileName, string ext)
    {
        var properties = obj.Properties();
        int totalCount = 0;
        foreach (var _ in properties) totalCount++; // 获取键值对总数
        if (totalCount == 0)
        {
            EditorUtility.DisplayDialog("提示", "JSON 对象为空，无需分割。", "确定");
            return;
        }

        int fileCount = Mathf.CeilToInt((float)totalCount / maxItemsPerFile);
        var propList = obj.Properties().ToList(); // 转为列表便于索引

        for (int i = 0; i < fileCount; i++)
        {
            int startIdx = i * maxItemsPerFile;
            int endIdx = Mathf.Min(startIdx + maxItemsPerFile, totalCount);
            JObject subObj = new JObject();
            for (int j = startIdx; j < endIdx; j++)
            {
                subObj.Add(propList[j]);
            }

            string fileName = $"{baseFileName}_part{i + 1}{ext}";
            string fullPath = Path.Combine(outputFolderPath, fileName);
            File.WriteAllText(fullPath, subObj.ToString(Newtonsoft.Json.Formatting.Indented));
            Debug.Log($"已生成: {fullPath}");
        }

        EditorUtility.DisplayDialog("完成", $"对象已分割为 {fileCount} 个文件。", "确定");
    }
}