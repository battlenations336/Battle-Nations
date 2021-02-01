using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    float wRatio;
    float hRatio;

    void Awake()
    {
        GameObject go = GameObject.Find("Background");
        SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();

        float cameraHeight = Camera.main.orthographicSize * 2;
        Vector2 cameraSize = new Vector2(Camera.main.aspect * cameraHeight, cameraHeight);
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        wRatio = cameraSize.x / spriteSize.x;
        hRatio = cameraSize.y / spriteSize.y;
        Vector2 scale = transform.localScale;

        if (hRatio < wRatio)
        { // Landscape (or equal)
            scale *= wRatio;
        }
        else
        { // Portrait
            scale *= hRatio;
        }

        //transform.position = Vector2.zero; // Optional
        //transform.localScale = scale;
        Camera.main.orthographicSize *= 1.0f/wRatio;
    }

    public float GetRatio()
    {
        if (hRatio < wRatio)
            return (wRatio);

        return (hRatio);
    }
}
