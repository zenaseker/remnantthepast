using UnityEngine;

namespace SelectControl
{
    public class MonsterSelectControl : ObjectSelectControl
    {
        public override void OnInit(object select)
        {
            base.OnInit(select);
            ((GameObject)select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(true);
            UIManager.Instance.OnClickEnemy(((GameObject)select).GetComponent<EnemyBase>());
            UIManager.Instance.ShowUI(UIManager.UIEnum.EnemyInfo);
            MapManager.Instance.UpdateOneEnemyRange(((GameObject)select).GetComponent<IRoundQueneObject>());
        }
        public override void OnSelect(object obj, string type)
        {
            base.OnSelect(obj, type);
            switch (type)
            {
                case "Cancel":
                    UIManager.Instance.HideUI(UIManager.UIEnum.EnemyInfo);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    MapManager.Instance.UpdateRange("monster");
                    GameApp.Instance.FinishSelect();
                    break;
                case "Player":
                    UIManager.Instance.HideUI(UIManager.UIEnum.EnemyInfo);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    MapManager.Instance.UpdateRange("monster");
                    GameApp.Instance.FinishSelect();
                    GameApp.Instance.SetSelectObj(obj,type);
                    break;
                case "Monster":
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    Select = obj;
                    UIManager.Instance.OnClickEnemy(((GameObject)Select).GetComponent<EnemyBase>());
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(true);
                    MapManager.Instance.UpdateOneEnemyRange(((GameObject)Select).GetComponent<IRoundQueneObject>());
                    break;
                case "BackGround":
                    UIManager.Instance.HideUI(UIManager.UIEnum.EnemyInfo);
                    ((GameObject)Select).GetComponent<IRoundQueneObject>().SelectRing.SetActive(false);
                    MapManager.Instance.UpdateRange("monster");
                    GameApp.Instance.FinishSelect();
                    break;
            }
        }
    }
}