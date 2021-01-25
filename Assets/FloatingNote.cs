
using BNR;
using System;
using TMPro;
using UnityEngine;

public class FloatingNote : MonoBehaviour
{
    private float timeToLive = 2f;
    private float speed = 1f;
    private EventHandler onComplete;
    public SpriteRenderer Icon;
    public TextMeshPro Text;
    public AudioSource audio;
    private float offset;
    private TextPosition textPosition;
    private Vector3 textOffset;

    public void Play(float speed, string txt, int size, TextPosition _textPosition)
    {
        this.textPosition = _textPosition;
    }

    public void SetDuration(float duration)
    {
        this.timeToLive = duration;
    }

    public void SetSpeed(float _speed)
    {
        this.speed = _speed;
    }

    public void SetSound(string soundfile)
    {
        if (!(bool)(UnityEngine.Object)this.audio)
            return;
        AudioClip clip = UnityEngine.Resources.Load<AudioClip>("Audio/" + soundfile);
        if (!((UnityEngine.Object)clip != (UnityEngine.Object)null))
            return;
        this.audio.PlayOneShot(clip, 0.7f);
    }

    public void SetText(string txt, int size)
    {
        this.SetText(txt);
        this.Text.fontSize = (float)size;
    }

    public void SetPosition(TextPosition _textPosition, int row)
    {
        switch (_textPosition)
        {
            case TextPosition.Centre:
                this.transform.position += new Vector3(0.0f, -0.1f * (float)(row + 1), 0.0f);
                break;
            case TextPosition.Left:
                this.transform.position += new Vector3(-0.2f, -0.2f * (float)(row + 1), 0.0f);
                break;
            case TextPosition.Right:
                this.transform.position += new Vector3(0.2f, -0.3f * (float)(row + 1), 0.0f);
                break;
            default:
                this.textOffset = Vector3.zero;
                break;
        }
    }

    public void SetText(string txt, int size, TextPosition _textPosition)
    {
        this.SetText(txt);
        this.Text.fontSize = (float)size;
        this.SetPosition(_textPosition, 0);
    }

    public void SetText(string txt)
    {
        this.Text.text = txt;
        this.Text.sortingOrder = 152;
        if (!((UnityEngine.Object)this.Icon != (UnityEngine.Object)null))
            return;
        this.Icon.sortingOrder = 152;
    }

    public void SetColour(Color _colour)
    {
        this.Text.faceColor = (Color32)_colour;
    }

    public void SetSize(int size)
    {
        this.Text.fontSize = (float)size;
    }

    private void Update()
    {
        if (!this.GetPosition())
        {
            this.Completed();
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        }
        else
        {
            if ((double)this.timeToLive - 1.20000004768372 > 0.0)
                return;
            Color color;
            if ((UnityEngine.Object)this.Icon != (UnityEngine.Object)null)
            {
                color = this.Icon.color;
                color.a -= 0.08f;
                this.Icon.color = color;
            }
            color = this.Text.color;
            color.a -= 0.02f;
            this.Text.color = color;
        }
    }

    public bool GetPosition()
    {
        if ((double)(this.timeToLive -= Time.deltaTime) <= 0.0)
            return false;
        this.transform.position += new Vector3(0.0f, Time.deltaTime * this.speed, 0.0f);
        return true;
    }

    public EventHandler OnComplete
    {
        get
        {
            return this.onComplete;
        }
        set
        {
            this.onComplete = value;
        }
    }

    public void Completed()
    {
        if (this.OnComplete == null)
            return;
        this.OnComplete((object)this, new EventArgs());
    }
}
