using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
    public GameObject Container;
    public int Rows;
    public int Columns;

    Sprite CL_DialogTL;
    Sprite CL_DialogTR;
    Sprite CL_DialogBL;
    Sprite CL_DialogBR;
    Sprite CL_DialogL;
    Sprite CL_DialogR;
    Sprite CL_DialogT;
    Sprite CL_DialogB;
    Sprite CL_DialogC;

    GameObject prefab;

    Camera MainCamera;

    Vector3 centre;
    Vector3 origin;

    public void Open()
    {
        float x, y;

        MainCamera = Camera.main;

        centre = MainCamera.ViewportToScreenPoint(MainCamera.rect.center);

        LoadResources();

        prefab = (GameObject)Resources.Load("DialogPanel");
        prefab.name = "Dialog";

        origin = new Vector3(centre.x - (getWidth() * 0.25f), centre.y - (getHeight() * 0.25f), 0);

        x = origin.x + (CL_DialogTL.rect.width * 0.25f);
        y = origin.y + (CL_DialogBL.rect.height * 0.5f) + (CL_DialogTL.rect.height * 0.25f) + (CL_DialogL.rect.height * Rows * 0.5f);
        addPanel(CL_DialogTL, x, y);

        x = origin.x + (CL_DialogTL.rect.height * 0.5f) + (CL_DialogTR.rect.width * 0.25f) + (CL_DialogT.rect.width * Columns * 0.5f);
        y = origin.y + (CL_DialogBL.rect.height * 0.5f) + (CL_DialogTR.rect.height * 0.25f) + (CL_DialogL.rect.height * Rows * 0.5f);
        addPanel(CL_DialogTR, x, y);

        x = origin.x + (CL_DialogBL.rect.width * 0.25f);
        y = origin.y + (CL_DialogBL.rect.height * 0.25f);
        addPanel(CL_DialogBL, x, y);

        x = origin.x + (CL_DialogBL.rect.width * 0.5f) + (CL_DialogBR.rect.width * 0.25f) + (CL_DialogT.rect.width * Columns * 0.5f);
        y = origin.y + (CL_DialogBR.rect.height * 0.25f);
        addPanel(CL_DialogBR, x, y);

        for (int k = 1; k <= Columns; k++)
        {
            x = origin.x + (CL_DialogBL.rect.width * 0.5f) + (CL_DialogT.rect.width * (k - 1) * 0.5f) + (CL_DialogT.rect.width * 0.25f);
            y = origin.y + (CL_DialogBR.rect.height * 0.5f) + (CL_DialogL.rect.height * Rows * 0.5f) + (CL_DialogT.rect.height * 0.25f);
            addPanel(CL_DialogT, x, y);

            y = origin.y + (CL_DialogB.rect.height * 0.25f);
            addPanel(CL_DialogB, x, y);
        }

        for (int k = 1; k <= Rows; k++)
        {
            x = origin.x + (CL_DialogL.rect.width * 0.25f);
            y = origin.y + (CL_DialogBL.rect.height * 0.5f) + (CL_DialogL.rect.height * (k - 1) * 0.5f) + (CL_DialogL.rect.height * 0.25f);
            addPanel(CL_DialogL, x, y);

            x = origin.x + (CL_DialogL.rect.width * 0.5f) + (CL_DialogT.rect.width * Columns * 0.5f) + +(CL_DialogR.rect.width * 0.25f);
            addPanel(CL_DialogR, x, y);
        }

        for (int k = 1; k <= Columns; k++)
        {
            for (int j = 1; j <= Rows; j++)
            {
                x = origin.x + (CL_DialogL.rect.width * 0.5f) + (CL_DialogC.rect.width * (k - 1) * 0.5f) + (CL_DialogC.rect.width * 0.25f);
                y = origin.y + (CL_DialogBL.rect.height * 0.5f) + (CL_DialogC.rect.height * (j - 1) * 0.5f) + (CL_DialogC.rect.height * 0.25f);
                addPanel(CL_DialogC, x, y);
            }
        }
    }

    void LoadResources()
    {
        CL_DialogTL = Resources.Load<Sprite>("UI/CL_dialogTL@2x") as Sprite;
        CL_DialogTR = Resources.Load<Sprite>("UI/CL_dialogTR@2x") as Sprite;
        CL_DialogBL = Resources.Load<Sprite>("UI/CL_dialogBL@2x") as Sprite;
        CL_DialogBR = Resources.Load<Sprite>("UI/CL_dialogBR@2x") as Sprite;

        CL_DialogL = Resources.Load<Sprite>("UI/CL_dialogL@2x") as Sprite;
        CL_DialogR = Resources.Load<Sprite>("UI/CL_dialogR@2x") as Sprite;
        CL_DialogT = Resources.Load<Sprite>("UI/CL_dialogT@2x") as Sprite;
        CL_DialogB = Resources.Load<Sprite>("UI/CL_dialogB@2x") as Sprite;

        CL_DialogC = Resources.Load<Sprite>("UI/CL_dialogC@2x") as Sprite;
    }

    void addPanel(Sprite _sprite, float x, float y)
    {
        GameObject createImage = Instantiate(prefab);
        Image img = createImage.GetComponentInChildren<Image>();
        RectTransform rectTrans = createImage.GetComponentInChildren<RectTransform>();
        //rectTrans.sizeDelta = new Vector2(_sprite.rect.width / 2, _sprite.rect.height / 2);

        img.sprite = _sprite;
        createImage.transform.SetParent(Container.transform);

        Vector3 V3 = new Vector3(x, y, 0);
        //V3 = MainCamera.ScreenToWorldPoint(V3);
        createImage.transform.position = V3;
    }

    public float getWidth()
    {
        float width = 0;

        width = CL_DialogTL.rect.width + CL_DialogTR.rect.width + (CL_DialogT.rect.width * Columns);

        return (width);
    }

    public float getHeight()
    {
        float height = 0;

        height = CL_DialogTL.rect.height + CL_DialogTR.rect.height + (CL_DialogL.rect.height * Rows);

        return (height);
    }

    public float getMargin_H()
    {
        return (CL_DialogL.rect.width);
    }

    public float getMargin_V()
    {
        return (CL_DialogT.rect.height);
    }

    public Vector3 getOrigin()
    {
        return (origin);
    }

    public Vector3 getCentre()
    {
        return (centre);
    }
}
