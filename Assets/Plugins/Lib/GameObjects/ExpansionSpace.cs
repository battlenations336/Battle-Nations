using System;
using UnityEngine;

namespace BNR
{
    public class ExpansionSpace
    {
        bool open;

        public bool Open
        {
            get { return open; }
            set { open = value; }
        }

        Vector2 position;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        Boolean blocked;

        public Boolean Blocked
        {
            get { return blocked; }
            set { blocked = value; }
        }

        Point index;

        public Point Index
        {
            get { return index; }
            set { index = value; }
        }

        GameObject mesh;

        public GameObject Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }

        public ExpansionSpace()
        {

        }
    }
}
