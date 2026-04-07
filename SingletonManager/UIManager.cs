using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗UI管理器
/// </summary>
public class UIManager : Singleton<UIManager>
{
    public enum UIEnum
    {
        CharacterControlAndInfo,
        CharacterInfo,
        ActionTimeLine,
        EnemyInfo
    }
    public CharacterControlUI characterControl;//干员控制
    public ActionTimeLineUI actionTimeLine;//时间轴UI
    public CharacterInfoUI characterInfo;//干员信息
    public WarningBillboard warningBillboard;//警告弹窗（右上）
    public EnemyInfoUI enemyInfoUI;//敌人信息
    public TeamUIContorl teamUIContorl;//队伍信息
    public GameObject debug;//Debug窗口
    public void Start()
    {
        if (characterControl == null)
        {
            characterControl = GetComponentInChildren<CharacterControlUI>();
            characterControl.gameObject.SetActive(false);
        }
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.B))
        {
            debug.SetActive(!debug.activeSelf);
        }
    }
    public void ShowUI(UIEnum uIEnum)
    {
        switch (uIEnum)
        {
            case UIEnum.CharacterControlAndInfo:
                characterControl.gameObject.SetActive(true);
                characterInfo.gameObject.SetActive(true);
                break;
            case UIEnum.CharacterInfo:
                characterInfo.gameObject.SetActive(true);
                break;
            case UIEnum.ActionTimeLine:
                actionTimeLine.gameObject.SetActive(true);
                break;
            case UIEnum.EnemyInfo:
                enemyInfoUI.gameObject.SetActive(true);
                break;
        }
    }
    public void HideUI(UIEnum uIEnum)
    {
        switch (uIEnum)
        {
            case UIEnum.CharacterControlAndInfo:
                characterControl.gameObject.SetActive(false);
                characterInfo.gameObject.SetActive(false);
                break;
            case UIEnum.CharacterInfo:
                characterInfo.gameObject.SetActive(false);
                break;
            case UIEnum.ActionTimeLine:
                actionTimeLine.gameObject.SetActive(false);
                break;
            case UIEnum.EnemyInfo:
                enemyInfoUI.gameObject.SetActive(false);
                break;
        }
    }
    public void OnClickCharacter(CharacterBase character)
    {
        characterControl.OnClickCharacter(character);
        characterInfo.OnClickCharacter(character);
    }
    public void OnClickEnemy(EnemyBase enemy)
    {
        enemyInfoUI.OnClickEnemy(enemy.GetInfoToUI());
    }
}
