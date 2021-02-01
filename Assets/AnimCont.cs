using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimCont : MonoBehaviour
{
    EventHandler onComplete;
    public int cellNo;
    public Text AmountTxt;
    public Image Icon;

    public EventHandler OnComplete
    {
        get { return onComplete; }
        set { onComplete = value; }
    }

    public void Completed()
    {
        if (OnComplete != null)
            OnComplete(this, new EventArgs());
    }

    public void SetText(string text)
    {
        AmountTxt.text = text;
    }
}
