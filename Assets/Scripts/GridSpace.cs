using UnityEngine;

namespace BNR
{
    public class GridSpace : PathNode
    {
        BuildingEntity occupant;
        Vector2 position;
        bool isOpen;

        /// <summary>
        /// Building occupying this grid space
        /// </summary>
        public BuildingEntity Occupant
        {
            get { return occupant; }
            set { occupant = value; }
        }

        public bool IsOpen
        {
            get { return isOpen; }
            set { isOpen = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        int tileIndex;

        public int TileIndex
        {
            get { return tileIndex; }
            set { tileIndex = value; }
        }

        public GridSpace()
        {
            IsPassible = true;
        }

        public bool IsAvailable()
        {
            if (occupant != null)
                return (true);

            return (false);
        }
    }
}
