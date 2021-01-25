using BNR;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUnlockedCtrl : MonoBehaviour
{
    public EventHandler OnClose { get; set; }

    public Text Title;
    public Image Icon;
    public Text Description;
    public Text Z2Reward;
    public Text Currency;

    BNAchievement achievement;

    Dictionary<string, GameObject> MenuItems = new Dictionary<string, GameObject>();

    public AudioSource audio;

    int level;
    LevelDialog[] dialog = null;
    int count;

    public void InitFromAchievement(string achievementId)
    {
        if (!GameData.Achievements.ContainsKey(achievementId))
            return;

        achievement = GameData.Achievements[achievementId];

        Title.text = achievement.title;
        Description.text = achievement.description;

        Z2Reward.text = string.Format("{0} Z2Points", achievement.reward.z2points);
        Currency.text = string.Format("{0}", achievement.reward.currency);

        Icon.sprite = GameData.GetIcon(achievement.objective.icon);
    }
    
    public void SetSound(string soundfile)
    {
        AudioClip clip;

        if (audio)
        {
            clip = (AudioClip)Resources.Load<AudioClip>("Audio/" + soundfile);
            if (clip != null)
                audio.PlayOneShot(clip, 0.7F);
        }
    }

    public void CloseDialog()
    {
        if (OnClose != null)
            OnClose(this, new EventArgs());
    }
}
