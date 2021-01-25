using BNR;
using UnityEngine;
using UnityEngine.UI;

public class RotateCtrl : MonoBehaviour
{
    public Image Image;
    float Speed = 20f;
    bool active = false;
    float rotateAmount;

    public void Play()
    {
        rotateAmount = 0;
        active = true;
    }

    public void InitBurst(bool isLevelUp)
    {
        if (isLevelUp)
            Image.sprite = GameData.GetIcon("starburst@2x");
        else
            Image.sprite = GameData.GetIcon("starburstBlue@2x");
    }

    void Update()
    {
        if (active)
        {
            RotateImage();
        }
    }

    void RotateImage()
    {       
        float delta = Time.deltaTime * Speed;

        rotateAmount += delta;
        transform.Rotate(new Vector3(0, 0, delta));
    }
}
