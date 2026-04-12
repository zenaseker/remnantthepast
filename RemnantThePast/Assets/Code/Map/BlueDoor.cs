using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蓝门
/// </summary>
public class BlueDoor : MonoBehaviour
{
    public void OnCreate(Vector2Int pos)
    {
        for (int i = 0; i < DataLibrary.Instance.Save.team.roleIds.Length; i++)
        {
            int teamid = DataLibrary.Instance.Save.team.roleIds[i];
            if (teamid != 0)
            {
                RoleData roledata = DataLibrary.Instance.Save.unlockedRoles.Find(x => x.roleId == teamid);
                if (roledata == null) { Debug.Log("干员" + teamid + "的存档信息未找到"); continue; }
                PoolManage.Instance.GetPoolGameObject("Char", "Player", transform.position, Quaternion.identity)
                    .GetComponent<CharacterBase>().Init(roledata, i == 0);//只有第一个干员会显示，其余在生成时隐藏
            }

        }
    }
}
