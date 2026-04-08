using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuUI
{
    /// <summary>
    /// 主菜单初始化信息
    /// </summary>
    public class InitalizationManager : MonoBehaviour
    {
        void Awake()
        {
            if (MusicManage.Instance == null)
            {
                GameObject.Instantiate(Resources.Load("Prefab/UI/MusicManager"));
            }
            if (TimeDestory.Instance == null)
            {
                GameObject.Instantiate(Resources.Load("Prefab/UI/TimeDestory"));
            }
            DataLibrary.LoadLibrary();
        }
        public void ToBattle()
        {
            DataLibrary.Instance.MapID = "0-01";
            SceneManager.LoadScene("Battle");
            MusicManage.Instance.StopAllEffect();

        }
    }
}
