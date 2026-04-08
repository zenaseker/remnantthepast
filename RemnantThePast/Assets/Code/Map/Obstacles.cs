using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//暂时没用上
public class Obstacles : MonoBehaviour, IBlocked
{
    public void Start()
    {
        Vector2Int now = new Vector2Int((int)this.transform.position.x, (int)(this.transform.position.y - 0.13f));
        MapManager.Instance.GetTile(now).UnitInGrid = this;
    }
}
