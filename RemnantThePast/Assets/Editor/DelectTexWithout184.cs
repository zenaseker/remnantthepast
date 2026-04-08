using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class DelectTexWithout184 : EditorWindow
{
    [MenuItem("Tools/Delete Non-184x184 PNGs")]
    static void DeletePNGs()
    {
        // 让用户选择文件夹
        string folder = EditorUtility.OpenFolderPanel("选择包含PNG的文件夹", Application.dataPath, "");
        if (string.IsNullOrEmpty(folder))
            return;

        // 获取所有PNG文件（包含子文件夹）
        string[] files = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
        List<string> toDelete = new List<string>();

        foreach (string file in files)
        {
            // 获取图片尺寸
            if (GetPNGSize(file, out int width, out int height))
            {
                if (width != 184 || height != 184)
                    toDelete.Add(file);
            }
            else
            {
                Debug.LogWarning($"无法读取图片尺寸: {file}");
            }
        }

        if (toDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "没有找到尺寸不为184x184的PNG文件。", "确定");
            return;
        }

        // 确认删除
        string message = $"将删除以下 {toDelete.Count} 个文件:\n";
        for (int i = 0; i < Mathf.Min(10, toDelete.Count); i++)
            message += toDelete[i] + "\n";
        if (toDelete.Count > 10) message += "...\n";
        message += "\n是否继续？";

        if (EditorUtility.DisplayDialog("确认删除", message, "删除", "取消"))
        {
            foreach (string file in toDelete)
            {
                File.Delete(file);
                Debug.Log("已删除: " + file);
            }
            AssetDatabase.Refresh();  // 刷新Unity项目视图
            EditorUtility.DisplayDialog("完成", $"已删除 {toDelete.Count} 个文件。", "确定");
        }
    }

    /// <summary>
    /// 解析PNG文件头获取宽度和高度（不加载整张图片）
    /// </summary>
    static bool GetPNGSize(string path, out int width, out int height)
    {
        width = height = 0;
        if (!File.Exists(path)) return false;

        byte[] header = new byte[24];
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            if (fs.Read(header, 0, 24) != 24)
                return false;
        }

        // PNG 文件头签名
        if (header[0] != 137 || header[1] != 80 || header[2] != 78 || header[3] != 71 ||
            header[4] != 13 || header[5] != 10 || header[6] != 26 || header[7] != 10)
            return false;

        // 检查第一个 chunk 是否为 IHDR (通常就是)
        if (header[12] == 73 && header[13] == 72 && header[14] == 68 && header[15] == 82) // "IHDR"
        {
            // 宽度和高度存储在 IHDR 数据部分，大端序
            width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
            height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
            return true;
        }
        else
        {
            // 理论上第一个 chunk 必定是 IHDR，但若格式异常则返回 false
            return false;
        }
    }
}