using SelectControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 关卡管理器
/// </summary>
public class GameApp : Singleton<GameApp>
{
    protected override void OnAwake()
    {
        base.OnAwake();
        if (MusicManage.Instance == null)
        {
            GameObject.Instantiate(Resources.Load("Prefab/UI/MusicManager"));
        }
        if (TimeDestory.Instance == null)
        {
            GameObject.Instantiate(Resources.Load("Prefab/UI/TimeDestory"));
        }
#if UNITY_EDITOR
        if (!DataLibrary.Instance.Informationed)
        {
            DataLibrary.LoadLibrary();
        }
#endif
    }
    public void Update()
    {
        CheckEventAction();
        _selectObj.text = objectSelect != null ? $"当前选择控制：{objectSelect.GetType().Name}" : "无选择控制";
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseClick();
        }
        if (Input.GetMouseButtonDown(1))
        {
            InputSelect(null, "Cancel");
        }
        objectSelect?.Update();
    }
    #region 干员信息
    public List<CharacterBase> characterList = new List<CharacterBase>();
    public List<CharacterBase> battlecharacterList = new List<CharacterBase>();
    /// <summary>
    /// 添加干员
    /// </summary>
    /// <param name="character"></param>
    public void AddCharacter(CharacterBase character)
    {
        if (!characterList.Contains(character))
        {
            characterList.Add(character);
            battlecharacterList.Add(character);
            UIManager.Instance.teamUIContorl.AddChar(character);
        }
    }
    /// <summary>
    /// 移除干员
    /// </summary>
    /// <param name="character"></param>
    public void RemoveCharacter(CharacterBase character)
    {
        if (characterList.Contains(character))
        {
            characterList.Remove(character);
            UIManager.Instance.teamUIContorl.RemoveChar(character.CharacterInfo.ID);
        }
    }
    /// <summary>
    /// 干员死亡
    /// </summary>
    /// <param name="character"></param>
    public void CharacterDead(CharacterBase character)
    {
        if (battlecharacterList.Contains(character))
        {
            battlecharacterList.Remove(character);
            UIManager.Instance.teamUIContorl.ChangeCharState(character.CharacterInfo.ID,TeamCharUI.StatusType.Dead);
        }
    }
    #endregion

    #region 行为处理
    public TextMeshProUGUI _Eventactions;
    public List<IEventAction> eventActions = new List<IEventAction>();
    public void CheckEventAction()
    {
        StringBuilder sb = new StringBuilder("当前进程:");
        if (TimeManager.Instance._EventState == TimeManager.EventState.Animationexecution)
        {
            if (eventActions.Count > 0)
            {
                List<IEventAction> remove = new List<IEventAction>(eventActions.Count);
                foreach (IEventAction action in eventActions)
                {
                    if (action.IsFinished)
                    {
                        remove.Add(action);
                    }
                    else
                    {
                        sb.Append(action.ID).Append(",");
                    }
                }
                eventActions.RemoveAll(x => x.IsFinished);
                foreach (IEventAction action in remove)
                {
                    PoolManage.Instance.Return(action);
                }
            }
            else
            {
                TimeManager.Instance.EventCallBack();
            }
        }
        _Eventactions.text = sb.ToString();
    }
    /// <summary>
    /// 添加并行行为，如果列表中没有行为则开启行为状态循环
    /// </summary>
    /// <param name="ID"></param>
    public void AddEvent(string ID,IRoundQueneObject owner)
    {
        if (TimeManager.Instance._EventState == TimeManager.EventState.Waitinginstruction && eventActions.Count == 0)
        {
            TimeManager.Instance.EventCallBack();
        }
        IEventAction action = PoolManage.Instance.GetClass<IEventAction>();
        action.Init(ID,owner);
        eventActions.Add(action);
    }
    /// <summary>
    /// 移除某单位正在执行的意图
    /// </summary>
    /// <param name="target"></param>
    public void ClearActionByOnce(IRoundQueneObject target)
    {
        eventActions.RemoveAll(x => x.original == target);
    }
    /// <summary>
    /// 声明某个行为已经完成
    /// </summary>
    /// <param name="ID"></param>
    public void SetEventTrue(string ID)
    {
        if (eventActions.Count > 0)
        {
            IEventAction action = eventActions.Find(x => x.ID == ID && !x.IsFinished);
            if (action != null)
            {
                action.IsFinished = true;
                return;
            }
        }
        Debug.Log("未能寻找到事件" + ID);
    }
    #endregion

    #region 点击处理
    public ObjectSelectControl objectSelect;
    public GameObject _nowSelect;
    public TextMeshProUGUI _selectObj;
    public void OnMouseClick()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;//点在UI上
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))//发射射线
        {
            if (hitInfo.transform.tag == "BackGround")//点击地板只会向现有控制器传输信息
            {
                float roundedX = Mathf.Round(hitInfo.point.x);
                float roundedY = Mathf.Round(hitInfo.point.y);
                float xFraction = hitInfo.point.x - Mathf.Floor(hitInfo.point.x);
                float yFraction = hitInfo.point.y - Mathf.Floor(hitInfo.point.y);
                if (Mathf.Abs(xFraction - 0.5f) < Mathf.Epsilon)
                {
                    roundedX = Mathf.Floor(hitInfo.point.x);
                }
                if (Mathf.Abs(yFraction - 0.5f) < Mathf.Epsilon)
                {
                    roundedY = Mathf.Floor(hitInfo.point.y);
                }
                InputSelect(new Vector2Int((int)roundedX, (int)roundedY), hitInfo.transform.tag);
            }
            else
            {
                SetSelectObj(hitInfo.transform.gameObject, hitInfo.transform.tag);
            }
        }
    }
    /// <summary>
    /// 更新控制器
    /// </summary>
    /// <param name="control">控制器</param>
    /// <param name="obj">初始控制目标</param>
    public void SetSelectControl(ObjectSelectControl control, object obj)
    {
        objectSelect = control;
        objectSelect?.Init(obj);
    }
    public void SetSelectControl(ObjectSelectControl control, object obj, params object[] subobj)
    {
        objectSelect = control;
        objectSelect?.Init(obj);
        objectSelect?.SubInit(subobj);
    }
    public void SetSelectObj(object obj,string Type)
    {
        Debug.Log("监测到输入：" + obj.ToString() + "类型为" + Type);
        if (objectSelect == null)
        {
            switch (Type)
            {
                case "Player":
                    if (!TimeManager.Instance.PlayerCanControl()) return;
                    SetSelectControl(DataLibrary.Instance.GetSelectControl("CharacterDefaultSelectControl"), obj);
                    _nowSelect = (GameObject)obj;
                    _nowSelect.GetComponent<IRoundQueneObject>().SelectRing.SetActive(true);
                    break;
                case "Monster":
                    _nowSelect = (GameObject)obj;
                    SetSelectControl(DataLibrary.Instance.GetSelectControl("MonsterSelectControl"), obj);
                    break;
                case "Facilities":
                    UIManager.Instance.warningBillboard.Warning("这是一个设施");
                    break;
            }
        }
        else
        {
            objectSelect.OnSelect(obj,Type);
        }
    }
    /// <summary>
    /// 向控制器输入信息
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="type"></param>
    public void InputSelect(object obj, string type)
    {
        objectSelect?.OnSelect(obj, type);
    }
    /// <summary>
    /// 结束控制
    /// </summary>
    public void FinishSelect()
    {
        _nowSelect = null;
        objectSelect = null;
    }
    #endregion

    #region 战斗处理
    public TextMeshProUGUI CombatUnits;
    public List<IRoundQueneObject> CombatCharacterList = new List<IRoundQueneObject>();//干员列表
    public List<IRoundQueneObject> CombatMonsterList = new List<IRoundQueneObject>();//敌人列表
    public IRoundQueneObject NowRound;//正在执行的行动单位列表
    /// <summary>
    /// 添加战斗单位
    /// </summary>
    /// <param name="obj"></param>
    public void AddCombatUnit(IRoundQueneObject obj)
    {
        if (TimeManager.Instance._GameState == TimeManager.GameState.Explore)//调整为战斗状态
        {
            TimeManager.Instance.SetGameState(TimeManager.GameState.Combat);
            TimeManager.Instance.SetBattleState(TimeManager.BattleState.Continue);
            UIManager.Instance.actionTimeLine.InCombat(true);
        }
        if (obj.IsPlayer)
        {
            CombatCharacterList.Add(obj);
        }
        else
        {
            CombatMonsterList.Add(obj);
            obj.InCombat = true;
        }
        obj.OnIntoCombat();
        ReWriteCombatUnit();
    }
    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartCombat()
    {
        TimeManager.Instance.SetBattleState(TimeManager.BattleState.Player);
        InputCharacter();
    }
    #region 候补角色操作
    private void InputCharacter()//放置未上场的干员
    {
        // 进入战斗时，放置剩余队友
        if (battlecharacterList.Count <= 0)
        {
            Debug.Log("战斗系统中没有干员");
            return;
        }
        var main = battlecharacterList.First();
        AddCombatUnit(main);
        Vector2Int center = main.GetGameObject().GetComponent<PlayerControl>().CurrentGrid;

        List<Vector2Int> allPositions = new List<Vector2Int>();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                if (MapManager.Instance.GrilCanTo(pos)) allPositions.Add(pos);
            }
        }

        Vector2Int forwardDir = main.GetGameObject().GetComponent<PlayerControl>().face;
        Vector2Int backDir = -forwardDir;
        Vector2Int leftDir = new Vector2Int(-forwardDir.y, forwardDir.x);
        Vector2Int rightDir = new Vector2Int(forwardDir.y, -forwardDir.x);

        List<Vector2Int> frontPositions = new List<Vector2Int>(3);
        List<Vector2Int> backPositions = new List<Vector2Int>(3);
        List<Vector2Int> sidePositions = new List<Vector2Int>(3);

        foreach (var pos in allPositions)
        {
            Vector2Int delta = pos - center;
            int dot = forwardDir.x * delta.x + forwardDir.y * delta.y;
            if (dot == 1)
                frontPositions.Add(pos);
            else if (dot == -1)
                backPositions.Add(pos);
            else
                sidePositions.Add(pos);
        }
        var remainingAllies = battlecharacterList.Where(c => !c.InCombat).ToList();
        List<Vector2Int> usedPositions = new List<Vector2Int>();

        var frontAllies = remainingAllies.Where(a => frontProfessions.Contains(a.CharacterInfo._Profession)).ToList();
        var backAllies = remainingAllies.Where(a => backProfessions.Contains(a.CharacterInfo._Profession)).ToList();
        var sideAllies = remainingAllies.Except(frontAllies).Except(backAllies).ToList();

        PlaceAlliesToPositions(frontAllies, frontPositions, usedPositions);
        PlaceAlliesToPositions(backAllies, backPositions, usedPositions);
        PlaceAlliesToPositions(sideAllies, sidePositions, usedPositions);

        if (remainingAllies.Count > usedPositions.Count)
        {
            Debug.LogWarning($"可放置格子不足：需要 {remainingAllies.Count} 个，实际放置 {usedPositions.Count} 个");
        }
    }

    readonly CharacterInfo.Profession[] frontProfessions = new CharacterInfo.Profession[3] { CharacterInfo.Profession.Pioneer, CharacterInfo.Profession.Tank, CharacterInfo.Profession.Warrior };
    readonly CharacterInfo.Profession[] backProfessions = new CharacterInfo.Profession[3] { CharacterInfo.Profession.Caster, CharacterInfo.Profession.Medic, CharacterInfo.Profession.Sniper };

    /// <summary>
    /// 将队友列表放置到可用格点列表（按顺序分配，每个格点只放一个）
    /// </summary>
    private void PlaceAlliesToPositions(List<CharacterBase> allies, List<Vector2Int> positions, List<Vector2Int> usedPositions)
    {
        for (int i = 0; i < allies.Count && i < positions.Count; i++)
        {
            if (usedPositions.Contains(positions[i])) continue;
            CharacterBase ally = allies[i];
            Vector2Int targetPos = positions[i];
            AddCombatUnit(ally);
            ally.GetComponent<PlayerControl>().InputIntoMap(targetPos);
            usedPositions.Add(targetPos);
        }
    }
    void RetreatAlternateInMap()
    {
        for(int i = 0;i < characterList.Count;i++)
        {
            if (i == 0) 
            {
                UIManager.Instance.teamUIContorl.ChangeCharState(characterList[i].CharacterInfo.ID, TeamCharUI.StatusType.MainControl);
                continue;
            }
            characterList[i].GetComponent<PlayerControl>().RetreatInMap();
        }
    }
    #endregion
    /// <summary>
    /// 战斗结束
    /// </summary>
    public void EndCombat()
    {
        foreach(IRoundQueneObject action in CombatCharacterList)
        {
            action.OnExitCombat();
        }
        CombatCharacterList.Clear();
        CombatMonsterList.Clear();
        TimeManager.Instance.SetBattleState(TimeManager.BattleState.Uninitialized);
        TimeManager.Instance.SetGameState(TimeManager.GameState.Explore);
        UIManager.Instance.actionTimeLine.InCombat(false);
        RetreatAlternateInMap();
    }
    //行为结束后调用
    public void UpdateCombat()
    {
        Debug.Log("UpdateCombat");
        if (CombatCharacterList.Count <= 0 || CombatMonsterList.Count <= 0)
        {
            EndCombat();
            ReWriteCombatUnit();
            return;
        }
        TimeManager.Instance.SetBattleState(TimeManager.BattleState.Player);
    }
    /// <summary>
    /// 单位死亡或离开战斗
    /// </summary>
    /// <param name="exitObject"></param>
    public void UnitExitCombat(IRoundQueneObject exitObject)
    {
        if (exitObject.IsPlayer)
        {
            CombatCharacterList.Remove(exitObject);
        }
        else
        {
            CombatMonsterList.Remove(exitObject);
            UIManager.Instance.actionTimeLine.RemoveAllAction(x => x.action.unit == exitObject);
        }
        ReWriteCombatUnit();
    }
    /// <summary>
    /// Debug显示战斗单位
    /// </summary>
    void ReWriteCombatUnit()
    {
        StringBuilder stringBuilder = new StringBuilder("战斗单位:");
        foreach (var item in CombatCharacterList)
        {
            stringBuilder.Append(((CharacterBase)item).CharacterInfo.Name).Append(",");
        }
        stringBuilder.Append("|");
        foreach (var item in CombatMonsterList)
        {
            stringBuilder.Append(((EnemyBase)item).MonsterInfo.Name).Append(",");
        }
        CombatUnits.text = stringBuilder.ToString();
    }
    #endregion
}
