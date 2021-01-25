using UnityEngine;
using UnityEngine.UI;

public class DebugWindowCtrl : MonoBehaviour
{
    public Text debugText;
    private static DebugWindowCtrl modalPanel;

    public static DebugWindowCtrl Instance()
    {
        if (!modalPanel)
        {
            modalPanel = FindObjectOfType<DebugWindowCtrl>() as DebugWindowCtrl;
            if (!modalPanel)
                Debug.Log("No debug panel found");
        }

        return (modalPanel);
    }
}
