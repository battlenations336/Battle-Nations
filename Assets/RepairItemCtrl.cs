using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RepairItemCtrl : MonoBehaviour
{
    public Text Name;
    public Text Count;
    public Image Icon;
    public Image Locked;

    public string ConfigName;

    public void LockItem()
    {
        Color c;

        Locked.gameObject.SetActive(true);

        c = Icon.color;
        c.a = 0.3f;
        Icon.color = c;
    }
}
