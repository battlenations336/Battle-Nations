
using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{
    public Text Available;
    public Text Total;
    public Image BackgroundImage;
    public Image UnitImage;
    public Image Promote;

    public void SetUnitImage(Sprite _sprite)
    {
        this.UnitImage.sprite = _sprite;
        this.UnitImage.SetNativeSize();
        RectTransform transform = this.gameObject.transform as RectTransform;
        Vector2 localScale = (Vector2)transform.localScale;
        Rect rect1 = transform.rect;
        double height1 = (double)rect1.height;
        rect1 = _sprite.rect;
        double height2 = (double)rect1.height;
        float num1 = (float)(height1 / height2);
        Rect rect2 = transform.rect;
        double width1 = (double)rect2.width;
        rect2 = _sprite.rect;
        double width2 = (double)rect2.width;
        float num2 = (float)(width1 / width2);
        this.UnitImage.transform.localScale = (Vector3)(((double)num1 <= (double)num2 ? localScale * num1 : localScale * num2) / (Vector2)this.BackgroundImage.transform.localScale * (Vector2)new Vector3(0.8f, 0.8f, 1f));
    }

    public void SetBackgroundImage(Sprite _sprite)
    {
        if ((Object)_sprite != (Object)null)
        {
            this.BackgroundImage.color = new Color((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue);
            this.BackgroundImage.sprite = _sprite;
            this.BackgroundImage.SetNativeSize();
            RectTransform transform = this.gameObject.transform as RectTransform;
            Vector2 localScale = (Vector2)transform.localScale;
            Rect rect = transform.rect;
            double height1 = (double)rect.height;
            rect = _sprite.rect;
            double height2 = (double)rect.height;
            float num1 = (float)(height1 / height2);
            rect = transform.rect;
            double width1 = (double)rect.width;
            rect = _sprite.rect;
            double width2 = (double)rect.width;
            float num2 = (float)(width1 / width2);
            this.BackgroundImage.transform.localScale = (Vector3)((double)num1 <= (double)num2 ? localScale * num1 : localScale * num2);
        }
        else
            this.BackgroundImage.sprite = _sprite;
    }
}
