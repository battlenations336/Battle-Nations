using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FightButtonController : MonoBehaviour
{
    public void SetPressed()
    {
        Image img = (Image)GetComponent<Image>();

        img.sprite = (Sprite)Resources.Load("UI/fightActive@2x", typeof(Sprite));
    }

    public void OnClick()
    {
        BattleMapCtrl BM;

        BM = BattleMapCtrl.instance;

        Image img = (Image)GetComponent<Image>();

        img.sprite = (Sprite)Resources.Load("UI/fightActive@2x", typeof(Sprite));

        BM.StartBattle();
    }

    public void Reset()
    {
        Image img = (Image)GetComponent<Image>();

        img.sprite = (Sprite)Resources.Load("UI/fightInactive@2x", typeof(Sprite));
    }
}
