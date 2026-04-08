using System.Collections.Generic;
using UnityEngine;

public class TimeDestory : Singleton<TimeDestory>
{
    public enum EndExecute
    {
        DisActive,
        Destroy,
        PushPool
    }
    public class DestoryItem
    {
        public DestoryItem(GameObject obj, float time, EndExecute endExecute)
        {
            this.obj = obj;
            this.time = time;
            this.endExecute = endExecute;
        }

        public GameObject obj {  get; set; }
        public float time { get; set; }
        public EndExecute endExecute { get; set; }
    }
    public List<DestoryItem> Objs = new List<DestoryItem>();
    public void Add(GameObject obj,float time, EndExecute endExecute)
    {
        Objs.Add(new DestoryItem(obj,time,endExecute));
    }
    protected override void OnAwake()
    {
        base.OnAwake();
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
    public void FixedUpdate()
    {
        if (Objs.Count <= 0) return;
        for (int i = 0; i < Objs.Count;)
        {
            DestoryItem iem = Objs[i];
            iem.time -= Time.fixedDeltaTime;
            if (iem.time <= 0)
            {
                switch (iem.endExecute)
                {
                    case EndExecute.DisActive:
                        iem.obj.SetActive(false);
                        break;
                    case EndExecute.Destroy:
                        GameObject.Destroy(iem.obj);
                        break;
                    case EndExecute.PushPool:
                        PoolManage.Instance.PushGameObject(iem.obj);
                        break;
                }
                Objs.RemoveAt(i);
            }
            else i++;
        }
    }

}
