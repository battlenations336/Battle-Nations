using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButtonCtrl : MonoBehaviour
{
    public EventHandler OnComplete { get; set; }

    public Image Symbol;
    public Image Icon;
    public TextMeshProUGUI Text;
    public int Id;

    float Speed = 10000.0f;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float Offset;

    bool offsetComplete;
    int direction;
    bool active = false;

    public void Init(string symbol, string icon, string text)
    {
        Symbol.sprite = Resources.Load<Sprite>("UI/" + symbol);
        Icon.sprite = Resources.Load<Sprite>("UI/" + icon);
        Text.text = text;
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.position = StartPos;
    }

    public void Play()
    {
        active = true;
        offsetComplete = false;
        if (StartPos.x > EndPos.x)
            direction = -1;
        else
            direction = 1;
    }

    void Update()
    {
        if (active)
        {
            if (direction < 0)
                GetPositionLeft();
            else
                GetPositionRight();
        }
    }

    void GetPositionLeft()
    {
        float delta = Time.deltaTime * Speed;

        if (transform.position.x > EndPos.x)
        {
            if (transform.position.x - delta < EndPos.x)
                transform.position = EndPos;
            else
                transform.position -= new Vector3(delta, 0, 0);
        }
        else
        {
            active = false;
//            if (OnComplete != null)
//                OnComplete(this, new EventArgs());
        }

        if (transform.position.x < (StartPos.x - Offset) && !offsetComplete)
        {
            if (OnComplete != null)
                OnComplete(this, new EventArgs());
            offsetComplete = true;
        }
    }

    void GetPositionRight()
    {
        float delta = Time.deltaTime * Speed;

        if (transform.position.x < EndPos.x)
        {
            if (transform.position.x + delta > EndPos.x)
                transform.position = EndPos;
            else
                transform.position += new Vector3(delta, 0, 0);
        }
        else
        {
            active = false;
            //            if (OnComplete != null)
            //                OnComplete(this, new EventArgs());
        }

        if (transform.position.x > (StartPos.x + Offset) && !offsetComplete)
        {
            if (OnComplete != null)
                OnComplete(this, new EventArgs());
            offsetComplete = true;
        }
    }
}
