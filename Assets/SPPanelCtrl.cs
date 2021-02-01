
using BNR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SPPanelCtrl : MonoBehaviour
{
  private Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();
  public Text ResultText;
  public Button ButtonOK;
  public GameObject ModalPanelObject;
  public GameObject ModalDialog;
  public GameObject RewardGrid;
  private static SPPanelCtrl SPPanel;
  private GridLayoutCtrl_Base playerGrid;
  private int totalSP;
  private int count;
  private Dictionary<string, UnitEntity> spUnits;

  public static SPPanelCtrl Instance()
  {
    if (!(bool) (Object) SPPanelCtrl.SPPanel)
    {
      SPPanelCtrl.SPPanel = Object.FindObjectOfType<SPPanelCtrl>();
      if (!(bool) (Object) SPPanelCtrl.SPPanel)
        Debug.Log((object) "No active modal panel found");
    }
    return SPPanelCtrl.SPPanel;
  }

  public void Show(GridLayoutCtrl_Base _playerGrid, UnityAction OKEvent, int _totalSP)
  {
    this.playerGrid = _playerGrid;
    this.totalSP = _totalSP;
    this.ModalPanelObject.SetActive(true);
    this.ModalDialog.SetActive(true);
    this.ButtonOK.onClick.RemoveAllListeners();
    this.ButtonOK.onClick.AddListener(OKEvent);
    this.ButtonOK.onClick.AddListener(new UnityAction(this.ClosePanel));
    this.spUnits = new Dictionary<string, UnitEntity>();
    foreach (BattleGrid.GridCell cell in this.playerGrid.Cells)
    {
      if (cell.UnitAlive() && !this.spUnits.ContainsKey(cell.Unit.Name))
        this.spUnits.Add(cell.Unit.Name, cell.Unit);
    }
    this.BuildList(this.spUnits);
  }

  public void BuildList(Dictionary<string, UnitEntity> spUnits)
  {
    foreach (string key in this.MenuItems.Keys)
      Object.Destroy((Object) this.MenuItems[key]);
    this.MenuItems = new Dictionary<string, GameObject>();
    this.count = 0;
    if (spUnits == null || spUnits.Count < 1)
      return;
    foreach (UnitEntity unit in spUnits.Values)
      this.AddMenuItem(unit, spUnits.Count);
  }

  public void AddMenuItem(UnitEntity unit, int unitCount)
  {
    GameObject gameObject = Object.Instantiate<GameObject>((GameObject) UnityEngine.Resources.Load("SkillItem"), new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
    if ((Object) this.RewardGrid != (Object) null)
      gameObject.transform.SetParent(this.RewardGrid.transform, false);
    SkillPointsItemCtrl component = gameObject.GetComponent<SkillPointsItemCtrl>();
    component.Icon.sprite = GameData.GetIcon(unit.BattleUnit.icon);
    float spFactor = (float) this.playerGrid.GetSPFactor(unit.Name);
    float num = (float) (this.totalSP / unitCount) * (spFactor / 100f);
    if (GameData.Player.Army.ContainsKey(unit.Name))
    {
      component.Level.text = (GameData.Player.Army[unit.Name].level + 1).ToString();
      component.SP.text = "+" + ((int) num).ToString();
      float xp = (float) GameData.Player.Army[unit.Name].xp;
      float levelCutOff = (float) GameData.BattleUnits[unit.Name].stats[GameData.Player.Army[unit.Name].level].levelCutOff;
      GameData.Player.Army[unit.Name].IncreaseSkill((int) num);
      component.Init(unit.Name, xp, (float) GameData.Player.Army[unit.Name].xp, levelCutOff);
    }
    else
    {
      component.Level.text = "Max";
      component.SP.text = "Max";
      component.Init(unit.Name, 10f, 0.0f, 10f);
    }
    ++this.count;
    this.MenuItems.Add(this.count.ToString(), gameObject);
  }

  public void ClosePanel()
  {
    this.ModalDialog.SetActive(false);
    this.ModalPanelObject.SetActive(false);
  }
}
