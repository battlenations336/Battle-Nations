
using BNR;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UnitLevelUpCtrl : MonoBehaviour
{
  private List<GameObject> MenuItems = new List<GameObject>();
  public Text Title;
  public Image BackgroundImage;
  public Image UnitImage;
  public Text UnitName;
  public GameObject RewardGrid;
  public GameObject PromoPanel;
  public GameObject ProgressPanel;
  public GameObject ProgressBar;
  public Text ProgressText;
  public Text HurryCostText;
  public GameObject btnPromote;
  public GameObject btnHurry;
  public GameObject btnCollect;
  private ArmyUnit armyUnit;
  private int count;
  private MessageBoxCtrl messageBoxCtrl;

  public EventHandler OnClose { get; set; }

  public EventHandler<ButtonEventArgs> OnPromote { get; set; }

  public EventHandler<HurryEventArgs> OnHurry { get; set; }

  public EventHandler<CollectEventArgs> OnCollect { get; set; }

  public void Show(ArmyUnit _armyUnit)
  {
    this.armyUnit = _armyUnit;
    this.UnitImage.sprite = GameData.GetIcon(Path.GetFileNameWithoutExtension(this.armyUnit.GetBattleUnit().icon));
    this.UnitName.text = GameData.GetText(this.armyUnit.GetBattleUnit().shortName);
    this.BackgroundImage.sprite = !GameData.IsUnitPremium(this.armyUnit.Name) ? (Sprite) null : (Sprite) UnityEngine.Resources.Load("UI/starburst@2x", typeof (Sprite));
    this.PromoPanel.SetActive(!this.armyUnit.Upgrading);
    this.ProgressPanel.SetActive(this.armyUnit.Upgrading);
    this.btnPromote.SetActive(!this.armyUnit.Upgrading);
    this.btnHurry.SetActive(this.armyUnit.Upgrading && !this.armyUnit.PromotionComplete());
    this.btnCollect.SetActive(this.armyUnit.Upgrading && this.armyUnit.PromotionComplete());
    this.Title.text = "Levelling up";
    if (this.armyUnit.xp >= this.armyUnit.GetBattleUnit().stats[this.armyUnit.level].levelCutOff)
      this.Title.text = "Ready For Promotion!";
    if (this.armyUnit.Upgrading)
      this.Title.text = (double) this.armyUnit.GetProgressPercent() < 1.0 ? "Training In Progress ..." : "Training completed ...";
    if (this.armyUnit.level + 1 == this.armyUnit.GetBattleUnit().stats.Length)
      this.Title.text = "Maximum Rank";
    this.BuildList();
  }

  public void UpdateProgressBar()
  {
    DateTime.Now.Subtract(this.armyUnit.UpgradeStart);
    float progressPercent = this.armyUnit.GetProgressPercent();
    if (this.armyUnit.PromotionComplete() && !this.btnPromote.activeSelf)
      this.Show(this.armyUnit);
    if ((UnityEngine.Object) this.ProgressBar != (UnityEngine.Object) null)
      this.ProgressBar.transform.localScale = new Vector3(1f - progressPercent, 1f);
    if ((UnityEngine.Object) this.ProgressText != (UnityEngine.Object) null)
      this.ProgressText.text = !this.armyUnit.PromotionComplete() ? string.Format("Ready in {0}", (object) this.armyUnit.GetTaskTime()) : string.Format("Ready to promote", (object[]) Array.Empty<object>());
    if (!((UnityEngine.Object) this.HurryCostText != (UnityEngine.Object) null))
      return;
    this.HurryCostText.text = string.Format("{0}", (object) this.armyUnit.GetHurryCost());
  }

  public void BuildList()
  {
    foreach (UnityEngine.Object menuItem in this.MenuItems)
      UnityEngine.Object.Destroy(menuItem);
    this.MenuItems = new List<GameObject>();
    this.count = 0;
    if (this.armyUnit.GetPromotionCost() == null)
      return;
    Cost levelUpCost = this.armyUnit.GetBattleUnit().stats[this.armyUnit.level].levelUpCost;
    int num;
    if (levelUpCost.money > 0)
    {
      Sprite sprite = GameData.GetSprite("UI/resource_moneyicon_0");
      num = levelUpCost.money;
      string qty = num.ToString();
      this.AddMenuItem(sprite, qty);
    }
    if (levelUpCost.currency > 0)
    {
      Sprite sprite = GameData.GetSprite("UI/resource_currency@2x");
      num = levelUpCost.currency;
      string qty = num.ToString();
      this.AddMenuItem(sprite, qty);
    }
    if (this.armyUnit.GetBattleUnit().stats[this.armyUnit.level].levelUpTime <= 0)
      return;
    this.AddMenuItem(GameData.GetSprite("UI/resource_time@2x"), Functions.GetShortTaskTime(this.armyUnit.GetBattleUnit().stats[this.armyUnit.level].levelUpTime, 0.0f));
  }

  public void AddMenuItem(Sprite iconSprite, string qty)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject) UnityEngine.Resources.Load("RewardItem"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    gameObject.transform.SetParent(this.RewardGrid.transform, true);
    RewardItemCtrl component = gameObject.GetComponent<RewardItemCtrl>();
    if ((UnityEngine.Object) iconSprite != (UnityEngine.Object) null)
      component.Icon.sprite = iconSprite;
    component.Quantity.text = qty;
    this.MenuItems.Add(gameObject);
  }

  public void CloseDialog()
  {
    if (this.OnClose == null)
      return;
    this.OnClose((object) this, new EventArgs());
  }

  public void PromoteButton_OnClick()
  {
    if (!GameData.Player.Affordable(this.armyUnit.GetPromotionCost()))
    {
      this.messageBoxCtrl.Show("Not enough resources to promote unit");
    }
    else
    {
      if (this.OnPromote == null)
        return;
      this.OnPromote((object) this, new ButtonEventArgs(ButtonValue.OK, this.armyUnit.Name));
    }
  }

  public void HurryButton_OnClick()
  {
    if (!GameData.Player.Affordable(0, this.armyUnit.GetHurryCost(), (ResourceList) null, 0))
    {
      this.messageBoxCtrl.Show("Not enough nanopods to hurry this job");
    }
    else
    {
      if (this.OnHurry == null)
        return;
      this.OnHurry((object) this, new HurryEventArgs(this.armyUnit));
    }
  }

  public void CollectButton_OnClick()
  {
    if (this.OnCollect != null)
      this.OnCollect((object) this, new CollectEventArgs(this.armyUnit));
    this.CloseDialog();
  }
}
