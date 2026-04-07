using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpUI : MonoBehaviour
{
    RectTransform rectTransform;
    public RectTransform Hp;
    public RectTransform CurrentHp;
    public RectTransform Sp;
    Transform player;
    bool InChange = false;
    (float, float) ChangingHp = (0, 0);
    public void Init(Transform _player)
    {
        player = _player;
        rectTransform = GetComponent<RectTransform>();
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, player.position);
        rectTransform.position = screenPos;
    }
    public void HpChange(float currenthp, float lasthp)
    {
        ChangingHp = (currenthp, lasthp);
        Vector3 scale = Hp.localScale;
        scale.x = lasthp;
        Hp.localScale = scale;
        if (InChange)
        {
            CurrentHp.localScale = scale;
        }
        InChange = true;
        CurrentHp.gameObject.SetActive(true);
    }
    public void HpChange(float currenthp)
    {
        Vector3 scale = Hp.localScale;
        scale.x = currenthp;
        Hp.localScale = scale;
        CurrentHp.localScale = scale;
    }
    public void ChangeSp(float sp)
    {
        if (Sp == null)
        {
            Debug.Log("错误使用了不含SP条的UI，请检查设置");
            return;
        }
        Sp.localScale = new Vector3(sp,1,1);
    }
    public void Update()
    {
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, player.position);
        rectTransform.position = screenPos;
        if (InChange)
        {
            if (ChangingHp.Item1 > ChangingHp.Item2)
            {
                ChangingHp.Item1 -= 1f * Time.deltaTime;
                if (ChangingHp.Item1 <= ChangingHp.Item2)
                {
                    ChangingHp.Item1 = ChangingHp.Item2;
                    InChange = false;
                    CurrentHp.gameObject.SetActive(false);
                }
            }
            if (ChangingHp.Item1 < ChangingHp.Item2)
            {
                ChangingHp.Item1 += 1f * Time.deltaTime;
                if (ChangingHp.Item1 >= ChangingHp.Item2)
                {
                    ChangingHp.Item1 = ChangingHp.Item2;
                    InChange = false;
                    CurrentHp.gameObject.SetActive(false);
                }
            }
            Vector3 scale = CurrentHp.localScale;
            scale.x = ChangingHp.Item1;
            CurrentHp.localScale = scale;
        }
    }
}
