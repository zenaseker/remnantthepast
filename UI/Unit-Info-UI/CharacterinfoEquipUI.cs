using SaveLoad;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterinfoEquipUI : MonoBehaviour
{
    public Image Lights;
    public Image Icons;
    public TextMeshProUGUI Names;

    public void Load(EquipmentData equipments)
    {
        if (false && DataLibrary.Instance.equipinfos.TryGetValue(equipments.id, out EquipInfo equipInfos))
        {
            Icons.gameObject.SetActive(true);
            Icons.sprite = equipInfos.sprite;
            Names.text = equipInfos.Name;
            Lights.color = equipInfos.GetLight();
        }
        else
        {
            Icons.gameObject.SetActive(false);
            Names.text = "Î´×°±¸";
            Lights.color = Color.white;
        }
    }
}