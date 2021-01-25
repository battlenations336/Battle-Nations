
using BNR;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PasswordEntry : DialogPanel
{
    public GameObject ErrorPanel;
    public Text ErrorMessage;
    private GameObject value;
    private GameObject button;
    private GameObject part_prefab;
    public AuthenticationView authenticationView;
    public Text Name;
    public InputField Password;
    public Button CancelButton;
    public Button OKButton;

    public new void Open()
    {
        base.Open();
        this.part_prefab = (GameObject)UnityEngine.Resources.Load("DialogText");
        this.part_prefab.name = "Title";
        this.value = UnityEngine.Object.Instantiate<GameObject>(this.part_prefab);
        this.value.GetComponent<Text>().text = "Enter password";
        this.value.transform.SetParent(this.Container.transform);
        this.value.transform.position = new Vector3(this.getOrigin().x + this.getWidth() * 0.25f, (float)((double)this.getOrigin().y + (double)this.getHeight() * 0.5 - (double)this.getMargin_V() * 0.25), 0.0f);
        this.part_prefab = (GameObject)UnityEngine.Resources.Load("DialogButton");
        this.part_prefab.name = "Button";
        this.button = UnityEngine.Object.Instantiate<GameObject>(this.part_prefab);
        this.button.GetComponentInChildren<Text>().text = "Cancel";
        this.button.transform.SetParent(this.Container.transform);
        this.button.transform.position = new Vector3(this.getOrigin().x + this.getMargin_H() * 0.75f, this.getOrigin().y + this.getMargin_V() * 0.25f, 0.0f);
        this.CancelButton = this.button.GetComponent<Button>();
        this.button = UnityEngine.Object.Instantiate<GameObject>(this.part_prefab);
        this.button.GetComponentInChildren<Text>().text = "OK";
        this.button.transform.SetParent(this.Container.transform, true);
        this.button.transform.position = new Vector3(this.getOrigin().x + this.getWidth() * 0.35f, this.getOrigin().y + this.getMargin_V() * 0.25f, 0.0f);
        this.OKButton = this.button.GetComponent<Button>();
        this.OKButton.onClick.AddListener((UnityAction)(() => this.Login(this.Name.text)));
    }

    public void SetError(string message)
    {
        this.ErrorPanel.SetActive(true);
        this.ErrorMessage.text = message;
    }

    public void ClearError()
    {
        this.ErrorPanel.SetActive(false);
        this.ErrorMessage.text = string.Empty;
    }

    private void Login(string loginName)
    {
        this.ClearError();
        GameData.Player.Name = loginName;
        AccountSelectionList accountSelectionList = GameData.AccountSelectionList.Where<AccountSelectionList>((Func<AccountSelectionList, bool>)(x => x.accountName == loginName)).FirstOrDefault<AccountSelectionList>();
        if (accountSelectionList != null && !string.IsNullOrEmpty(accountSelectionList.accountName))
        {
            GameData.AccountSelectionList.Remove(accountSelectionList);
            GameData.AccountSelectionList.Insert(0, accountSelectionList);
            Functions.SaveAccountList();
        }
        this.authenticationView.SetAuthenticationValues(loginName, this.Password.text);
        this.authenticationView.SendLoginRequest();
    }
}
