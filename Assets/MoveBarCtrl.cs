
using UnityEngine;

public class MoveBarCtrl : MonoBehaviour
{
    public GameObject ButtonOK;
    public GameObject ButtonTurn;
    public GameObject ButtonCancel;
    public GameObject ButtonSell;
    public SpriteRenderer SpriteRenderer;

    public void Start()
    {
        this.SpriteRenderer.sortingOrder = 295;
    }
}
