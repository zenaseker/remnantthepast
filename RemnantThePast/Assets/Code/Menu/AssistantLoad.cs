using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI
{
    /// <summary>
    /// 主菜单助理加载
    /// </summary>
    public class AssistantLoad : MonoBehaviour
    {
        public Transform AssistantTas;
        public Image LeftBg;
        public Image RightBg;
        public string BGM;
        public bool nointro;
        public string Assistant;
        public string Bg;
        AssistantClick assistant;
        public void Awake()
        {
            LeftBg.sprite = Resources.Load<Sprite>($"wrapper/{Bg}/{Bg}_left");
            RightBg.sprite = Resources.Load<Sprite>($"wrapper/{Bg}/{Bg}_right");
            assistant = GameObject.Instantiate(Resources.Load("Prefab/CharPack/" + Assistant), AssistantTas.transform)
                .GetComponent<AssistantClick>();
        }
        private void Start()
        {
            if (BGM != null)
            {
                if (nointro)
                {
                    MusicManage.Instance.ChangeBGM($"music/{BGM}/{BGM}_loop", true);
                }
                else
                {
                    MusicManage.Instance.ChangeBGM($"music/{BGM}/{BGM}_intro", false);
                    MusicManage.Instance.LaterBGM($"music/{BGM}/{BGM}_loop", true);
                }
            }
        }
        public void OnClick()
        {
            assistant?.OnClick();
        }
    }
}
