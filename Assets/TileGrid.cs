using UnityEngine;

namespace BNR
{
    public class TileGrid<T> where T : PathNode, new()
    {
        /// <summary>
        /// Size of the grid
        /// </summary>
        readonly int width, height;

        /// <summary>
        /// Grid with
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Grid Height
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Rectangle representing the bounds used for determining if a node 
        /// is inside the grid while path finding
        /// </summary>
        readonly Rectangle bounds;

        T[][] grid;

        /// <summary>
        /// Grid item at the given position
        /// </summary>
        /// <param name="x"> X position </param>
        /// <param name="y"> Y position </param>
        public T this[int x, int y]
        {
            get { return grid[x][y]; }
            set { grid[x][y] = value; }
        }

        /// <summary>
        /// Grid item at the given position
        /// </summary>
        /// <param name="p"> Node location </param>
        public T this[Point p]
        {
            get
            {
                if (IsInBounds(p))
                    return grid[p.X][p.Y];
                else
                    return (null);
            }

            set { grid[p.X][p.Y] = value; }
        }

        public bool IsInBounds(Point p)
        {
            if (p.X < 0 || p.X > 143)
                return (false);

            if (p.Y < 0 || p.Y > 143)
                return (false);

            return (true);
        }

        public TileGrid(int width, int height)
        {
            this.width = width;
            this.height = height;

            bounds = new Rectangle(0, 0, width, height);

            grid = new T[width][];

            // initialize our grid with empty grid spaces
            for (int x = 0; x < width; x++)
            {
                grid[x] = new T[height];

                for (int y = 0; y < height; y++)
                {
                    grid[x][y] = new T()
                    {
                        Index = new Point(x, y)
                    };
                }
            }
        }

        public Point GetTileIndex(Vector3 _position)
        {
            Vector3 position = _position; // new Vector3(_position.x + (Constants.OutpostWidth / 2), _position.y - (Constants.OutpostHeight / 2), 0);

            float tileWidthHalf = Constants.OutpostWidth / 144.0f / 2.0f;
            float tileHeightHalf = Constants.OutpostHeight / 144.0f / 2.0f;

            Point tilePos = new Point();

            tilePos.X = (int)(((position.x / tileWidthHalf + position.y / tileHeightHalf) / 2) + 72);
            tilePos.Y = (int)(((position.y / tileHeightHalf - (position.x / tileWidthHalf)) / 2) + 72);

            return (tilePos);
        }

        public Vector3 GetTilePos(Vector3 _position)
        {
            Point point = GetTileIndex(_position);

            var tg = PlayerMap.instance.tileGrid[point];
            if (tg != null)
                return (PlayerMap.instance.tileGrid[point].Position);
            else
                return (Vector3.zero);
        }

        public void OpenExpansionSpace(Point index, int width, int height)
        {
            int x = index.X * 12;
            int y = index.Y * 12;

            Debug.Log(string.Format("Tiling {0}, {1} to {2}, {3}", x, y, x + width - 1, y + height - 1));
            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    PlayerMap.instance.tileGrid[x + dx, y + dy].IsOpen = true;
                }
            }
        }

        public BuildingEntity GetOccupant(Vector3 position, bool offset = false)
        {
            Point index = GetTileIndex(position);
            if (offset)
            {
                index.X-=0;
                index.Y-=2;
            }
            if (index.X < 0 || index.Y < 0)
                return(null);
            if (index.X > 143 || index.Y > 143)

                return (null);
            return (PlayerMap.instance.tileGrid[index.X, index.Y].Occupant);
        }

        public BuildingEntity UpdatePlacement(BuildingEntity building, bool insert)
        {
            Vector3 offsetExp = Vector3.zero;
            
            //if (building.IsExpansion())
            //    offsetExp = new Vector3(Constants.OutpostWidth / 144 / 2, Constants.OutpostHeight / 144, 0);

            return (UpdatePlacement(building, building.entitySprite.transform.position - offsetExp, insert));
        }

        public BuildingEntity UpdatePlacement(BuildingEntity building, Vector3 position, bool insert)
        {
            BuildingEntity replaced = null;

            Point index = GetTileIndex(position);

            for (int dx = 0; dx < building.composition.componentConfigs.Placeable.width; dx++)
            {
                for (int dy = 0; dy < building.composition.componentConfigs.Placeable.height; dy++)
                {
                    if (index.X < 0 || index.Y < 0)
                        continue;
                    if (index.X + dx > 143 || index.Y + dy > 143)
                        continue;
                    if (insert)
                    {
                        if (building.IsResourceProducer() && PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant != null)
                        {
                            replaced = PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant;
                            PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant.State = GameCommon.BuildingState.Hidden;
                            PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant = building;
                            Debug.Log(string.Format("WARNING - Hiding {0}", PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant.Name));
                        }
                        else if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant == null)
                            PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant = building;
                    }
                    else
                    {
                        var space = PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy];
                        if (space.Occupant != null)
                        {
                            if (space.Occupant.Name == building.Name)
                                space.Occupant = null;
                            else
                                Debug.Log(string.Format("WARNING - {0} overlaps existing {1}", building.Name, space.Occupant.Name));
                        }
                    }

                    string sOpen = "False";
                    if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].IsOpen)
                        sOpen = "True";
                }
            }
            // DBM
            // Debug.Log(string.Format("Adding {0} - [{1}, {2}] : W{3} H{4}", building.Name, index.X, index.Y, building.composition.componentConfigs.Touchable.width, building.composition.componentConfigs.Touchable.depth));
            return (replaced);
        }

        public bool CheckObjectMoveable(BuildingEntity _entity)
        {
            int width = _entity.composition.componentConfigs.Placeable.width;
            int height = _entity.composition.componentConfigs.Placeable.height;
            Point index = GetTileIndex(_entity.entitySprite.transform.position);

            if (index.X < 0 || index.Y < 0)
                return (false);

            for (int dx = 0; dx < width; dx++)
            {
                for (int dy = 0; dy < height; dy++)
                {
                    if (index.X + dx > 143 || index.Y + dy > 143)
                        return (false);

                    if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].IsOpen == false)
                    {
                        Debug.Log(string.Format("Failed {0}, {1}", index.X + dx, index.Y + dy));
                        return (false);
                    }
                    if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant != null && PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant != _entity)
                    {
                        return (false);
                    }
                }
            }

            return (true);
        }

        public bool CheckPlacement(Vector3 position, BuildingEntity _entity)
        {
            return(CheckPlacement(_entity, GetTileIndex(position), _entity.composition.componentConfigs.Placeable.width, _entity.composition.componentConfigs.Placeable.height));
        }

        public bool CheckPlacement(BuildingEntity _entity, Point index, int _width, int _height)
        {
            if (index.X < 0 || index.Y < 0)
                return (false);

            for (int dx = 0; dx < _width; dx++)
            {
                for (int dy = 0; dy < _height; dy++)
                {
                    if (index.X + dx > 143 || index.Y + dy > 143)
                        return (false);

                    if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].IsOpen == false)
                    {
                        Debug.Log(string.Format("Failed {0}, {1}", index.X + dx, index.Y + dy));
                        return (false);
                    }
                    if (_entity.IsResourceProducer() && _entity.composition.componentConfigs.ResourceProducer.nodeType != null)
                    {
                        if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant == null)
                        {
                            return (false);
                        }
                        if (!PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant.Name.Contains(_entity.composition.componentConfigs.ResourceProducer.nodeType))
                            return (false);
                    }
                    else
                    {
                        if (PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant != null && PlayerMap.instance.tileGrid[index.X + dx, index.Y + dy].Occupant != _entity)
                        {
                            return (false);
                        }
                    }
                }
            }

            return (true);
        }
    }
}
