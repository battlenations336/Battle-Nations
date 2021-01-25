
using BNR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public GameObject HB_Bar;
    public GameObject Armour_Bar;
    public Text HPText;
    private UnitEntity unit;
    private float HP;
    private float Armour;

    public void Init(UnitEntity unit)
    {
        this.unit = unit;
        unit.OnDamage += new EventHandler(this.HealthSystem_OnHealthChanged);
        this.UpdateBars();
    }

    private void HealthSystem_OnHealthChanged(object sender, EventArgs e)
    {
        this.UpdateBars();
    }

    public void UpdateBars(UnitEntity unitEntity)
    {
        this.unit = unitEntity;
        this.UpdateBars();
    }

    private void UpdateBars()
    {
        this.HP = this.unit.GetHealthPercent();
        this.Armour = this.unit.GetArmourPercent();
        if ((double)this.Armour > 0.0)
            this.Armour += this.HP;
        this.UpdateHealthBar();
        this.UpdateArmourBar();
        this.UpdateText();
    }

    private void UpdateHealthBar()
    {
        this.HB_Bar.transform.localScale = new Vector3(this.HP, 1f);
    }

    private void UpdateArmourBar()
    {
        this.Armour_Bar.transform.localScale = new Vector3(this.Armour, 1f);
    }

    private void UpdateText()
    {
        if (!((UnityEngine.Object)this.HPText != (UnityEngine.Object)null))
            return;
        this.HPText.text = string.Format("{0}/{1} HP", (object)this.unit.GetHealth(), (object)this.unit.GetMaxHealth());
    }
}
