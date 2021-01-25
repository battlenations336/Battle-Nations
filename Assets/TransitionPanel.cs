using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPanel : MonoBehaviour
{
    public EventHandler OnComplete { get; set; }

    public Vector3 StartPos;
    public Vector3 EndPos;

    float Speed = 500.0f;
    bool offsetComplete;
    int direction;
    bool active = false;

    void Start()
    {

    }

    void Update()
    {
        if (active)
        {
            if (direction < 0)
                GetPositionDown();
            else
                GetPositionUp();
        }
    }

    public void Play()
    {
        active = true;
        offsetComplete = false;
        if (transform.position.y > EndPos.y)
            direction = -1;
        else
            direction = 1;
    }

    void GetPositionDown()
    {
        float delta = Time.deltaTime * Speed;

        if (transform.position.y > EndPos.y)
        {
            if (transform.position.y - delta < EndPos.y)
                transform.position = EndPos;
            else
                transform.position -= new Vector3(0, delta, 0);
        }
        else
        {
            active = false;
            if (OnComplete != null)
                OnComplete(this, new EventArgs());
        }
    }

    void GetPositionUp()
    {
        float delta = Time.deltaTime * Speed;

        if (transform.position.y < EndPos.y)
        {
            if (transform.position.y + delta > EndPos.y)
                transform.position = EndPos;
            else
                transform.position += new Vector3(0, delta, 0);
        }
        else
        {
            active = false;
            if (OnComplete != null)
                OnComplete(this, new EventArgs());
        }
    }
}
