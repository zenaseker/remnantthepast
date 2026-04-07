using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TeamCharUI;

public class TeamUIContorl : MonoBehaviour
{
    public void AddChar(CharacterBase character)
    {
        GameObject obj = PoolManage.Instance.GetPoolGameObject("UI", "TeamChar", this.transform);
        obj.GetComponent<TeamCharUI>().AddChar(character);
    }
    public void ChangeCharState(int id, StatusType statusType)
    {
        for (int i = 0; transform.childCount > 0; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out TeamCharUI obj))
            {
                if (obj.id == id)
                {
                    obj.ChangeCharState(statusType);
                    return;
                }
            }
        }
    }
    public void RemoveChar(int id)
    {
        for (int i = 0;transform.childCount > 0;i++)
        {
            if (transform.GetChild(i).TryGetComponent(out TeamCharUI obj))
            {
                if (obj.id == id)
                {
                    GameObject.Destroy(transform.GetChild(i).gameObject);
                    return;
                }
            }
        }
        Debug.Log("未找到对应id的干员");
    }
}
