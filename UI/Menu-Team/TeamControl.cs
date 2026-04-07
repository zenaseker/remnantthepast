using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MenuUI
{
    public class TeamControl : MonoBehaviour
    {
        int nowTeamindex = -1;
        public TeamCharitemUI[] teamCharBoxes;
        public void OnEnable()
        {
            DrawTeam();
        }
        void DrawTeam()
        {
            for (int i = 0; i < DataLibrary.Instance.Save.team.roleIds.Length; i++)
            {
                teamCharBoxes[i].Init(DataLibrary.Instance.Save.team.roleIds[i]);
            }
        }

        public void OnClickTeam(int index)
        {
            nowTeamindex = index;
            AllCharacterSelect.Instance.Using(SelectCharToTeam, AllCharacterSelect.Filterby.NotInTeam);
        }

        public void CancelToManu()
        {
            gameObject.SetActive(false);
        }

        public void SelectCharToTeam(int id)
        {
            DataLibrary.Instance.Save.team.roleIds[nowTeamindex] = id;
            nowTeamindex = -1;
            DataLibrary.Instance.SaveGame();
            DrawTeam();
        }
    }


}