
using BNR;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    public AccountSelection AccountSelectionCtrl;
    public CreateAccount CreateAccountCtrl;
    public AuthenticationView authenticationView;
    public AccountEntry AccountEntry;
    public PasswordEntry PasswordEntry;
    public MOTD MOTD;
    public PatreonAuthentication PatreonAuthentication;
    public AppError AppError;

    private void Awake()
    {
        if ((UnityEngine.Object)MainMenu.instance == (UnityEngine.Object)null)
        {
            MainMenu.instance = this;
        }
        else
        {
            if (!((UnityEngine.Object)MainMenu.instance != (UnityEngine.Object)this))
                return;
            UnityEngine.Object.Destroy((UnityEngine.Object)this.gameObject);
        }
    }

    private void Start()
    {
        this.StartingScreen();
        this.CreateAccountCtrl.Open();
        this.CreateAccountCtrl.CancelButton.onClick.AddListener(new UnityAction(this.OnBack_Clicked));
        this.CreateAccountCtrl.OKButton.onClick.AddListener(new UnityAction(this.OnOK_Clicked));
        //this.AccountEntry.Open(); --THIS NEEDS TO BE FIXED
        //this.AccountEntry.CancelButton.onClick.AddListener(new UnityAction(this.OnBack_Clicked));
        this.PasswordEntry.Open();
        this.PasswordEntry.CancelButton.onClick.AddListener(new UnityAction(this.OnBack_Clicked));
        this.MOTD.Open();
        this.MOTD.OKButton.onClick.AddListener(new UnityAction(this.OnAcceptMessage_Clicked));
        this.PatreonAuthentication.OnClose += new EventHandler(this.OnPatreonConfirmed_Clicked);
        this.AppError.Open();
        this.AppError.OKButton.onClick.AddListener(new UnityAction(this.OnAcceptError_Clicked));
    }

    private void StartingScreen()
    {
        if (MenuConfig.Error != null && MenuConfig.Error != string.Empty)
        {
            this.AppError.gameObject.SetActive(true);
            this.AppError.SetErrorMessage(MenuConfig.Error);
            this.MOTD.gameObject.SetActive(false);
            this.AccountSelectionCtrl.gameObject.SetActive(false);
        }
        else if (!MenuConfig.MTOD)
        {
            this.MOTD.gameObject.SetActive(true);
            this.PatreonAuthentication.gameObject.SetActive(false);
            this.AccountSelectionCtrl.gameObject.SetActive(false);
        }
        else if (!MenuConfig.Patreon)
        {
            this.MOTD.gameObject.SetActive(false);
            this.PatreonAuthentication.gameObject.SetActive(true);
            this.AccountSelectionCtrl.gameObject.SetActive(false);
        }
        else
        {
            this.AccountSelectionCtrl.Open();
            this.AccountSelectionCtrl.gameObject.SetActive(true);
            this.MOTD.gameObject.SetActive(false);
            this.PatreonAuthentication.gameObject.SetActive(false);
        }
        this.CreateAccountCtrl.gameObject.SetActive(false);
        this.PasswordEntry.gameObject.SetActive(false);
    }

    private void AccountSelectionCtrl_OnSelect(object sender, ButtonEventArgs e)
    {
        this.AccountSelectionCtrl.gameObject.SetActive(false);
        this.PasswordEntry.gameObject.SetActive(true);
        this.PasswordEntry.Name.text = e.StringValue;
    }

    private void Update()
    {
        if (!Input.GetKeyUp(KeyCode.Escape))
            return;
        Application.Quit();
    }

    private void OnAcceptMessage_Clicked()
    {
        MenuConfig.MTOD = true;
        this.MOTD.gameObject.SetActive(false);
        this.PatreonAuthentication.gameObject.SetActive(true);
        this.PatreonAuthentication.Open();
    }

    private void OnPatreonConfirmed_Clicked(object sender, EventArgs e)
    {
        this.PatreonAuthentication.gameObject.SetActive(false);
        if (MenuConfig.Patreon)
        {
            this.OpenAccountList();
        }
        else
        {
            MenuConfig.Error = "Patreon check failed.";
            this.StartingScreen();
        }
    }

    private void OpenAccountList()
    {
        this.AccountSelectionCtrl.gameObject.SetActive(true);
        this.AccountSelectionCtrl.Open();
        this.AccountSelectionCtrl.CreateButton.onClick.AddListener(new UnityAction(this.OnCreate_Clicked));
        this.AccountSelectionCtrl.OtherButton.onClick.AddListener(new UnityAction(this.OnOther_Clicked));
        this.AccountSelectionCtrl.OnAccountSelected += new EventHandler<ButtonEventArgs>(this.AccountSelectionCtrl_OnSelect);
        this.CreateAccountCtrl.gameObject.SetActive(false);
    }

    private void OnAcceptError_Clicked()
    {
        Application.Quit();
    }

    private void OnCreate_Clicked()
    {
        this.AccountSelectionCtrl.gameObject.SetActive(false);
        this.CreateAccountCtrl.gameObject.SetActive(true);
    }

    private void OnOther_Clicked()
    {
        this.AccountSelectionCtrl.gameObject.SetActive(false);
        this.AccountEntry.gameObject.SetActive(true);
    }

    private void OnBack_Clicked()
    {
        this.PasswordEntry.ClearError();
        this.PasswordEntry.gameObject.SetActive(false);
        this.AccountSelectionCtrl.gameObject.SetActive(true);
        this.CreateAccountCtrl.gameObject.SetActive(false);
    }

    private void OnOK_Clicked()
    {
        this.AccountSelectionCtrl.gameObject.SetActive(false);
        this.CreateAccountCtrl.gameObject.SetActive(false);
        this.authenticationView.SetAuthenticationValues(this.CreateAccountCtrl.Name.text, this.CreateAccountCtrl.Password.text);
        this.authenticationView.SendNewAccountRequest();
    }

    public void NewAccountCreated()
    {
        List<AccountSelectionList> accountSelectionListList = new List<AccountSelectionList>();
        AccountSelectionList accountEntry = new AccountSelectionList()
        {
            accountName = this.CreateAccountCtrl.Name.text,
            password = this.CreateAccountCtrl.Password.text
        };
        List<AccountSelectionList> list = GameData.AccountSelectionList.Where<AccountSelectionList>((Func<AccountSelectionList, bool>)(x => x.accountName == accountEntry.accountName)).ToList<AccountSelectionList>();
        foreach (AccountSelectionList accountSelection in GameData.AccountSelectionList)
        {
            if (!list.Contains(accountSelection))
                accountSelectionListList.Add(accountSelection);
        }
        GameData.AccountSelectionList = accountSelectionListList;
        GameData.AccountSelectionList.Insert(0, accountEntry);
        Functions.SaveAccountList();
        this.AccountSelectionCtrl.gameObject.SetActive(true);
        this.AccountSelectionCtrl.CreateAccountList();
        this.AccountSelectionCtrl.ResetPanel();
    }
}
