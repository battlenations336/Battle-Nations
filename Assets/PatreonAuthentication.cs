using BNR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class PatreonAuthentication : DialogPanel
{
    public EventHandler OnClose;

    public Text Title;
    public Text Message;

    public GameObject ButtonObject;
    GameObject part_prefab;    

    Button OKButton;

    public Patreon patreon;

    enum PatreonPhase
    {
        Waiting,
        Connected,
        Success,
        Failure
    }

    public new void Open()
    {
        base.Open();

        Title.text = "Patreon Authentication";

        /*part_prefab = (GameObject)Resources.Load("DialogButton");
        part_prefab.name = "Button";

        ButtonObject = Instantiate(part_prefab);
        ButtonObject.GetComponentInChildren<Text>().text = "Connect";
        ButtonObject.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        ButtonObject.transform.SetParent(Container.transform, true);
        ButtonObject.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.35f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        */
        OKButton = ButtonObject.GetComponent<Button>();
        OKButton.onClick.AddListener(ConnectToPatreon);
        
    }

    public void ConnectToPatreon()
    {
        OKButton.onClick.RemoveAllListeners();

        Message.text = "Waiting for website response ...";

        if (patreon != null)
        {
            patreon.onConnect = PatreonConnected;
            patreon.onError = PatreonError;

            patreon.connect();
        }
        else
        {
            Debug.Log("Patreon settings not found");
        }
    }

    public void PatreonConnected(string text)
    {
        Debug.Log(text);
        MenuConfig.Patreon = false;
        if (patreon.hasReward("5") || patreon.hasReward("15"))
        {
            Message.text = "Patreon reward found.";
            MenuConfig.Patreon = true;
            if (patreon.rewards.Count > 0)
                Debug.Log(string.Format("User {0}, Reward {1}", patreon.email, patreon.rewards[0]));
        }
        else
        {
            Debug.Log(string.Format("User {0}, No Rewards", patreon.email));
            Message.text = "No currently active Patreon reward found.";
        }
        ButtonObject.GetComponentInChildren<Text>().text = "Continue";
        OKButton.onClick.AddListener(CloseDialog);
    }

    public void PatreonError(string text)
    {
        Debug.Log(text);

        Message.text = text;

        ButtonObject.GetComponentInChildren<Text>().text = "Continue";
        OKButton.onClick.AddListener(CloseDialog);
    }

    public void CloseDialog()
    {
        if (OnClose != null)
            OnClose(this, new EventArgs());
    }
}
