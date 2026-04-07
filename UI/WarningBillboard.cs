using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningBillboard : MonoBehaviour
{
    public Animator Animator;
    public TextMeshProUGUI Text;
    public void Warning(string message)
    {
        Debug.Log(message);
        gameObject.SetActive(true);
        Animator.Play("ErrotShow");
        Text.text = message;
    }
}
