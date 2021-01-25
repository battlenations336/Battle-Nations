using System;
using UnityEngine;
using UnityEngine.UI;

public class WaitForMatchCtrl : MonoBehaviour
{
    public EventHandler OnTimeout { get; set; }
    public EventHandler OnCancel { get; set; }

    public GameObject ProgressBar;
    public Button ButtonCancel;
    public GameObject ModalPanelObject;
    public GameObject ModalDialog;

    private static WaitForMatchCtrl modalPanel;

    float waitTime;
    float maxTime = 120.0f;

    bool active = false;

    private void Update()
    {
        float delta = Time.deltaTime;

        if (active)
        {
            waitTime += delta;
            UpdateProgressBar();
            if (waitTime > maxTime)
            {
                active = false;
                Timeout();
            }
        }
    }

    public static WaitForMatchCtrl Instance()
    {
        if (!modalPanel)
        {
            modalPanel = FindObjectOfType<WaitForMatchCtrl>() as WaitForMatchCtrl;
            if (!modalPanel)
                Debug.Log("No active modal panel found");
        }

        return (modalPanel);
    }

    public void Show()
    {
        ModalPanelObject.SetActive(true);
        ModalDialog.SetActive(true);

        ButtonCancel.onClick.RemoveAllListeners();
        ButtonCancel.onClick.AddListener(btnCancel_OnClick);

        waitTime = 0.0f;
        active = true;
    }

    public void btnCancel_OnClick()
    {
        if (OnCancel != null)
            OnCancel(this, new EventArgs());

        ClosePanel();
    }

    public void Timeout()
    {
        if (OnTimeout != null)
            OnTimeout(this, new EventArgs());

        ClosePanel();
    }

    public void ClosePanel()
    {
        active = false;
        ModalDialog.SetActive(false);
        ModalPanelObject.SetActive(false);
    }

    public void UpdateProgressBar()
    {
        float percent = waitTime / maxTime;

        if (percent > 1.0f)
            percent = 1.0f;

        if (ProgressBar != null)
            ProgressBar.transform.localScale = new Vector3(1 - percent, 1);
    }
}
