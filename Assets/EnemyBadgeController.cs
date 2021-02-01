
using BNR;
using UnityEngine;
using UnityEngine.UI;

public class EnemyBadgeController : MonoBehaviour
{
    public Text EnemyName;
    public Text LevelText;
    public Image EnemyAvatar;

    public void Init(string _name, int _lvl)
    {
        this.EnemyName.text = _name;
        this.LevelText.text = string.Format("Lvl. {0}", (object)_lvl);
    }

    public void Init(string encounterId)
    {
        BattleEncounterArmy army = GameData.BattleEncounters.armies[encounterId];
        this.Init(GameData.GetText(army.name), army.level);
        this.EnemyAvatar.sprite = GameData.GetIcon(army.icon);
    }
}
