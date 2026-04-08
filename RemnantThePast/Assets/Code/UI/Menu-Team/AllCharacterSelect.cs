using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace MenuUI
{
    public class AllCharacterSelect : Singleton<AllCharacterSelect>
    {
        public enum Filterby
        {
            None,
            All,
            OnlyHas,
            NotInTeam
        }
        public Transform CharList;
        Action<int> ReturnAction;
        Filterby filterby = Filterby.None;
        public void Using(Action<int> action, Filterby filterby)
        {
            ReturnAction = action;
            this.filterby = filterby;
            gameObject.SetActive(true);
        }
        void OnEnable()
        {
            switch (filterby)
            {
                case Filterby.All:
                    foreach (CharacterInfo info in DataLibrary.Instance.characterInfos.Values)
                    {
                        PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "CharSelect", CharList).GetComponent<TeamCharSelectButton>().Init(info, this);
                    }
                    break;
                case Filterby.OnlyHas:
                    foreach (CharacterInfo info in DataLibrary.Instance.characterInfos.Values)
                    {
                        if (DataLibrary.Instance.Save.unlockedRoles.Find(x => x.roleId == info.ID) != null)
                        {
                            PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "CharSelect", CharList).GetComponent<TeamCharSelectButton>().Init(info, this);
                        }
                    }
                    break;
                case Filterby.NotInTeam:
                    PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "CharSelect", CharList).GetComponent<TeamCharSelectButton>().Init(null, this);
                    foreach (CharacterInfo info in DataLibrary.Instance.characterInfos.Values)
                    {
                        if (DataLibrary.Instance.Save.team.roleIds.Contains(info.ID)) continue;
                        PoolManage.Instance.GetPoolGameObject("UI/CharSquad", "CharSelect", CharList).GetComponent<TeamCharSelectButton>().Init(info, this);
                    }
                    break;
                default:
                    gameObject.SetActive(false);
                    break;
            }
        }
        public void OnChoose(int id)
        {
            ReturnAction?.Invoke(id);
            gameObject.SetActive(false);
        }
        public void OnDisable()
        {
            for (int i = 0; i < CharList.childCount; i++)
            {
                PoolManage.Instance.PushGameObject(CharList.GetChild(i).gameObject);
            }
            ReturnAction = null;
            filterby = Filterby.None;
        }
    }

}