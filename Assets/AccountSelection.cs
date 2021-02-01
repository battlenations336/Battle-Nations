using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountSelection : DialogPanel
{
    public EventHandler<ButtonEventArgs> OnAccountSelected { get; set; }

    GameObject title;
    GameObject part_prefab;
    GameObject accountButton;
    GameObject accountGrid;

    public AuthenticationView authenticationView;
    public Button CreateButton;
    public Button OtherButton;

    RectTransform rectTrans;
    List<GameObject> buttonList;
    ScrollRect scrollRect;

    public new void Open()
    {
        buttonList = new List<GameObject>();
        GameData.AccountSelectionList = new List<AccountSelectionList>();
        GameData.LoadAccounList();

        base.Open();

        part_prefab = (GameObject)Resources.Load("DialogText");
        part_prefab.name = "Title";

        title = Instantiate(part_prefab);
        title.GetComponent<Text>().text = "Account Selection";
        title.transform.SetParent(transform);
        title.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getHeight() * 0.5f) - (getMargin_V() * 0.25f), 0);

        part_prefab = (GameObject)Resources.Load("DialogButton");
        part_prefab.name = "Other";

        title = Instantiate(part_prefab);
        title.GetComponentInChildren<Text>().text = "Other Account";
        title.transform.SetParent(transform);
        title.transform.position = new Vector3(getOrigin().x + (getMargin_H() * 0.75f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        OtherButton = title.GetComponent<Button>();

        title = Instantiate(part_prefab);
        title.GetComponentInChildren<Text>().text = "Create Account";
        title.transform.SetParent(transform);
        title.transform.position = new Vector3(getOrigin().x + (getWidth() * 0.35f), getOrigin().y + (getMargin_V() * 0.25f), 0);
        CreateButton = title.GetComponent<Button>();

        part_prefab = (GameObject)Resources.Load("ListPanel");
        part_prefab.name = "AccountSelection";
        
        title = Instantiate(part_prefab);
        title.transform.SetParent(Container.transform);
        accountGrid = GameObject.Find("Grid");
        scrollRect = title.GetComponent<ScrollRect>();

        rectTrans = title.GetComponentInChildren<RectTransform>();
        //        rectTrans.sizeDelta = new Vector2(getCentre().x - (getWidth() * 0.5f) - (getMargin_H() * 1.2f), getCentre().y - (getHeight() * 0.5f) - (getMargin_V() * 1.5f));
        rectTrans.sizeDelta = new Vector2((getWidth() * 0.4f), (getHeight() * 0.25f));
        title.transform.position = getCentre(); // new Vector3(getOrigin().x + (getWidth() * 0.25f), getOrigin().y + (getMargin_V() * 0.25f), 0);

        CreateAccountList();
    }

    public void ResetPanel()
    {
        if (gameObject.activeSelf)
        {
            StartCoroutine(ScrollToTop());
        }
    }

    System.Collections.IEnumerator ScrollToTop()
    {
        yield return new WaitForEndOfFrame();
        scrollRect.gameObject.SetActive(true);
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void CreateAccountList()
    {
        if (buttonList != null)
        {
            foreach (GameObject GO in buttonList)
            {
                Destroy(GO);
            }
            buttonList = new List<GameObject>();
        }
        if (GameData.AccountSelectionList != null && GameData.AccountSelectionList.Count > 0)
        {
            foreach (AccountSelectionList account in GameData.AccountSelectionList)
            {
                if (accountGrid != null)
                    buttonList.Add(AddAccountButton(account.accountName, rectTrans, accountGrid));
            }
        }
        
    }

    GameObject AddAccountButton(string name, RectTransform rectTrans, GameObject accountGrid)
    {
        Sprite arrow = Resources.Load<Sprite>("UI/CL_arrowSilver") as Sprite;

        part_prefab = (GameObject)Resources.Load("AccountButton");
        part_prefab.name = "AccountButton";
        accountButton = Instantiate(part_prefab);
        var button = accountButton.GetComponent<Button>() as Button;
        button.onClick.AddListener(delegate { Login(name); });

        accountButton.transform.SetParent(accountGrid.transform);
        rectTrans = accountButton.GetComponentInChildren<RectTransform>();
        //rectTrans.sizeDelta = new Vector2((getWidth() * 0.4f), rectTrans.rect.height);

        accountButton.GetComponentInChildren<Text>().text = name;
        AccountButtonController accountButtonController = accountButton.GetComponent<AccountButtonController>();
        accountButtonController.ArrowImage.sprite = arrow;

        return (accountButton);
    }

    void Login(string loginName)
    {
        if (OnAccountSelected != null)
            OnAccountSelected(this, new ButtonEventArgs(ButtonValue.OK, loginName));
    }
}
