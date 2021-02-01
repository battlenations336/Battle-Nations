
using BNR;
using UnityEngine;
using UnityEngine.UI;

public class SkillPointsItemCtrl : MonoBehaviour
{
    public Image Icon;
    public Text Level;
    public Text SP;
    public Text ProgressText;
    public GameObject currentSPbar;
    public GameObject newSPbar;
    public Image segment1;
    public Image segment2;
    public Image segment3;
    public Image segment4;
    public Image segment5;
    public Image segment6;
    public Image segment7;
    public Image segment8;
    public Image segment9;
    public Image segment10;
    private float currentSP;
    private float newSP;
    private float maxSP;
    private Color currentColor;
    private Color newColor;

    public void Init(string unitName, float _currentSP, float _newSP, float _maxSP)
    {
        this.currentColor = Functions.GetColor(10f, 250f, 204f, (float)byte.MaxValue);
        this.newColor = Functions.GetColor(242f, 250f, 30f, (float)byte.MaxValue);
        this.currentSP = _currentSP;
        this.newSP = _newSP;
        this.maxSP = _maxSP;
        if ((double)this.newSP >= (double)this.maxSP)
            this.newSP = this.maxSP;
        if ((Object)this.ProgressText != (Object)null)
        {
            this.ProgressText.text = string.Empty;
            if (!GameData.Player.Army.ContainsKey(unitName))
                this.ProgressText.text = "Max";
            else if (GameData.Player.Army[unitName].ReadyToPromote())
                this.ProgressText.text = "Ready To Promote!";
        }
        this.UpdateSPBar();
    }

    private void UpdateSPBar()
    {
        float num1 = this.maxSP / 11f;
        float num2 = this.newSP;
        float num3 = this.currentSP;
        if ((double)this.currentSP == (double)this.maxSP)
        {
            num2 = 0.0f;
            num3 = num1;
        }
        else
        {
            if ((double)num2 > (double)num1)
                num2 = this.newSP % num1;
            if ((double)num3 > (double)num1)
                num3 = this.currentSP % num1;
        }
        if ((Object)this.newSPbar != (Object)null)
            this.newSPbar.transform.localScale = new Vector3(1f - this.GetPercent(num2, num1), 1f);
        if ((Object)this.currentSPbar != (Object)null)
            this.currentSPbar.transform.localScale = (double)this.newSP != (double)this.maxSP ? new Vector3(this.GetPercent(num3, num1), 1f) : new Vector3(this.GetPercent(num1, num1), 1f);
        this.UpdateSegments(num1, this.newSP, this.newColor);
        this.UpdateSegments(num1, this.currentSP, this.currentColor);
    }

    private void UpdateSegments(float segmentSize, float spValue, Color color)
    {
        int num = (int)((double)spValue / (double)segmentSize);
        if (num < 1)
        {
            for (int segment = 1; segment <= 10; ++segment)
                this.SetSegmentColor(segment, Functions.GetColor(0.0f, 0.0f, 0.0f));
        }
        else
        {
            if (num > 10)
                num = 10;
            for (int segment = 1; segment <= num; ++segment)
                this.SetSegmentColor(segment, color);
        }
    }

    private float GetPercent(float value, float maxValue)
    {
        float num = 0.0f;
        if ((double)value > 0.0)
            num = value / maxValue;
        if ((double)num > 1.0)
            num = 1f;
        return num;
    }

    private void SetSegmentColor(int segment, Color color)
    {
        switch (segment)
        {
            case 1:
                this.segment1.color = color;
                break;
            case 2:
                this.segment2.color = color;
                break;
            case 3:
                this.segment3.color = color;
                break;
            case 4:
                this.segment4.color = color;
                break;
            case 5:
                this.segment5.color = color;
                break;
            case 6:
                this.segment6.color = color;
                break;
            case 7:
                this.segment7.color = color;
                break;
            case 8:
                this.segment8.color = color;
                break;
            case 9:
                this.segment9.color = color;
                break;
            case 10:
                this.segment10.color = color;
                break;
        }
    }
}
