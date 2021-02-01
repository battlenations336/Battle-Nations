using BNR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TabSingleCtrl : MonoBehaviour
{
    public EventHandler<WeaponSelectEventArgs> OnSelect { get; set; }
    public string WeaponName;

    [SerializeField]
    public AbilityDetail AbilityDetail1 = new AbilityDetail();

    public AbilityDetail GetAbilityDetail(int _detail)
    {
        switch (_detail)
        {
            case 1:
                return (AbilityDetail1);
        }

        return (null);
    }

    public void Button1_OnClick()
    {
        if (OnSelect != null)
            OnSelect(this, new WeaponSelectEventArgs(WeaponName, GetAbilityDetail(1).AbilityName));
    }

    public void UpdateTabs(UnitEntity _entity)
    {
        if (_entity.GetAbility(WeaponName, AbilityDetail1.AbilityName).Cooldown == 0)
        {
            AbilityDetail1.Text.text = string.Empty;
            AbilityDetail1.CooldownFilter.SetActive(false);
        }
        else
        {
            AbilityDetail1.CooldownFilter.SetActive(true);
            AbilityDetail1.Text.text = _entity.GetAbility(WeaponName, AbilityDetail1.AbilityName).Cooldown.ToString();
        }
    }
}
