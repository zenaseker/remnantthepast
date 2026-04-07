using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeSpeedUI : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public void SetSpeed(float speed)
    {
        speed *= 2;
        Time.timeScale = speed;
        Text.text = speed.ToString();
    }
}
