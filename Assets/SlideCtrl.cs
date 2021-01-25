using System;
using UnityEngine;

/// Lets a RectTransform fly in from one of the edges of its canvas.
/// This can be used with an animator.
public class SlideCtrl : MonoBehaviour
{
    public EventHandler OnComplete { get; set; }

    public enum Sides
    {
        Left,
        Right,
        Top,
        Bottom
    }
    /// Reference to the canvas component this RectTransform is on.
    public Canvas canvas;
    /// Reference to the rect transform component.
    public RectTransform rectTransform;
    /// <summary>
    /// Don't always slide off the screen
    /// </summary>
    public RectTransform rectTransformOffset;
    /// The side from where the rect transform should fly in.
    public Sides side;

    /// The transition factor (from 0 to 1) between inside and outside.
    [Range(0, 1)]
    public float transition;

    /// Inside is assumed to be the start position of the RectTransform.
    private Vector2 inside;
    private Vector2 globalPosition;

    /// Outside is the position
    /// where the rect transform is completely outside of its canvas on the given side.
    private Vector2 outside;

    private bool active = false;
    private float speed = 2.0f;
    private int direction = 0;

    private void Start()
    {
        // rectTransform = GetComponent<RectTransform>();
        // canvas = GetComponentInParent<Canvas>();
    }

    public void Hide()
    {
        if (IsHidden())
            return;

        // rectTransform = GetComponent<RectTransform>();
        // canvas = GetComponentInParent<Canvas>();
        active = true;
        globalPosition = GetGlobalPosition(rectTransform);
        inside = rectTransform.localPosition;
        transition = 1.0f;
        direction = -1;
        //if (side == Sides.Right)
            outside = inside + (GetDifferenceToOutside());
        //else
        //    outside = inside - new Vector2(0f, rectTransform.rect.size.y);
    }

    public void Show()
    {
        if (!IsHidden())
            return;

        // rectTransform = GetComponent<RectTransform>();
        // canvas = GetComponentInParent<Canvas>();
        active = true;
        globalPosition = GetGlobalPosition(rectTransform);
        inside = rectTransform.localPosition;
        transition = 0.0f;
        direction = 1;
        outside = inside + (GetDifferenceToInside());
    }

    Vector2 GetGlobalPosition(RectTransform trans)
    {
        // Calculate the global position of the RectTransform on the canvas
        // by summing up all local positions of parents.
        var pos = Vector3.zero;
        foreach (var parent in trans.GetComponentsInParent<RectTransform>())
        {
            if (parent.GetComponent<Canvas>() == null)
            {
                pos += parent.localPosition;
            }
            else
            {
                return pos;
            }
        }
        return pos;
    }

    void Update()
    {
        if (active && rectTransform != null)
        {
            Execute();
        }
    }

    void Execute()
    {
        transition += ((Time.deltaTime * speed) * direction);
        if (direction == -1)
            rectTransform.localPosition = Vector2.Lerp(outside, inside, transition);
        else
            rectTransform.localPosition = Vector2.Lerp(inside, outside, transition);
        if (TransitionComplete())
        {
            active = false;
            if (OnComplete != null)
                OnComplete(this, new EventArgs());
        }
    }

    bool TransitionComplete()
    {
        bool result = true;

        if (direction == -1)
        {
            if (transition > 0)
            {
                result = false;                
            }
            else
            {
                transition = 0;
            }
        }
        else
        {
            if (transition < 1)
            {
                result = false;
            }
            else
            {
                transition = 1;
            }
        }

        return (result);
    }

    public bool IsHidden()
    {
        var size = rectTransform.rect.size;
        var pivot = rectTransform.pivot;
        var canvasSize = canvas.GetComponent<RectTransform>().rect.size;
        var canvasPos = canvas.GetComponent<RectTransform>().rect.position;
        var pos = globalPosition + (canvasSize / 2.0f);

        bool hidden = false;

        switch (side)
        {
            case Sides.Top:
                var distanceToTop = canvasSize.y - pos.y;
                if (rectTransform.localPosition.y > (canvasPos.y + canvasSize.y))
                    hidden = true;
                // return new Vector2(0f, distanceToTop + size.y * (pivot.y));
                break;
            case Sides.Bottom:
                var distanceToBottom = pos.y;
                if (rectTransform.localPosition.y < canvasPos.y)
                    hidden = true;
                // return new Vector2(0f, -distanceToBottom - size.y * (1 - pivot.y));
                break;
            case Sides.Left:
                var distanceToLeft = pos.x;
                if (rectTransform.localPosition.x < canvasPos.x)
                    hidden = true;
                // return new Vector2(-distanceToLeft - size.x * (1 - pivot.x), 0f);
                break;
            case Sides.Right:
                var distanceToRight = canvasSize.x - pos.x;
                if (rectTransform.localPosition.x > (canvasPos.x + canvasSize.x))
                    hidden = true;
                // return new Vector2(distanceToRight + size.x * (pivot.x), 0f);
                break;
            // default:
                //return Vector2.zero;
        }


        return (hidden);
    }

    Vector2 GetDifferenceToOutside()
    {
        // Pixel size of this RectTransform in normal resolution
        var size = rectTransform.rect.size;
        var pivot = rectTransform.pivot;
        // Pixel size of the canvas in normal resolution
        var canvasSize = canvas.GetComponent<RectTransform>().rect.size;
        var canvasPos = canvas.GetComponent<RectTransform>().rect.position;
        // The summed up position has its origin in the center of the canvas.
        // However, in the calculation below, the position is assumed to have its origin in the lower left corner.
        // So we move the coords by canvasSize/2.
        var pos = globalPosition + (canvasSize / 2.0f);

        switch (side)
        {
            case Sides.Top:
                var distanceToTop = canvasSize.y - pos.y;
                return new Vector2(0f, distanceToTop + size.y * (pivot.y));
            case Sides.Bottom:
                var distanceToBottom = pos.y;
                var offset = 0f;
                //if (rectTransformOffset != null)
                //    offset = rectTransformOffset.rect.size.y;
                return new Vector2(0f, -distanceToBottom - size.y * (1 - pivot.y));
                //return new Vector2(0f, canvasPos.y + size.y);// * (1 - pivot.y) + (offset * (1 - pivot.y)));
            case Sides.Left:
                var distanceToLeft = pos.x;
                return new Vector2(-distanceToLeft - size.x * (1 - pivot.x), 0f);
            case Sides.Right:
                var distanceToRight = canvasSize.x - pos.x;
                return new Vector2(distanceToRight + size.x * (pivot.x), 0f);
            default:
                return Vector2.zero;
        }
    }

    Vector2 GetDifferenceToInside()
    {
        // Pixel size of this RectTransform in normal resolution
        var size = rectTransform.rect.size;
        var pivot = rectTransform.pivot;
        // Pixel size of the canvas in normal resolution
        var canvasSize = canvas.GetComponent<RectTransform>().rect.size;
        // The summed up position has its origin in the center of the canvas.
        // However, in the calculation below, the position is assumed to have its origin in the lower left corner.
        // So we move the coords by canvasSize/2.
        var pos = globalPosition + (canvasSize / 2.0f);

        switch (side)
        {
            case Sides.Top:
                var distanceToTop = canvasSize.y - pos.y;
                return new Vector2(0f, distanceToTop - size.y * (pivot.y));
            case Sides.Bottom:
                var distanceToBottom = pos.y;
                return new Vector2(0f, -distanceToBottom + size.y * (1 - pivot.y));
            case Sides.Left:
                var distanceToLeft = pos.x;
                return new Vector2(-distanceToLeft + size.x * (1 - pivot.x), 0f);
            case Sides.Right:
                var distanceToRight = canvasSize.x - pos.x;
                return new Vector2(distanceToRight - size.x * (pivot.x), 0f);
            default:
                return Vector2.zero;
        }
    }

    public void ToggleVisible()
    {
        var anim = GetComponent<Animator>();
        anim.SetBool("Hide", !anim.GetBool("Hide"));
    }
}