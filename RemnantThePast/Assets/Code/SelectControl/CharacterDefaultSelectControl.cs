using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace SelectControl
{
    /// <summary>
    /// 基础干员控制
    /// </summary>
    public class CharacterDefaultSelectControl : ObjectSelectControl
    {
        public override void OnInit(object select)
        {
            base.OnInit(select);
            UIManager.Instance.OnClickCharacter(((GameObject)select).GetComponent<CharacterBase>());
            UIManager.Instance.ShowUI(UIManager.UIEnum.CharacterControlAndInfo);
        }
        public override void OnSelect(object obj, string type)
        {
            base.OnSelect(obj, type);
            switch (type)
            {
                case "Cancel":
                    GameApp.Instance.FinishSelect();
                    UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    break;
                case "Player":
                    GameApp.Instance.FinishSelect();
                    UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    break;
                case "ToGrid":
                    GameApp.Instance.SetSelectControl(DataLibrary.Instance.GetSelectControl("CharacterDefaultSelectControl_Grid"), Select);
                    break;
                case "ToSkill":
                    SkillUIInfo info = (SkillUIInfo)obj;
                    GameApp.Instance.SetSelectControl(DataLibrary.Instance.GetSelectControl(info.skillEvent.SelectControl()), Select, obj);
                    break;
            }
        }
    }
    /// <summary>
    /// 干员控制-移动
    /// </summary>
    public class CharacterDefaultSelectControl_Grid : ObjectSelectControl
    {
        HashSet<Vector3> CanMovePos;
        GameObject effect;
        Vector3 prePos;
        public override void OnInit(object select)
        {
            base.OnInit(select);
            MapManager.Instance.UpdateRange("player", "monster");
            UIManager.Instance.characterControl.SetButtonInteractable(true, false);
            CanMovePos = new HashSet<Vector3>();
            foreach(Vector2Int pos in MapManager.Instance.reachableCells)
            {
                CanMovePos.Add(new Vector3(pos.x, pos.y));
            }
            effect = PoolManage.Instance.GetPoolGameObject("VFX", "movetarget_vfx");
            effect.transform.position = new Vector3(0, 0, 10);
            CameraManager.Instance.RayAction += UpdateRay;
        }
        void UpdateRay(RaycastHit hitInfo)
        {
            if (hitInfo.transform.tag == "BackGround")
            {
                Vector3 pos = hitInfo.point;
                pos.x = Mathf.Round(pos.x);
                pos.y = Mathf.Round(pos.y);
                pos.z = 0;
                if (prePos != pos && CanMovePos.Contains(pos))
                {
                    effect.transform.position = pos;
                    Vector3 now = ((GameObject)Select).transform.position;
                    var path = MapManager.Instance.FindPath(new Vector2Int((int)now.x, (int)now.y), new Vector2Int((int)pos.x, (int)pos.y));
                    if (path != null && path.Count > 1)
                    {
                        ActionTimeLineUI.Instance.ShowAddTime((path.Count - 1) * 0.1f);
                    }
                    else
                    {
                        ActionTimeLineUI.Instance.ShowAddTime(0);
                    }
                }
            }
        }
        public override void OnSelect(object obj, string type)
        {
            base.OnSelect(obj, type);
            switch (type)
            {
                case "Cancel":
                    GameApp.Instance.SetSelectControl(DataLibrary.Instance.GetSelectControl("CharacterDefaultSelectControl"), Select);
                    UIManager.Instance.characterControl.SetDefaultButtonInteractable();
                    break;
                case "BackGround":
                    Debug.Log("PlayerToGrid");
                    if (MapManager.Instance.reachableCells != null && MapManager.Instance.reachableCells.Contains((Vector2Int)obj))
                    {
                        ((GameObject)Select).GetComponent<PlayerControl>().MoveTo((Vector2Int)obj);
                        ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                        UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                        GameApp.Instance.FinishSelect();
                        break;
                    }
                    Debug.Log("点击目标超出移动范围");
                    return;
                default:
                    return;
            }
            MapManager.Instance.UpdateRange("monster");
            UIManager.Instance.characterControl.CancelChoose();
            PoolManage.Instance.PushGameObject(effect);
            CameraManager.Instance.RayAction -= UpdateRay;
        }
    }
    /// <summary>
    /// 干员控制-释放技能-默认(对单个敌人)
    /// </summary>
    public class CharacterDefaultSelectControl_Skill : ObjectSelectControl
    {
        SkillUIInfo skill;
        List<IRoundQueneObject> targets;
        public override void OnInit(object select)
        {
            base.OnInit(select);
            UIManager.Instance.characterControl.SetButtonInteractable(false, true);
            CameraManager.Instance.RayIn += RayIn;
            CameraManager.Instance.RayOut += RayOut;
        }
        public override void SubInit(params object[] obj)
        {
            if (obj.Length == 0)
            {
                Debug.Log("传入了空的技能信息");
                return;
            }
            base.SubInit(obj);
            skill = (SkillUIInfo)obj[0];
            UIManager.Instance.actionTimeLine.ShowAddTime(skill.Usetime);
            MapManager.Instance.skillrangeCells = skill.Range;
            MapManager.Instance.UpdateRange("skill");
        }

        void RayIn(RaycastHit hit)
        {
            if (hit.transform.gameObject.tag is "Monster")
            {
                hit.transform.GetComponent<IRoundQueneObject>().SelectRing.SetActive(true);
            }
        }
        void RayOut(RaycastHit hit)
        {
            if (hit.transform.gameObject.tag is "Monster")
            {
                hit.transform.GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
            }
        }

        public override void OnSelect(object obj, string type)
        {
            base.OnSelect(obj, type);
            switch (type)
            {
                case "Cancel":
                    GameApp.Instance.SetSelectControl(DataLibrary.Instance.GetSelectControl("CharacterDefaultSelectControl"), Select);
                    UIManager.Instance.characterControl.SetDefaultButtonInteractable();
                    UIManager.Instance.actionTimeLine.HideAddTime();
                    break;
                case "Monster":
                    Debug.Log("PlayerSkillToMonster");
                    if (!targets.Contains(((GameObject)obj).GetComponent<IRoundQueneObject>()))
                    {
                        Debug.Log("选择了技能可选敌人外的敌人");
                        return;
                    }
                    ((GameObject)Select).GetComponent<CharacterBase>().UsingSkill(skill.index,obj);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                    GameApp.Instance.FinishSelect();
                    break;
                default:
                    return;
            }
            UIManager.Instance.characterControl.CancelChoose();
            MapManager.Instance.skillrangeCells = null;
            MapManager.Instance.UpdateRange("monster");
            CameraManager.Instance.RayIn -= RayIn;
            CameraManager.Instance.RayOut -= RayOut;
        }
    }
}