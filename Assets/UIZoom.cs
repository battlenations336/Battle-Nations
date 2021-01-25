using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIZoom : MonoBehaviour, IScrollHandler

{
    private Vector3 initialScale;

    [SerializeField]
    private float zoomSpeed = 0.1f;

    [SerializeField]
    private float maxZoom = 10f;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Start1()
    {
        var img = GetComponent<Image>();

        img.rectTransform.position = new Vector3(-400f, 200f, 0);
        img.rectTransform.localScale = new Vector3(1.06f, 1.06f, 1.06f);
    }

    public void OnScroll(PointerEventData eventData)
    {
        var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
        var desiredScale = transform.localScale + delta;

        desiredScale = ClampDesiredScale(desiredScale);
        transform.localScale = desiredScale;
    }



    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(initialScale, desiredScale);
        desiredScale = Vector3.Min(initialScale * maxZoom, desiredScale);

        return desiredScale;
    }
}