
using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UnitInformationCtrl : MonoBehaviour
{
  private List<GameObject> StatItems = new List<GameObject>();
  public UnitSummaryCtrl UnitSummaryCtrl;
  public SkillPointsItemCtrl SkillPointsItemCtrl;
  public Button PromotionBtn;
  public Text ProgressText;
  public Text StatUnitName;
  public Text StatUnitType;
  public GameObject StatPanel;
  private ArmyUnit armyUnit;

  public EventHandler OnClose { get; set; }

  public EventHandler<ButtonEventArgs> OnPromote { get; set; }

  public void Show(string unitName)
  {
    BattleUnit battleUnit = GameData.BattleUnits[unitName];
    this.armyUnit = GameData.Player.Army[unitName];
    this.UnitSummaryCtrl.UnitName.text = string.IsNullOrEmpty(GameData.BattleUnits[unitName].shortName) ? GameData.GetText(GameData.BattleUnits[unitName].name) : GameData.GetText(GameData.BattleUnits[unitName].shortName);
    this.UnitSummaryCtrl.Level.text = (this.armyUnit.level + 1).ToString();
    Sprite icon = GameData.GetIcon(GameData.BattleUnits[unitName].icon);
    if ((UnityEngine.Object) icon != (UnityEngine.Object) null)
      this.UnitSummaryCtrl.Icon.sprite = icon;
    this.UnitSummaryCtrl.HP.text = string.Format("{0} HP", (object) battleUnit.stats[this.armyUnit.level].hp);
    this.StatUnitName.text = this.UnitSummaryCtrl.UnitName.text;
    this.StatUnitType.text = this.armyUnit.GetBattleUnit().tags[0];
    float xp = (float) GameData.Player.Army[unitName].xp;
    float levelCutOff = (float) GameData.BattleUnits[unitName].stats[this.armyUnit.level].levelCutOff;
    this.SkillPointsItemCtrl.Init(unitName, xp, 0.0f, levelCutOff);
    this.ProgressText.text = string.Empty;
    if (this.armyUnit.PromotionComplete())
      this.ProgressText.text = "Ready!";
    if (this.armyUnit.ReadyToPromote())
      this.ProgressText.text = "Promote Now!";
    if (this.armyUnit.Upgrading && !this.armyUnit.PromotionComplete())
      this.ProgressText.text = "Training In Progress...";
    this.PromotionBtn.onClick.AddListener((UnityAction) (() => this.ExecuteButton(unitName)));
    this.PromotionBtn.interactable = this.armyUnit.ReadyToPromote() || this.armyUnit.PromotionComplete() || this.armyUnit.PromotionInProgress();
    this.buildStatList();
  }

  private void buildStatList()
  {
    foreach (UnityEngine.Object statItem in this.StatItems)
      UnityEngine.Object.Destroy(statItem);
    this.StatItems = new List<GameObject>();
    int num = this.armyUnit.GetBattleUnitStats().hp;
    this.addStatItem("Hit Points", num.ToString());
    if (this.armyUnit.GetBattleUnitStats().armorHp > 0)
    {
      num = this.armyUnit.GetBattleUnitStats().armorHp;
      this.addStatItem("Armor", num.ToString());
    }
    else
      this.addStatItem(string.Empty, string.Empty);
    num = this.armyUnit.GetBattleUnitStats().bravery;
    this.addStatItem("Bravery", num.ToString());
    num = this.armyUnit.GetBattleUnitStats().defense;
    this.addStatItem("Defense", num.ToString());
    num = this.armyUnit.GetBattleUnit().blocking;
    this.addStatItem("Blocking", num.ToString());
    num = this.armyUnit.GetBattleUnitStats().critical;
    this.addStatItem("Critical", num.ToString());
    num = this.armyUnit.GetBattleUnitStats().power;
    this.addStatItem("Power", num.ToString());
  }

  private void addStatItem(string name, string value)
  {
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject) UnityEngine.Resources.Load("StatLine"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    if ((UnityEngine.Object) this.StatPanel != (UnityEngine.Object) null)
      gameObject.transform.SetParent(this.StatPanel.transform, false);
    gameObject.GetComponent<StatLineCtrl>().Show(name, value);
    this.StatItems.Add(gameObject);
  }

  private void ExecuteButton(string unitName)
  {
    if (this.OnPromote == null)
      return;
    this.OnPromote((object) this, new ButtonEventArgs(ButtonValue.OK, unitName));
  }

  public void CloseDialog()
  {
    if (this.OnClose == null)
      return;
    this.OnClose((object) this, new EventArgs());
  }
}
