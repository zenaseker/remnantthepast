using Spine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RedDoor : MonoBehaviour
{
    public Animator Animator;
    public void OnCreate(Vector2Int pos)
    {
        MapManager.Instance.AddGridAction(pos, PlayerIn, PlayerOut);
    }
    public void PlayerIn(GameObject player)
    {
        Animator.Play("RedDoor");
        if (TimeManager.Instance._GameState == TimeManager.GameState.Combat)
        {
            UIManager.Instance.warningBillboard.Warning("«Îª˜∞‹ £”‡µ–»À");
            return;
        }
        GameObject.Instantiate(Resources.Load("Prefab/UI/BattleEnd")).GetComponent<BattleEnd>().GoAnimator(true,() => {
            MusicManage.Instance.StopAllEffect();
            SceneManager.LoadScene("Menu");
        });

    }
    public void PlayerOut(GameObject player)
    {
        Animator.Play("RedDoor 2");
    }

}
