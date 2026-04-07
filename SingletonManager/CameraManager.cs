using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// 相机管理器
/// </summary>
public class CameraManager : Singleton<CameraManager>
{
    public PlayerControl Player;
    public Vector3 MapCenter = Vector2.zero;//地图中心
    public Vector2 MapSize = Vector2.zero;//地图大小
    public Vector3 StartPos = Vector2.zero;//初始位置

    void Start()
    {
        CalculateAndSetInitialPosition();
    }
    private void Update()
    {
        transform.position = ClampCameraPosition();
    }
    void CalculateAndSetInitialPosition()
    {
        float baseLength = MapManager.Instance.mapMaxY - MapManager.Instance.mapMinY;
        MapSize = new Vector2(MapManager.Instance.mapMaxX - MapManager.Instance.mapMinX, baseLength);
        MapCenter = new Vector3((MapManager.Instance.mapMaxX + MapManager.Instance.mapMinX) / 2, (MapManager.Instance.mapMaxY + MapManager.Instance.mapMinY) / 2);
        if(baseLength > 10)
        {
            //如果地图纵长大于10，修改为中点计算
            baseLength = 10;
            this.transform.position = StartPos = new Vector3(MapCenter.x, MapCenter.y - 10, -baseLength);
        }
        else
        {
            //以地图最底端计算
            this.transform.position = StartPos = new Vector3(MapCenter.x, MapManager.Instance.mapMinY, -baseLength);
        }
    }

    // 计算相机位置
    public Vector3 ClampCameraPosition()
    {
        if (Player == null) return StartPos;
        Vector3 PlayerCenterDis = Player.transform.position - MapCenter;
        float z = transform.position.z;//相机高度
        Vector3 targetPos = StartPos + PlayerCenterDis;
        if (MapSize.y <= z || MapSize.x <= z * 2)//过小地图
        {
            targetPos = StartPos;
        }
        else
        {
            if (targetPos.y < MapManager.Instance.mapMinY)
            {
                targetPos.y = MapManager.Instance.mapMinY;
            }
            if (targetPos.y + z > MapManager.Instance.mapMaxY)
            {
                targetPos.y = MapManager.Instance.mapMaxY - z;
            }
            if (targetPos.x - z < MapManager.Instance.mapMinX)
            {
                targetPos.x = MapManager.Instance.mapMinX + z;
            }
            if (targetPos.x + z > MapManager.Instance.mapMaxX)
            {
                targetPos.x = MapManager.Instance.mapMaxX - z;
            }
        }
        return targetPos;
    }
}
