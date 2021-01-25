using BNR;

public class PathNode
{
    Point index;
    bool isPassible;

    /// <summary>
    /// Grid location
    /// </summary>
    public Point Index
    {
        get { return index; }
        set { index = value; }
    }

    /// <summary>
    /// Whether or not this node can be used as a path
    /// </summary>
    public bool IsPassible
    {
        get { return isPassible; }
        set { isPassible = value; }
    }
}

