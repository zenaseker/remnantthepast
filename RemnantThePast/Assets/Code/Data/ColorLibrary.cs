using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

/// <summary>
/// 颜色管理
/// </summary>
public class ColorLibrary
{
    private static ColorLibrary instance;
    public static ColorLibrary Instance { get
        {
            if (instance == null)
            {
                instance = new ColorLibrary();
                instance.OnCreate();
            }
            return instance; 
        } set { instance = value; } }
    public enum ColorEnum
    {
        PlayerRange,
        MonsterWarningRange,
        MonsterAttackRange,
        PlayerMoveWarningRange,
        Orange,

    }
    public Dictionary<ColorEnum,Color> Colores;
    public Dictionary<string, string> ColorTexts;

    private void OnCreate()
    {
        Colores = new Dictionary<ColorEnum, Color>
        {
            { ColorEnum.PlayerRange, new Color(0, 1, 1, 1) },
            { ColorEnum.MonsterWarningRange, new Color(1, 0.63f, 0, 1) },
            { ColorEnum.MonsterAttackRange, new Color(1, 0, 0, 1) },
            { ColorEnum.PlayerMoveWarningRange, new Color(1, 1, 0, 1) },
            { ColorEnum.Orange, new Color(1, 0.5f, 0, 1) }
        };
        ColorTexts = new Dictionary<string, string>
        {
            { "#eu" ,"</u>" },
            { "#ec" ,"</color>" },
            { "#r" ,"<color=red>" },
            { "#y" ,"<color=yellow>" },
            { "#b" ,"<color=blue>" },
            { "#u" ,"<u>" },
        };
    }
    public static Color FindColor(ColorEnum color)
    {
        if (Instance.Colores.ContainsKey(color))
        {
            return Instance.Colores[color];
        }
        return Color.white;
    }
    /// <summary>
    /// 文本改色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string TextDrawColor(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        var sb = new StringBuilder(input);
        foreach(string key in Instance.ColorTexts.Keys)
        {
            sb.Replace(key, Instance.ColorTexts[key]);
        }
        return sb.ToString();
    }
}
