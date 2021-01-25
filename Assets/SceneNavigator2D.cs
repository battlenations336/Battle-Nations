/**
 * 2D camera panning and scrolling. Always clamped to given container.
 */

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SceneNavigator2D : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public Camera Camera;

    /// <summary>
    /// The container that we cannot break out of (usually a background image)
    /// </summary>
    public SpriteRenderer Container;

    /// <summary>
    /// Panning speed
    /// </summary>
    public float PanSpeed = 5.0f;

    /// <summary>
    /// Whether we can pan horizontically (left + right)
    /// </summary>
    public bool PanHorizontal = true;

    /// <summary>
    /// Whether we can vertically horizontically (up + down)
    /// </summary>
    public bool PanVertical = true;

    /// <summary>
    /// Zoom speed. During zooming, the camera size is adjusted.
    /// </summary>
    public float ZoomSpeed = 6.0f;

    /// <summary>
    /// The smallest height of the camera view area when zooming in
    /// </summary>
    public float MinSize = 1;

    /// <summary>
    /// The greatest height of the camera view area when zooming out
    /// </summary>
    public float MaxSize = 10;

    bool panning = false;

    void Start()
    {
        Camera = Camera ?? Camera.main;
    }

    void Update()
    {
        //Pan();
        Zoom();
    }

    void LateUpdate()
    {
        // ClampToContainer();
    }

    public void OnPointerDown(PointerEventData data)
    {
        panning = true;
    }

    public void OnPointerUp(PointerEventData data)
    {
        panning = false;
    }

    void Pan()
    {
        // compute movement
        float dx = 0, dy = 0;
        if (panning && PanHorizontal)
        {
            dx = Input.GetAxis("Mouse X") * PanSpeed * Time.deltaTime;
        }
        if (panning && PanVertical)
        {
            dy = Input.GetAxis("Mouse Y") * PanSpeed * Time.deltaTime;
        }

        transform.Translate(dx, dy, 0);
    }

    void Zoom()
    {
        var newSize = Camera.orthographicSize + Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
        newSize = Mathf.Clamp(newSize, MinSize / 2, MaxSize / 2);
        Camera.orthographicSize = newSize;
    }

    void ClampToContainer()
    {
        if (Container != null)
        {
            // clamp to container bounds
            var vertExtent = Camera.orthographicSize;
            var horzExtent = vertExtent * Screen.width / Screen.height;
            var pos = transform.position;
            var containerMin = Container.bounds.min;
            var containerMax = Container.bounds.max;
            float dx = 0, dy = 0;

            // compute camera bounds
            var camMin = pos;
            camMin.x -= horzExtent;
            camMin.y -= vertExtent;

            var camMax = pos;
            camMax.x += horzExtent;
            camMax.y += vertExtent;

            // clamp horizontically
            Debug.Log(string.Format("CamMin: {0}. containerMin:{1}", camMin.x, containerMin.x));
            if (camMin.x < containerMin.x)
            {
                dx = containerMin.x - camMin.x;
            }
            if (camMax.x > containerMax.x)
            {
                dx = containerMax.x - camMax.x;
            }

            // clamp vertically
            if (camMin.y < containerMin.y)
            {
                dy = containerMin.y - camMin.y;
            }
            if (camMax.y > containerMax.y)
            {
                dy = containerMax.y - camMax.y;
            }

            transform.Translate(dx, dy, 0);
        }
    }
}