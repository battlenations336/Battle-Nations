using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugTextController : MonoBehaviour
{
    public Text DebugText;

    public void Start()
    {
        DebugText.text = string.Empty;
    }

    public void ShowDebug(bool _show)
    {
        gameObject.SetActive(_show);
    }
}
