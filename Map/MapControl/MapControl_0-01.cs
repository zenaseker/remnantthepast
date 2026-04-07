using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapControl
{
    public class MapControl_0_01 : MapControlBase
    {
        public override void MapCreate()
        {
            GameObject.Instantiate(Resources.Load<GameObject>("Prefab/Map/LockDoor"),MapManager.Instance.transform.GetChild(0))
                .transform.position = new Vector3(-2, 0.13f, 0);
            MusicManage.Instance.ChangeBGM("music/Dia_calamity/Dia_calamity_intro", false);
            MusicManage.Instance.LaterBGM("music/Dia_calamity/Dia_calamity_loop", true);
        }
    }
}
