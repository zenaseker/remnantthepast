using UnityEngine;
using Spine.Unity;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Unity.VisualScripting;
using System.Threading;

public class PlayerControl : MonoBehaviour
{
    CharacterBase CharacterBase;
    private Vector2Int PreviousGrid;
    public Vector2Int CurrentGrid
    {
        get
        {
            return PreviousGrid;
        }
    }
    public Vector2Int face = Vector2Int.right;
    

    private void Start()
    {
        CharacterBase = GetComponent<CharacterBase>();
        PreviousGrid = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.y);
    }
    #region 移动
    public void MoveTo(Vector2Int move)
    {
        Debug.Log($"接收到点击在{move}");
        if (!MapManager.Instance.GrilCanTo(move))
        {
            Debug.Log($"目标位置{move}是不可进入的");
            //return;
        }
        if (PreviousGrid == move) return;
        List<Vector2Int> poses = MapManager.Instance.FindPath(PreviousGrid, move);
        if (poses == null) return;
        List<Vector3> moveposes = new List<Vector3>();
        foreach (Vector2Int po in poses)
        {
            moveposes.Add(new Vector3(po.x, po.y));
        }
        StartCoroutine(Move(moveposes));
    }
    public IEnumerator Move(List<Vector3> vector3s)
    {
        if (vector3s.Count < 2) yield break;
        CharacterBase.Animator.SetBool("InMove", true);
        GameApp.Instance.AddEvent($"Character{CharacterBase.CharacterInfo.ID}Move", CharacterBase);
        List<Vector3> targetpos = new List<Vector3>(vector3s);
        targetpos.RemoveAt(0);
        while (targetpos.Count > 0)
        {
            Vector3 target = targetpos.First() - this.transform.position;
            if (target.x < 0) CharacterBase.SetFace(false);
            else if (target.x > 0) CharacterBase.SetFace(true);
            face = new Vector2Int((int)target.x, (int)target.y);
            var tween = this.transform.DOMove(targetpos.First(), target.magnitude).SetEase(Ease.Linear);
            targetpos.RemoveAt(0);
            yield return tween.WaitForCompletion();
            if (OnChangeGrid())
            {
                break;
            }
        }
        CharacterBase.Animator.SetBool("InMove", false);
        GameApp.Instance.SetEventTrue($"Character{CharacterBase.CharacterInfo.ID}Move");
    }
    public bool OnChangeGrid()
    {
        ActionTimeLineUI.Instance.TimeMove(0.1f);
        Vector2Int NowGrid = new Vector2Int((int)this.transform.position.x, (int)this.transform.position.y);
        MapManager.Instance.OnUnitIntoGrid(gameObject, PreviousGrid, NowGrid,out bool Stop);
        PreviousGrid = NowGrid;
        return Stop;
    }
    // 方向数组：上、右、下、左（不包含斜角）
    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };
    /// <summary>
    /// 计算单次移动所有可达格点
    /// </summary>
    /// <param name="movePower">最大移动步数</param>
    /// <returns>可达格点的集合（包含起点）</returns>
    public HashSet<Vector2Int> CalculateReachableCells()
    {
        int movePower = CharacterBase.CharacterInfo.MoveDistance;
        movePower = Mathf.Min(movePower,10);//移动距离最大为10
        movePower = Mathf.Max(movePower, 0);//最小为0
        HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
        Queue<(Vector2Int pos, int stepsLeft)> frontier = new Queue<(Vector2Int, int)>();
        reachable.Add(PreviousGrid);
        frontier.Enqueue((PreviousGrid, movePower));
        while (frontier.Count > 0)
        {
            var (currentPos, stepsLeft) = frontier.Dequeue();
            if (stepsLeft <= 0) continue;
            foreach (var dir in Directions)
            {
                Vector2Int nextPos = currentPos + dir;
                if (reachable.Contains(nextPos)) continue;
                if (MapManager.Instance.GrilCanTo(nextPos))
                {
                    reachable.Add(nextPos);
                    if (stepsLeft > 1)
                    {
                        frontier.Enqueue((nextPos, stepsLeft - 1));
                    }
                }
            }
        }
        if (MapManager.Instance.GetTile(PreviousGrid).Doors != null)
        {
            foreach(MapTile.Door door in MapManager.Instance.GetTile(PreviousGrid).Doors)
            {
                if (MapManager.Instance.mapInstances.TryGetValue(door.TargetMapId, out var map))
                {
                    Vector2Int nextPos = door.TargetMapDoorPos + map.WorldToLocalOffest;
                    reachable.Add(nextPos);
                }
            }
        }
        return reachable;
    }
    #endregion
    #region 部署
    public void InputIntoMap(Vector2Int targetPos)
    {
        transform.position = new Vector3(targetPos.x, targetPos.y);
        PreviousGrid = targetPos;
        transform.gameObject.SetActive(true);
        CharacterBase character = GetComponent<CharacterBase>();
        character.InCombat = true;
        character.Hp.ShowUI();
        character.ChangeAttackState(true);
        UIManager.Instance.teamUIContorl.ChangeCharState(character.CharacterInfo.ID, TeamCharUI.StatusType.Playing);
        Vector2Int mappos = new Vector2Int((int)transform.position.x, (int)transform.position.y) - MapManager.Instance.MainmapInstance.CenterMove;
        MapManager.Instance.MainmapInstance.GetGrid(mappos).UnitInGrid = character;
        MusicManage.Instance.PlayEffect($"voice/btl_snd_1/b_char_set");
        MusicManage.Instance.PlayRamdonEffect($"voice/{character.CharacterInfo.Icon}/CN_023", $"voice/{character.CharacterInfo.Icon}/CN_024");
    }

    public void RetreatInMap()
    {
        transform.gameObject.SetActive(false);
        CharacterBase character = GetComponent<CharacterBase>();
        character.InCombat = false;
        character.Hp.HideUI();
        character.ChangeAttackState(false);
        GameApp.Instance.ClearActionByOnce(character);
        UIManager.Instance.teamUIContorl.ChangeCharState(character.CharacterInfo.ID, TeamCharUI.StatusType.NotPlaying);
        Vector2Int mappos = new Vector2Int((int)transform.position.x, (int)transform.position.y) - MapManager.Instance.MainmapInstance.CenterMove;
        MapManager.Instance.MainmapInstance.GetGrid(mappos).UnitInGrid = null;
    }
    #endregion
}
