using BNR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCardCtrl : MonoBehaviour
{
    public EventHandler OnComplete { get; set; }

    public string ShortName;
    public Image Border;
    public Text Name;
    public Image PremiumIcon;
    public Image MainIcon;
    public Image LockIcon;
    public Image TypeIcon;
    public Text Line1;
    public Text Line2;
    public Text Line3;
    public int Id;

    float Speed = 5000f; //2500.0f;
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float Offset;

    bool active = false;
    bool offsetComplete;
    int direction;

    public void InitFromComposition(string config, Composition comp)
    {
        if (GameData.IsBuildingPremium(config))
        {
            Border.color = new Color(61f / 255f, 159f / 255f, 11f / 255f, 1);
            PremiumIcon.gameObject.SetActive(true);
        }
        else
        {
            Border.color = Functions.GetColor(207f, 79f, 7f, 204f);
        }
        if (GameData.IsBuildingLocked(config))
        {
            LockIcon.sprite = Resources.Load<Sprite>("UI/lock_icon@2x");
        }
    }

    void Start()
    {
        //transform.position = StartPos;
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

    public void AutoComplete()
    {
        transform.position = EndPos;
        if (OnComplete != null)
            OnComplete(this, new EventArgs());
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
            {
                transform.position = EndPos;
                //CompleteMove();
            }
            else
                transform.position -= new Vector3(delta, 0, 0);
        }
        else
        {
            //CompleteMove();
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
            {
                transform.position = EndPos;
                //CompleteMove();
            }
            else
                transform.position += new Vector3(delta, 0, 0);
        }
        else
        {
            //CompleteMove();
        }

        if (transform.position.x > (StartPos.x + Offset) && !offsetComplete)
        {
            if (OnComplete != null)
                OnComplete(this, new EventArgs());
            offsetComplete = true;
        }
    }

    void CompleteMove()
    {
        active = false;
        if (OnComplete != null)
            OnComplete(this, new EventArgs());
    }
}
