using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SelectControl
{
    /// <summary>
    /// 瞬发技能-给予自身buff
    /// </summary>
    public class SkillSelectControl_PowerToSelf : ObjectSelectControl
    {
        byte skillindex;
        public override void OnInit(object select)
        {
            base.OnInit(select);
            UIManager.Instance.characterControl.SetButtonInteractable(false, true);
        }
        public override void SubInit(params object[] obj)
        {
            if (obj.Length == 0)
            {
                Debug.Log("传入了空的技能信息");
                return;
            }
            base.SubInit(obj);
            SkillUIInfo skill = (SkillUIInfo)obj[0];
            skillindex = skill.index;
            UIManager.Instance.actionTimeLine.ShowAddTime(skill.Usetime);
            MapManager.Instance.skillrangeCells = skill.Range;
            MapManager.Instance.UpdateRange("skill");
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
                case "Player":
                    Debug.Log("PlayerBufSkillToSelf");
                    ((GameObject)Select).GetComponent<CharacterBase>().UsingSkill(skillindex, null);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                    GameApp.Instance.FinishSelect();
                    break;
            }
            UIManager.Instance.characterControl.CancelChoose();
            MapManager.Instance.skillrangeCells = null;
            MapManager.Instance.UpdateRange("monster");
        }
    }
    /// <summary>
    /// 技能-选取方向（上下左右），同时会截取攻击范围
    /// </summary>
    public class SkillSelectControl_DirectionSelection : ObjectSelectControl
    {
        byte skillindex;
        Vector2Int predirection;
        Vector2Int direction = Vector2Int.zero;
        Vector2Int pos;
        HashSet<Vector2Int> orginrange;
        public override void OnInit(object select)
        {
            base.OnInit(select);
            Vector3 charpos = ((GameObject)select).transform.position;
            pos = new Vector2Int((int)charpos.x, (int)charpos.y);
            UIManager.Instance.characterControl.SetButtonInteractable(false, true);
            CameraManager.Instance.RayAction += UpdateRay;
        }
        void UpdateRay(RaycastHit hitInfo)
        {
            if (hitInfo.transform.tag == "BackGround")
            {
                Vector2 delta = new Vector2(hitInfo.point.x, hitInfo.point.y) - pos;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
                }
                else
                {
                    direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
                }
                if (predirection == null || predirection != direction)
                {
                    HashSet<Vector2Int> mapmpos = new HashSet<Vector2Int>();
                    foreach (var range in MathHelper.RotateRange(orginrange, direction))
                    {
                        mapmpos.Add(range + pos);
                    }
                    MapManager.Instance.skillrangeCells = mapmpos;
                    MapManager.Instance.UpdateRange("skill");
                    predirection = direction;
                }
            }
        }

        public override void SubInit(params object[] obj)
        {
            if (obj.Length == 0)
            {
                Debug.Log("传入了空的技能信息");
                return;
            }
            base.SubInit(obj);
            SkillUIInfo skill = (SkillUIInfo)obj[0];
            skillindex = skill.index;
            UIManager.Instance.actionTimeLine.ShowAddTime(skill.Usetime);
            orginrange = new HashSet<Vector2Int>();
            foreach (var range in skill.Range)
            {
                orginrange.Add(range - pos);
            }
            MapManager.Instance.skillrangeCells = skill.Range;
            MapManager.Instance.UpdateRange("skill");
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
                default:
                    Debug.Log("PlayerSkillDirection");
                    ((GameObject)Select).GetComponent<CharacterBase>().UsingSkill(skillindex, direction);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                    GameApp.Instance.FinishSelect();
                    break;
            }
            MapManager.Instance.skillrangeCells = null;
            MapManager.Instance.UpdateRange("monster");
            UIManager.Instance.characterControl.CancelChoose();
            CameraManager.Instance.RayAction -= UpdateRay;
        }
    }
    /// <summary>
    /// 瞬发技能-回复干员生命
    /// </summary>
    public class SkillSelectControl_Heal : ObjectSelectControl
    {
        byte skillindex;
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
            SkillUIInfo skill = (SkillUIInfo)obj[0];
            skillindex = skill.index;
            UIManager.Instance.actionTimeLine.ShowAddTime(skill.Usetime);
            MapManager.Instance.skillrangeCells = skill.Range;
            MapManager.Instance.UpdateRange("skill");
        }
        void RayIn(RaycastHit hit)
        {
            if (hit.transform.gameObject.tag is "Player")
            {
                hit.transform.GetComponent<IRoundQueneObject>().SelectRing.SetActive(true);
            }
        }
        void RayOut(RaycastHit hit)
        {
            if (hit.transform.gameObject.tag is "Player")
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
                case "Player":
                    Debug.Log("PlayerHeal");
                    ((GameObject)Select).GetComponent<CharacterBase>().UsingSkill(skillindex, obj);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    UIManager.Instance.HideUI(UIManager.UIEnum.CharacterControlAndInfo);
                    GameApp.Instance.FinishSelect();
                    break;
            }
            MapManager.Instance.skillrangeCells = null;
            MapManager.Instance.UpdateRange("monster");
            UIManager.Instance.characterControl.CancelChoose();
            CameraManager.Instance.RayIn -= RayIn;
            CameraManager.Instance.RayOut -= RayOut;
        }
    }
}