using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI
{
    public class CharlIllustration : MonoBehaviour
    {
        RectTransform rectTransform;
        Image Image;
        public void Load(string spritename,int level)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }
            if (Image == null)
            {
                Image = GetComponent<Image>();
            }
            Sprite sprite = PoolManage.Instance.GetSprite("char_illustration/" + spritename + (level >= 60 ? "_2" : ""));
            if (sprite == null)
            {
                Debug.Log("干员立绘图片未找到：" + spritename);
                return;
            }
            Image.sprite = sprite;
            Image.SetNativeSize();

            Vector2 pivot = sprite.pivot / sprite.rect.size;
            Vector2 imageSize = sprite.rect.size;
            Vector2 targetPivotInContainer = new Vector2(0.5f, 0.5f);
            Vector2 offset = (targetPivotInContainer - pivot) * imageSize;
            rectTransform.anchoredPosition = offset;
        }
    }
}
