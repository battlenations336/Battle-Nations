using BNR;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExpansionGrid : MonoBehaviour
{
    Vector3[] vertices = new Vector3[8];
    float width = Constants.OutpostWidth;
    float height = Constants.OutpostHeight;

    int rows = 12;
    int columns = 12;

    public ExpansionSpace[][] grid;

    public void Init()
    {
        vertices[0] = new Vector3(-width * 0.5f, 0);
        vertices[1] = new Vector3(-width * 0.5f, height * 0.5f);
        vertices[2] = new Vector3(0, height * 0.5f);

        vertices[3] = new Vector3(width * 0.5f, height * 0.5f);
        vertices[4] = new Vector3(width * 0.5f, 0);

        vertices[5] = new Vector3(width * 0.5f, -height * 0.5f);
        vertices[6] = new Vector3(0, -height * 0.5f);
        vertices[7] = new Vector3(-width * 0.5f, -height * 0.5f);
        InitCorner();

        initGrid();

        showGrid();
        //showTileGrid();
    }

    private void initGrid()
    {
        gridtilewidth = width / 12f;
        gridtileheight = height / 12f;

        grid = new ExpansionSpace[columns][];

        // initialize our grid with empty grid spaces
        for (int x = 0; x < columns; x++)
        {
            grid[x] = new ExpansionSpace[rows];

            for (int y = 0; y < rows; y++)
            {
                grid[x][y] = new ExpansionSpace()
                {
                    Index = new Point(x, y)
                };
            }
        }

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 pos = new Vector3();

                pos.x = (x - y) * gridtilewidth / 2;
                pos.y = (x + y) * gridtileheight / 2;
                //pos.x += (gridtilewidth);
                pos.y -= (height / 2);
                pos.y += (height / 24);
                grid[x][y].Position = pos;
            }
        }

/*        int b1 = 3;
        int b2 = 2;
        int b3 = 8;
        int b4 = 9;

        grid[b3][b1].Open = true;
        PlayerMap.instance.tileGrid.OpenExpansionSpace(new Point(b3, b1), 12, 12); // 44
        grid[b4][b1].Open = true;
        PlayerMap.instance.tileGrid.OpenExpansionSpace(new Point(b4, b1), 12, 12); // 45
        grid[b3][b2].Open = true;
        PlayerMap.instance.tileGrid.OpenExpansionSpace(new Point(b3, b2), 12, 12); // 32
        grid[b4][b2].Open = true;
        PlayerMap.instance.tileGrid.OpenExpansionSpace(new Point(b4, b2), 12, 12); // 33
        */
        foreach(int index in GameData.Player.WorldMaps[Constants.Outpost].Expansions)
        {
            int y = (int)(index / 12);
            int x = index - (y * 12);

            grid[x][y].Open = true;
            PlayerMap.instance.tileGrid.OpenExpansionSpace(new Point(x, y), 12, 12);
        }
    }

    void showGrid()
    {
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (!grid[x][y].Open)
                    grid[x][y].Mesh = gridMesh(x, y, false);
            }
        }
    }

    public Point GetRandomTile()
    {
        Point expansion = GetRandomExpansion();

        int randX = UnityEngine.Random.Range(0, 11);
        int randY = UnityEngine.Random.Range(0, 11);

        return (new Point((expansion.X * 12) + randX, (expansion.Y * 12) + randY));
    }

    public Point GetRandomExpansion()
    {
        int result = 0;
        int expansionCount = GameData.Player.WorldMaps[Constants.Outpost].Expansions.Count();

        if (expansionCount > 0)
        {
            int space = UnityEngine.Random.Range(0, expansionCount - 1);
            result = GameData.Player.WorldMaps[Constants.Outpost].Expansions[space];
        }

        return (IndexToPoint(result));
    }

    Point IndexToPoint(int index)
    {
        int y = (int)(index / 12);
        int x = index - (y * 12);

        return (new Point(x, y));
    }

    public void ShowBorder(Vector3 pos, Point point, bool border)
    {
        Debug.Log(string.Format("Show border for {0}", point.ToString()));
        MeshGenerator meshGen = grid[point.X][point.Y].Mesh.GetComponent<MeshGenerator>();
        //meshGen.DrawBorder = true;
//        meshGen.Init(adj, vertices);
        //meshGen.InitBorder(transform.position, 12, 12);
        //meshGen.UpdateBorders();*/
        meshGen.DrawBorder = border;
        meshGen.Init(pos, 12, 12, false);
        meshGen.SetColour(Functions.GetColor(255, 255, 255, 20f));
        meshGen.SetBorderColour(Functions.GetColor(0, 0, 0));
    }

    public void RemoveBorder(Point point)
    {
        MeshGenerator meshGen = grid[point.X][point.Y].Mesh.GetComponent<MeshGenerator>();
        if (meshGen != null)
            meshGen.RemoveBorder();
    }

    float gridtilewidth;
    float gridtileheight;

    Vector3 GetVert(float dx1, float dy1)
    {
        float scaleRatio = 1.0f;

        dx1 *= (scaleRatio * 1f);
        dy1 *= (scaleRatio * 1f);

        Vector3 newVector = new Vector3((gridtilewidth * dx1), (gridtileheight * dy1), -1);
        newVector.z = -1;
        return (newVector);
    }

    GameObject gridMesh(int x, int y, bool borderOnly)
    {
        GameObject prefab;

        Vector3[] vertices = new Vector3[4];

        vertices[0] = GetVert(-0.5f, 0f);
        vertices[1] = GetVert(0f, 0.5f);
        vertices[2] = GetVert(0.5f, 0f);
        vertices[3] = GetVert(0f, -0.5f);

        prefab = (GameObject)Resources.Load("MeshHighlightExpansion");
        prefab.name = "MeshHighlight";
        Vector3 delta = new Vector3(gridtilewidth / 2, gridtileheight / 2, 10);
        GameObject temp = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        MeshGenerator meshGen = temp.GetComponent<MeshGenerator>();
        
        Vector3 pos = new Vector3();

        pos.x = (x - y) * gridtilewidth / 2;
        pos.y = (x + y) * gridtileheight / 2;
        pos.y -= (height / 2);
        pos.y += (height / 24);

        pos = grid[x][y].Position;
        //Vector3 coord = pos / 200.0f;
        temp.transform.SetParent(transform, false);

        Vector3 adj = pos;
        adj.z = -1;

        temp.SetActive(true);

        if (borderOnly)
        {
            meshGen.DrawBorder = true;
            meshGen.MaterialColor = Functions.GetColor(255, 255, 255, 0f);
            meshGen.Init(adj, vertices);
            meshGen.InitBorder(transform.position, 12, 12);
            meshGen.SetBorderColour(Functions.GetColor(0, 0, 0));
            meshGen.UpdateBorders();
        }
        else
        {
            meshGen.MaterialColor = Functions.GetColor(0, 0, 0, 50f);
            meshGen.Init(adj, vertices);
        }

        return (temp);
    }

    void InitCorner()
    { 
        Mesh mesh = new Mesh();

        Color[] colors = new Color[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
            colors[i] = Functions.GetColor(0, 0, 0, 100f);

        mesh.vertices = vertices;
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 0 };
        mesh.colors = colors;

        GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshRenderer>().material.color = Functions.GetColor(86f, 87f, 89f, 200f);

        GetComponent<MeshRenderer>().sortingOrder = 7 + 144;
    }

    GameObject CellMesh(Vector3 pos, Vector3[] vectors)
    {
        GameObject prefab;

        prefab = (GameObject)Resources.Load("MeshHighlight");
        prefab.name = "MeshHighlight";
        Vector3 delta = new Vector3(Constants.DefaultBattleTileWidth / 2, Constants.DefaultBattleTileHeight / 2, 10);
        GameObject temp = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        MeshGenerator meshGen = temp.GetComponent<MeshGenerator>();

        Vector3 coord = pos;
        temp.transform.SetParent(transform, false);

        Vector3 adj = pos;
        adj.z = -1;

        temp.SetActive(true);

        meshGen.Init(adj, vectors);

        return (temp);
    }

    public int GetIndex(Vector3 _position)
    {
        Point point = GetTileIndex(_position);

        return (point.X + (point.Y * 12));
    }

    public Point GetTileIndex(Vector3 _position)
    {
        Vector3 position = _position; 

        float tileWidthHalf = Constants.OutpostWidth / 12 / 2;
        float tileHeightHalf = Constants.OutpostHeight / 12 / 2;

        Point tilePos = new Point();

        tilePos.X = (int)(((position.x / tileWidthHalf + position.y / tileHeightHalf) / 2) + 6);
        tilePos.Y = (int)(((position.y / tileHeightHalf - (position.x / tileWidthHalf)) / 2) + 6);
        //float xpos = position.x / (Constants.OutpostWidth / 12);
        //float ypos = position.x / (Constants.OutpostHeight / 12);

        //tilePos.X = (int)(xpos - ypos);
        //tilePos.Y = (int)(xpos + ypos);
        if (tilePos.X > 11)
            tilePos.X = 11;
        if (tilePos.X < 0)
            tilePos.X = 0;
        if (tilePos.Y > 11)
            tilePos.Y = 11;
        if (tilePos.Y < 0)
            tilePos.Y = 0;

        return (tilePos);
    }

    public Vector3 GetTilePos(Vector3 _position)
    {
        Point point = GetTileIndex(_position);

        return (grid[point.X][point.Y].Position - new Vector2(-1f * (Constants.OutpostWidth / 144), 5.5f * (Constants.OutpostHeight / 144)));
//        return (grid[point.X][point.Y].Position);// - new Vector2(-1f * (Constants.OutpostWidth / 144), 5.5f * (Constants.OutpostHeight / 144)));
    }

    public bool CheckPlacement(Point index, int _width, int _height)
    {
        if (index.X < 0 || index.Y < 0)
            return (false);

        if (grid[index.X][index.Y].Open == true)
        {
            return (false);
        }

        if (!HasOpenNeighbour(index))
            return (false);

        return (true);
    }

    public void OpenExpansion(Vector3 _position)
    {
        Debug.Log(string.Format("New vector : {0}", _position));
        Point point = GetTileIndex(_position);
        int index = GetIndex(_position + new Vector3(0, Constants.OutpostHeight / 144));
        point.Y = (int)(index / 12);
        point.X = index - (point.Y * 12);
        Debug.Log(string.Format("New point : {0}", point));
        PlayerMap.instance.tileGrid.OpenExpansionSpace(point, 12, 12);
        Destroy(grid[point.X][point.Y].Mesh);
        grid[point.X][point.Y].Mesh = null;
        grid[point.X][point.Y].Open = true;
        GameData.Player.WorldMaps[Constants.Outpost].Expansions.Add(point.Y * 12 + point.X);
    }

    public bool HasOpenNeighbour(Point node)
    {
        List<ExpansionSpace> neighbors = new List<ExpansionSpace>();
        bool result = false;

        /*
        int x = 0;
        int y = 0;

        //node.X += 1;
        //node.Y -= 3;

        node.X -= node.X % 12;
        node.Y -= node.Y % 12;

        x = Math.Abs(node.X / 12) - 1;
        y = Math.Abs(node.Y / 12) - 1;

        if (x >= 0 && x < 12 && y >= 0 && y < 12)
            if (grid[x][y].Open == true)
                return result;
                */
        neighbors = getNeighbors(new Point(node.X, node.Y), false);

        foreach (ExpansionSpace space in neighbors)
        {
            if (space.Open == true)
                result = true;
        }

        return result;
    }

    /// <summary>
    /// Gets all neighbors at the given point
    /// </summary>
    List<ExpansionSpace> getNeighbors(Point node, bool diagonal)
    {
        var neighbors = new List<ExpansionSpace>();

        int x = node.X,
            y = node.Y;

        var points = new List<Point>()
            {
                new Point(x - 1, y), // west
                new Point(x + 1, y), // east
                new Point(x, y - 1), // north
                new Point(x, y + 1), // south
            };

        if (diagonal)
        {
            points.Add(new Point(x - 1, y - 1)); // north west
            points.Add(new Point(x + 1, y - 1)); // north east
            points.Add(new Point(x - 1, y + 1)); // south west
            points.Add(new Point(x + 1, y + 1)); // south east
        }
        Rectangle bounds = new Rectangle(0, 0, 12, 12);
        foreach (var point in points)
        {
            if (bounds.Contains(point))
                neighbors.Add(grid[point.X][point.Y]);
        }

        return neighbors;
    }
    // Test stuff

    public GameObject TileGridMesh(int x, int y)
    {
        int gridtilewidth = (int)(Constants.OutpostWidth / 144f);
        int gridtileheight = (int)(Constants.OutpostHeight / 144f);
        GameObject prefab;

        Vector3[] vertices = new Vector3[4];

        vertices[0] = GetTileVert(-0.5f, 0f);
        vertices[1] = GetTileVert(0f, 0.5f);
        vertices[2] = GetTileVert(0.5f, 0f);
        vertices[3] = GetTileVert(0f, -0.5f);

        prefab = (GameObject)Resources.Load("MeshHighlight");
        prefab.name = "MeshHighlight";
        Vector3 delta = new Vector3(gridtilewidth / 2, gridtileheight / 2, 10);
        GameObject temp = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        MeshGenerator meshGen = temp.GetComponent<MeshGenerator>();

        Vector3 pos = new Vector3();

        pos.x = (x - y) * gridtilewidth / 2;
        pos.y = (x + y) * gridtileheight / 2;
        pos.y -= (Constants.OutpostHeight / 2);
        pos.y -= (Constants.OutpostHeight / 24);

        pos = PlayerMap.instance.tileGrid[x, y].Position;
        //Vector3 coord = pos / 200.0f;
        temp.transform.SetParent(transform, false);

        Vector3 adj = pos;
        adj.z = -1;

        temp.SetActive(true);

        meshGen.MaterialColor = Functions.GetColor(0, 0, 255, 50f);
        meshGen.Init(adj, vertices);

        return (temp);
    }

    Vector3 GetTileVert(float dx1, float dy1)
    {
        float gridtilewidth = Constants.OutpostWidth / 144f;
        float gridtileheight = Constants.OutpostHeight / 144f;
        float scaleRatio = 1.0f;

        dx1 *= (scaleRatio * 1f);
        dy1 *= (scaleRatio * 1f);

        Vector3 newVector = new Vector3((gridtilewidth * dx1), (gridtileheight * dy1), -1);
        newVector.z = -1;
        return (newVector);
    }

    public void showTileGrid()
    {
        for (int x = 0; x < 143; x++)
        {
            for (int y = 0; y < 143; y++)
            {
                if (PlayerMap.instance.tileGrid[x, y].Occupant != null)
                //if (PlayerMap.instance.tileGrid[x, y].IsOpen == true)
                    TileGridMesh(x, y);
            }
        }
    }

    public void showTileGrid_open()
    {
        for (int x = 0; x < 143; x++)
        {
            for (int y = 0; y < 143; y++)
            {
                if (PlayerMap.instance.tileGrid[x, y].IsOpen == true)
                    //if (PlayerMap.instance.tileGrid[x, y].IsOpen == true)
                    TileGridMesh(x, y);
            }
        }
    }

    public void showGridPoints()
    {
        for (int x = 0; x < 11; x++)
        {
            for (int y = 0; y < 11; y++)
            {
                Point point = PlayerMap.instance.tileGrid.GetTileIndex(grid[x][y].Position);
                TileGridMesh(point.X, point.Y);
            }
        }
    }
}
