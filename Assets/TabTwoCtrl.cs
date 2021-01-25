using BNR;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TabTwoCtrl : MonoBehaviour
{
    public EventHandler<WeaponSelectEventArgs> OnSelect { get; set; }
    public string WeaponName;

    [SerializeField]
    public AbilityDetail AbilityDetail1 = new AbilityDetail();
    [SerializeField]
    public AbilityDetail AbilityDetail2 = new AbilityDetail();

    public AbilityDetail GetAbilityDetail(int _detail)
    {
        switch (_detail)
        {
            case 1:
                return (AbilityDetail1);
            case 2:
                return (AbilityDetail2);
        }

        return (null);
    }

    public void Button1_OnClick()
    {
        if (OnSelect != null)
            OnSelect(this, new WeaponSelectEventArgs(WeaponName, GetAbilityDetail(1).AbilityName));
    }

    public void Button2_OnClick()
    {
        if (OnSelect != null)
            OnSelect(this, new WeaponSelectEventArgs(WeaponName, GetAbilityDetail(2).AbilityName));
    }

    public void UpdateTabs(UnitEntity _entity)
    {
        UpdateTab(AbilityDetail1, _entity);       
        UpdateTab(AbilityDetail2, _entity);
    }

    public void UpdateTab(AbilityDetail detail, UnitEntity _entity)
    {
        if (_entity.GetAbility(WeaponName, detail.AbilityName).Cooldown == 0)
        {
            detail.Text.text = string.Empty;
            detail.CooldownFilter.SetActive(false);
        }
        else
        {
            detail.CooldownFilter.SetActive(true);
            detail.Text.text = _entity.GetAbility(WeaponName, AbilityDetail1.AbilityName).Cooldown.ToString();
        }
    }
}
