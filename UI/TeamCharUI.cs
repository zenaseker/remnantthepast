using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class TeamCharUI : MonoBehaviour
{
    public int id;
    public Image Icon;
    public Image Hp;
    public Image Sp;
    public Image Status;
    public GameObject Dead;
    
    public void AddChar(CharacterBase @char)
    {
        CharacterInfoUI.CharacterInfoToUI infoToUI = @char.GetInfoToUI();
        id = @char.CharacterInfo.ID;
        Icon.sprite = infoToUI.icon;
        Hp.fillAmount = (float)infoToUI.hp.Item1 / (float)infoToUI.hp.Item2;
        Sp.fillAmount = (float)infoToUI.sp.Item1 / (float)infoToUI.sp.Item2 / 2;
        @char.HpChangeAction += ChangeHp;
        @char.SpChangeAction += ChangeSp;
    }
    public void OnClick()
    {
        CharacterBase Char = GameApp.Instance.characterList.Find(x => x.CharacterInfo.ID == id);
        if (Char != null)
        {
            UIManager.Instance.OnClickCharacter(Char);
            UIManager.Instance.ShowUI(UIManager.UIEnum.CharacterInfo);
        }
    }
    public void ChangeCharState(StatusType statusType)
    {
        switch (statusType)
        {
            case StatusType.NotPlaying:
                Status.gameObject.SetActive(false);
                break;
            case StatusType.Playing:
                Status.gameObject.SetActive(true);
                Status.sprite = PoolManage.Instance.GetSprite("icon/Team information");
                break;
            case StatusType.MainControl:
                Status.gameObject.SetActive(true);
                Status.sprite = PoolManage.Instance.GetSprite("icon/Team information_master control");
                break;
            case StatusType.Dead:
                Status.gameObject.SetActive(false);
                Dead.gameObject.SetActive(true);
                break;
        }
    }
    public void ChangeHp(int delta,float percen)
    {
        Hp.fillAmount = percen;
    }
    public void ChangeSp(float delta, float percen)
    {
        Sp.fillAmount = percen / 2;
    }
    public enum StatusType
    {
        NotPlaying,//未上场
        Playing,//上场非主控
        MainControl,//上场主控
        Dead//死亡
    }
}
