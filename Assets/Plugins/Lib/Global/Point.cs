using UnityEngine;

namespace BNR
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point()
        {
            X = 0;
            Y = 0;
        }

        public Point(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        public Vector3 ToVector3()
        {
            return (new Vector3(X, Y, 0));
        }

        public override string ToString()
        {
            return (string.Format("{0}, {1}", X, Y));
        }
    }
}
