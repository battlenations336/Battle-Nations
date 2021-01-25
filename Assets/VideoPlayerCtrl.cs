using BNR;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerCtrl : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    public Text Subtitle;

    Cutscenes cutscenes;
    Subtitle subtitle;
    int cutIndex;
    float start;
    float end;
    float timer;
    bool active = false;
    bool isSet = false;
    // Start is called before the first frame update
    void Start()
    {
        Subtitle.text = string.Empty;
        if (VideoConfig.VideoName != null && VideoConfig.VideoName != string.Empty)
        {
            cutscenes = GameData.Cutscenes[VideoConfig.VideoName];
            VideoPlayer.clip = Resources.Load<VideoClip>("Video/" + cutscenes.movieName);
        }
        else
            cutscenes = GameData.Cutscenes["intro_cutscene"];

        cutIndex = 0;

        if (VideoPlayer != null)
        {
            setNextSubTitle();
            VideoPlayer.loopPointReached += EndScene;
            VideoPlayer.Play();
        }
    }

    void setNextSubTitle()
    {
        if (cutIndex < cutscenes.subtitles.Length)
        {
            subtitle = cutscenes.subtitles[cutIndex];
            start = subtitle.startTimeOffset;
            end = subtitle.endTimeOffset;
            cutIndex++;
            active = true;
        }
        else
        {
            active = false;
        }
    }
    // Update is called once per frame

    void Update()
    {
        if (!active)
            return;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            VideoPlayer.Stop();
            ExitPlayer();
        }

        if (isSet)
        {
            UpdateSubtitle_End();
        }
        else
        {
            UpdateSubtitle_Start();
        }
    }

    public void UpdateSubtitle_Start()
    {
        timer += Time.deltaTime;
        if (timer >= start)
        {
            Subtitle.text = GameData.GetText(subtitle.body);
            isSet = true;
        }
    }

    public void UpdateSubtitle_End()
    {
        timer += Time.deltaTime;
        if (timer >= end)
        {
            Subtitle.text = string.Empty;
            isSet = false;
            setNextSubTitle();
        }
    }

    public void EndScene(object sender)
    {
        ExitPlayer();
    }

    public void ExitPlayer()
    {
        GameData.Player.Intro = true;
        MapConfig.InitEmpty();
        MapConfig.npcId = string.Empty;
        SceneManager.LoadScene("WorldMap");
    }
}
