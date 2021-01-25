using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxCtrl : MonoBehaviour
{
    public GameObject ModalDialog;
    public Text MessageTxt;

    private static MessageBoxCtrl modalPanel;

    float waitTime;
    float maxTime = 3.0f;

    private void Update()
    {
        float delta = Time.deltaTime;

        waitTime += delta;
        if (waitTime > maxTime)
            ClosePanel();
    }

    public static MessageBoxCtrl Instance()
    {
        if (!modalPanel)
        {
            modalPanel = FindObjectOfType<MessageBoxCtrl>() as MessageBoxCtrl;
            if (!modalPanel)
                Debug.Log("No active modal panel found");
        }

        return (modalPanel);
    }

    public void Show(string _messageText = "")
    {
        if (_messageText != string.Empty)
            MessageTxt.text = _messageText;

        ModalDialog.SetActive(true);

        waitTime = 0.0f;
    }

    public void ClosePanel()
    {
        MessageTxt.text = string.Empty;
        ModalDialog.SetActive(false);
    }
}
