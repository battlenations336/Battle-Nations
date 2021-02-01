
using BNR;
using System;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public float lineWidth = 0.07f;
    private GameObject[] borders = new GameObject[4];
    private Color matColor = Color.green;
    private float width = 0.4f;
    private float height = 0.2f;
    private float sx = 4f;
    private float sy = 4f;
    public float moveSpeed;
    public bool DrawBorder;
    public Color lineColor;
    public Material material;
    public GameObject ControlPanelSml;
    public GameObject ControlPanelLrg;
    private GameObject ControlPanel;
    private bool following;
    private Vector3 offset;
    private BoxCollider2D boxcol;
    private Vector3 initOff;
    private bool bordersCreated;
    private bool ShowSellBtn;
    public Vector3 origin;

    public EventHandler OnOKButtonClick { get; set; }

    public EventHandler<ButtonEventArgs> OnCancelButtonClick { get; set; }

    public EventHandler<ButtonEventArgs> OnSellButtonClick { get; set; }

    public EventHandler<ButtonEventArgs> OnFlipButtonClick { get; set; }

    private void Start()
    {
    }

    public Color MaterialColor
    {
        get
        {
            return this.matColor;
        }
        set
        {
            this.matColor = value;
        }
    }

    public void Show()
    {
        this.ControlPanel.SetActive(true);
        this.transform.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.ControlPanelSml.SetActive(false);
        this.ControlPanelLrg.SetActive(false);
        this.transform.gameObject.SetActive(false);
    }

    private void InitShape()
    {
        Mesh mesh = new Mesh();
        Vector3[] vector3Array = new Vector3[4];
        float num1 = 0.4f;
        float num2 = 0.2f;
        this.lineColor = Functions.GetColor(47f, 179f, 82f);
        this.offset = new Vector3(0.0f, 0.0f, 0.0f);
        this.initOff = new Vector3(this.width * 1f, (float)(-(double)this.height * 0.0), 0.0f);
        vector3Array[0] = new Vector3(0.0f, (float)(-(double)num2 / 2.0));
        vector3Array[1] = new Vector3((float)-((double)this.sx * ((double)num1 / 2.0)), (float)(((double)this.sx - 1.0) * ((double)num2 / 2.0)));
        vector3Array[2] = new Vector3((float)(((double)this.sy - (double)this.sx) * ((double)num1 / 2.0)), (float)(((double)this.sx + (double)this.sy - 1.0) * ((double)num2 / 2.0)));
        vector3Array[3] = new Vector3(this.sy * (num1 / 2f), (float)(((double)this.sy - 1.0) * ((double)num2 / 2.0)));
        Color[] colorArray = new Color[vector3Array.Length];
        for (int index = 0; index < vector3Array.Length; ++index)
        {
            colorArray[index] = this.MaterialColor;
            vector3Array[index] = vector3Array[index] - this.initOff;
        }
        mesh.vertices = vector3Array;
        mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
        this.GetComponent<MeshFilter>().mesh = mesh;
        this.SetColour(this.MaterialColor);
        this.following = false;
        if (this.DrawBorder && !this.bordersCreated)
            this.CreateBorders();
        if ((UnityEngine.Object)this.ControlPanel != (UnityEngine.Object)null)
        {
            MoveBarCtrl component1 = this.ControlPanel.GetComponent<MoveBarCtrl>();
            if ((UnityEngine.Object)component1 == (UnityEngine.Object)null)
                return;
            SpriteButtonCtrl component2 = component1.ButtonOK.GetComponent<SpriteButtonCtrl>();
            if ((UnityEngine.Object)component2 != (UnityEngine.Object)null)
                component2.OnClick += new EventHandler(this.BarCtrl_OK_Clicked);
            SpriteButtonCtrl component3 = component1.ButtonCancel.GetComponent<SpriteButtonCtrl>();
            if ((UnityEngine.Object)component3 != (UnityEngine.Object)null)
                component3.OnClick += new EventHandler(this.BarCtrl_Cancel_Clicked);
            SpriteButtonCtrl component4 = component1.ButtonTurn.GetComponent<SpriteButtonCtrl>();
            if ((UnityEngine.Object)component4 != (UnityEngine.Object)null)
                component4.OnClick += new EventHandler(this.BarCtrl_Flip_Clicked);
            if (this.ShowSellBtn)
            {
                component1.ButtonSell.SetActive(true);
                SpriteButtonCtrl component5 = component1.ButtonSell.GetComponent<SpriteButtonCtrl>();
                if ((UnityEngine.Object)component5 != (UnityEngine.Object)null)
                    component5.OnClick += new EventHandler(this.BarCtrl_Sell_Clicked);
            }
            else
                component1.ButtonSell.SetActive(false);
        }
        this.boxcol = this.GetComponent<BoxCollider2D>();
        if ((UnityEngine.Object)this.boxcol != (UnityEngine.Object)null)
        {
            double sy1 = (double)this.sy;
            double sx = (double)this.sx;
            double num3 = (double)num1 / 2.0;
            double num4 = (double)this.height / 2.0;
            double sy2 = (double)this.sy;
            double num5 = ((double)this.sx - (double)this.sy) * (double)this.height / 2.0;
            double num6 = (double)this.height / 2.0;
            double sy3 = (double)this.sy;
            this.boxcol.size = new Vector2((float)(0.0 + ((double)this.sy + (double)this.sx) * ((double)this.width / 2.0)), (float)((double)this.height / 2.0 * (double)this.sy + ((double)this.sx - (double)this.sy) * (double)this.height / 2.0 + (double)this.height / 2.0 * (double)this.sy));
            Vector3 position = this.boxcol.transform.position;
            if ((double)this.sx == 12.0)
                this.boxcol.offset = new Vector2(-num1, num2 * 5.5f);
            else
                this.boxcol.offset = new Vector2(-num1, num2 * 1.5f);
        }
        this.GetComponent<MeshRenderer>().sortingOrder = 151;
    }

    public void Init(Vector3 position, float sizeX, float sizeY, bool showSellBtn)
    {
        this.ShowSellBtn = showSellBtn;
        this.ControlPanel = !this.ShowSellBtn ? this.ControlPanelSml : this.ControlPanelLrg;
        this.sx = sizeX;
        this.sy = sizeY;
        this.Init(position);
    }

    public void InitBorder(Vector3 position, float sizeX, float sizeY)
    {
        if (this.bordersCreated)
            return;
        this.CreateBorders();
    }

    public void Init(Vector3 position)
    {
        Vector3 vector3 = new Vector3(-0.4f, -0.4f, 0.0f);
        this.InitShape();
        this.origin = position;
        double num1 = (double)this.height / 2.0;
        double sy1 = (double)this.sy;
        double num2 = ((double)this.sx - (double)this.sy) * (double)this.height / 2.0;
        double num3 = (double)this.height / 2.0;
        double sy2 = (double)this.sy;
        this.transform.position = position;
        if ((UnityEngine.Object)this.ControlPanel != (UnityEngine.Object)null)
            this.ControlPanel.transform.position = this.transform.position + vector3;
        if (!this.DrawBorder)
            return;
        this.UpdateBorders();
    }

    public void Init1(Vector3 position, float _height, float _width)
    {
        this.height = _height;
        this.width = _width;
        this.sx = 1f;
        this.sy = 1f;
        this.Init(position);
        Vector3[] vector3Array = new Vector3[4];
        Mesh mesh = new Mesh();
        vector3Array[0] = new Vector3((float)(-(double)this.width / 2.0), 0.0f);
        vector3Array[1] = new Vector3(0.0f, this.height / 2f);
        vector3Array[2] = new Vector3(this.width / 2f, 0.0f);
        vector3Array[3] = new Vector3(0.0f, (float)(-(double)this.height / 2.0));
        Color[] colorArray = new Color[vector3Array.Length];
        for (int index = 0; index < vector3Array.Length; ++index)
            colorArray[index] = this.MaterialColor;
        mesh.vertices = vector3Array;
        mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
        mesh.colors = colorArray;
        this.GetComponent<MeshFilter>().mesh = mesh;
    }

    public void Init(Vector3 position, Vector3[] vectors, Color _color)
    {
        this.MaterialColor = _color;
        this.Init(position, vectors);
    }

    public void Init(Vector3 position, Vector3[] vectors)
    {
        this.sx = 1f;
        this.sy = 1f;
        Vector3[] vector3Array = new Vector3[4];
        Mesh mesh = new Mesh();
        Color[] colorArray = new Color[vector3Array.Length];
        mesh.vertices = vectors;
        mesh.triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
        this.GetComponent<MeshFilter>().mesh = mesh;
        this.GetComponent<MeshRenderer>().material.color = this.MaterialColor;
        this.transform.position = position;
    }

    public void RemoveBorder()
    {
        if (this.borders == null)
            return;
        foreach (UnityEngine.Object border in this.borders)
            UnityEngine.Object.Destroy(border);
    }

    public void SetColour(Color _colour)
    {
        this.GetComponent<MeshRenderer>().material.color = _colour;
    }

    public void SetBorderColour(Color _colour)
    {
        foreach (GameObject border in this.borders)
        {
            LineRenderer component = border.GetComponent<LineRenderer>();
            if ((UnityEngine.Object)component != (UnityEngine.Object)null)
            {
                component.startColor = _colour;
                component.endColor = _colour;
            }
        }
    }

    public void SetColour(HighlightType _highlightType)
    {
        switch (_highlightType)
        {
            case HighlightType.None:
                this.SetColour(Functions.GetColor((float)byte.MaxValue, (float)byte.MaxValue, (float)byte.MaxValue, 0.0f));
                break;
            case HighlightType.InRange:
            case HighlightType.Selected:
                this.SetColour(Functions.GetColor(0.0f, 149f, 234f, 166f));
                break;
            case HighlightType.Damage:
                this.SetColour(Functions.GetColor(0.0f, 149f, 234f, 166f));
                break;
            case HighlightType.SplashDamage:
                this.SetColour(Functions.GetColor((float)byte.MaxValue, 250f, 0.0f, 166f));
                break;
            case HighlightType.Hit:
                this.SetColour(Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f, 166f));
                break;
            case HighlightType.Splashed:
                this.SetColour(Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f, 166f));
                break;
        }
    }

    public void SetColour(bool placementValid)
    {
        if (placementValid)
        {
            this.SetColour(Functions.GetColor(0.0f, (float)byte.MaxValue, 0.0f, 166f));
            this.SetBorderColour(Functions.GetColor(47f, 179f, 82f));
        }
        else
        {
            this.SetColour(Functions.GetColor((float)byte.MaxValue, 0.0f, 0.0f, 166f));
            this.SetBorderColour(Functions.GetColor(179f, 47f, 82f));
        }
    }

    public void Move(Vector3 _delta, Vector3 mousePosition, bool isExpansion)
    {
        this.origin += _delta;
        Vector3 origin = this.origin;
        Vector3 vector3 = new Vector3(-0.4f, -0.4f, 0.0f);
        float num1;
        float num2;
        if (isExpansion)
        {
            num1 = 4.8f;
            num2 = 2.4f;
        }
        else
        {
            num1 = this.width / 2f;
            num2 = this.height / 2f;
        }
        double num3 = (double)origin.x / (double)num1;
        double num4 = (double)origin.y / (double)num2;
        if (isExpansion)
        {
            this.transform.position = PlayerMap.instance.expansionGrid.GetTilePos(mousePosition);
            if ((UnityEngine.Object)this.ControlPanel != (UnityEngine.Object)null)
                this.ControlPanel.transform.position = this.transform.position + vector3;
        }
        else
        {
            this.transform.position = PlayerMap.instance.tileGrid.GetTilePos(this.origin);
            if ((UnityEngine.Object)this.ControlPanel != (UnityEngine.Object)null)
                this.ControlPanel.transform.position = PlayerMap.instance.tileGrid.GetTilePos(this.origin) + vector3;
        }
        if (!this.DrawBorder)
            return;
        this.UpdateBorders();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow))
            this.boxcol.transform.position += new Vector3(1f, 0.0f, 0.0f);
        if (!Input.GetKeyUp(KeyCode.LeftArrow))
            return;
        this.boxcol.transform.position += new Vector3(-1f, 0.0f, 0.0f);
    }

    private void BarCtrl_OK_Clicked(object sender, EventArgs e)
    {
        ButtonEventArgs buttonEventArgs = new ButtonEventArgs(ButtonValue.OK, string.Empty);
        if (this.OnOKButtonClick == null)
            return;
        this.OnOKButtonClick((object)this, (EventArgs)buttonEventArgs);
    }

    private void BarCtrl_Cancel_Clicked(object sender, EventArgs e)
    {
        ButtonEventArgs e1 = new ButtonEventArgs(ButtonValue.Cancel, string.Empty);
        if (this.OnCancelButtonClick == null)
            return;
        this.OnCancelButtonClick((object)this, e1);
    }

    private void BarCtrl_Sell_Clicked(object sender, EventArgs e)
    {
        ButtonEventArgs e1 = new ButtonEventArgs(ButtonValue.Sell, string.Empty);
        if (this.OnSellButtonClick == null)
            return;
        this.OnSellButtonClick((object)this, e1);
    }

    private void BarCtrl_Flip_Clicked(object sender, EventArgs e)
    {
        ButtonEventArgs e1 = new ButtonEventArgs(ButtonValue.Turn, string.Empty);
        if (this.OnFlipButtonClick == null)
            return;
        this.OnFlipButtonClick((object)this, e1);
    }

    private void CreateBorders()
    {
        Vector3[] vertices = this.GetComponent<MeshFilter>().mesh.vertices;
        this.borders[0] = this.createVertexPlayer(vertices[0], vertices[1]);
        this.borders[1] = this.createVertexPlayer(vertices[1], vertices[2]);
        this.borders[2] = this.createVertexPlayer(vertices[2], vertices[3]);
        this.borders[3] = this.createVertexPlayer(vertices[3], vertices[0]);
        this.bordersCreated = true;
    }

    public void UpdateBorders()
    {
        Vector3[] vertices = this.GetComponent<MeshFilter>().mesh.vertices;
        this.MoveBorder(0, vertices[0], vertices[1]);
        this.MoveBorder(1, vertices[1], vertices[2]);
        this.MoveBorder(2, vertices[2], vertices[3]);
        this.MoveBorder(3, vertices[3], vertices[0]);
    }

    private void MoveBorder(int index, Vector3 from, Vector3 to)
    {
        this.borders[index].GetComponent<LineRenderer>().SetPosition(0, from);
        this.borders[index].GetComponent<LineRenderer>().SetPosition(1, to);
    }

    private GameObject createVertexPlayer(Vector3 _from, Vector3 _to)
    {
        GameObject gameObject1 = this.gameObject;
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>((GameObject)Resources.Load("Gridline"), Vector3.zero, Quaternion.identity);
        gameObject2.name = "Border";
        LineRenderer component = gameObject2.GetComponent<LineRenderer>();
        component.material = this.material;
        component.startWidth = this.lineWidth;
        component.endWidth = this.lineWidth;
        component.startColor = this.lineColor;
        component.endColor = this.lineColor;
        component.positionCount = 2;
        component.sortingOrder = 152;
        component.useWorldSpace = false;
        component.numCapVertices = 20;
        component.numCornerVertices = 20;
        component.SetPosition(0, _from + this.offset);
        component.SetPosition(1, _to + this.offset);
        component.transform.SetParent(this.transform);
        component.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        return gameObject2;
    }
}
